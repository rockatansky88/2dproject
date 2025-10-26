using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 용병 상세 정보 팝업
/// 모드 1: 고용 모드 (상점 용병 클릭 시) - 고용 버튼 표시
/// 모드 2: 추방 모드 (파티 용병 클릭 시) - 추방 버튼 표시
/// </summary>
public class MercenaryDetailPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupRoot;         // 팝업 루트
    [SerializeField] private Image fullBodyImage;          // 전신 이미지
    [SerializeField] private Text nameText;                // 이름
    [SerializeField] private Text levelText;               // 레벨
    [SerializeField] private Text costText;                // 고용 비용 (고용 모드에만 표시)
    [SerializeField] private GameObject costPanel;         // 비용 패널 (고용 모드에만 활성화)

    [Header("Stat Display")]
    [SerializeField] private Text healthText;              // 체력
    [SerializeField] private Text strengthText;            // 힘
    [SerializeField] private Text dexterityText;           // 민첩
    [SerializeField] private Text wisdomText;              // 지혜
    [SerializeField] private Text intelligenceText;        // 지능
    [SerializeField] private Text speedText;               // 속도

    [Header("Buttons")]
    [SerializeField] private Button recruitButton;         // 고용 버튼
    [SerializeField] private Button dismissButton;         // 추방 버튼
    [SerializeField] private Button closeButton;           // 닫기 버튼

    [Header("Button Texts")]
    [SerializeField] private Text recruitButtonText;       // 고용 버튼 텍스트
    [SerializeField] private Text dismissButtonText;       // 추방 버튼 텍스트

    private MercenaryInstance currentMercenary;
    private PopupMode currentMode;

    private enum PopupMode
    {
        Recruit,    // 고용 모드
        Dismiss     // 추방 모드
    }

    private void Awake()
    {
        // 버튼 리스너 등록
        if (recruitButton != null)
        {
            recruitButton.onClick.AddListener(OnRecruitClicked);
            Debug.Log("[MercenaryDetailPopup] 고용 버튼 리스너 등록됨");
        }

        if (dismissButton != null)
        {
            dismissButton.onClick.AddListener(OnDismissClicked);
            Debug.Log("[MercenaryDetailPopup] 추방 버튼 리스너 등록됨");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            Debug.Log("[MercenaryDetailPopup] 닫기 버튼 리스너 등록됨");
        }

        // 초기 상태: 비활성화
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

        Debug.Log("[MercenaryDetailPopup] Awake 완료");
    }

    /// <summary>
    /// 고용 모드로 팝업 표시
    /// </summary>
    public void ShowRecruitMode(MercenaryInstance mercenary)
    {
        currentMercenary = mercenary;
        currentMode = PopupMode.Recruit;

        Debug.Log($"[MercenaryDetailPopup] ━━━ 고용 모드 표시: {mercenary?.mercenaryName ?? "null"} ━━━");

        if (mercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ mercenary가 null입니다!");
            return;
        }

        // 팝업 활성화
        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
        }

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

        Debug.Log("[MercenaryDetailPopup] ✅ 고용 모드 표시 완료");
    }

    /// <summary>
    /// 추방 모드로 팝업 표시
    /// </summary>
    public void ShowDismissMode(MercenaryInstance mercenary)
    {
        currentMercenary = mercenary;
        currentMode = PopupMode.Dismiss;

        Debug.Log($"[MercenaryDetailPopup] ━━━ 추방 모드 표시: {mercenary?.mercenaryName ?? "null"} ━━━");

        if (mercenary == null)
        {
            Debug.LogError("[MercenaryDetailPopup] ❌ mercenary가 null입니다!");
            return;
        }

        // 팝업 활성화
        if (popupRoot != null)
        {
            popupRoot.SetActive(true);
        }

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

        Debug.Log("[MercenaryDetailPopup] ✅ 추방 모드 표시 완료");
    }

    /// <summary>
    /// UI 요소 설정
    /// </summary>
    private void SetupUI(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryDetailPopup] UI 설정 시작: {mercenary.mercenaryName}");

        // 전신 이미지
        if (fullBodyImage != null)
        {
            fullBodyImage.sprite = mercenary.fullBodySprite;
            fullBodyImage.enabled = mercenary.fullBodySprite != null;
            Debug.Log($"[MercenaryDetailPopup] 전신 이미지 설정: {mercenary.fullBodySprite?.name ?? "null"}");
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

        // 스탯 표시
        if (healthText != null)
        {
            healthText.text = $"HP: {mercenary.health}";
        }

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

        Debug.Log($"[MercenaryDetailPopup] ✅ UI 설정 완료");
        Debug.Log($"[MercenaryDetailPopup] 스탯 - HP:{mercenary.health} STR:{mercenary.strength} " +
                  $"DEX:{mercenary.dexterity} WIS:{mercenary.wisdom} INT:{mercenary.intelligence} SPD:{mercenary.speed}");
    }

    /// <summary>
    /// 고용 버튼 클릭 핸들러
    /// </summary>
    private void OnRecruitClicked()
    {
        Debug.Log($"[MercenaryDetailPopup] 🖱️ 고용 버튼 클릭: {currentMercenary?.mercenaryName ?? "null"}");

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
        Debug.Log($"[MercenaryDetailPopup] MercenaryManager.RecruitMercenary() 호출...");
        bool success = MercenaryManager.Instance.RecruitMercenary(currentMercenary);

        if (success)
        {
            Debug.Log($"[MercenaryDetailPopup] ✅✅✅ 고용 성공: {currentMercenary.mercenaryName}");
            Close();
        }
        else
        {
            Debug.LogWarning($"[MercenaryDetailPopup] ❌ 고용 실패: {currentMercenary.mercenaryName} (골드 부족 또는 파티 가득참)");
            // TODO: 실패 메시지 표시 (선택 사항)
        }
    }

    /// <summary>
    /// 추방 버튼 클릭 핸들러
    /// </summary>
    private void OnDismissClicked()
    {
        Debug.Log($"[MercenaryDetailPopup] 🖱️ 추방 버튼 클릭: {currentMercenary?.mercenaryName ?? "null"}");

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
        Debug.Log($"[MercenaryDetailPopup] MercenaryManager.DismissMercenary() 호출...");
        bool success = MercenaryManager.Instance.DismissMercenary(currentMercenary);

        if (success)
        {
            Debug.Log($"[MercenaryDetailPopup] ✅ 추방 성공: {currentMercenary.mercenaryName}");
            Close();
        }
        else
        {
            Debug.LogWarning($"[MercenaryDetailPopup] ❌ 추방 실패: {currentMercenary.mercenaryName} (최소 인원 유지)");
            // TODO: 실패 메시지 표시
        }
    }

    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public void Close()
    {
        Debug.Log("[MercenaryDetailPopup] 팝업 닫기");

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }

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