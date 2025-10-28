using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopItemSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Image coinIcon;
    [SerializeField] private Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Text buyButtonText;

    private ItemDataSO itemData;

    public event Action<ItemDataSO> OnBuyClicked;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnButtonClicked);
            Debug.Log("[ShopItemSlot] 구매 버튼 리스너 등록됨");
        }
        else
        {
            Debug.LogError("[ShopItemSlot] ❌ buyButton이 null입니다!");
        }
    }

    public void Initialize(ItemDataSO item)
    {
        itemData = item;

        Debug.Log($"[ShopItemSlot] Initialize - 아이템: {item?.itemName ?? "null"}");

        if (itemData == null)
        {
            Debug.LogWarning("[ShopItemSlot] ❌ itemData가 null입니다!");
            return;
        }

        // 아이템 아이콘
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.icon;
            itemIcon.enabled = itemData.icon != null;
            Debug.Log($"[ShopItemSlot] 아이콘 설정: {itemData.icon?.name ?? "null"}");
        }

        // 아이템 이름
        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        // 가격
        if (priceText != null)
        {
            priceText.text = itemData.buyPrice.ToString();
        }

        // 버튼 텍스트
        if (buyButtonText != null)
        {
            buyButtonText.text = "구매";
        }

        Debug.Log($"[ShopItemSlot] ✅ 초기화 완료: {itemData.itemName}");
    }

    private void OnButtonClicked()
    {
        Debug.Log($"[ShopItemSlot] 🖱️ 구매 버튼 클릭됨! 아이템: {itemData?.itemName ?? "null"}");

        if (itemData != null)
        {
            Debug.Log($"[ShopItemSlot] OnBuyClicked 이벤트 발생: {itemData.itemName}");
            OnBuyClicked?.Invoke(itemData);
        }
        else
        {
            Debug.LogError("[ShopItemSlot] ❌ itemData가 null이라 이벤트를 발생시킬 수 없습니다!");
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}