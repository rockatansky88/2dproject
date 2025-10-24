using UnityEngine;
using UnityEngine.UI;
using System;

public enum SlotType
{
    Shop,    // 상점 (구매)
    Player   // 플레이어 (판매)
}

public class ItemSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text quantityText;  // 수량 표시 추가
    [SerializeField] private Text priceText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private Image backgroundImage;

    private ItemDataSO itemData;
    private SlotType slotType;
    private int quantity;
    private int maxStack = 5; // 최대 스택 수

    public event Action<ItemDataSO, SlotType> OnSlotClicked;

    public ItemDataSO ItemData => itemData;
    public int Quantity => quantity;

    private void Awake()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(ItemDataSO item, SlotType type, int qty = 1)
    {
        itemData = item;
        slotType = type;
        quantity = qty;

        UpdateDisplay();
    }

    /// <summary>
    /// 빈 슬롯으로 설정 - 이 메서드가 누락되어 있었습니다!
    /// </summary>
    public void SetEmpty()
    {
        itemData = null;
        quantity = 0;
        UpdateDisplay();
    }

    /// <summary>
    /// 수량 추가 (스택 가능 여부 체크)
    /// </summary>
    public bool TryAddQuantity(int amount)
    {
        if (itemData == null) return false;
        if (itemData.itemType != ItemType.Potion) return false; // 포션만 스택 가능
        if (quantity + amount > maxStack) return false;

        quantity += amount;
        UpdateDisplay();
        return true;
    }

    /// <summary>
    /// 수량 감소
    /// </summary>
    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
        if (quantity <= 0)
        {
            SetEmpty();
        }
        else
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        bool isEmpty = itemData == null;

        // 아이콘
        if (iconImage != null)
        {
            iconImage.sprite = isEmpty ? null : itemData.icon;
            iconImage.enabled = !isEmpty && itemData.icon != null;
        }

        // 이름
        if (nameText != null)
        {
            nameText.text = isEmpty ? "" : itemData.itemName;
        }

        // 수량 (포션만 표시)
        if (quantityText != null)
        {
            if (!isEmpty && itemData.itemType == ItemType.Potion && quantity > 1)
            {
                quantityText.text = $"x{quantity}";
                quantityText.enabled = true;
            }
            else
            {
                quantityText.enabled = false;
            }
        }

        // 가격 & 버튼
        if (priceText != null && buttonText != null && actionButton != null)
        {
            if (isEmpty)
            {
                priceText.text = "";
                buttonText.text = "";
                actionButton.interactable = false;
            }
            else
            {
                if (slotType == SlotType.Shop)
                {
                    // 구매
                    priceText.text = $"{itemData.buyPrice}";
                    buttonText.text = "구매";
                }
                else
                {
                    // 판매
                    priceText.text = $"{itemData.sellPrice}";
                    buttonText.text = "판매";
                }
                actionButton.interactable = true;
            }
        }

        // 배경 투명도
        if (backgroundImage != null)
        {
            var color = backgroundImage.color;
            color.a = isEmpty ? 0.3f : 1f;
            backgroundImage.color = color;
        }
    }

    private void OnButtonClicked()
    {
        if (itemData != null)
        {
            OnSlotClicked?.Invoke(itemData, slotType);
        }
    }

    private void OnDestroy()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}