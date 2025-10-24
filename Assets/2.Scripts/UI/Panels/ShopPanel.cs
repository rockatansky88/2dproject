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

        // 필수 참조 체크
        if (shopItemSlotPrefab == null)
            Debug.LogError("[ShopPanel] shopItemSlotPrefab이 설정되지 않았습니다!");

        if (shopItemContainer == null)
            Debug.LogError("[ShopPanel] shopItemContainer가 설정되지 않았습니다!");
    }

    private void Start()
    {
        // 골드 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(GameManager.Instance.Gold);
        }

        // 패널이 활성화되어 있으면 즉시 상점 로드
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("[ShopPanel] Start()에서 RefreshShop() 호출");
            RefreshShop();
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
        Debug.Log("[ShopPanel] OnShow() 호출됨");
        RefreshShop();
    }

    /// <summary>
    /// 상점 새로고침
    /// </summary>
    public void RefreshShop()
    {
        Debug.Log("[ShopPanel] RefreshShop() 시작");

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

        if (ShopManager.Instance == null)
        {
            Debug.LogError("[ShopPanel] ShopManager.Instance가 null입니다!");
            return;
        }

        if (shopItemSlotPrefab == null)
        {
            Debug.LogError("[ShopPanel] shopItemSlotPrefab이 null입니다!");
            return;
        }

        if (shopItemContainer == null)
        {
            Debug.LogError("[ShopPanel] shopItemContainer가 null입니다!");
            return;
        }

        // 상점 아이템 가져오기
        List<ItemDataSO> items = ShopManager.Instance.GetShopItems();
        Debug.Log($"[ShopPanel] 생성할 아이템 개수: {items.Count}");

        if (items.Count == 0)
        {
            Debug.LogWarning("[ShopPanel] ShopManager에 아이템이 없습니다! ShopManager의 availableItems를 확인하세요.");
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
            Debug.Log($"[ShopPanel] 슬롯 생성됨: {item.itemName}");

            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                slot.Initialize(item);
                slot.OnBuyClicked += OnItemBuyClicked;
                shopSlots.Add(slot);
            }
            else
            {
                Debug.LogError("[ShopPanel] ShopItemSlot 컴포넌트를 찾을 수 없습니다!");
            }
        }

        Debug.Log($"[ShopPanel] 총 {shopSlots.Count}개 슬롯 생성 완료");
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
