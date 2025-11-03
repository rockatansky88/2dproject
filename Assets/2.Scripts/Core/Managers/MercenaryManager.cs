using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// 용병 시스템의 핵심 관리자
/// - 용병 상점 목록 관리 (8명)
/// - 고용된 용병 관리 (최대 4명)
/// - 용병 고용/추방 처리
/// </summary>
public class MercenaryManager : MonoBehaviour
{
    public static MercenaryManager Instance { get; private set; }

    [Header("Mercenary Templates")]
    [SerializeField] private List<MercenaryDataSO> mercenaryTemplates = new List<MercenaryDataSO>();

    [Header("Shop Settings")]
    [SerializeField] private int shopMercenaryCount = 8; // 상점에 표시할 용병 수

    [Header("Party Settings")]
    [SerializeField] private int maxPartySize = 4; // 최대 고용 가능 인원
    [SerializeField] private MercenaryDataSO starterMercenary; // 게임 시작 시 기본 용병

    // 현재 상점에 표시되는 용병 목록 (랜덤 생성된 인스턴스)
    private List<MercenaryInstance> currentShopMercenaries = new List<MercenaryInstance>();

    // 고용된 용병 목록
    private List<MercenaryInstance> recruitedMercenaries = new List<MercenaryInstance>();

    // 이벤트
    public event Action OnShopRefreshed;
    public event Action OnPartyChanged;

    // 프로퍼티
    public List<MercenaryInstance> ShopMercenaries => currentShopMercenaries;
    public List<MercenaryInstance> RecruitedMercenaries => recruitedMercenaries;
    public int CurrentPartySize => recruitedMercenaries.Count;
    public int MaxPartySize => maxPartySize;
    public bool IsPartyFull => recruitedMercenaries.Count >= maxPartySize;

    /// <summary>
    /// 현재 전투 가능한 파티 (RecruitedMercenaries와 동일) 
    /// </summary>
    public List<MercenaryInstance> CurrentParty => recruitedMercenaries;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MercenaryManager] ? Instance 생성됨");
        }
        else
        {
            Debug.Log("[MercenaryManager] 중복 Instance 제거");
            Destroy(gameObject);
            return;
        }

        // 유효성 검사
        if (mercenaryTemplates == null || mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[MercenaryManager] ? mercenaryTemplates가 비어있습니다! Inspector에서 설정해주세요.");
        }
    }

    private void Start()
    {
        // 게임 시작 시 기본 용병 추가
        AddStarterMercenary();

        // 상점 초기화
        RefreshShop();
    }

    /// <summary>
    /// 게임 시작 시 기본 용병 추가
    /// </summary>
    private void AddStarterMercenary()
    {
        if (starterMercenary == null)
        {
            Debug.LogWarning("[MercenaryManager] ?? starterMercenary가 설정되지 않았습니다. 기본 용병을 추가하지 않습니다.");
            return;
        }

        MercenaryInstance starter = starterMercenary.CreateRandomInstance();
        starter.isRecruited = true;
        recruitedMercenaries.Add(starter);

        Debug.Log($"[MercenaryManager] ? 시작 용병 추가: {starter.mercenaryName} (Lv.{starter.level})");
        OnPartyChanged?.Invoke();
    }

    /// <summary>
    /// 상점 용병 목록 새로고침 (랜덤 생성)
    /// </summary>
    public void RefreshShop()
    {
        Debug.Log("[MercenaryManager] ━━━ 상점 용병 목록 새로고침 시작 ━━━");

        currentShopMercenaries.Clear();

        if (mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[MercenaryManager] ? mercenaryTemplates가 비어있습니다!");
            return;
        }

        // 8명의 랜덤 용병 생성
        for (int i = 0; i < shopMercenaryCount; i++)
        {
            // 랜덤 템플릿 선택
            MercenaryDataSO template = mercenaryTemplates[UnityEngine.Random.Range(0, mercenaryTemplates.Count)];

            // 랜덤 인스턴스 생성
            MercenaryInstance instance = template.CreateRandomInstance();
            currentShopMercenaries.Add(instance);

            Debug.Log($"[MercenaryManager] 상점 용병 {i + 1}/8 생성: {instance.mercenaryName} (Lv.{instance.level})");
        }

        Debug.Log($"[MercenaryManager] ? 상점 용병 {currentShopMercenaries.Count}명 생성 완료");
        OnShopRefreshed?.Invoke();
    }

    /// <summary>
    /// 용병 고용
    /// </summary>
    public bool RecruitMercenary(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryManager] ━━━ 용병 고용 시도: {mercenary.mercenaryName} ━━━");

        // 파티가 가득 찼는지 확인
        if (IsPartyFull)
        {
            Debug.LogWarning($"[MercenaryManager] ? 파티가 가득 찼습니다! ({recruitedMercenaries.Count}/{maxPartySize})");
            return false;
        }

        // 이미 고용되었는지 확인
        if (mercenary.isRecruited)
        {
            Debug.LogWarning($"[MercenaryManager] ? 이미 고용된 용병입니다: {mercenary.mercenaryName}");
            return false;
        }

        // 골드 확인 및 차감
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MercenaryManager] ? GameManager.Instance가 null입니다!");
            return false;
        }

        Debug.Log($"[MercenaryManager] 고용 비용: {mercenary.recruitCost} 골드");
        Debug.Log($"[MercenaryManager] 현재 골드: {GameManager.Instance.Gold} 골드");

        if (!GameManager.Instance.SpendGold(mercenary.recruitCost))
        {
            Debug.LogWarning("[MercenaryManager] ? 골드가 부족합니다!");
            return false;
        }

        // 고용 처리
        mercenary.isRecruited = true;
        recruitedMercenaries.Add(mercenary);

        // 상점 목록에서 제거
        if (currentShopMercenaries.Contains(mercenary))
        {
            currentShopMercenaries.Remove(mercenary);
            Debug.Log($"[MercenaryManager] 상점 목록에서 제거됨");
        }

        Debug.Log($"[MercenaryManager] ??? 용병 고용 완료: {mercenary.mercenaryName}");
        Debug.Log($"[MercenaryManager] 현재 파티 인원: {recruitedMercenaries.Count}/{maxPartySize}");

        OnPartyChanged?.Invoke();
        OnShopRefreshed?.Invoke(); // 상점 UI도 갱신

        return true;
    }

    /// <summary>
    /// 용병 추방
    /// </summary>
    public bool DismissMercenary(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryManager] ━━━ 용병 추방 시도: {mercenary.mercenaryName} ━━━");

        if (!recruitedMercenaries.Contains(mercenary))
        {
            Debug.LogWarning($"[MercenaryManager] ? 고용되지 않은 용병입니다: {mercenary.mercenaryName}");
            return false;
        }

        // 마지막 한 명은 추방 불가 (옵션)
        if (recruitedMercenaries.Count <= 1)
        {
            Debug.LogWarning("[MercenaryManager] ? 최소 1명의 용병은 유지해야 합니다!");
            return false;
        }

        recruitedMercenaries.Remove(mercenary);
        mercenary.isRecruited = false;

        Debug.Log($"[MercenaryManager] ? 용병 추방 완료: {mercenary.mercenaryName}");
        Debug.Log($"[MercenaryManager] 현재 파티 인원: {recruitedMercenaries.Count}/{maxPartySize}");

        OnPartyChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 특정 용병이 고용되었는지 확인
    /// </summary>
    public bool IsMercenaryRecruited(MercenaryInstance mercenary)
    {
        return recruitedMercenaries.Contains(mercenary);
    }

    /// <summary>
    /// 디버그: 모든 고용된 용병 출력
    /// </summary>
    [ContextMenu("Debug: Print Recruited Mercenaries")]
    public void DebugPrintRecruitedMercenaries()
    {
        Debug.Log($"[MercenaryManager] ━━━ 고용된 용병 목록 ({recruitedMercenaries.Count}명) ━━━");
        for (int i = 0; i < recruitedMercenaries.Count; i++)
        {
            var merc = recruitedMercenaries[i];
            Debug.Log($"{i + 1}. {merc.mercenaryName} (Lv.{merc.level}) - HP:{merc.health} STR:{merc.strength}");
        }
    }

    internal IEnumerable<object> GetPartyMembers()
    {
        throw new NotImplementedException();
        // 파티 멤버 로직 반환 추가 필요

    }
}