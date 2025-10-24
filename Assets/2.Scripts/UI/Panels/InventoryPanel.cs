using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Transform slotContainer; // GridLayoutGroup을 가진 부모
    [SerializeField] private int maxSlots = 24;

    private List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        InitializeSlots();

        // 인벤토리 변경 이벤트 구독
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
    /// 슬롯 초기화
    /// </summary>
    private void InitializeSlots()
    {
        // 기존 슬롯 제거
        foreach (var slot in slots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        slots.Clear();

        // 슬롯 생성
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
    /// 인벤토리 새로고침
    /// </summary>
    private void RefreshInventory()
    {
        if (InventoryManager.Instance == null) return;

        // 모든 슬롯 비우기
        foreach (var slot in slots)
        {
            slot.SetEmpty();
        }

        // 인벤토리 아이템 가져오기
        var allItems = InventoryManager.Instance.GetAllItems();
        int slotIndex = 0;

        foreach (var kvp in allItems)
        {
            string itemID = kvp.Key;
            int quantity = kvp.Value;

            // ItemDataSO 찾기 (Resources 폴더에서 로드)
            ItemDataSO itemData = Resources.Load<ItemDataSO>($"Items/{itemID}");

            if (itemData == null)
            {
                Debug.LogWarning($"아이템을 찾을 수 없습니다: {itemID}");
                continue;
            }

            // 포션인 경우 스택으로 처리
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
                // 장비는 개별 슬롯
                for (int i = 0; i < quantity && slotIndex < slots.Count; i++)
                {
                    slots[slotIndex].Initialize(itemData, SlotType.Player, 1);
                    slotIndex++;
                }
            }
        }
    }

    /// <summary>
    /// 슬롯 클릭 처리 (판매)
    /// </summary>
    private void OnSlotClicked(ItemDataSO item, SlotType slotType)
    {
        if (item == null || slotType != SlotType.Player) return;

        // 판매 처리
        if (ShopManager.Instance != null)
        {
            bool success = ShopManager.Instance.SellItem(item, 1);

            if (success)
            {
                Debug.Log($"{item.itemName} 판매 완료!");
            }
        }
    }
}
