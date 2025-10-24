using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 윈도우 메인 컨트롤러
/// </summary>
public class InventoryWindow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainWindow; // ✅ 새로 추가: 실제 UI 창
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isOpen = false;

    private void Start()
    {
        Debug.Log("[InventoryWindow] Start() 호출");
        // 초기 상태: 창만 닫기 (이 스크립트는 활성화 유지)
        //CloseWindow();
    }

    private void Update()
    {
        // I 키로 인벤토리 토글
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[InventoryWindow] I 키 눌림 감지!");
            ToggleInventory();
        }

        // ESC 키로 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            Debug.Log("[InventoryWindow] ESC 키 눌림 감지!");
            CloseWindow();
        }
    }

    /// <summary>
    /// 인벤토리 토글 (I키)
    /// </summary>
    public void ToggleInventory()
    {
        Debug.Log($"[InventoryWindow] ToggleInventory 호출 (현재 상태: {(isOpen ? "열림" : "닫힘")})");

        if (isOpen)
        {
            CloseWindow();
        }
        else
        {
            OpenInventoryMode();
        }
    }

    /// <summary>
    /// 인벤토리 모드 (스탯 표시)
    /// </summary>
    public void OpenInventoryMode()
    {
        Debug.Log("[InventoryWindow] 인벤토리 모드 열기");

        if (mainWindow != null) mainWindow.SetActive(true);
        isOpen = true;

        // 좌측: 스탯 패널
        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);

        // 우측: 인벤토리 패널
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 모드 열림");
    }

    /// <summary>
    /// 상점 모드
    /// </summary>
    public void OpenShopMode()
    {
        Debug.Log("[InventoryWindow] 상점 모드 열기");

        if (mainWindow != null) mainWindow.SetActive(true);
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

        Debug.Log("[InventoryWindow] ✅ 상점 모드 열림");
    }

    /// <summary>
    /// 윈도우 닫기
    /// </summary>
    public void CloseWindow()
    {
        Debug.Log("[InventoryWindow] 윈도우 닫기");

        if (mainWindow != null) mainWindow.SetActive(false);
        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("[InventoryWindow] ✅ 인벤토리 윈도우 닫힘");
    }
}