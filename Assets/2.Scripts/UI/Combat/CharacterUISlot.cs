using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ĳ���� UI ����
/// - HP/MP �� ǥ��
/// - ĳ���� �ʻ�ȭ
/// - ���� ��ȭ �ǽð� �ݿ�
/// </summary>
public class CharacterUISlot : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText;

    [Header("HP ��")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Text hpText;

    [Header("MP ��")]
    [SerializeField] private Image mpFillImage;
    [SerializeField] private Text mpText;

    [Header("���� �� ǥ��")]
    [SerializeField] private GameObject turnIndicator;

    private Character character;

    /// <summary>
    /// �ʱ�ȭ
    /// </summary>
    public void Initialize(Character target)
    {
        character = target;

        if (character == null)
        {
            Debug.LogError("[CharacterUISlot] character�� null�Դϴ�!");
            return;
        }

        // �⺻ ���� ����
        if (nameText != null)
        {
            nameText.text = character.Name;
        }

        if (portraitImage != null && character.mercenaryData != null)
        {
            portraitImage.sprite = character.mercenaryData.portrait;
        }

        // ���� ��ȭ �̺�Ʈ ����
        character.Stats.OnHPChanged += UpdateHPBar;
        character.Stats.OnMPChanged += UpdateMPBar;

        // �ʱ� �� ������Ʈ
        UpdateHPBar(character.Stats.CurrentHP, character.Stats.MaxHP);
        UpdateMPBar(character.Stats.CurrentMP, character.Stats.MaxMP);

        // �� ǥ�� ����
        if (turnIndicator != null)
        {
            turnIndicator.SetActive(false);
        }

        Debug.Log($"[CharacterUISlot] {character.Name} UI ���� �ʱ�ȭ �Ϸ�");
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

            Debug.Log($"[CharacterUISlot] {character.Name} HP �� ������Ʈ: {currentHP}/{maxHP} ({fillAmount:P0})");
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }
    }

    /// <summary>
    /// MP �� ������Ʈ
    /// </summary>
    private void UpdateMPBar(int currentMP, int maxMP)
    {
        if (mpFillImage != null)
        {
            float fillAmount = maxMP > 0 ? (float)currentMP / maxMP : 0f;
            mpFillImage.fillAmount = fillAmount;

            Debug.Log($"[CharacterUISlot] {character.Name} MP �� ������Ʈ: {currentMP}/{maxMP} ({fillAmount:P0})");
        }

        if (mpText != null)
        {
            mpText.text = $"{currentMP}/{maxMP}";
        }
    }

    /// <summary>
    /// ���� �� ǥ��
    /// </summary>
    public void SetTurnActive(bool active)
    {
        if (turnIndicator != null)
        {
            turnIndicator.SetActive(active);
            Debug.Log($"[CharacterUISlot] {character.Name} �� ǥ��: {active}");
        }
    }

    /// <summary>
    /// ĳ���� ���� ��ȯ
    /// </summary>
    public Character GetCharacter()
    {
        return character;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (character != null && character.Stats != null)
        {
            character.Stats.OnHPChanged -= UpdateHPBar;
            character.Stats.OnMPChanged -= UpdateMPBar;
        }
    }
}