using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ��ų ���� UI
/// - ��ų ������ ǥ��
/// - ���� �Ҹ� ǥ��
/// - ���� �׵θ� ǥ�� (������)
/// - Ŭ�� �̺�Ʈ
/// </summary>
public class SkillSlot : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private Image skillIconImage;
    [SerializeField] private Text skillNameText;
    [SerializeField] private Text manaCostText;

    [Header("���� �׵θ� - ������")]
    [SerializeField] private Image selectionBorder;

    [Header("��ٿ� ǥ�� (���� Ȯ��)")]
    [SerializeField] private Image cooldownOverlay;

    [Header("��ư")]
    [SerializeField] private Button skillButton;

    private SkillDataSO skill;

    // Ŭ�� �̺�Ʈ
    public event Action<SkillDataSO> OnSkillClicked;

    /// <summary>
    /// ��ų ���� ������Ƽ
    /// </summary>
    public SkillDataSO Skill => skill;

    /// <summary>
    /// �ʱ�ȭ
    /// </summary>
    public void Initialize(SkillDataSO skillData)
    {
        skill = skillData;

        if (skill == null)
        {
            Debug.LogError("[SkillSlot] skill�� null�Դϴ�!");
            return;
        }

        // ��ų ������ ����
        if (skillIconImage != null && skill.skillIcon != null)
        {
            skillIconImage.sprite = skill.skillIcon;
        }

        // ��ų �̸� ����
        if (skillNameText != null)
        {
            skillNameText.text = skill.skillName;
        }

        // ���� �Ҹ� ǥ��
        if (manaCostText != null)
        {
            if (skill.isBasicAttack || skill.manaCost == 0)
            {
                manaCostText.text = "";  // �⺻ ������ ���� ǥ�� �� ��
            }
            else
            {
                manaCostText.text = $"MP: {skill.manaCost}";
            }
        }

        // ���� �׵θ� ����
        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(false);
        }

        // ��ٿ� �������� ���� (���� Ȯ��)
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }

        // ��ư �̺�Ʈ ����
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnButtonClicked);
        }

        Debug.Log($"[SkillSlot] {skill.skillName} ���� �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// ���� �׵θ� ǥ��/���� (������)
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(selected);

            // ���������� ����
            if (selected)
            {
                selectionBorder.color = Color.red;
            }

            Debug.Log($"[SkillSlot] {skill.skillName} ���� ����: {selected}");
        }
    }

    /// <summary>
    /// ��ų ��� ���� ���� ����
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (skillButton != null)
        {
            skillButton.interactable = interactable;
        }

        // ���� ���� �� ������ ó��
        if (!interactable && skillIconImage != null)
        {
            Color color = skillIconImage.color;
            color.a = 0.5f;
            skillIconImage.color = color;
        }
        else if (skillIconImage != null)
        {
            Color color = skillIconImage.color;
            color.a = 1f;
            skillIconImage.color = color;
        }
    }

    /// <summary>
    /// ���� üũ �� ��ư ���� ������Ʈ
    /// </summary>
    public void UpdateManaCost(int currentMP)
    {
        if (skill == null) return;

        // �⺻ �����̸� �׻� ��� ����
        if (skill.isBasicAttack)
        {
            SetInteractable(true);
            return;
        }

        // ���� ���� �� ��Ȱ��ȭ
        bool canUse = currentMP >= skill.manaCost;
        SetInteractable(canUse);

        Debug.Log($"[SkillSlot] {skill.skillName} ��� ����: {canUse} (���� MP: {currentMP}, �ʿ� MP: {skill.manaCost})");
    }

    /// <summary>
    /// ��ư Ŭ�� �̺�Ʈ
    /// </summary>
    private void OnButtonClicked()
    {
        if (skill == null)
        {
            Debug.LogError("[SkillSlot] skill�� null�Դϴ�!");
            return;
        }

        Debug.Log($"[SkillSlot] ��ų Ŭ��: {skill.skillName}")
;
        OnSkillClicked?.Invoke(skill);
    }

    private void OnDestroy()
    {
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}