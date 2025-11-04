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
        Debug.Log("[InventoryPanel] Start() 호출");
        InitializeSlots();

        // 인벤토리 변경 이벤트 구독
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
            Debug.Log("[InventoryPanel] ✅ OnInventoryChanged 이벤트 구독 완료");
        }
        else
        {
            Debug.LogError("[InventoryPanel] ❌ InventoryManager.Instance가 null입니다!");
        }

        RefreshInventory();
    }

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    private void InitializeSlots()
    {
        Debug.Log($"[InventoryPanel] 슬롯 초기화 시작 (총 {maxSlots}개)");

        // 기존 슬롯 제거
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                // 🆕 추가: 이벤트 구독 해제
                slot.OnItemUsed -= OnItemUsed;
                slot.OnItemSold -= OnItemSold;

                Destroy(slot.gameObject);
            }
        }
        slots.Clear();

        if (inventorySlotPrefab == null)
        {
            Debug.LogError("[InventoryPanel] ❌ inventorySlotPrefab이 null입니다!");
            return;
        }

        if (slotContainer == null)
        {
            Debug.LogError("[InventoryPanel] ❌ slotContainer가 null입니다!");
            return;
        }

        // 슬롯 생성
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(inventorySlotPrefab, slotContainer);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();

            if (slot != null)
            {
                slot.Initialize(null, SlotType.Player, 0);

                // 🆕 추가: 이벤트 구독
                slot.OnItemUsed += OnItemUsed;
                slot.OnItemSold += OnItemSold;

                slots.Add(slot);
            }
            else
            {
                Debug.LogError($"[InventoryPanel] ❌ 슬롯 {i}에 ItemSlot 컴포넌트가 없습니다!");
            }
        }

        Debug.Log($"[InventoryPanel] ✅ 슬롯 {slots.Count}개 생성 완료");
    }

    /// <summary>
    /// 인벤토리 새로고침
    /// </summary>
    private void RefreshInventory()
    {
        Debug.Log("[InventoryPanel] ━━━┛ RefreshInventory 호출됨 ━━━┛");

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[InventoryPanel] ❌ InventoryManager.Instance가 null입니다!");
            return;
        }

        // 모든 슬롯 비우기
        foreach (var slot in slots)
        {
            slot.SetEmpty();
        }

        // 인벤토리 아이템 가져오기
        var allItems = InventoryManager.Instance.GetAllItems();
        Debug.Log($"[InventoryPanel] 인벤토리에서 가져온 아이템: {allItems.Count}개");

        int slotIndex = 0;

        foreach (var kvp in allItems)
        {
            string itemID = kvp.Key;
            int quantity = kvp.Value;

            Debug.Log($"[InventoryPanel] 처리 중: {itemID} x{quantity}");

            // ✅ 수정: InventoryManager의 캐시된 데이터 사용
            ItemDataSO itemData = InventoryManager.Instance.GetItemData(itemID);

            if (itemData == null)
            {
                Debug.LogError($"[InventoryPanel] ❌ 아이템 데이터를 찾을 수 없습니다: {itemID}");
                continue;
            }

            Debug.Log($"[InventoryPanel] 아이템 데이터 로드 성공: {itemData.itemName} (타입: {itemData.itemType})");

            // 포션인 경우 스택으로 처리
            if (itemData.itemType == ItemType.Potion)
            {
                int maxStack = 5;
                while (quantity > 0 && slotIndex < slots.Count)
                {
                    int stackAmount = Mathf.Min(quantity, maxStack);
                    Debug.Log($"[InventoryPanel] 슬롯 {slotIndex}에 포션 배치: {itemData.itemName} x{stackAmount}");
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
                    Debug.Log($"[InventoryPanel] 슬롯 {slotIndex}에 장비 배치: {itemData.itemName}");
                    slots[slotIndex].Initialize(itemData, SlotType.Player, 1);
                    slotIndex++;
                }
            }
        }

        Debug.Log($"[InventoryPanel] ✅ 총 {slotIndex}개 슬롯에 아이템 배치 완료");
    }

    // 🆕 추가: 아이템 사용 핸들러
    /// <summary>
    /// 아이템 사용 (우클릭)
    /// </summary>
    private void OnItemUsed(ItemDataSO item)
    {
        if (item == null) return;

        Debug.Log($"[InventoryPanel] 아이템 사용: {item.itemName}");

        // 대상 용병 가져오기
        InventoryWindow inventoryWindow = GetComponentInParent<InventoryWindow>();
        MercenaryInstance targetMercenary = inventoryWindow?.GetSelectedMercenary();

        if (targetMercenary == null)
        {
            Debug.LogWarning("[InventoryPanel] 대상 용병이 선택되지 않았습니다");
            return;
        }

        // ItemUsageManager로 처리 위임
        if (ItemUsageManager.Instance != null)
        {
            ItemUsageManager.Instance.UseItem(item, targetMercenary);
        }
        else
        {
            Debug.LogError("[InventoryPanel] ItemUsageManager.Instance가 null입니다!");
        }
    }

    // 🆕 수정: OnSlotClicked → OnItemSold로 변경
    /// <summary>
    /// 아이템 판매 (Ctrl + 좌클릭, 상점 모드만)
    /// </summary>
    private void OnItemSold(ItemDataSO item)
    {
        Debug.Log($"[InventoryPanel] 아이템 판매: {item?.itemName ?? "null"}");

        if (item == null) return;

        // 판매 처리
        if (ShopManager.Instance != null)
        {
            bool success = ShopManager.Instance.SellItem(item, 1);

            if (success)
            {
                Debug.Log($"[InventoryPanel] ✅ {item.itemName} 판매 완료!");
            }
            else
            {
                Debug.LogWarning($"[InventoryPanel] ❌ {item.itemName} 판매 실패");
            }
        }
        else
        {
            Debug.LogError("[InventoryPanel] ❌ ShopManager.Instance가 null입니다!");
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
            Debug.Log("[InventoryPanel] 이벤트 구독 해제");
        }

        // 🆕 추가: 슬롯 이벤트 구독 해제
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.OnItemUsed -= OnItemUsed;
                slot.OnItemSold -= OnItemSold;
            }
        }
    }
}
