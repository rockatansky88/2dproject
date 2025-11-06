using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class ShopPanel : BasePanel
{
	[Header("References")]
	[SerializeField] private GameObject shopItemSlotPrefab;
	[SerializeField] private Transform shopItemContainer;
	[SerializeField] private Text goldText;

	private List<ShopItemSlot> shopSlots = new List<ShopItemSlot>();

	protected override void Awake()
	{
		base.Awake();

		if (shopItemSlotPrefab == null)
			Debug.LogError("[ShopPanel] ❌ shopItemSlotPrefab이 설정되지 않았습니다!");

		if (shopItemContainer == null)
			Debug.LogError("[ShopPanel] ❌ shopItemContainer가 설정되지 않았습니다!");
	}

	private IEnumerator DelayedRefreshShop()
	{
		yield return new WaitUntil(() => ShopManager.Instance != null); // ShopManager가 준비될 때까지 대기
		RefreshShop();
	}

	private void Start()
	{

		// 골드 변경 이벤트 구독
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(GameManager.Instance.Gold);
		}
		else
		{
			Debug.LogError("[ShopPanel] ❌ GameManager.Instance가 null입니다!");
		}

		// 패널이 활성화되어 있으면 즉시 상점 로드
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(DelayedRefreshShop());
		}
	}

	private void OnDestroy()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
		}

		// 이벤트 해제
		foreach (var slot in shopSlots)
		{
			if (slot != null)
			{
				slot.OnBuyClicked -= OnItemBuyClicked;
			}
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		RefreshShop();
	}

	public void RefreshShop()
	{

		// 기존 슬롯 제거
		foreach (var slot in shopSlots)
		{
			if (slot != null)
			{
				slot.OnBuyClicked -= OnItemBuyClicked;
				Destroy(slot.gameObject);
			}
		}
		shopSlots.Clear(); // 리스트 비우기 

		if (ShopManager.Instance == null) // ShopManager 확인
		{
			Debug.LogError("[ShopPanel] ❌ ShopManager.Instance가 null입니다!");
			return;
		}

		if (shopItemSlotPrefab == null)
		{
			Debug.LogError("[ShopPanel] ❌ shopItemSlotPrefab이 null입니다!");
			return;
		}

		if (shopItemContainer == null)
		{
			Debug.LogError("[ShopPanel] ❌ shopItemContainer가 null입니다!");
			return;
		}

		// 상점 아이템 가져오기
		List<ItemDataSO> items = ShopManager.Instance.GetShopItems(); // 아이템 목록 가져오기

		if (items.Count == 0)
		{
			Debug.LogWarning("[ShopPanel] ShopManager에 아이템이 없습니다!");
			return;
		}

		foreach (var item in items)
		{
			if (item == null)
			{
				Debug.LogWarning("[ShopPanel] null 아이템 발견, 스킵합니다.");
				continue;
			}

			GameObject slotObj = Instantiate(shopItemSlotPrefab, shopItemContainer);

			ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

			if (slot != null)
			{
				slot.Initialize(item);

				slot.OnBuyClicked += OnItemBuyClicked;

				shopSlots.Add(slot);
			}
			else
			{
				Debug.LogError("[ShopPanel] ❌ ShopItemSlot 컴포넌트를 찾을 수 없습니다!");
			}
		}

	}

	private void OnItemBuyClicked(ItemDataSO item)
	{

		if (item == null)
		{
			Debug.LogError("[ShopPanel] ❌ 아이템이 null입니다!");
			return;
		}

		if (ShopManager.Instance == null)
		{
			Debug.LogError("[ShopPanel] ❌ ShopManager.Instance가 null입니다!");
			return;
		}

		bool success = ShopManager.Instance.BuyItem(item);

		if (success)
		{
		}
		else
		{
			Debug.LogWarning($"[ShopPanel] ❌ {item.itemName} 구매 실패! (골드 부족?)");
		}
	}

	private void UpdateGoldDisplay(int gold)
	{
		if (goldText != null)
		{
			goldText.text = $"{gold}";
		}
	}
}
