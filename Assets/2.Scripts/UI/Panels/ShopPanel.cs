using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopPanel : BasePanel
{
    [Header("References")]
    [SerializeField] private GameObject shopItemSlotPrefab;
    [SerializeField] private Transform shopItemContainer; // ScrollView의 Content
    [SerializeField] private Text goldText;

    private List<ShopItemSlot> shopSlots = new List<ShopItemSlot>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // 골드 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(GameManager.Instance.Gold);
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

    /// <summary>
    /// 상점 새로고침
    /// </summary>
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
        shopSlots.Clear();

        if (ShopManager.Instance == null) return;

        // 상점 아이템 가져오기
        List<ItemDataSO> items = ShopManager.Instance.GetShopItems();

        foreach (var item in items)
        {
            GameObject slotObj = Instantiate(shopItemSlotPrefab, shopItemContainer);
            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                slot.Initialize(item);
                slot.OnBuyClicked += OnItemBuyClicked;
                shopSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// 아이템 구매 클릭
    /// </summary>
    private void OnItemBuyClicked(ItemDataSO item)
    {
        if (ShopManager.Instance != null)
        {
            bool success = ShopManager.Instance.BuyItem(item);

            if (success)
            {
                Debug.Log($"{item.itemName} 구매 완료!");
            }
            else
            {
                Debug.Log("골드가 부족합니다!");
            }
        }
    }

    /// <summary>
    /// 골드 표시 업데이트
    /// </summary>
    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"{gold}";
        }
    }
}
