using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Inventory")]
    [SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>(); // �Ǹ� ������ ��� ������
    [SerializeField] private int shopItemCount = 10; // ������ ǥ���� ������ ��
    [SerializeField] private bool randomizeShop = true; // ���� ���� ����

    private List<ItemDataSO> currentShopItems = new List<ItemDataSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshShopInventory();
    }

    /// <summary>
    /// ���� ��� ���ΰ�ħ (���� �Ǵ� ��ü)
    /// </summary>
    public void RefreshShopInventory()
    {
        currentShopItems.Clear();

        if (randomizeShop && availableItems.Count > shopItemCount)
        {
            // �����ϰ� ������ ����
            List<ItemDataSO> shuffled = availableItems.OrderBy(x => Random.value).ToList();
            currentShopItems = shuffled.Take(shopItemCount).ToList();
            Debug.Log($"���� ��� ���� ����: {currentShopItems.Count}��");
        }
        else
        {
            // ��ü ������ ǥ��
            currentShopItems = new List<ItemDataSO>(availableItems);
            Debug.Log($"���� ��� ��ü ǥ��: {currentShopItems.Count}��");
        }
    }

    /// <summary>
    /// �������� �Ǹ��ϴ� ������ ���
    /// </summary>
    public List<ItemDataSO> GetShopItems()
    {
        Debug.Log($"[ShopManager] ���� ���� ������ ����: {currentShopItems.Count}");
        return currentShopItems;
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    public bool BuyItem(ItemDataSO item)
    {
        if (item == null) return false;

        // ��� Ȯ��
        if (GameManager.Instance.SpendGold(item.buyPrice))
        {
            // �κ��丮�� �߰�
            InventoryManager.Instance.AddItem(item, 1);
            Debug.Log($"���� �Ϸ�: {item.itemName}");
            return true;
        }

        Debug.Log("��尡 �����մϴ�!");
        return false;
    }

    /// <summary>
    /// ������ �Ǹ�
    /// </summary>
    public bool SellItem(ItemDataSO item, int amount = 1)
    {
        if (item == null) return false;

        // �κ��丮���� ����
        if (InventoryManager.Instance.RemoveItem(item.itemID, amount))
        {
            // ��� �߰�
            int totalPrice = item.sellPrice * amount;
            GameManager.Instance.AddGold(totalPrice);
            Debug.Log($"�Ǹ� �Ϸ�: {item.itemName} x{amount} = {totalPrice}���");
            return true;
        }

        return false;
    }
}