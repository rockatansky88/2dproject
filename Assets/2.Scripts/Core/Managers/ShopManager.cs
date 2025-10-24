using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Inventory")]
    [SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>();
    [SerializeField] private int shopItemCount = 10;
    [SerializeField] private bool randomizeShop = true;

    private List<ItemDataSO> currentShopItems = new List<ItemDataSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ShopManager] ✅ Instance 생성됨");
        }
        else
        {
            Debug.Log("[ShopManager] 중복 Instance 제거");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshShopInventory();
    }

    public void RefreshShopInventory()
    {
        currentShopItems.Clear();

        Debug.Log($"[ShopManager] 상점 재고 갱신 시작 - availableItems: {availableItems.Count}개");

        if (randomizeShop && availableItems.Count > shopItemCount)
        {
            List<ItemDataSO> shuffled = availableItems.OrderBy(x => Random.value).ToList();
            currentShopItems = shuffled.Take(shopItemCount).ToList();
            Debug.Log($"[ShopManager] 랜덤 선택: {currentShopItems.Count}개");
        }
        else
        {
            currentShopItems = new List<ItemDataSO>(availableItems);
            Debug.Log($"[ShopManager] 전체 표시: {currentShopItems.Count}개");
        }
    }

    public List<ItemDataSO> GetShopItems()
    {
        Debug.Log($"[ShopManager] GetShopItems() 호출 - 반환: {currentShopItems.Count}개");
        return currentShopItems;
    }

    public bool BuyItem(ItemDataSO item)
    {
        Debug.Log($"[ShopManager] ━━━ 구매 시도 ━━━");
        Debug.Log($"[ShopManager] 아이템: {item?.itemName ?? "null"}");

        if (item == null)
        {
            Debug.LogError("[ShopManager] ❌ 아이템이 null입니다!");
            return false;
        }

        Debug.Log($"[ShopManager] 구매 가격: {item.buyPrice}");
        Debug.Log($"[ShopManager] 현재 골드: {GameManager.Instance?.Gold ?? -1}");

        // 골드 확인
        if (GameManager.Instance == null)
        {
            Debug.LogError("[ShopManager] ❌ GameManager.Instance가 null입니다!");
            return false;
        }

        if (GameManager.Instance.SpendGold(item.buyPrice))
        {
            Debug.Log($"[ShopManager] ✅ 골드 차감 성공");

            // 인벤토리에 추가
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("[ShopManager] ❌ InventoryManager.Instance가 null입니다!");
                return false;
            }

            Debug.Log($"[ShopManager] 인벤토리에 추가 시도: {item.itemName}");
            InventoryManager.Instance.AddItem(item, 1);
            Debug.Log($"[ShopManager] ✅✅✅ 구매 완료: {item.itemName}");
            return true;
        }

        Debug.LogWarning("[ShopManager] ❌ 골드가 부족합니다!");
        return false;
    }

    public bool SellItem(ItemDataSO item, int amount = 1)
    {
        Debug.Log($"[ShopManager] ━━━ 판매 시도 ━━━");
        Debug.Log($"[ShopManager] 아이템: {item?.itemName ?? "null"}, 수량: {amount}");

        if (item == null)
        {
            Debug.LogError("[ShopManager] ❌ 아이템이 null입니다!");
            return false;
        }

        if (InventoryManager.Instance.RemoveItem(item.itemID, amount))
        {
            int totalPrice = item.sellPrice * amount;
            GameManager.Instance.AddGold(totalPrice);
            Debug.Log($"[ShopManager] ✅ 판매 완료: {item.itemName} x{amount} = {totalPrice}골드");
            return true;
        }

        Debug.LogWarning("[ShopManager] ❌ 아이템 제거 실패!");
        return false;
    }
}