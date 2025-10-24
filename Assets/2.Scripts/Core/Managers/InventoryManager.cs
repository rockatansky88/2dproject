using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // ������ID, ����
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    // �κ��丮 ���� �̺�Ʈ
    public event Action OnInventoryChanged;

    // ������ ������ ĳ�� (Resources �������� �ε�)
    private Dictionary<string, ItemDataSO> itemDataCache = new Dictionary<string, ItemDataSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllItemData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Resources �������� ��� ������ ������ �ε�
    /// </summary>
    private void LoadAllItemData()
    {
        ItemDataSO[] items = Resources.LoadAll<ItemDataSO>("Items");

        foreach (var item in items)
        {
            if (!itemDataCache.ContainsKey(item.itemID))
            {
                itemDataCache[item.itemID] = item;
            }
        }

        Debug.Log($"������ ������ �ε� �Ϸ�: {itemDataCache.Count}��");
    }

    /// <summary>
    /// ������ ������ ��������
    /// </summary>
    public ItemDataSO GetItemData(string itemID)
    {
        if (itemDataCache.ContainsKey(itemID))
        {
            return itemDataCache[itemID];
        }

        Debug.LogWarning($"������ �����͸� ã�� �� �����ϴ�: {itemID}");
        return null;
    }

    /// <summary>
    /// ������ �߰�
    /// </summary>
    public void AddItem(ItemDataSO item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("�������� null�Դϴ�!");
            return;
        }

        if (inventory.ContainsKey(item.itemID))
        {
            inventory[item.itemID] += amount;
        }
        else
        {
            inventory[item.itemID] = amount;
        }

        Debug.Log($"������ �߰�: {item.itemName} x{amount}");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    public bool RemoveItem(string itemID, int amount = 1)
    {
        if (!inventory.ContainsKey(itemID))
        {
            Debug.Log("�ش� �������� �����ϴ�!");
            return false;
        }

        if (inventory[itemID] < amount)
        {
            Debug.Log("������ ������ �����մϴ�!");
            return false;
        }

        inventory[itemID] -= amount;

        if (inventory[itemID] <= 0)
        {
            inventory.Remove(itemID);
        }

        Debug.Log($"������ ����: {itemID} x{amount}");
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// ������ ���� Ȯ��
    /// </summary>
    public int GetItemCount(string itemID)
    {
        return inventory.ContainsKey(itemID) ? inventory[itemID] : 0;
    }

    /// <summary>
    /// ��ü �κ��丮 ��ȯ
    /// </summary>
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(inventory);
    }

    /// <summary>
    /// �κ��丮 �ʱ�ȭ (�׽�Ʈ��)
    /// </summary>
    public void Clear()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke();
    }
}