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
        Debug.Log("[ShopPanel] Start() 호출됨");

        // 골드 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(GameManager.Instance.Gold);
            Debug.Log("[ShopPanel] ✅ GameManager 이벤트 구독 완료");
        }
        else
        {
            Debug.LogError("[ShopPanel] ❌ GameManager.Instance가 null입니다!");
        }

        // 패널이 활성화되어 있으면 즉시 상점 로드
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("[ShopPanel] Start()에서 DelayedRefreshShop() 시작");
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
        Debug.Log("[ShopPanel] OnShow() 호출됨");
        RefreshShop();
    }

    public void RefreshShop()
    {
        Debug.Log("[ShopPanel] ━━━ RefreshShop() 시작 ━━━");

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
        Debug.Log($"[ShopPanel] 생성할 아이템 개수: {items.Count}");

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
            Debug.Log($"[ShopPanel] 슬롯 생성됨: {item.itemName}");

            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                Debug.Log($"[ShopPanel] 슬롯 Initialize 호출: {item.itemName}");
                slot.Initialize(item);

                Debug.Log($"[ShopPanel] 이벤트 구독 시작: {item.itemName}");
                slot.OnBuyClicked += OnItemBuyClicked;
                Debug.Log($"[ShopPanel] ✅ 이벤트 구독 완료: {item.itemName}");

                shopSlots.Add(slot);
            }
            else
            {
                Debug.LogError("[ShopPanel] ❌ ShopItemSlot 컴포넌트를 찾을 수 없습니다!");
            }
        }

        Debug.Log($"[ShopPanel] ✅✅✅ 총 {shopSlots.Count}개 슬롯 생성 완료");
    }

    private void OnItemBuyClicked(ItemDataSO item)
    {
        Debug.Log($"[ShopPanel] ━━━━━━ OnItemBuyClicked 호출됨 ━━━━━━");
        Debug.Log($"[ShopPanel] 구매 시도 아이템: {item?.itemName ?? "null"}");

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

        Debug.Log("[ShopPanel] ShopManager.BuyItem() 호출...");
        bool success = ShopManager.Instance.BuyItem(item);

        if (success)
        {
            Debug.Log($"[ShopPanel] ✅✅✅ {item.itemName} 구매 완료!");
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
