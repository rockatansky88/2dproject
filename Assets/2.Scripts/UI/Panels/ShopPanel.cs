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

        // �ʼ� ���� üũ
        if (shopItemSlotPrefab == null)
            Debug.LogError("[ShopPanel] shopItemSlotPrefab�� �������� �ʾҽ��ϴ�!");

        if (shopItemContainer == null)
            Debug.LogError("[ShopPanel] shopItemContainer�� �������� �ʾҽ��ϴ�!");
    }

    private void Start()
    {
        // ��� ���� �̺�Ʈ ����
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            UpdateGoldDisplay(GameManager.Instance.Gold);
        }

        // �г��� Ȱ��ȭ�Ǿ� ������ ��� ���� �ε�
        if (gameObject.activeInHierarchy)
        {
            Debug.Log("[ShopPanel] Start()���� RefreshShop() ȣ��");
            RefreshShop();
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
        Debug.Log("[ShopPanel] OnShow() ȣ���");
        RefreshShop();
    }

    /// <summary>
    /// ���� ���ΰ�ħ
    /// </summary>
    public void RefreshShop()
    {
        Debug.Log("[ShopPanel] RefreshShop() ����");

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

        if (ShopManager.Instance == null)
        {
            Debug.LogError("[ShopPanel] ShopManager.Instance�� null�Դϴ�!");
            return;
        }

        if (shopItemSlotPrefab == null)
        {
            Debug.LogError("[ShopPanel] shopItemSlotPrefab�� null�Դϴ�!");
            return;
        }

        if (shopItemContainer == null)
        {
            Debug.LogError("[ShopPanel] shopItemContainer�� null�Դϴ�!");
            return;
        }

        // ���� ������ ��������
        List<ItemDataSO> items = ShopManager.Instance.GetShopItems();
        Debug.Log($"[ShopPanel] ������ ������ ����: {items.Count}");

        if (items.Count == 0)
        {
            Debug.LogWarning("[ShopPanel] ShopManager�� �������� �����ϴ�! ShopManager�� availableItems�� Ȯ���ϼ���.");
            return;
        }

        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("[ShopPanel] null ������ �߰�, ��ŵ�մϴ�.");
                continue;
            }

            GameObject slotObj = Instantiate(shopItemSlotPrefab, shopItemContainer);
            Debug.Log($"[ShopPanel] ���� ������: {item.itemName}");

            ShopItemSlot slot = slotObj.GetComponent<ShopItemSlot>();

            if (slot != null)
            {
                slot.Initialize(item);
                slot.OnBuyClicked += OnItemBuyClicked;
                shopSlots.Add(slot);
            }
            else
            {
                Debug.LogError("[ShopPanel] ShopItemSlot ������Ʈ�� ã�� �� �����ϴ�!");
            }
        }

        Debug.Log($"[ShopPanel] �� {shopSlots.Count}�� ���� ���� �Ϸ�");
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
