using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 용병 상세 정보 팝업
/// 모드 1: 고용 모드 (상점 용병 클릭 시) - 고용 버튼 표시
/// 모드 2: 추방 모드 (파티 용병 클릭 시) - 추방 버튼 표시
/// HP/MP Fill Bar를 통해 용병의 현재 상태를 시각적으로 표시합니다.
/// </summary>
public class MercenaryDetailPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupRoot;         // 팝업 루트
    [SerializeField] private Image fullBodyImage;          // 전신 이미지
    [SerializeField] private Text nameText;                // 이름
    [SerializeField] private Text levelText;               // 레벨
    [SerializeField] private Text costText;                // 고용 비용 (추후 사용)
    [SerializeField] private GameObject costPanel;         // 비용 패널 ( 추후 사용)

    [Header("Stat Display")]
    [SerializeField] private Text healthText;              // 체력
    [SerializeField] private Text manaText;                // 마나
    [SerializeField] private Text strengthText;            // 힘
    [SerializeField] private Text dexterityText;           // 민첩
    [SerializeField] private Text wisdomText;              // 지혜
    [SerializeField] private Text intelligenceText;        // 지능
    [SerializeField] private Text speedText;               // 속도

    [Header("HP/MP Fill Bars")]
    [Tooltip("HP를 시각적으로 표시하는 Fill Image (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] private Image healthFillImage;        // HP Fill Bar

    [Tooltip("MP를 시각적으로 표시하는 Fill Image (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] private Image manaFillImage;          // MP Fill Bar

    [Header("Buttons")]
    [SerializeField] private Button recruitButton;         // 고용 버튼
    [SerializeField] private Button dismissButton;         // 추방 버튼
    [SerializeField] private Button closeButton;           // 닫기 버튼

    [Header("Button Texts")]
    [SerializeField] private Text recruitButtonText;       // 고용 버튼 텍스트
    [SerializeField] private Text dismissButtonText;       // 추방 버튼 텍스트

    private MercenaryInstance currentMercenary;
    private PopupMode currentMode;
    private CanvasGroup canvasGroup;

    private enum PopupMode
    {
        Recruit,    // 고용 모드
        Dismiss     // 추방 모드
    }

    private void Awake()
    {

        // CanvasGroup 설정 (popupRoot에 있어야 함)
        if (popupRoot != null)
        {
            canvasGroup = popupRoot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupRoot.AddComponent<CanvasGroup>();
            }
        }

        // 버튼 리스너 등록
        if (recruitButton != null)
        {
            recruitButton.onClick.AddListener(OnRecruitClicked);
        }
        else
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ recruitButton이 null입니다!");
        }

        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(OnDismissClicked);
        }
        else
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ dismissButton이 null입니다!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
        else
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ closeButton이 null입니다!");
        }

        // 초기 상태: CanvasGroup으로 숨김 (popupRoot는 활성화 유지)
        HidePopup();

    }

    /// <summary>
    /// 팝업 숨기기 (CanvasGroup 사용)
    /// </summary>
    private void HidePopup()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

    }

    /// <summary>
    /// 팝업 표시 (CanvasGroup 사용)
    /// </summary>
    private void ShowPopup()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

    }

    /// <summary>
    /// 고용 모드로 팝업 표시
    /// </summary>
    public void ShowRecruitMode(MercenaryInstance mercenary)
    {
        currentMercenary = mercenary;
        currentMode = PopupMode.Recruit;


        if (mercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ mercenary가 null입니다!");
            return;
        }

        // 팝업 표시
        ShowPopup();

        // UI 설정
        SetupUI(mercenary);

        // 버튼 표시 설정
        if (recruitButton != null)
        {
            recruitButton.gameObject.SetActive(true);
        }

        if (dismissButton != null)
        {
            dismissButton.gameObject.SetActive(false);
        }

        // 비용 패널 표시
        if (costPanel != null)
        {
            costPanel.SetActive(true);
        }

    }

    /// <summary>
    /// 추방 모드로 팝업 표시
    /// </summary>
    public void ShowDismissMode(MercenaryInstance mercenary)
    {
        currentMercenary = mercenary;
        currentMode = PopupMode.Dismiss;


        if (mercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ mercenary가 null입니다!");
            return;
        }

        // 팝업 표시
        ShowPopup();

        // UI 설정
        SetupUI(mercenary);

        // 버튼 표시 설정
        if (recruitButton != null)
        {
            recruitButton.gameObject.SetActive(false);
        }

        if (dismissButton != null)
        {
            dismissButton.gameObject.SetActive(true);
        }

        // 비용 패널 숨김
        if (costPanel != null)
        {
            costPanel.SetActive(false);
        }

    }



    /// <summary>
    /// UI 요소 설정
    /// 용병의 스탯 기반 HP/MP(maxHP/maxMP)를 표시합니다.
    /// 전투/비전투 상관없이 동일한 값을 사용합니다.
    /// HP/MP Fill Bar를 통해 시각적으로 게이지를 표시합니다.
    /// </summary>
    private void SetupUI(MercenaryInstance mercenary)
    {

        // 전신 이미지
        if (fullBodyImage != null)
        {
            fullBodyImage.sprite = mercenary.fullBodySprite;
            fullBodyImage.enabled = mercenary.fullBodySprite != null;
        }

        // 이름
        if (nameText != null)
        {
            nameText.text = mercenary.mercenaryName;
        }

        // 레벨
        if (levelText != null)
        {
            levelText.text = $"Level {mercenary.level}";
        }

        // 고용 비용
        if (costText != null)
        {
            costText.text = $"{mercenary.recruitCost}";
        }

        // HP 텍스트
        if (healthText != null)
        {
            healthText.text = $"HP: {mercenary.currentHP}/{mercenary.maxHP}";
        }

        // MP 텍스트
        if (manaText != null)
        {
            manaText.text = $"MP: {mercenary.currentMP}/{mercenary.maxMP}";
        }

        if (healthFillImage != null)
        {
            float hpFillAmount = mercenary.maxHP > 0 ? (float)mercenary.currentHP / mercenary.maxHP : 0f;
            healthFillImage.fillAmount = hpFillAmount;
        }
        else
        {
            Debug.LogWarning("[MercenaryDetailPopup] ⚠️ healthFillImage가 null입니다! Inspector에서 할당해주세요");
        }


        if (manaFillImage != null)
        {
            float mpFillAmount = mercenary.maxMP > 0 ? (float)mercenary.currentMP / mercenary.maxMP : 0f;
            manaFillImage.fillAmount = mpFillAmount;
        }
        else
        {
            Debug.LogWarning("[MercenaryDetailPopup] ⚠️ manaFillImage가 null입니다! Inspector에서 할당해주세요");
        }

        // 기타 스탯
        if (strengthText != null)
        {
            strengthText.text = $"STR: {mercenary.strength}";
        }

        if (dexterityText != null)
        {
            dexterityText.text = $"DEX: {mercenary.dexterity}";
        }

        if (wisdomText != null)
        {
            wisdomText.text = $"WIS: {mercenary.wisdom}";
        }

        if (intelligenceText != null)
        {
            intelligenceText.text = $"INT: {mercenary.intelligence}";
        }

        if (speedText != null)
        {
            speedText.text = $"SPD: {mercenary.speed}";
        }

    }

    /// <summary>
    /// 고용 버튼 클릭 핸들러
    /// </summary>
    private void OnRecruitClicked()
    {

        if (currentMercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ currentMercenary가 null입니다!");
            return;
        }

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        // 고용 시도
        bool success = MercenaryManager.Instance.RecruitMercenary(currentMercenary);

        if (success)
        {
            Close();
        }
        else
        {
            Debug.LogWarning($"[MercenaryDetailPopup] ❌ 고용 실패: {currentMercenary.mercenaryName} (골드 부족 또는 파티 가득참)");
        }
    }

    /// <summary>
    /// 추방 버튼 클릭 핸들러
    /// </summary>
    private void OnDismissClicked()
    {

        if (currentMercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ currentMercenary가 null입니다!");
            return;
        }

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        // 추방 시도
        bool success = MercenaryManager.Instance.DismissMercenary(currentMercenary);

        if (success)
        {
            Close();
        }
        else
        {
            Debug.LogWarning($"[MercenaryDetailPopup] ❌ 추방 실패: {currentMercenary.mercenaryName} (최소 인원 유지)");
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Close()
    {

        HidePopup();
        currentMercenary = null;
    }

    private void OnDestroy()
    {
        // 버튼 리스너 해제
        if (recruitButton != null)
        {
            recruitButton.onClick.RemoveListener(OnRecruitClicked);
        }

        if (dismissButton != null)
        {
            dismissButton.onClick.RemoveListener(OnDismissClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }
}