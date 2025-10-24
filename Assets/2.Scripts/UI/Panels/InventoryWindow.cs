using UnityEngine;

/// <summary>
/// 인벤토리 윈도우 메인 컨트롤러
/// </summary>
public class InventoryWindow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject inventoryPanel;

    private bool isOpen = false;

    private void Awake()
    {
        // 초기 상태: InventoryWindow 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 인벤토리 모드 (스탯 표시)
    /// </summary>
    public void OpenInventoryMode()
    {
        Debug.Log("[InventoryWindow] 인벤토리 모드 열기");

        gameObject.SetActive(true);
        isOpen = true;

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

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 윈도우 닫힘");
    }

    public bool IsOpen => isOpen;
}