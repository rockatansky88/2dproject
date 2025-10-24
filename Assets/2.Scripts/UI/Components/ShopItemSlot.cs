using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 상점 전용 아이템 슬롯 (아이콘 + 이름 + 코인 아이콘 + 가격 + 구매 버튼)
/// </summary>
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
        }
    }

    public void Initialize(ItemDataSO item)
    {
        itemData = item;

        if (itemData == null) return;

        // 아이템 아이콘
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.icon;
            itemIcon.enabled = itemData.icon != null;
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
    }

    private void OnButtonClicked()
    {
        if (itemData != null)
        {
            OnBuyClicked?.Invoke(itemData);
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