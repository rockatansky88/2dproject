using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum SlotType
{
    Shop,    // 상점 (구매)
    Player   // 플레이어 (사용/판매)
}

/// <summary>
/// 아이템 슬롯 (인벤토리 및 상점)
/// - 좌클릭: 아이템 드래그 시작
/// - 우클릭: 아이템 사용
/// - Ctrl + 좌클릭: 상점에서 판매
/// </summary>
public class ItemSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text quantityText;
    [SerializeField] private Text priceText;
    [SerializeField] private Image backgroundImage;

    private ItemDataSO itemData;
    private SlotType slotType;
    private int quantity;
    private int maxStack = 5;

    // 이벤트
    public event Action<ItemDataSO> OnItemUsed;          // 아이템 사용 (우클릭)
    public event Action<ItemDataSO> OnItemSold;          // 아이템 판매 (Ctrl+좌클릭, 상점 모드만)
    public event Action<ItemDataSO> OnItemBought;        // 아이템 구매 (좌클릭, 상점 슬롯만)

    public ItemDataSO ItemData => itemData;
    public int Quantity => quantity;
    public SlotType SlotType => slotType;

    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(ItemDataSO item, SlotType type, int qty = 1)
    {
        itemData = item;
        slotType = type;
        quantity = qty;

        UpdateDisplay();

        Debug.Log($"[ItemSlot] 슬롯 초기화: {itemData?.itemName ?? "빈 슬롯"}, 타입: {slotType}, 수량: {quantity}");
    }

    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void SetEmpty()
    {
        itemData = null;
        quantity = 0;
        UpdateDisplay();
    }

    /// <summary>
    /// 수량 추가 (스택 가능한 경우)
    /// </summary>
    public bool TryAddQuantity(int amount)
    {
        if (itemData == null) return false;
        if (itemData.itemType != ItemType.Potion) return false;
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

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateDisplay()
    {
        bool isEmpty = itemData == null;

        // 아이콘
        if (iconImage != null)
        {
            if (isEmpty)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            else
            {
                iconImage.sprite = itemData.icon;
                iconImage.enabled = itemData.icon != null;

                if (iconImage.enabled)
                {
                    var iconColor = iconImage.color;
                    iconColor.a = 1f;
                    iconImage.color = iconColor;
                }
            }
        }

        // 이름
        if (nameText != null)
        {
            nameText.text = isEmpty ? "" : itemData.itemName;
        }

        // 수량 (포션만)
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

        // 가격
        if (priceText != null)
        {
            if (isEmpty)
            {
                priceText.text = "";
            }
            else
            {
                if (slotType == SlotType.Shop)
                {
                    priceText.text = $"{itemData.buyPrice}G";
                }
                else
                {
                    priceText.text = $"{itemData.sellPrice}G";
                }
            }
        }

        // 배경 투명도
        if (backgroundImage != null)
        {
            var color = backgroundImage.color;
            color.a = isEmpty ? 0.3f : 0.5f;
            backgroundImage.color = color;
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리
    /// - 좌클릭: 상점에서 구매 / 인벤토리에서 드래그 준비
    /// - 우클릭: 아이템 사용 (던전에서만)
    /// - Ctrl + 좌클릭: 판매 (상점 모드만)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemData == null)
        {
            Debug.Log("[ItemSlot] 빈 슬롯 클릭됨");
            return;
        }

        // 우클릭: 아이템 사용
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"[ItemSlot] 우클릭: {itemData.itemName} 사용 시도");
            OnRightClick();
            return;
        }

        // 좌클릭
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Ctrl + 좌클릭: 판매 (상점 모드 + 인벤토리 슬롯만)
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (IsShopMode() && slotType == SlotType.Player)
                {
                    Debug.Log($"[ItemSlot] Ctrl+좌클릭: {itemData.itemName} 판매");
                    OnItemSold?.Invoke(itemData);
                }
                else
                {
                    Debug.LogWarning("[ItemSlot] 상점이 아니므로 판매할 수 없습니다");
                }
                return;
            }

            // 일반 좌클릭: 상점 슬롯이면 구매
            if (slotType == SlotType.Shop)
            {
                Debug.Log($"[ItemSlot] 좌클릭: {itemData.itemName} 구매");
                OnItemBought?.Invoke(itemData);
            }
            // 드래그는 IBeginDragHandler에서 처리
        }
    }

    /// <summary>
    /// 우클릭: 아이템 사용
    /// </summary>
    private void OnRightClick()
    {
        if (itemData == null) return;

        // 포션이 아니면 사용 불가
        if (itemData.itemType != ItemType.Potion)
        {
            Debug.LogWarning($"[ItemSlot] {itemData.itemName}은(는) 사용할 수 없는 아이템입니다");
            return;
        }

        // 던전에서만 사용 가능
        if (!IsInDungeon())
        {
            Debug.LogWarning("[ItemSlot] 던전에서만 아이템을 사용할 수 있습니다");
            ShowMessage("던전에서만 사용 가능합니다");
            return;
        }

        Debug.Log($"[ItemSlot] 아이템 사용: {itemData.itemName}");
        OnItemUsed?.Invoke(itemData);
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemData == null) return;
        if (slotType == SlotType.Shop) return; // 상점 아이템은 드래그 불가

        Debug.Log($"[ItemSlot] 드래그 시작: {itemData.itemName}");

        if (ItemDragHandler.Instance != null)
        {
            ItemDragHandler.Instance.BeginDrag(this, itemData);
        }
    }

    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (ItemDragHandler.Instance != null && ItemDragHandler.Instance.IsDragging)
        {
            Debug.Log("[ItemSlot] 드래그 종료");
            ItemDragHandler.Instance.CancelDrag();
        }
    }

    /// <summary>
    /// 다른 슬롯에 드롭
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (ItemDragHandler.Instance != null && ItemDragHandler.Instance.IsDragging)
        {
            Debug.Log($"[ItemSlot] 드롭: {itemData?.itemName ?? "빈 슬롯"}");
            ItemDragHandler.Instance.EndDrag(this);
        }
    }

    /// <summary>
    /// 던전 내부인지 확인
    /// </summary>
    private bool IsInDungeon()
    {
        return DungeonManager.Instance != null && DungeonManager.Instance.IsInDungeon;
    }

    /// <summary>
    /// 상점 모드인지 확인 (InventoryWindow의 Shop 패널이 열려있는지)
    /// </summary>
    private bool IsShopMode()
    {
        InventoryWindow inventoryWindow = FindObjectOfType<InventoryWindow>();
        if (inventoryWindow == null) return false;

        // ShopPanel이 활성화되어 있으면 상점 모드
        return inventoryWindow.IsShopModeActive;
    }

    /// <summary>
    /// 메시지 표시 (간단한 로그 버전, 추후 UI 토스트로 확장 가능)
    /// </summary>
    private void ShowMessage(string message)
    {
        Debug.Log($"[ItemSlot] 메시지: {message}");
        // TODO: UI 토스트 메시지 시스템 구현 시 추가
    }
}