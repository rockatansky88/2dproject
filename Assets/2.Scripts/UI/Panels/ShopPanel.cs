using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopPanel : BasePanel
{
    [Header("References")]
    [SerializeField] private GameObject shopItemSlotPrefab;
    [SerializeField] private Transform shopItemContainer; // ScrollView�� Content
    [SerializeField] private Text goldText;

    private List<ShopItemSlot> shopSlots = new List<ShopItemSlot>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        // ��� ���� �̺�Ʈ ����
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

        // �̺�Ʈ ����
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
    /// ���� ���ΰ�ħ
    /// </summary>
    public void RefreshShop()
    {
        // ���� ���� ����
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

        // ���� ������ ��������
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
    /// ������ ���� Ŭ��
    /// </summary>
    private void OnItemBuyClicked(ItemDataSO item)
    {
        if (ShopManager.Instance != null)
        {
            bool success = ShopManager.Instance.BuyItem(item);

            if (success)
            {
                Debug.Log($"{item.itemName} ���� �Ϸ�!");
            }
            else
            {
                Debug.Log("��尡 �����մϴ�!");
            }
        }
    }

    /// <summary>
    /// ��� ǥ�� ������Ʈ
    /// </summary>
    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
        {
            goldText.text = $"{gold}";
        }
    }
}
