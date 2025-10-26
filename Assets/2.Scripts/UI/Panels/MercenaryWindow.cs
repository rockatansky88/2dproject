using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 용병 상점 윈도우 (전체 UI 컨테이너)
/// 좌측: 상점 용병 목록 (8명)
/// 우측: 고용된 용병 슬롯 (4명)
/// </summary>
public class MercenaryWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject windowRoot; // 전체 윈도우 루트
    [SerializeField] private Button closeButton;

    [Header("Shop Panel - 좌측 상점 목록")]
    [SerializeField] private Transform shopMercenaryContainer; // 용병 슬롯들의 부모
    [SerializeField] private GameObject mercenaryShopSlotPrefab; // 상점 용병 슬롯 프리팹

    [Header("Party Panel - 우측 고용된 용병")]
    [SerializeField] private Transform partyMercenaryContainer; // 고용된 용병 슬롯들의 부모
    [SerializeField] private GameObject mercenaryPartySlotPrefab; // 파티 용병 슬롯 프리팹
    [SerializeField] private int maxPartySlots = 4;

    [Header("Detail Popup")]
    [SerializeField] private MercenaryDetailPopup detailPopup;

    [Header("Gold Display")]
    [SerializeField] private Text goldText;

    // 슬롯 목록
    private List<MercenaryShopSlot> shopSlots = new List<MercenaryShopSlot>();
    private List<MercenaryPartySlot> partySlots = new List<MercenaryPartySlot>();

    // 프로퍼티
    public bool IsOpen => windowRoot != null && windowRoot.activeSelf;

    private void Awake()
    {
        // Close 버튼 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            Debug.Log("[MercenaryWindow] Close 버튼 리스너 등록됨");
        }

        // 초기 상태: 비활성화
        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }

        Debug.Log("[MercenaryWindow] Awake 완료");
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
            MercenaryManager.Instance.OnPartyChanged += RefreshPartyPanel;
        }

        // 파티 슬롯 초기화 (4개 고정 슬롯)
        InitializePartySlots();

        Debug.Log("[MercenaryWindow] Start 완료");
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
            MercenaryManager.Instance.OnPartyChanged -= RefreshPartyPanel;
        }

        // 슬롯 이벤트 해제
        foreach (var slot in shopSlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnShopSlotClicked;
            }
        }

        foreach (var slot in partySlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnPartySlotClicked;
            }
        }
    }

    /// <summary>
    /// 윈도우 열기
    /// </summary>
    public void Open()
    {
        Debug.Log("[MercenaryWindow] ━━━ 윈도우 열기 시작 ━━━");

        if (windowRoot != null)
        {
            windowRoot.SetActive(true);
        }

        // 상점 및 파티 패널 갱신
        RefreshShopPanel();
        RefreshPartyPanel();

        // 골드 표시 갱신
        if (GameManager.Instance != null)
        {
            UpdateGoldDisplay(GameManager.Instance.Gold);
        }

        Debug.Log("[MercenaryWindow] ✅ 윈도우 열기 완료");
    }

    /// <summary>
    /// 윈도우 닫기
    /// </summary>
    public void Close()
    {
        Debug.Log("[MercenaryWindow] 윈도우 닫기");

        if (windowRoot != null)
        {
            windowRoot.SetActive(false);
        }

        // 상세 팝업도 닫기
        if (detailPopup != null)
        {
            detailPopup.Close();
        }
    }

    /// <summary>
    /// 상점 패널 갱신 (좌측 8개 슬롯)
    /// </summary>
    private void RefreshShopPanel()
    {
        Debug.Log("[MercenaryWindow] ━━━ 상점 패널 갱신 시작 ━━━");

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
        Debug.Log($"[MercenaryWindow] 상점 용병 수: {mercenaries.Count}");

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

                Debug.Log($"[MercenaryWindow] 상점 슬롯 생성: {mercenary.mercenaryName}");
            }
            else
            {
                Debug.LogError("[MercenaryWindow] ❌ MercenaryShopSlot 컴포넌트를 찾을 수 없습니다!");
            }
        }

        Debug.Log($"[MercenaryWindow] ✅ 상점 슬롯 {shopSlots.Count}개 생성 완료");
    }

    /// <summary>
    /// 파티 슬롯 초기화 (4개 고정)
    /// </summary>
    private void InitializePartySlots()
    {
        Debug.Log("[MercenaryWindow] ━━━ 파티 슬롯 초기화 시작 ━━━");

        for (int i = 0; i < maxPartySlots; i++)
        {
            GameObject slotObj = Instantiate(mercenaryPartySlotPrefab, partyMercenaryContainer);
            MercenaryPartySlot slot = slotObj.GetComponent<MercenaryPartySlot>();

            if (slot != null)
            {
                slot.SetEmpty(); // 빈 슬롯으로 초기화
                slot.OnSlotClicked += OnPartySlotClicked;
                partySlots.Add(slot);

                Debug.Log($"[MercenaryWindow] 파티 슬롯 {i + 1} 생성");
            }
        }

        Debug.Log($"[MercenaryWindow] ✅ 파티 슬롯 {partySlots.Count}개 초기화 완료");
    }

    /// <summary>
    /// 파티 패널 갱신 (우측 4개 슬롯)
    /// </summary>
    private void RefreshPartyPanel()
    {
        Debug.Log("[MercenaryWindow] ━━━ 파티 패널 갱신 시작 ━━━");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[MercenaryWindow] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> recruited = MercenaryManager.Instance.RecruitedMercenaries;
        Debug.Log($"[MercenaryWindow] 고용된 용병 수: {recruited.Count}");

        // 슬롯 갱신
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < recruited.Count)
            {
                partySlots[i].Initialize(recruited[i]);
                Debug.Log($"[MercenaryWindow] 파티 슬롯 {i + 1} 갱신: {recruited[i].mercenaryName}");
            }
            else
            {
                partySlots[i].SetEmpty();
                Debug.Log($"[MercenaryWindow] 파티 슬롯 {i + 1} 비움");
            }
        }

        Debug.Log("[MercenaryWindow] ✅ 파티 패널 갱신 완료");
    }

    /// <summary>
    /// 상점 슬롯 클릭 시 상세 팝업 표시
    /// </summary>
    private void OnShopSlotClicked(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryWindow] 🖱️ 상점 슬롯 클릭: {mercenary.mercenaryName}");

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
    /// 파티 슬롯 클릭 시 상세 팝업 표시 (추방 모드)
    /// </summary>
    private void OnPartySlotClicked(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            Debug.Log("[MercenaryWindow] 빈 파티 슬롯 클릭됨");
            return;
        }

        Debug.Log($"[MercenaryWindow] 🖱️ 파티 슬롯 클릭: {mercenary.mercenaryName}");

        if (detailPopup != null)
        {
            detailPopup.ShowDismissMode(mercenary);
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