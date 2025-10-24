using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 윈도우 메인 컨트롤러
/// </summary>
public class InventoryWindow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject inventoryPanel;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isOpen = false;

    private void Start()
    {
        // 초기 상태: 모두 닫기
        //CloseWindow();
    }

    private void Update()
    {
        // I 키로 인벤토리 토글
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        // ESC 키로 닫기
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseWindow();
        }
    }

    /// <summary>
    /// 인벤토리 토글 (I키)
    /// </summary>
    public void ToggleInventory()
    {
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
        gameObject.SetActive(true);
        isOpen = true;

        // 좌측: 스탯 패널
        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);

        // 우측: 인벤토리 패널
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        Debug.Log("인벤토리 모드 열림");
    }

    /// <summary>
    /// 상점 모드
    /// </summary>
    public void OpenShopMode()
    {
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

        Debug.Log("상점 모드 열림");
    }

    /// <summary>
    /// 윈도우 닫기
    /// </summary>
    public void CloseWindow()
    {
        gameObject.SetActive(false);
        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("인벤토리 윈도우 닫힘");
    }
}