using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Inventory")]
    [SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>(); // 판매 가능한 모든 아이템
    [SerializeField] private int shopItemCount = 10; // 상점에 표시할 아이템 수
    [SerializeField] private bool randomizeShop = true; // 랜덤 상점 여부

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
    /// 상점 재고 새로고침 (랜덤 또는 전체)
    /// </summary>
    public void RefreshShopInventory()
    {
        currentShopItems.Clear();

        if (randomizeShop && availableItems.Count > shopItemCount)
        {
            // 랜덤하게 아이템 선택
            List<ItemDataSO> shuffled = availableItems.OrderBy(x => Random.value).ToList();
            currentShopItems = shuffled.Take(shopItemCount).ToList();
            Debug.Log($"상점 재고 랜덤 생성: {currentShopItems.Count}개");
        }
        else
        {
            // 전체 아이템 표시
            currentShopItems = new List<ItemDataSO>(availableItems);
            Debug.Log($"상점 재고 전체 표시: {currentShopItems.Count}개");
        }
    }

    /// <summary>
    /// 상점에서 판매하는 아이템 목록
    /// </summary>
    public List<ItemDataSO> GetShopItems()
    {
        Debug.Log($"[ShopManager] 현재 상점 아이템 개수: {currentShopItems.Count}");
        return currentShopItems;
    }

    /// <summary>
    /// 아이템 구매
    /// </summary>
    public bool BuyItem(ItemDataSO item)
    {
        if (item == null) return false;

        // 골드 확인
        if (GameManager.Instance.SpendGold(item.buyPrice))
        {
            // 인벤토리에 추가
            InventoryManager.Instance.AddItem(item, 1);
            Debug.Log($"구매 완료: {item.itemName}");
            return true;
        }

        Debug.Log("골드가 부족합니다!");
        return false;
    }

    /// <summary>
    /// 아이템 판매
    /// </summary>
    public bool SellItem(ItemDataSO item, int amount = 1)
    {
        if (item == null) return false;

        // 인벤토리에서 제거
        if (InventoryManager.Instance.RemoveItem(item.itemID, amount))
        {
            // 골드 추가
            int totalPrice = item.sellPrice * amount;
            GameManager.Instance.AddGold(totalPrice);
            Debug.Log($"판매 완료: {item.itemName} x{amount} = {totalPrice}골드");
            return true;
        }

        return false;
    }
}