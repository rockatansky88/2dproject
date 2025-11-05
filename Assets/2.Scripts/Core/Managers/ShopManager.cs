using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopManager : MonoBehaviour
{
	public static ShopManager Instance { get; private set; }

	[Header("Shop Inventory")]
	[SerializeField] private List<ItemDataSO> availableItems = new List<ItemDataSO>(); // 상점에서 판매 가능한 아이템 목록
	[SerializeField] private int shopItemCount = 10; // 상점에 표시할 아이템 수
	[SerializeField] private bool randomizeShop = true; // 상점 아이템을 랜덤으로 선택할지 여부

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
		RefreshShopInventory();  // 게임 시작 시 상점 재고 초기화
	}

	public void RefreshShopInventory()
	{
		currentShopItems.Clear(); // 기존 재고 초기화


		if (randomizeShop && availableItems.Count > shopItemCount) // 랜덤 선택, 충분한 아이템이 있을 때

		{
			List<ItemDataSO> shuffled = availableItems.OrderBy(x => Random.value).ToList(); // 아이템 섞기
			currentShopItems = shuffled.Take(shopItemCount).ToList(); // 상점에 표시할 아이템 선택
		}
		else
		{
			currentShopItems = new List<ItemDataSO>(availableItems);
		}
	}

	public List<ItemDataSO> GetShopItems()
	{
		return currentShopItems;
	}

	public bool BuyItem(ItemDataSO item)
	{

		if (item == null)
		{
			Debug.LogError("[ShopManager] ❌ 아이템이 null입니다!");
			return false;
		}


		// 골드 확인
		if (GameManager.Instance == null)
		{
			Debug.LogError("[ShopManager] ❌ GameManager.Instance가 null입니다!");
			return false;
		}

		if (GameManager.Instance.SpendGold(item.buyPrice))
		{

			// 인벤토리에 추가
			if (InventoryManager.Instance == null)
			{
				Debug.LogError("[ShopManager] ❌ InventoryManager.Instance가 null입니다!");
				return false;
			}

			InventoryManager.Instance.AddItem(item, 1);
			return true;
		}

		Debug.LogWarning("[ShopManager] ❌ 골드가 부족합니다!");
		return false;
	}

	public bool SellItem(ItemDataSO item, int amount = 1)
	{

		if (item == null)
		{
			Debug.LogError("[ShopManager] ❌ 아이템이 null입니다!");
			return false;
		}

		if (InventoryManager.Instance.RemoveItem(item.itemID, amount))
		{
			int totalPrice = item.sellPrice * amount;
			GameManager.Instance.AddGold(totalPrice);
			return true;
		}

		Debug.LogWarning("[ShopManager] ❌ 아이템 제거 실패!");
		return false;
	}
}