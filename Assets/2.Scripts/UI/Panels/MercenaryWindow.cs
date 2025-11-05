using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 용병 상점 윈도우 (전체 UI 컨테이너)
/// 좌측: 상점 용병 목록 (8명)
/// 우측: 파티 슬롯은 MercenaryParty 컴포넌트가 별도로 관리
/// </summary>
public class MercenaryWindow : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private GameObject windowRoot; // 전체 윈도우 루트
	[SerializeField] private Button closeButton;

	[Header("Shop Panel - 좌측 상점 목록")]
	[SerializeField] private Transform shopMercenaryContainer; // 용병 슬롯들의 부모
	[SerializeField] private GameObject mercenaryShopSlotPrefab; // 상점 용병 슬롯 프리팹

	[Header("Detail Popup")]
	[SerializeField] private MercenaryDetailPopup detailPopup;

	[Header("Gold Display")]
	[SerializeField] private Text goldText;

	[Header("Background Blocker")]
	[SerializeField] private Image backgroundBlocker; // 배경 클릭 차단용 이미지

	// 슬롯 목록
	private List<MercenaryShopSlot> shopSlots = new List<MercenaryShopSlot>();
	private CanvasGroup canvasGroup;

	// 프로퍼티
	public bool IsOpen => windowRoot != null && windowRoot.activeSelf;

	private void Awake()
	{
		// Close 버튼 연결
		if (closeButton != null)
		{
			closeButton.onClick.AddListener(Close);
		}

		// CanvasGroup 가져오기 또는 추가
		if (windowRoot != null)
		{
			canvasGroup = windowRoot.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = windowRoot.AddComponent<CanvasGroup>();
			}
		}

		// 초기 상태: 비활성화
		if (windowRoot != null)
		{
			windowRoot.SetActive(false);
		}

		if (backgroundBlocker == null)
		{
			Debug.LogWarning("[MercenaryWindow] backgroundBlocker가 설정되지 않았습니다!");
		}

	}

	private void Start()
	{
		// 골드 변경 이벤트 구독
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
			UpdateGoldDisplay(GameManager.Instance.Gold);
		}

		// MercenaryManager 이벤트 구독
		if (MercenaryManager.Instance != null)
		{
			MercenaryManager.Instance.OnShopRefreshed += RefreshShopPanel;
		}

	}

	private void OnDestroy()
	{
		// 이벤트 구독 해제
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
		}

		if (MercenaryManager.Instance != null)
		{
			MercenaryManager.Instance.OnShopRefreshed -= RefreshShopPanel;
		}

		// 슬롯 이벤트 해제
		foreach (var slot in shopSlots)
		{
			if (slot != null)
			{
				slot.OnSlotClicked -= OnShopSlotClicked;
			}
		}
	}

	/// <summary>
	/// 윈도우 열기
	/// </summary>
	public void Open()
	{

		if (windowRoot != null)
		{
			windowRoot.SetActive(true);
		}

		// 레이캐스트 차단 활성화
		SetRaycastBlocking(true);

		// 상점 패널 갱신
		RefreshShopPanel();

		//// 골드 표시 갱신
		//if (GameManager.Instance != null)
		//{
		//    UpdateGoldDisplay(GameManager.Instance.Gold);
		//}

		// 🆕 추가: MercenaryParty 다시 표시
		MercenaryParty mercenaryParty = FindObjectOfType<MercenaryParty>();
		if (mercenaryParty != null)
		{
			mercenaryParty.Show();
		}
		else
		{
			Debug.LogWarning("[MercenaryWindow] ⚠️ MercenaryParty를 찾을 수 없습니다!");
		}

	}

	/// <summary>
	/// 윈도우 닫기
	/// </summary>
	public void Close()
	{

		if (windowRoot != null)
		{
			windowRoot.SetActive(false);
		}

		// 레이캐스트 차단 비활성화
		SetRaycastBlocking(false);

		// 상세 팝업도 닫기
		if (detailPopup != null)
		{
			detailPopup.Close();
		}

		// 🆕 추가: MercenaryParty 다시 표시
		MercenaryParty mercenaryParty = FindObjectOfType<MercenaryParty>();
		if (mercenaryParty != null)
		{
			mercenaryParty.Show();
		}
		else
		{
			Debug.LogWarning("[MercenaryWindow] ⚠️ MercenaryParty를 찾을 수 없습니다!");
		}
	}

	/// <summary>
	/// 레이캐스트 차단 설정
	/// </summary>
	private void SetRaycastBlocking(bool block)
	{
		if (canvasGroup != null)
		{
			canvasGroup.blocksRaycasts = block;
			canvasGroup.interactable = block;
		}

		if (backgroundBlocker != null)
		{
			backgroundBlocker.raycastTarget = block;
		}
	}

	/// <summary>
	/// 상점 패널 갱신 (좌측 8개 슬롯)
	/// </summary>
	private void RefreshShopPanel()
	{

		// 기존 슬롯 제거
		foreach (var slot in shopSlots)
		{
			if (slot != null)
			{
				slot.OnSlotClicked -= OnShopSlotClicked;
				Destroy(slot.gameObject);
			}
		}
		shopSlots.Clear();

		if (MercenaryManager.Instance == null)
		{
			Debug.LogError("[MercenaryWindow] ❌ MercenaryManager.Instance가 null입니다!");
			return;
		}

		// 상점 용병 목록 가져오기
		List<MercenaryInstance> mercenaries = MercenaryManager.Instance.ShopMercenaries;

		// 슬롯 생성
		foreach (var mercenary in mercenaries)
		{
			GameObject slotObj = Instantiate(mercenaryShopSlotPrefab, shopMercenaryContainer);
			MercenaryShopSlot slot = slotObj.GetComponent<MercenaryShopSlot>();

			if (slot != null)
			{
				slot.Initialize(mercenary);
				slot.OnSlotClicked += OnShopSlotClicked;
				shopSlots.Add(slot);

			}
			else
			{
				Debug.LogError("[MercenaryWindow] ❌ MercenaryShopSlot 컴포넌트를 찾을 수 없습니다!");
			}
		}

	}

	/// <summary>
	/// 상점 슬롯 클릭 시 상세 팝업 표시
	/// </summary>
	private void OnShopSlotClicked(MercenaryInstance mercenary)
	{

		if (detailPopup != null)
		{
			detailPopup.ShowRecruitMode(mercenary);
		}
		else
		{
			Debug.LogError("[MercenaryWindow] ❌ detailPopup이 null입니다!");
		}
	}

	/// <summary>
	/// 골드 표시 갱신
	/// </summary>
	private void UpdateGoldDisplay(int gold)
	{
		if (goldText != null)
		{
			goldText.text = $"{gold}";
		}
	}
}