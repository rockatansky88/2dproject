using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform slotContainer; // GridLayoutGroup�� ���� �θ�
    [SerializeField] private int maxSlots = 24;

    private List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        InitializeSlots();

        // �κ��丮 ���� �̺�Ʈ ����
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        }

        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    private void InitializeSlots()
    {
        // ���� ���� ����
        foreach (var slot in slots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        slots.Clear();

        // ���� ����
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, slotContainer);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();

            if (slot != null)
            {
                slot.Initialize(null, SlotType.Player, 0);
                slot.OnSlotClicked += OnSlotClicked;
                slots.Add(slot);
            }
        }
    }

    /// <summary>
    /// �κ��丮 ���ΰ�ħ
    /// </summary>
    private void RefreshInventory()
    {
        if (InventoryManager.Instance == null) return;

        // ��� ���� ����
        foreach (var slot in slots)
        {
            slot.SetEmpty();
        }

        // �κ��丮 ������ ��������
        var allItems = InventoryManager.Instance.GetAllItems();
        int slotIndex = 0;

        foreach (var kvp in allItems)
        {
            string itemID = kvp.Key;
            int quantity = kvp.Value;

            // ItemDataSO ã�� (Resources �������� �ε�)
            ItemDataSO itemData = Resources.Load<ItemDataSO>($"Items/{itemID}");

            if (itemData == null)
            {
                Debug.LogWarning($"�������� ã�� �� �����ϴ�: {itemID}");
                continue;
            }

            // ������ ��� �������� ó��
            if (itemData.itemType == ItemType.Potion)
            {
                int maxStack = 5;
                while (quantity > 0 && slotIndex < slots.Count)
                {
                    int stackAmount = Mathf.Min(quantity, maxStack);
                    slots[slotIndex].Initialize(itemData, SlotType.Player, stackAmount);
                    quantity -= stackAmount;
                    slotIndex++;
                }
            }
            else
            {
                // ���� ���� ����
                for (int i = 0; i < quantity && slotIndex < slots.Count; i++)
                {
                    slots[slotIndex].Initialize(itemData, SlotType.Player, 1);
                    slotIndex++;
                }
            }
        }
    }

    /// <summary>
    /// ���� Ŭ�� ó�� (�Ǹ�)
    /// </summary>
    private void OnSlotClicked(ItemDataSO item, SlotType slotType)
    {
        if (item == null || slotType != SlotType.Player) return;

        // �Ǹ� ó��
        if (ShopManager.Instance != null)
        {
            bool success = ShopManager.Instance.SellItem(item, 1);

            if (success)
            {
                Debug.Log($"{item.itemName} �Ǹ� �Ϸ�!");
            }
        }
    }
}
