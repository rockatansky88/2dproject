using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 드래그&드롭 핸들러
/// - 좌클릭: 아이템 픽업/이동
/// - 마우스를 따라다니는 드래그 이미지 표시
/// - 전역 싱글톤으로 관리되어 모든 ItemSlot에서 사용 가능
/// </summary>
public class ItemDragHandler : MonoBehaviour
{
	public static ItemDragHandler Instance { get; private set; }

	[Header("Drag Visual")]
	[Tooltip("드래그 이미지를 표시할 Canvas (메인 Canvas 할당)")]
	[SerializeField] private Canvas dragCanvas;

	[Tooltip("드래그 중 표시될 Image 오브젝트 (DragImageContainer)")]
	[SerializeField] private GameObject dragImageObject;

	[Tooltip("드래그 이미지 컴포넌트")]
	[SerializeField] private Image dragImage;

	private RectTransform dragRectTransform;    // RectTransform 캐시
	private ItemSlot draggedSlot;               // 드래그 중인 슬롯
	private ItemDataSO draggedItem;             // 드래그 중인 아이템
	private bool isDragging = false;

	private void Awake()
	{

		// 싱글톤 설정
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("[ItemDragHandler] ⚠️ 중복 인스턴스 파괴됨");
			Destroy(gameObject);
			return;
		}

		// RectTransform 캐시
		if (dragImageObject != null)
		{
			dragRectTransform = dragImageObject.GetComponent<RectTransform>();

			if (dragRectTransform == null)
			{
				Debug.LogError("[ItemDragHandler] ❌ DragImageObject에 RectTransform이 없습니다!");
			}
			else
			{
			}
		}
		else
		{
			Debug.LogError("[ItemDragHandler] ❌ dragImageObject가 null입니다! Inspector에서 할당해주세요!");
		}

		// 드래그 이미지 초기 비활성화
		if (dragImageObject != null)
		{
			dragImageObject.SetActive(false);
		}

		// Canvas 확인
		if (dragCanvas == null)
		{
			Debug.LogError("[ItemDragHandler] ❌ dragCanvas가 null입니다! Inspector에서 할당해주세요!");
		}
		else
		{
		}

	}

	private void Update()
	{
		// 드래그 중일 때 마우스 위치 추적
		if (isDragging && dragRectTransform != null && dragCanvas != null)
		{
			// 스크린 좌표를 Canvas 좌표로 변환
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				dragCanvas.transform as RectTransform,
				Input.mousePosition,
				dragCanvas.worldCamera,
				out localPoint
			);

			dragRectTransform.localPosition = localPoint;
		}
	}

	/// <summary>
	/// 드래그 시작
	/// </summary>
	public void BeginDrag(ItemSlot slot, ItemDataSO item)
	{

		if (item == null)
		{
			Debug.LogWarning("[ItemDragHandler] ⚠️ 드래그할 아이템이 null입니다");
			return;
		}


		draggedSlot = slot;
		draggedItem = item;
		isDragging = true;

		// 드래그 이미지 활성화
		if (dragImageObject != null && dragImage != null)
		{
			dragImage.sprite = item.icon;
			dragImageObject.SetActive(true);

			// 반투명 설정
			Color color = dragImage.color;
			color.a = 0.7f;
			dragImage.color = color;

		}
		else
		{
			Debug.LogError("[ItemDragHandler] ❌ dragImageObject 또는 dragImage가 null입니다!");
		}
	}

	/// <summary>
	/// 드래그 종료 (슬롯에 드롭)
	/// </summary>
	public void EndDrag(ItemSlot targetSlot)
	{

		if (!isDragging)
		{
			Debug.LogWarning("[ItemDragHandler] ⚠️ 드래그 중이 아닙니다");
			return;
		}


		// 드래그 이미지 비활성화
		if (dragImageObject != null)
		{
			dragImageObject.SetActive(false);
		}

		// 같은 슬롯에 드롭한 경우 무시
		if (targetSlot == draggedSlot)
		{
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

		if (dragImageObject != null)
		{
			dragImageObject.SetActive(false);
		}

		ResetDrag();
	}

	/// <summary>
	/// 아이템 스왑
	/// </summary>
	private void SwapItems(ItemSlot slotA, ItemSlot slotB)
	{

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