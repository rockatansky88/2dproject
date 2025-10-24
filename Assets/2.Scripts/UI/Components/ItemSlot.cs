using UnityEngine;
using UnityEngine.UI;
using System;

public enum SlotType
{
    Shop,    // ���� (����)
    Player   // �÷��̾� (�Ǹ�)
}

public class ItemSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text quantityText;  // ���� ǥ�� �߰�
    [SerializeField] private Text priceText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Text buttonText;
    [SerializeField] private Image backgroundImage;

    private ItemDataSO itemData;
    private SlotType slotType;
    private int quantity;
    private int maxStack = 5; // �ִ� ���� ��

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
    /// ���� �ʱ�ȭ
    /// </summary>
    public void Initialize(ItemDataSO item, SlotType type, int qty = 1)
    {
        itemData = item;
        slotType = type;
        quantity = qty;

        UpdateDisplay();
    }

    /// <summary>
    /// �� �������� ���� - �� �޼��尡 �����Ǿ� �־����ϴ�!
    /// </summary>
    public void SetEmpty()
    {
        itemData = null;
        quantity = 0;
        UpdateDisplay();
    }

    /// <summary>
    /// ���� �߰� (���� ���� ���� üũ)
    /// </summary>
    public bool TryAddQuantity(int amount)
    {
        if (itemData == null) return false;
        if (itemData.itemType != ItemType.Potion) return false; // ���Ǹ� ���� ����
        if (quantity + amount > maxStack) return false;

        quantity += amount;
        UpdateDisplay();
        return true;
    }

    /// <summary>
    /// ���� ����
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

        // ������
        if (iconImage != null)
        {
            iconImage.sprite = isEmpty ? null : itemData.icon;
            iconImage.enabled = !isEmpty && itemData.icon != null;
        }

        // �̸�
        if (nameText != null)
        {
            nameText.text = isEmpty ? "" : itemData.itemName;
        }

        // ���� (���Ǹ� ǥ��)
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

        // ���� & ��ư
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
                    // ����
                    priceText.text = $"{itemData.buyPrice}";
                    buttonText.text = "����";
                }
                else
                {
                    // �Ǹ�
                    priceText.text = $"{itemData.sellPrice}";
                    buttonText.text = "�Ǹ�";
                }
                actionButton.interactable = true;
            }
        }

        // ��� ����
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