using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ���� UI ����
/// - HP �� ǥ��
/// - ���� ��������Ʈ
/// - Ŭ�� �̺�Ʈ (Ÿ�� ����)
/// </summary>
public class MonsterUISlot : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private Image monsterImage;
    [SerializeField] private Text nameText;

    [Header("HP ��")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Text hpText;

    [Header("���� ǥ��")]
    [SerializeField] private GameObject selectionIndicator;

    [Header("��ư")]
    [SerializeField] private Button selectButton;

    private Monster monster;

    // Ŭ�� �̺�Ʈ
    public event Action<Monster> OnMonsterClicked;

    /// <summary>
    /// �ʱ�ȭ
    /// </summary>
    public void Initialize(Monster target)
    {
        monster = target;

        if (monster == null)
        {
            Debug.LogError("[MonsterUISlot] monster�� null�Դϴ�!");
            return;
        }

        // �⺻ ���� ����
        if (nameText != null)
        {
            nameText.text = monster.Name;
        }

        if (monsterImage != null && monster.spawnData != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
        }

        // ���� ��ȭ �̺�Ʈ ����
        monster.Stats.OnHPChanged += UpdateHPBar;

        // �ʱ� HP �� ������Ʈ
        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        // ���� ǥ�� ����
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }

        // ��ư �̺�Ʈ ����
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnButtonClicked);
        }

        Debug.Log($"[MonsterUISlot] {monster.Name} UI ���� �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// HP �� ������Ʈ
    /// </summary>
    private void UpdateHPBar(int currentHP, int maxHP)
    {
        if (hpFillImage != null)
        {
            float fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
            hpFillImage.fillAmount = fillAmount;

            Debug.Log($"[MonsterUISlot] {monster.Name} HP �� ������Ʈ: {currentHP}/{maxHP} ({fillAmount:P0})");
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        // ��� �� ��Ȱ��ȭ
        if (currentHP <= 0)
        {
            OnMonsterDeath();
        }
    }

    /// <summary>
    /// ���� ��� ó��
    /// </summary>
    private void OnMonsterDeath()
    {
        Debug.Log($"[MonsterUISlot] {monster.Name} ��� - UI ��Ȱ��ȭ");

        // ��ư ��Ȱ��ȭ
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }

        // �̹��� ������ ó��
        if (monsterImage != null)
        {
            Color color = monsterImage.color;
            color.a = 0.5f;
            monsterImage.color = color;
        }
    }

    /// <summary>
    /// ���� ǥ��
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
            Debug.Log($"[MonsterUISlot] {monster.Name} ���� ǥ��: {selected}");
        }
    }

    /// <summary>
    /// ��ư Ŭ�� �̺�Ʈ
    /// </summary>
    private void OnButtonClicked()
    {
        if (monster == null || !monster.IsAlive)
        {
            Debug.LogWarning("[MonsterUISlot] ����� ���ʹ� ������ �� �����ϴ�!");
            return;
        }

        Debug.Log($"[MonsterUISlot] ���� Ŭ��: {monster.Name}");
        OnMonsterClicked?.Invoke(monster);
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public Monster GetMonster()
    {
        return monster;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (monster != null && monster.Stats != null)
        {
            monster.Stats.OnHPChanged -= UpdateHPBar;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}