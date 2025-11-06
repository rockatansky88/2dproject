using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 윈도우 메인 컨트롤러
/// HP/MP Fill Bar를 통해 용병의 현재 상태를 시각적으로 표시합니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InventoryWindow : MonoBehaviour
{
	[Header("Panels")]
	[SerializeField] private GameObject shopPanel;
	[SerializeField] private GameObject statsPanel;
	[SerializeField] private GameObject inventoryPanel;

	[Header("Stats Panel UI")]
	[SerializeField] private Image statsFullBodyImage;     // 전신 이미지
	[SerializeField] private Text statsNameText;           // 이름
	[SerializeField] private Text statsLevelText;          // 레벨
	[SerializeField] private Text statsHealthText;         // HP
	[SerializeField] private Text statsMpText;             // MP
	[SerializeField] private Text statsStrengthText;       // STR
	[SerializeField] private Text statsDexterityText;      // DEX
	[SerializeField] private Text statsWisdomText;         // WIS
	[SerializeField] private Text statsIntelligenceText;   // INT
	[SerializeField] private Text statsSpeedText;          // SPD

	[Header("HP/MP Fill Bars")]
	[Tooltip("HP를 시각적으로 표시하는 Fill Image (Image Type: Filled, Fill Method: Horizontal)")]
	[SerializeField] private Image statsHpFillImage;       // HP Fill Bar

	[Tooltip("MP를 시각적으로 표시하는 Fill Image (Image Type: Filled, Fill Method: Horizontal)")]
	[SerializeField] private Image statsMpFillImage;       // MP Fill Bar

	[Header("Mercenary List Panel")]
	[SerializeField] private Transform mercenaryListContainer; // 용병 슬롯 생성 부모
	[SerializeField] private GameObject mercenaryInventorySlotPrefab; // 용병 슬롯 프리팹 (미작업)

	[Header("References")]
	[SerializeField] private MercenaryParty mercenaryParty; // 하단 용병 파티 슬롯

	[Header("Background Blocker")]
	[SerializeField] private Image backgroundBlocker;

	private bool isOpen = false;
	private CanvasGroup canvasGroup;

	//  현재 선택된 용병 (아이템 사용 대상)
	private MercenaryInstance selectedMercenary;

	// 상점 모드 확인 프로퍼티
	/// <summary>
	/// 현재 상점 패널이 활성화되어 있는지 확인합니다.
	/// 아이템 판매 가능 여부를 체크할 때 사용됩니다.
	/// </summary>
	public bool IsShopModeActive => shopPanel != null && shopPanel.activeSelf;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();

		// 초기 상태: InventoryWindow 비활성화
		gameObject.SetActive(false);

		if (backgroundBlocker == null)
		{
			Debug.LogWarning("[InventoryWindow] backgroundBlocker가 설정되지 않았습니다!");
		}

	}

	/// <summary>
	/// 인벤토리 모드 (I 키 입력 시)
	/// </summary>
	public void OpenInventoryMode()
	{

		gameObject.SetActive(true);
		isOpen = true;

		// 레이캐스트 차단 활성화
		SetRaycastBlocking(true);

		// MercenaryParty 숨기기
		HideMercenaryParty();

		// 좌측: 스탯 패널
		if (shopPanel != null) shopPanel.SetActive(false);
		if (statsPanel != null) statsPanel.SetActive(true);

		// 우측: 인벤토리 패널
		if (inventoryPanel != null) inventoryPanel.SetActive(true);

		// 용병 리스트 갱신 및 첫 번째 용병 스탯 표시
		RefreshMercenaryList();
		ShowFirstMercenaryStats();

	}

	/// <summary>
	/// 상점 모드 (MerchantShop 클릭 시)
	/// </summary>
	public void OpenShopMode()
	{

		gameObject.SetActive(true);
		isOpen = true;

		// 레이캐스트 차단 활성화
		SetRaycastBlocking(true);

		// MercenaryParty 숨기기
		HideMercenaryParty();

		// 좌측: 상점 패널
		if (statsPanel != null) statsPanel.SetActive(false);
		if (shopPanel != null)
		{
			shopPanel.SetActive(true);

			// ShopPanel 새로고침
			ShopPanel shop = shopPanel.GetComponent<ShopPanel>();
			if (shop != null) shop.RefreshShop();
		}

		// 우측: 인벤토리 패널
		if (inventoryPanel != null) inventoryPanel.SetActive(true);

		// 용병 리스트 갱신
		RefreshMercenaryList();

	}

	/// <summary>
	/// 윈도우 닫기
	/// </summary>
	public void CloseWindow()
	{

		gameObject.SetActive(false);
		isOpen = false;

		// 레이캐스트 차단 비활성화
		SetRaycastBlocking(false);

		// MercenaryParty 다시 표시
		ShowMercenaryParty();

		if (shopPanel != null) shopPanel.SetActive(false);
		if (statsPanel != null) statsPanel.SetActive(false);
		if (inventoryPanel != null) inventoryPanel.SetActive(false);

	}

	/// <summary>
	/// MercenaryParty 숨기기
	/// </summary>
	private void HideMercenaryParty()
	{
		if (mercenaryParty != null)
		{
			mercenaryParty.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// MercenaryParty 표시
	/// </summary>
	private void ShowMercenaryParty()
	{
		if (mercenaryParty != null)
		{
			mercenaryParty.gameObject.SetActive(true);
			mercenaryParty.Show(); // 🆕 추가: CanvasGroup.alpha = 1로 설정
		}
	}

	/// <summary>
	/// 용병 리스트 갱신 (MercenaryListPanel에 슬롯 생성)
	/// </summary>
	private void RefreshMercenaryList()
	{

		if (MercenaryManager.Instance == null)
		{
			Debug.LogError("[InventoryWindow] ❌ MercenaryManager.Instance가 null입니다!");
			return;
		}

		var mercenaries = MercenaryManager.Instance.RecruitedMercenaries;

		// 슬롯 프리팹이 없으면 경고
		if (mercenaryInventorySlotPrefab == null)
		{
			Debug.LogWarning("[InventoryWindow] ⚠️ mercenaryInventorySlotPrefab이 null입니다! 슬롯을 생성할 수 없습니다.");
			return;
		}

		// 기존 슬롯 제거
		foreach (Transform child in mercenaryListContainer)
		{
			Destroy(child.gameObject);
		}

		// 용병 슬롯 생성 및 초기화
		foreach (var mercenary in mercenaries)
		{
			GameObject slotObj = Instantiate(mercenaryInventorySlotPrefab, mercenaryListContainer);
			MercenaryInventorySlot slotScript = slotObj.GetComponent<MercenaryInventorySlot>();

			if (slotScript != null)
			{
				// 슬롯 초기화
				slotScript.Initialize(mercenary);

				// 슬롯 클릭 이벤트 구독
				slotScript.OnSlotClicked += ShowMercenaryStats;

			}
			else
			{
				Debug.LogError("[InventoryWindow] ❌ MercenaryInventorySlot 컴포넌트를 찾을 수 없습니다!");
			}
		}

	}

	/// <summary>
	/// 첫 번째 용병의 스탯을 StatsPanel에 표시
	/// </summary>
	private void ShowFirstMercenaryStats()
	{
		if (MercenaryManager.Instance == null)
		{
			Debug.LogError("[InventoryWindow] ❌ MercenaryManager.Instance가 null입니다!");
			return;
		}

		var mercenaries = MercenaryManager.Instance.RecruitedMercenaries;

		if (mercenaries.Count == 0)
		{
			ClearStatsPanel();
			return;
		}

		// 첫 번째 용병 스탯 표시
		ShowMercenaryStats(mercenaries[0]);
	}

	/// <summary>
	/// 특정 용병의 스탯을 StatsPanel에 표시
	/// HP/MP Fill Bar를 통해 시각적으로 게이지를 표시합니다.
	/// </summary>
	public void ShowMercenaryStats(MercenaryInstance mercenary)
	{
		if (mercenary == null)
		{
			Debug.LogError("[InventoryWindow] ❌ mercenary가 null입니다!");
			return;
		}

		// 🆕 추가: 선택된 용병 저장 (아이템 사용 대상)
		selectedMercenary = mercenary;


		// 전신 이미지
		if (statsFullBodyImage != null)
		{
			statsFullBodyImage.sprite = mercenary.fullBodySprite;
			statsFullBodyImage.enabled = mercenary.fullBodySprite != null;
		}

		// 이름
		if (statsNameText != null)
		{
			statsNameText.text = mercenary.mercenaryName;
		}

		// 레벨
		if (statsLevelText != null)
		{
			statsLevelText.text = $"Level {mercenary.level}";
		}

		// HP 텍스트
		if (statsHealthText != null)
		{
			statsHealthText.text = $"HP: {mercenary.currentHP}/{mercenary.maxHP}";
		}

		// MP 텍스트
		if (statsMpText != null)
		{
			statsMpText.text = $"MP: {mercenary.currentMP}/{mercenary.maxMP}";
		}

		if (statsHpFillImage != null)
		{
			float hpFillAmount = mercenary.maxHP > 0 ? (float)mercenary.currentHP / mercenary.maxHP : 0f;
			statsHpFillImage.fillAmount = hpFillAmount;
		}
		else
		{
			Debug.LogWarning("[InventoryWindow] ⚠️ statsHpFillImage가 null입니다! Inspector에서 할당해주세요");
		}

		if (statsMpFillImage != null)
		{
			float mpFillAmount = mercenary.maxMP > 0 ? (float)mercenary.currentMP / mercenary.maxMP : 0f;
			statsMpFillImage.fillAmount = mpFillAmount;
		}
		else
		{
			Debug.LogWarning("[InventoryWindow] ⚠️ statsMpFillImage가 null입니다! Inspector에서 할당해주세요");
		}

		// 기타 스탯
		if (statsStrengthText != null)
		{
			statsStrengthText.text = $"STR: {mercenary.strength}";
		}

		if (statsDexterityText != null)
		{
			statsDexterityText.text = $"DEX: {mercenary.dexterity}";
		}

		if (statsWisdomText != null)
		{
			statsWisdomText.text = $"WIS: {mercenary.wisdom}";
		}

		if (statsIntelligenceText != null)
		{
			statsIntelligenceText.text = $"INT: {mercenary.intelligence}";
		}

		if (statsSpeedText != null)
		{
			statsSpeedText.text = $"SPD: {mercenary.speed}";
		}

	}

	/// <summary>
	/// StatsPanel 비우기
	/// </summary>
	private void ClearStatsPanel()
	{
		if (statsFullBodyImage != null)
		{
			statsFullBodyImage.sprite = null;
			statsFullBodyImage.enabled = false;
		}

		if (statsNameText != null) statsNameText.text = "";
		if (statsLevelText != null) statsLevelText.text = "";
		if (statsHealthText != null) statsHealthText.text = "";
		if (statsMpText != null) statsMpText.text = "";
		if (statsStrengthText != null) statsStrengthText.text = "";
		if (statsDexterityText != null) statsDexterityText.text = "";
		if (statsWisdomText != null) statsWisdomText.text = "";
		if (statsIntelligenceText != null) statsIntelligenceText.text = "";
		if (statsSpeedText != null) statsSpeedText.text = "";

		if (statsHpFillImage != null)
		{
			statsHpFillImage.fillAmount = 0f;
		}

		if (statsMpFillImage != null)
		{
			statsMpFillImage.fillAmount = 0f;
		}

	}

	private void SetRaycastBlocking(bool block)
	{
		if (canvasGroup != null)
		{
			canvasGroup.blocksRaycasts = block;
		}

		if (backgroundBlocker != null)
		{
			backgroundBlocker.raycastTarget = block;
		}
	}

	public bool IsOpen => isOpen;

	/// <summary>
	/// 현재 선택된 용병을 반환합니다 (아이템 사용 대상).
	/// 선택된 용병이 없으면 null을 반환합니다.
	/// </summary>
	public MercenaryInstance GetSelectedMercenary()
	{
		return selectedMercenary;
	}
}