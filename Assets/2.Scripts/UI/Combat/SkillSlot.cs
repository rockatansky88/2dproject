using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 스킬 슬롯 UI
/// - 스킬 아이콘 표시
/// - 마나 소모 표시
/// - 선택 표시 (스킬 아이콘 이미지에 빨간색 Outline 깜빡임)
/// - 클릭 이벤트
/// </summary>
public class SkillSlot : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image skillIconImage;
    [SerializeField] private Text skillNameText;
    [SerializeField] private Text manaCostText;

    //[Header("선택 테두리 - 빨간색")]
    //[SerializeField] private Image selectionBorder; // ❌ 기존: 선택 테두리 Image

    private Outline selectionOutline; // 스킬 아이콘 이미지의 Outline 컴포넌트
    private Coroutine selectionBlinkCoroutine; // 깜빡임 코루틴 참조

    [Header("쿨다운 표시 (나중 확장)")]
    [SerializeField] private Image cooldownOverlay;

    [Header("버튼")]
    [SerializeField] private Button skillButton;

    private SkillDataSO skill;

    // 클릭 이벤트
    public event Action<SkillDataSO> OnSkillClicked;

    /// <summary>
    /// 스킬 데이터 프로퍼티
    /// </summary>
    public SkillDataSO Skill => skill;

    private void Awake()
    {
        // Outline 컴포넌트 초기화
        InitializeSelectionOutline();
    }

    /// <summary>
    /// Outline 컴포넌트 초기화 또는 자동 생성
    /// 스킬 아이콘 이미지(skillIconImage)에 Outline을 추가하여 선택 표시에 사용
    /// </summary>
    private void InitializeSelectionOutline()
    {
        if (skillIconImage == null)
        {
            Debug.LogWarning("[SkillSlot] ⚠️ skillIconImage가 null이어서 Outline을 생성할 수 없습니다!");
            return;
        }

        // 기존 Outline 컴포넌트가 있는지 확인
        selectionOutline = skillIconImage.GetComponent<Outline>();

        // 없으면 새로 추가
        if (selectionOutline == null)
        {
            selectionOutline = skillIconImage.gameObject.AddComponent<Outline>();
        }

        // 초기 설정: 빨간색, 두께 5, 비활성화
        selectionOutline.effectColor = new Color(1f, 0f, 0f, 1f); // 빨간색
        selectionOutline.effectDistance = new Vector2(5f, 5f); // 외곽선 두께
        selectionOutline.enabled = false; // 초기엔 비활성화

    }

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

        // 마나 소모 표시
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

        // Outline 비활성화
        if (selectionOutline != null)
        {
            selectionOutline.enabled = false;
        }

        // 쿨다운 오버레이 숨김 (나중 확장)
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(false);
        }

        // 버튼 이벤트 등록
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnButtonClicked); // 중복 방지
            skillButton.onClick.AddListener(OnButtonClicked);
        }

    }

    // Outline 기반 선택 표시로 변경

    /// <summary>
    /// 선택 표시 활성화/비활성화
    /// 스킬 아이콘 이미지의 Outline 컴포넌트를 사용하여 빨간색 외곽선 깜빡임
    /// </summary>
    public void SetSelected(bool selected)
    {
        // Outline null 체크
        if (selectionOutline == null)
        {
            Debug.LogWarning($"[SkillSlot] ⚠️ {gameObject.name}: Outline 컴포넌트가 없습니다!\n" +
                           $"  - Skill: {(skill != null ? skill.skillName : "null")}\n" +
                           $"  - SetSelected({selected}) 호출 무시\n" +
                           $"  - Outline은 Awake()에서 자동 생성됩니다");
            return;
        }

        // 기존 깜빡임 코루틴 중지
        if (selectionBlinkCoroutine != null)
        {
            StopCoroutine(selectionBlinkCoroutine);
            selectionBlinkCoroutine = null;
        }

        if (selected)
        {
            // 선택 표시 활성화 및 깜빡임 시작
            selectionOutline.enabled = true;
            selectionBlinkCoroutine = StartCoroutine(BlinkSelectionOutline());

            if (skill != null)
            {
            }
        }
        else
        {
            // 선택 표시 비활성화
            selectionOutline.enabled = false;

            if (skill != null)
            {
            }
        }
    }

    /// <summary>
    /// 빨간색 외곽선 깜빡임 효과
    /// Outline의 알파값을 0.5 ~ 1.0 사이에서 반복하여 깜빡이는 효과 생성
    /// </summary>
    private IEnumerator BlinkSelectionOutline()
    {
        float blinkSpeed = 2f; // 깜빡임 속도
        bool fadingOut = true;

        while (true)
        {
            Color color = selectionOutline.effectColor;

            if (fadingOut)
            {
                // 투명하게
                color.a -= Time.deltaTime * blinkSpeed;
                if (color.a <= 0.5f)
                {
                    color.a = 0.5f;
                    fadingOut = false;
                }
            }
            else
            {
                // 불투명하게
                color.a += Time.deltaTime * blinkSpeed;
                if (color.a >= 1f)
                {
                    color.a = 1f;
                    fadingOut = true;
                }
            }

            selectionOutline.effectColor = color;
            yield return null;
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

        // 사용 불가 시 반투명 처리
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
    /// 마나 체크 및 버튼 상태 업데이트
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

        OnSkillClicked?.Invoke(skill);
    }

    private void OnDestroy()
    {
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnButtonClicked);
        }

        // 코루틴 정리
        if (selectionBlinkCoroutine != null)
        {
            StopCoroutine(selectionBlinkCoroutine);
            selectionBlinkCoroutine = null;
        }
    }
}