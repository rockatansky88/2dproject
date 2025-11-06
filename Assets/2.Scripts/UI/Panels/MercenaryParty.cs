using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 고용된 용병 파티 UI (상황에 따라 표시/숨김)
/// 게임 시작 시 자동으로 초기화되며, MercenaryManager의 변경사항을 감지합니다.
/// 던전 입구/통로: 숨김, 전투/이벤트: 표시
/// </summary>
public class MercenaryParty : MonoBehaviour
{
	[Header("Party Slots")]
	[SerializeField] private Transform partySlotContainer; // 파티 슬롯들의 부모
	[SerializeField] private GameObject mercenaryPartySlotPrefab; // 파티 용병 슬롯 프리팹
	[SerializeField] private int maxPartySlots = 4;

	[Header("Detail Popup")]
	[SerializeField] private MercenaryDetailPopup detailPopup; // Canvas에서 참조

	[Header("Canvas Settings")]
	[SerializeField] private Canvas canvas; // 이 UI의 Canvas
	[SerializeField] private int combatSortOrder = 100; // 전투/이벤트 시 Sort Order

	// 슬롯 목록
	private List<MercenaryPartySlot> partySlots = new List<MercenaryPartySlot>();
	private CanvasGroup canvasGroup;

	private void Awake()
	{

		// CanvasGroup 가져오기 또는 추가
		canvasGroup = GetComponent<CanvasGroup>();
		if (canvasGroup == null)
		{
			canvasGroup = gameObject.AddComponent<CanvasGroup>();
		}

		// Canvas 가져오기
		if (canvas == null)
		{
			canvas = GetComponent<Canvas>();
		}

		// 파티 슬롯 초기화 (4개 고정)
		InitializePartySlots();

		// 초기 상태: 숨김
		Hide();
	}

	private void Start()
	{

		// MercenaryManager 이벤트 구독
		if (MercenaryManager.Instance != null)
		{
			MercenaryManager.Instance.OnPartyChanged += RefreshPartySlots;

			// 초기 파티 갱신
			RefreshPartySlots();
		}
		else
		{
			Debug.LogWarning("[MercenaryParty] ?? MercenaryManager.Instance가 null입니다. 나중에 재시도합니다.");
			// MercenaryManager가 아직 없으면 나중에 재시도
			Invoke(nameof(DelayedStart), 0.1f);
		}
	}

	private void DelayedStart()
	{
		if (MercenaryManager.Instance != null)
		{
			MercenaryManager.Instance.OnPartyChanged += RefreshPartySlots;
			RefreshPartySlots();
		}
		else
		{
			Debug.LogError("[MercenaryParty] ? MercenaryManager가 여전히 null입니다!");
		}
	}

	private void OnDestroy()
	{
		// 이벤트 구독 해제
		if (MercenaryManager.Instance != null)
		{
			MercenaryManager.Instance.OnPartyChanged -= RefreshPartySlots;
		}

		// 슬롯 이벤트 해제
		foreach (var slot in partySlots)
		{
			if (slot != null)
			{
				slot.OnSlotClicked -= OnPartySlotClicked;
			}
		}
	}

	/// <summary>
	/// 파티 슬롯 초기화 (4개 고정)
	/// </summary>
	private void InitializePartySlots()
	{

		// 기존 슬롯 제거 (중복 방지)
		foreach (var slot in partySlots)
		{
			if (slot != null)
			{
				Destroy(slot.gameObject);
			}
		}
		partySlots.Clear();

		// 4개의 파티 슬롯 생성
		for (int i = 0; i < maxPartySlots; i++)
		{
			GameObject slotObj = Instantiate(mercenaryPartySlotPrefab, partySlotContainer);
			MercenaryPartySlot slot = slotObj.GetComponent<MercenaryPartySlot>();

			if (slot != null)
			{
				slot.SetEmpty(); // 빈 슬롯으로 초기화
				slot.OnSlotClicked += OnPartySlotClicked;
				partySlots.Add(slot);

			}
			else
			{
				Debug.LogError($"[MercenaryParty] ? 슬롯 {i + 1}에 MercenaryPartySlot 컴포넌트가 없습니다!");
			}
		}

	}

	/// <summary>
	/// 파티 슬롯 갱신 (용병 데이터 반영)
	/// </summary>
	public void RefreshPartySlots()
	{

		if (MercenaryManager.Instance == null)
		{
			Debug.LogError("[MercenaryParty] ? MercenaryManager.Instance가 null입니다!");
			return;
		}

		List<MercenaryInstance> recruited = MercenaryManager.Instance.RecruitedMercenaries;

		// 슬롯 갱신
		for (int i = 0; i < partySlots.Count; i++)
		{
			if (i < recruited.Count)
			{
				partySlots[i].Initialize(recruited[i]);
			}
			else
			{
				partySlots[i].SetEmpty();
			}
		}

	}

	/// <summary>
	/// 파티 슬롯 클릭 시 상세 팝업 표시 (추방 모드)
	/// </summary>
	private void OnPartySlotClicked(MercenaryInstance mercenary)
	{
		if (mercenary == null)
		{
			return;
		}


		// DetailPopup이 없으면 찾기
		if (detailPopup == null)
		{
			detailPopup = FindObjectOfType<MercenaryDetailPopup>();

			if (detailPopup == null)
			{
				Debug.LogError("[MercenaryParty] ? MercenaryDetailPopup을 찾을 수 없습니다!");
				return;
			}
		}

		detailPopup.ShowDismissMode(mercenary);
	}

	/// <summary>
	/// 전투씬 모드 설정 (전투씬에서만 HP/MP 표시)
	/// </summary>
	public void SetCombatMode(bool isCombat)
	{

		foreach (var slot in partySlots)
		{
			if (slot != null)
			{
				slot.SetCombatMode(isCombat);
			}
		}

		// 전투 모드일 때 자동으로 표시
		if (isCombat)
		{
			Show();
		}
	}

	/// <summary>
	/// 전투 상태 완전 초기화 (마을 귀환 시)
	/// 빨간 테두리 제거 및 일반 모드로 전환
	/// </summary>
	public void ResetCombatState()
	{

		// 모든 슬롯을 일반 모드로 전환
		foreach (var slot in partySlots)
		{
			if (slot != null)
			{
				slot.SetCombatMode(false); // 전투 모드 해제
				slot.ResetHighlight();     // 빨간 테두리 제거
			}
		}

		// UI 갱신
		RefreshPartySlots();

	}

	/// <summary>
	/// 파티 UI 표시
	/// </summary>
	public void Show()
	{

		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}

		if (canvas != null)
		{
			canvas.sortingOrder = combatSortOrder;
		}

		gameObject.SetActive(true);
	}

	/// <summary>
	/// 파티 UI 숨김
	/// </summary>
	public void Hide()
	{

		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
	}
}