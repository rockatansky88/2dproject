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

    [Header("Background Blocker")]
    [SerializeField] private Image backgroundBlocker; // 투명 배경 (클릭 방지용)

    private bool isOpen = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        // 초기 상태: InventoryWindow 비활성화
        gameObject.SetActive(false);

        // 배경 블로커 설정 (없으면 경고)
        if (backgroundBlocker == null)
        {
            Debug.LogWarning("[InventoryWindow] backgroundBlocker가 설정되지 않았습니다! 뒤 클릭 방지가 작동하지 않을 수 있습니다.");
        }
    }

    /// <summary>
    /// 인벤토리 모드 (스탯 표시)
    /// </summary>
    public void OpenInventoryMode()
    {
        Debug.Log("[InventoryWindow] 인벤토리 모드 열기");

        gameObject.SetActive(true);
        isOpen = true;

        // 레이캐스트 차단 활성화
        SetRaycastBlocking(true);

        // 좌측: 스탯 패널
        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);

        // 우측: 인벤토리 패널
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 모드 열림 (StatPanel + InventoryPanel)");
    }

    /// <summary>
    /// 상점 모드 (MerchantShop 클릭 시)
    /// </summary>
    public void OpenShopMode()
    {
        Debug.Log("[InventoryWindow] 상점 모드 열기");

        gameObject.SetActive(true);
        isOpen = true;

        // 레이캐스트 차단 활성화
        SetRaycastBlocking(true);

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

        Debug.Log("[InventoryWindow] ✅ 상점 모드 열림 (ShopPanel + InventoryPanel)");
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

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 윈도우 닫힘");
    }

    private void SetRaycastBlocking(bool block)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = block;
            Debug.Log($"[InventoryWindow] 레이캐스트 차단: {block}");
        }

        if (backgroundBlocker != null)
        {
            backgroundBlocker.raycastTarget = block;
        }
    }

    public bool IsOpen => isOpen;
}