using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// ���� ���� ������ ���� (������ + �̸� + ���� ������ + ���� + ���� ��ư)
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

        // ������ ������
        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.icon;
            itemIcon.enabled = itemData.icon != null;
        }

        // ������ �̸�
        if (itemNameText != null)
        {
            itemNameText.text = itemData.itemName;
        }

        // ����
        if (priceText != null)
        {
            priceText.text = itemData.buyPrice.ToString();
        }

        // ��ư �ؽ�Ʈ
        if (buyButtonText != null)
        {
            buyButtonText.text = "����";
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