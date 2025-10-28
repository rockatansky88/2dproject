using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 고용된 용병 파티 UI (항상 표시됨)
/// 게임 시작 시 자동으로 초기화되며, MercenaryManager의 변경사항을 감지합니다.
/// </summary>
public class MercenaryParty : MonoBehaviour
{
    [Header("Party Slots")]
    [SerializeField] private Transform partySlotContainer; // 파티 슬롯들의 부모
    [SerializeField] private GameObject mercenaryPartySlotPrefab; // 파티 용병 슬롯 프리팹
    [SerializeField] private int maxPartySlots = 4;

    [Header("Detail Popup")]
    [SerializeField] private MercenaryDetailPopup detailPopup; // Canvas에서 참조

    // 슬롯 목록
    private List<MercenaryPartySlot> partySlots = new List<MercenaryPartySlot>();

    private void Awake()
    {
        Debug.Log("[MercenaryParty] Awake 시작");

        // 파티 슬롯 초기화 (4개 고정)
        InitializePartySlots();
    }

    private void Start()
    {
        Debug.Log("[MercenaryParty] Start 시작");

        // MercenaryManager 이벤트 구독
        if (MercenaryManager.Instance != null)
        {
            MercenaryManager.Instance.OnPartyChanged += RefreshPartySlots;
            Debug.Log("[MercenaryParty] MercenaryManager 이벤트 구독 완료");

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
            Debug.Log("[MercenaryParty] ? 지연된 초기화 성공");
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
        Debug.Log("[MercenaryParty] ━━━ 파티 슬롯 초기화 시작 ━━━");

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

                Debug.Log($"[MercenaryParty] 파티 슬롯 {i + 1} 생성 완료");
            }
            else
            {
                Debug.LogError($"[MercenaryParty] ? 슬롯 {i + 1}에 MercenaryPartySlot 컴포넌트가 없습니다!");
            }
        }

        Debug.Log($"[MercenaryParty] ? 파티 슬롯 {partySlots.Count}개 초기화 완료");
    }

    /// <summary>
    /// 파티 슬롯 갱신 (용병 데이터 반영)
    /// </summary>
    public void RefreshPartySlots()
    {
        Debug.Log("[MercenaryParty] ━━━ 파티 슬롯 갱신 시작 ━━━");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[MercenaryParty] ? MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> recruited = MercenaryManager.Instance.RecruitedMercenaries;
        Debug.Log($"[MercenaryParty] 고용된 용병 수: {recruited.Count}");

        // 슬롯 갱신
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < recruited.Count)
            {
                partySlots[i].Initialize(recruited[i]);
                Debug.Log($"[MercenaryParty] 파티 슬롯 {i + 1} 갱신: {recruited[i].mercenaryName}");
            }
            else
            {
                partySlots[i].SetEmpty();
                Debug.Log($"[MercenaryParty] 파티 슬롯 {i + 1} 비움");
            }
        }

        Debug.Log("[MercenaryParty] ? 파티 슬롯 갱신 완료");
    }

    /// <summary>
    /// 파티 슬롯 클릭 시 상세 팝업 표시 (추방 모드)
    /// </summary>
    private void OnPartySlotClicked(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            Debug.Log("[MercenaryParty] 빈 파티 슬롯 클릭됨 - 무시");
            return;
        }

        Debug.Log($"[MercenaryParty] ??? 파티 슬롯 클릭: {mercenary.mercenaryName}");

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
        Debug.Log($"[MercenaryParty] 전투 모드 설정: {isCombat}");

        foreach (var slot in partySlots)
        {
            if (slot != null)
            {
                slot.SetCombatMode(isCombat);
            }
        }
    }
}