using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 스킬 슬롯 UI
/// - 스킬 아이콘 표시
/// - 마나 소모량 표시
/// - 선택 테두리 표시 (빨간색)
/// - 클릭 이벤트
/// </summary>
public class SkillSlot : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image skillIconImage;
    [SerializeField] private Text skillNameText;
    [SerializeField] private Text manaCostText;

    [Header("선택 테두리 - 빨간색")]
    [SerializeField] private Image selectionBorder;

    [Header("쿨다운 표시 (추후 확장)")]
    [SerializeField] private Image cooldownOverlay;

    [Header("버튼")]
    [SerializeField] private Button skillButton;

    private SkillDataSO skill;

    // 클릭 이벤트
    public event Action<SkillDataSO> OnSkillClicked;

    /// <summary>
    /// 스킬 참조 프로퍼티
    /// </summary>
    public SkillDataSO Skill => skill;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(SkillDataSO skillData)
    {
        skill = skillData;

        if (skill == null)
        {
            Debug.LogError("[SkillSlot] skill이 null입니다!");
            return;
        }

        // 스킬 아이콘 설정
        if (skillIconImage != null && skill.skillIcon != null)
        {
            skillIconImage.sprite = skill.skillIcon;
        }

        // 스킬 이름 설정
        if (skillNameText != null)
        {
            skillNameText.text = skill.skillName;
        }

        // 마나 소모량 표시
        if (manaCostText != null)
        {
            if (skill.isBasicAttack || skill.manaCost == 0)
            {
                manaCostText.text = "";  // 기본 공격은 마나 표시 안 함
            }
            else
            {
                manaCostText.text = $"MP: {skill.manaCost}";
            }
        }

        // 선택 테두리 숨김
        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(false);
        }

        // 쿨다운 오버레이 숨김 (추후 확장)
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }

        // 버튼 이벤트 연결
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnButtonClicked);
        }

        Debug.Log($"[SkillSlot] {skill.skillName} 슬롯 초기화 완료");
    }

    /// <summary>
    /// 선택 테두리 표시/숨김 (빨간색)
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
        {
            selectionBorder.gameObject.SetActive(selected);

            // 빨간색으로 설정
            if (selected)
            {
                selectionBorder.color = Color.red;
            }

            Debug.Log($"[SkillSlot] {skill.skillName} 선택 상태: {selected}");
        }
    }

    /// <summary>
    /// 스킬 사용 가능 여부 설정
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (skillButton != null)
        {
            skillButton.interactable = interactable;
        }

        // 마나 부족 시 반투명 처리
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
    /// 마나 체크 후 버튼 상태 업데이트
    /// </summary>
    public void UpdateManaCost(int currentMP)
    {
        if (skill == null) return;

        // 기본 공격이면 항상 사용 가능
        if (skill.isBasicAttack)
        {
            SetInteractable(true);
            return;
        }

        // 마나 부족 시 비활성화
        bool canUse = currentMP >= skill.manaCost;
        SetInteractable(canUse);

        Debug.Log($"[SkillSlot] {skill.skillName} 사용 가능: {canUse} (현재 MP: {currentMP}, 필요 MP: {skill.manaCost})");
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        if (skill == null)
        {
            Debug.LogError("[SkillSlot] skill이 null입니다!");
            return;
        }

        Debug.Log($"[SkillSlot] 스킬 클릭: {skill.skillName}")
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