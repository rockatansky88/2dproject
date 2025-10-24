using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �κ��丮 ������ ���� ��Ʈ�ѷ�
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
        // �ʱ� ����: ��� �ݱ�
        //CloseWindow();
    }

    private void Update()
    {
        // I Ű�� �κ��丮 ���
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        // ESC Ű�� �ݱ�
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            CloseWindow();
        }
    }

    /// <summary>
    /// �κ��丮 ��� (IŰ)
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
    /// �κ��丮 ��� (���� ǥ��)
    /// </summary>
    public void OpenInventoryMode()
    {
        gameObject.SetActive(true);
        isOpen = true;

        // ����: ���� �г�
        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(true);

        // ����: �κ��丮 �г�
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        Debug.Log("�κ��丮 ��� ����");
    }

    /// <summary>
    /// ���� ���
    /// </summary>
    public void OpenShopMode()
    {
        gameObject.SetActive(true);
        isOpen = true;

        // ����: ���� �г�
        if (statsPanel != null) statsPanel.SetActive(false);
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);

            // ShopPanel ���ΰ�ħ
            ShopPanel shop = shopPanel.GetComponent<ShopPanel>();
            if (shop != null) shop.RefreshShop();
        }

        // ����: �κ��丮 �г�
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        Debug.Log("���� ��� ����");
    }

    /// <summary>
    /// ������ �ݱ�
    /// </summary>
    public void CloseWindow()
    {
        gameObject.SetActive(false);
        isOpen = false;

        if (shopPanel != null) shopPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        Debug.Log("�κ��丮 ������ ����");
    }
}