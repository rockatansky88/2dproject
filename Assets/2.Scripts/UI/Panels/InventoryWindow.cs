using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 윈도우 메인 컨트롤러
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InventoryWindow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Stats Panel UI")]
    [SerializeField] private Image statsFullBodyImage;     // 전신 이미지
    [SerializeField] private Text statsNameText;           // 이름
    [SerializeField] private Text statsLevelText;          // 레벨
    [SerializeField] private Text statsHealthText;         // HP
    [SerializeField] private Text statsStrengthText;       // STR
    [SerializeField] private Text statsDexterityText;      // DEX
    [SerializeField] private Text statsWisdomText;         // WIS
    [SerializeField] private Text statsIntelligenceText;   // INT
    [SerializeField] private Text statsSpeedText;          // SPD

    [Header("Mercenary List Panel")]
    [SerializeField] private Transform mercenaryListContainer; // 용병 슬롯 생성 부모
    [SerializeField] private GameObject mercenaryInventorySlotPrefab; // 용병 슬롯 프리팹 (미작업)

    [Header("References")]
    [SerializeField] private MercenaryParty mercenaryParty; // 하단 용병 파티 슬롯

    [Header("Background Blocker")]
    [SerializeField] private Image backgroundBlocker;

    private bool isOpen = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 초기 상태: InventoryWindow 비활성화
        gameObject.SetActive(false);

        if (backgroundBlocker == null)
        {
            Debug.LogWarning("[InventoryWindow] backgroundBlocker가 설정되지 않았습니다!");
        }

        Debug.Log("[InventoryWindow] Awake 완료");
    }

    /// <summary>
    /// 인벤토리 모드 (I 키 입력 시)
    /// </summary>
    public void OpenInventoryMode()
    {
        Debug.Log("[InventoryWindow] ━━━ 인벤토리 모드 열기 ━━━");

        gameObject.SetActive(true);
        isOpen = true;

        // 레이캐스트 차단 활성화
        SetRaycastBlocking(true);

        // MercenaryParty 숨기기
        HideMercenaryParty();

        // 좌측: 스탯 패널
        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);

        // 우측: 인벤토리 패널
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        // 용병 리스트 갱신 및 첫 번째 용병 스탯 표시
        RefreshMercenaryList();
        ShowFirstMercenaryStats();

        Debug.Log("[InventoryWindow] ✅ 인벤토리 모드 열림");
    }

    /// <summary>
    /// 상점 모드 (MerchantShop 클릭 시)
    /// </summary>
    public void OpenShopMode()
    {
        Debug.Log("[InventoryWindow] ━━━ 상점 모드 열기 ━━━");

        gameObject.SetActive(true);
        isOpen = true;

        // 레이캐스트 차단 활성화
        SetRaycastBlocking(true);

        // MercenaryParty 숨기기
        HideMercenaryParty();

        // 좌측: 상점 패널
        if (statsPanel != null) statsPanel.SetActive(false);
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);

            // ShopPanel 새로고침
            ShopPanel shop = shopPanel.GetComponent<ShopPanel>();
            if (shop != null) shop.RefreshShop();
        }

        // 우측: 인벤토리 패널
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        // 용병 리스트 갱신
        RefreshMercenaryList();

        Debug.Log("[InventoryWindow] ✅ 상점 모드 열림");
    }

    /// <summary>
    /// 윈도우 닫기
    /// </summary>
    public void CloseWindow()
    {
        Debug.Log("[InventoryWindow] 윈도우 닫기");

        gameObject.SetActive(false);
        isOpen = false;

        // 레이캐스트 차단 비활성화
        SetRaycastBlocking(false);

        // MercenaryParty 다시 표시
        ShowMercenaryParty();

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 윈도우 닫힘");
    }

    /// <summary>
    /// MercenaryParty 숨기기
    /// </summary>
    private void HideMercenaryParty()
    {
        if (mercenaryParty != null)
        {
            mercenaryParty.gameObject.SetActive(false);
            Debug.Log("[InventoryWindow] MercenaryParty 숨김");
        }
    }

    /// <summary>
    /// MercenaryParty 표시
    /// </summary>
    private void ShowMercenaryParty()
    {
        if (mercenaryParty != null)
        {
            mercenaryParty.gameObject.SetActive(true);
            Debug.Log("[InventoryWindow] MercenaryParty 표시");
        }
    }

    /// <summary>
    /// 용병 리스트 갱신 (MercenaryListPanel에 슬롯 생성)
    /// </summary>
    private void RefreshMercenaryList()
    {
        Debug.Log("[InventoryWindow] ━━━ 용병 리스트 갱신 시작 ━━━");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[InventoryWindow] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        // TODO: 기존 슬롯 제거
        // TODO: mercenaryInventorySlotPrefab으로 슬롯 생성
        // TODO: 슬롯 클릭 시 StatsPanel 업데이트

        var mercenaries = MercenaryManager.Instance.RecruitedMercenaries;
        Debug.Log($"[InventoryWindow] 보유 용병 수: {mercenaries.Count}");

        // 슬롯 프리팹이 없으면 경고
        if (mercenaryInventorySlotPrefab == null)
        {
            Debug.LogWarning("[InventoryWindow] ⚠️ mercenaryInventorySlotPrefab이 null입니다! 슬롯을 생성할 수 없습니다.");
            return;
        }

        // 기존 슬롯 제거
        foreach (Transform child in mercenaryListContainer)
        {
            Destroy(child.gameObject);
        }

        // 용병 슬롯 생성
        foreach (var mercenary in mercenaries)
        {
            GameObject slotObj = Instantiate(mercenaryInventorySlotPrefab, mercenaryListContainer);
            // TODO: MercenaryInventorySlot 스크립트 추가 후 Initialize
            Debug.Log($"[InventoryWindow] 용병 슬롯 생성: {mercenary.mercenaryName}");
        }

        Debug.Log("[InventoryWindow] ✅ 용병 리스트 갱신 완료");
    }

    /// <summary>
    /// 첫 번째 용병의 스탯을 StatsPanel에 표시
    /// </summary>
    private void ShowFirstMercenaryStats()
    {
        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[InventoryWindow] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        var mercenaries = MercenaryManager.Instance.RecruitedMercenaries;

        if (mercenaries.Count == 0)
        {
            Debug.Log("[InventoryWindow] 용병이 없어 StatsPanel을 비웁니다.");
            ClearStatsPanel();
            return;
        }

        // 첫 번째 용병 스탯 표시
        ShowMercenaryStats(mercenaries[0]);
    }

    /// <summary>
    /// 특정 용병의 스탯을 StatsPanel에 표시
    /// </summary>
    public void ShowMercenaryStats(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            Debug.LogError("[InventoryWindow] ❌ mercenary가 null입니다!");
            return;
        }

        Debug.Log($"[InventoryWindow] StatsPanel에 용병 표시: {mercenary.mercenaryName}");

        // 전신 이미지
        if (statsFullBodyImage != null)
        {
            statsFullBodyImage.sprite = mercenary.fullBodySprite;
            statsFullBodyImage.enabled = mercenary.fullBodySprite != null;
        }

        // 이름
        if (statsNameText != null)
        {
            statsNameText.text = mercenary.mercenaryName;
        }

        // 레벨
        if (statsLevelText != null)
        {
            statsLevelText.text = $"Level {mercenary.level}";
        }

        // 스탯
        if (statsHealthText != null)
        {
            statsHealthText.text = $"HP: {mercenary.health}";
        }

        if (statsStrengthText != null)
        {
            statsStrengthText.text = $"STR: {mercenary.strength}";
        }

        if (statsDexterityText != null)
        {
            statsDexterityText.text = $"DEX: {mercenary.dexterity}";
        }

        if (statsWisdomText != null)
        {
            statsWisdomText.text = $"WIS: {mercenary.wisdom}";
        }

        if (statsIntelligenceText != null)
        {
            statsIntelligenceText.text = $"INT: {mercenary.intelligence}";
        }

        if (statsSpeedText != null)
        {
            statsSpeedText.text = $"SPD: {mercenary.speed}";
        }

        Debug.Log("[InventoryWindow] ✅ StatsPanel 업데이트 완료");
    }

    /// <summary>
    /// StatsPanel 비우기
    /// </summary>
    private void ClearStatsPanel()
    {
        if (statsFullBodyImage != null)
        {
            statsFullBodyImage.sprite = null;
            statsFullBodyImage.enabled = false;
        }

        if (statsNameText != null) statsNameText.text = "";
        if (statsLevelText != null) statsLevelText.text = "";
        if (statsHealthText != null) statsHealthText.text = "";
        if (statsStrengthText != null) statsStrengthText.text = "";
        if (statsDexterityText != null) statsDexterityText.text = "";
        if (statsWisdomText != null) statsWisdomText.text = "";
        if (statsIntelligenceText != null) statsIntelligenceText.text = "";
        if (statsSpeedText != null) statsSpeedText.text = "";

        Debug.Log("[InventoryWindow] StatsPanel 비움");
    }

    private void SetRaycastBlocking(bool block)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = block;
        }

        if (backgroundBlocker != null)
        {
            backgroundBlocker.raycastTarget = block;
        }
    }

    public bool IsOpen => isOpen;
}