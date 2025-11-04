using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// - 좌클릭: 아이템 픽업/이동
/// - 우클릭: 아이템 사용
/// - Ctrl + 좌클릭: 상점에서 판매
/// </summary>
public class ItemDragHandler : MonoBehaviour
{
    public static ItemDragHandler Instance { get; private set; }

    [Header("Drag Visual")]
    [SerializeField] private Canvas dragCanvas; // 드래그 이미지를 표시할 캔버스
    [SerializeField] private Image dragImage;   // 드래그 중인 아이템 이미지

    private ItemSlot draggedSlot;               // 드래그 중인 슬롯
    private ItemDataSO draggedItem;             // 드래그 중인 아이템
    private bool isDragging = false;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[ItemDragHandler] 싱글톤 인스턴스 생성됨");
        }
        else
        {
            Debug.LogWarning("[ItemDragHandler] 중복 인스턴스 파괴됨");
            Destroy(gameObject);
            return;
        }

        // 드래그 이미지 초기 비활성화
        if (dragImage != null)
        {
            dragImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 드래그 중일 때 마우스 위치 추적
        if (isDragging && dragImage != null)
        {
            dragImage.transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    public void BeginDrag(ItemSlot slot, ItemDataSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemDragHandler] 드래그할 아이템이 null입니다");
            return;
        }

        Debug.Log($"[ItemDragHandler] 드래그 시작: {item.itemName}");

        draggedSlot = slot;
        draggedItem = item;
        isDragging = true;

        // 드래그 이미지 활성화
        if (dragImage != null)
        {
            dragImage.sprite = item.icon;
            dragImage.gameObject.SetActive(true);
            dragImage.transform.position = Input.mousePosition;

            // 반투명 설정
            Color color = dragImage.color;
            color.a = 0.7f;
            dragImage.color = color;
        }
    }

    /// <summary>
    /// 드래그 종료 (슬롯에 드롭)
    /// </summary>
    public void EndDrag(ItemSlot targetSlot)
    {
        if (!isDragging)
        {
            Debug.LogWarning("[ItemDragHandler] 드래그 중이 아닙니다");
            return;
        }

        Debug.Log($"[ItemDragHandler] 드래그 종료: {draggedItem?.itemName ?? "null"}");

        // 드래그 이미지 비활성화
        if (dragImage != null)
        {
            dragImage.gameObject.SetActive(false);
        }

        // 같은 슬롯에 드롭한 경우 무시
        if (targetSlot == draggedSlot)
        {
            Debug.Log("[ItemDragHandler] 같은 슬롯에 드롭함, 취소");
            ResetDrag();
            return;
        }

        // 타겟 슬롯이 있으면 스왑
        if (targetSlot != null)
        {
            SwapItems(draggedSlot, targetSlot);
        }

        ResetDrag();
    }

    /// <summary>
    /// 드래그 취소
    /// </summary>
    public void CancelDrag()
    {
        Debug.Log("[ItemDragHandler] 드래그 취소");

        if (dragImage != null)
        {
            dragImage.gameObject.SetActive(false);
        }

        ResetDrag();
    }

    /// <summary>
    /// 아이템 스왑
    /// </summary>
    private void SwapItems(ItemSlot slotA, ItemSlot slotB)
    {
        Debug.Log($"[ItemDragHandler] 아이템 스왑: {slotA.ItemData?.itemName} <-> {slotB.ItemData?.itemName}");

        ItemDataSO tempItem = slotA.ItemData;
        int tempQuantity = slotA.Quantity;

        slotA.Initialize(slotB.ItemData, slotA.SlotType, slotB.Quantity);
        slotB.Initialize(tempItem, slotB.SlotType, tempQuantity);
    }

    /// <summary>
    /// 드래그 상태 초기화
    /// </summary>
    private void ResetDrag()
    {
        draggedSlot = null;
        draggedItem = null;
        isDragging = false;
    }

    /// <summary>
    /// 현재 드래그 중인지 확인
    /// </summary>
    public bool IsDragging => isDragging;
}