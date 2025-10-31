using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 던전 진행을 관리하는 매니저
/// - 던전 입장/퇴장, 방 선택, 몬스터 스폰, 이벤트 적용 등을 처리합니다.
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("현재 던전 정보")]
    [SerializeField] private DungeonDataSO currentDungeon;

    [Header("던전 진행 상태")]
    [SerializeField] private int currentRoomIndex = 0; // 현재 방 번호 (0~4)
    [SerializeField] private int totalRooms = 5;       // 총 방 개수

    private DungeonRoomType currentRoomType;           // 현재 방 타입
    private List<MonsterSpawnData> spawnedMonsters;    // 스폰된 몬스터 리스트
    private RoomEventDataSO currentEvent;              // 현재 이벤트

    // 던전 상태 이벤트
    public event Action<DungeonDataSO> OnDungeonEntered;          // 던전 입장
    public event Action OnDungeonExited;                           // 던전 퇴장
    public event Action<int, int> OnRoomProgressed;                // 방 진행 (현재방, 총방수)
    public event Action<DungeonRoomType> OnRoomTypeSelected;       // 방 타입 선택 완료
    public event Action<List<MonsterSpawnData>> OnMonstersSpawned; // 몬스터 스폰
    public event Action<RoomEventDataSO> OnEventTriggered;         // 이벤트 발생

    private void Awake()
    {
        Debug.Log("[DungeonManager] ━━━ Awake 시작 ━━━");

        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DungeonManager] 싱글톤 인스턴스 생성됨");
        }
        else
        {
            Debug.LogWarning("[DungeonManager] 중복 인스턴스 파괴됨");
            Destroy(gameObject);
            return;
        }

        spawnedMonsters = new List<MonsterSpawnData>();

        Debug.Log("[DungeonManager] ? Awake 완료");
    }

    /// <summary>
    /// 던전 입장
    /// </summary>
    public void EnterDungeon(DungeonDataSO dungeon)
    {
        Debug.Log($"[DungeonManager] ━━━ 던전 입장: {dungeon.dungeonName} ━━━");

        if (dungeon == null)
        {
            Debug.LogError("[DungeonManager] ? dungeon이 null입니다!");
            return;
        }

        currentDungeon = dungeon;
        currentRoomIndex = 0;
        totalRooms = dungeon.totalRooms;

        Debug.Log($"[DungeonManager] 던전 데이터 로드 완료 - 총 방: {totalRooms}개");

        OnDungeonEntered?.Invoke(currentDungeon);

        Debug.Log($"[DungeonManager] ? OnDungeonEntered 이벤트 발생");
    }

    /// <summary>
    /// 던전 퇴장 (클리어 또는 패배)
    /// </summary>
    public void ExitDungeon()
    {
        Debug.Log("[DungeonManager] ━━━ 던전 퇴장 ━━━");

        currentDungeon = null;
        currentRoomIndex = 0;
        spawnedMonsters.Clear();
        currentEvent = null;

        OnDungeonExited?.Invoke();

        Debug.Log("[DungeonManager] ? 던전 데이터 초기화 완료");
    }

    /// <summary>
    /// 방 선택 (3갈래 중 하나 선택)
    /// </summary>
    public void SelectPath(int pathIndex)
    {
        Debug.Log($"[DungeonManager] ━━━ 통로 선택: {pathIndex}번 (0~2) ━━━");

        if (currentDungeon == null)
        {
            Debug.LogError("[DungeonManager] ? currentDungeon이 null입니다!");
            return;
        }

        // 방 타입 랜덤 결정
        currentRoomType = DecideRoomType();

        Debug.Log($"[DungeonManager] 결정된 방 타입: {currentRoomType}");

        // 방 번호 증가
        currentRoomIndex++;

        Debug.Log($"[DungeonManager] 현재 방 진행도: {currentRoomIndex}/{totalRooms}");

        OnRoomProgressed?.Invoke(currentRoomIndex, totalRooms);
        OnRoomTypeSelected?.Invoke(currentRoomType);

        // 방 타입에 따라 처리
        ProcessRoom();

        Debug.Log($"[DungeonManager] ? 방 선택 완료");
    }

    /// <summary>
    /// 방 타입 랜덤 결정
    /// </summary>
    private DungeonRoomType DecideRoomType()
    {
        Debug.Log("[DungeonManager] 방 타입 랜덤 결정 중...");

        // 마지막 방은 무조건 보스
        if (currentRoomIndex >= totalRooms - 1)
        {
            Debug.Log("[DungeonManager] 마지막 방 → 보스방 확정");
            return DungeonRoomType.Boss;
        }

        // 확률: 이벤트 20%, 일반전투 60%, 보스 20%
        int randomValue = UnityEngine.Random.Range(0, 100);

        if (randomValue < 20)
        {
            Debug.Log("[DungeonManager] 랜덤 결과 → 이벤트방 (20%)");
            return DungeonRoomType.Event;
        }
        else if (randomValue < 80)
        {
            Debug.Log("[DungeonManager] 랜덤 결과 → 일반전투 (60%)");
            return DungeonRoomType.Combat;
        }
        else
        {
            Debug.Log("[DungeonManager] 랜덤 결과 → 보스방 (20%)");
            return DungeonRoomType.Boss;
        }
    }

    /// <summary>
    /// 방 타입에 따른 처리
    /// </summary>
    private void ProcessRoom()
    {
        Debug.Log($"[DungeonManager] ━━━ 방 처리 시작: {currentRoomType} ━━━");

        switch (currentRoomType)
        {
            case DungeonRoomType.Event:
                Debug.Log("[DungeonManager] 이벤트 발생 처리...");
                TriggerRandomEvent();
                break;

            case DungeonRoomType.Combat:
                Debug.Log("[DungeonManager] 일반 몬스터 스폰 처리...");
                SpawnNormalMonsters();
                break;

            case DungeonRoomType.Boss:
                Debug.Log("[DungeonManager] 보스 몬스터 스폰 처리...");
                SpawnBossMonster();
                break;
        }

        Debug.Log("[DungeonManager] ? 방 처리 완료");
    }

    /// <summary>
    /// 랜덤 이벤트 발생
    /// </summary>
    private void TriggerRandomEvent()
    {
        Debug.Log("[DungeonManager] ━━━ 랜덤 이벤트 선택 중... ━━━");

        if (currentDungeon.possibleEvents == null || currentDungeon.possibleEvents.Length == 0)
        {
            Debug.LogWarning("[DungeonManager] ? 이벤트 리스트가 비어있습니다!");
            return;
        }

        // 랜덤으로 이벤트 선택
        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.possibleEvents.Length);
        currentEvent = currentDungeon.possibleEvents[randomIndex];

        Debug.Log($"[DungeonManager] 선택된 이벤트: {currentEvent.eventName} (ID: {currentEvent.eventID})");

        OnEventTriggered?.Invoke(currentEvent);

        Debug.Log("[DungeonManager] ? OnEventTriggered 이벤트 발생");
    }

    /// <summary>
    /// 일반 몬스터 스폰 (1~3마리 랜덤)
    /// </summary>
    private void SpawnNormalMonsters()
    {
        Debug.Log("[DungeonManager] ━━━ 일반 몬스터 스폰 시작 ━━━");

        spawnedMonsters.Clear();

        if (currentDungeon.normalMonsters == null || currentDungeon.normalMonsters.Length == 0)
        {
            Debug.LogError("[DungeonManager] ❌ normalMonsters 리스트가 비어있습니다!");
            return;
        }

        // 일반~에픽 등급만 필터링 (보스 제외)
        var validMonsters = currentDungeon.normalMonsters
            .Where(m => m.rarity != MonsterRarity.Boss)
            .ToList();

        if (validMonsters.Count == 0)
        {
            Debug.LogError("[DungeonManager] ❌ 스폰 가능한 일반 몬스터가 없습니다!");
            return;
        }

        Debug.Log($"[DungeonManager] 스폰 가능한 몬스터 종류: {validMonsters.Count}개");

        // 1~3마리 랜덤 결정
        int monsterCount = UnityEngine.Random.Range(1, 4);
        Debug.Log($"[DungeonManager] 스폰할 몬스터 수: {monsterCount}마리");

        for (int i = 0; i < monsterCount; i++)
        {
            // 가중치 기반 랜덤 선택
            MonsterSpawnData selectedMonster = GetWeightedRandomMonster(validMonsters);

            if (selectedMonster != null)
            {
                spawnedMonsters.Add(selectedMonster);
                Debug.Log($"[DungeonManager] 몬스터 {i + 1} 스폰: {selectedMonster.monsterName} (등급: {selectedMonster.rarity})");
            }
        }

        OnMonstersSpawned?.Invoke(spawnedMonsters);

        Debug.Log($"[DungeonManager] ✅ 총 {spawnedMonsters.Count}마리 스폰 완료");
    }

    /// <summary>
    /// 보스 몬스터 스폰 (1마리)
    /// </summary>
    private void SpawnBossMonster()
    {
        Debug.Log("[DungeonManager] ━━━ 보스 몬스터 스폰 시작 ━━━");

        spawnedMonsters.Clear();

        if (currentDungeon.bossMonsters == null || currentDungeon.bossMonsters.Length == 0)
        {
            Debug.LogError("[DungeonManager] ? bossMonsters 리스트가 비어있습니다!");
            return;
        }

        // 랜덤으로 보스 1마리 선택
        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.bossMonsters.Length);
        MonsterSpawnData selectedBoss = currentDungeon.bossMonsters[randomIndex];

        spawnedMonsters.Add(selectedBoss);

        Debug.Log($"[DungeonManager] 보스 스폰: {selectedBoss.monsterName}");

        OnMonstersSpawned?.Invoke(spawnedMonsters);

        Debug.Log("[DungeonManager] ? 보스 스폰 완료");
    }

    /// <summary>
    /// 가중치 기반 랜덤 몬스터 선택
    /// </summary>
    private MonsterSpawnData GetWeightedRandomMonster(List<MonsterSpawnData> monsters)
    {
        Debug.Log("[DungeonManager] 가중치 기반 몬스터 선택 중...");

        // 총 가중치 계산
        int totalWeight = 0;
        foreach (var monster in monsters)
        {
            totalWeight += monster.spawnWeight;
        }

        Debug.Log($"[DungeonManager] 총 가중치: {totalWeight}");

        // 랜덤 값 생성
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log($"[DungeonManager] 랜덤 값: {randomValue}");

        // 가중치 범위 확인
        int cumulativeWeight = 0;
        foreach (var monster in monsters)
        {
            cumulativeWeight += monster.spawnWeight;
            if (randomValue < cumulativeWeight)
            {
                Debug.Log($"[DungeonManager] 선택된 몬스터: {monster.monsterName} (가중치: {monster.spawnWeight})");
                return monster;
            }
        }

        Debug.LogWarning("[DungeonManager] ?? 몬스터 선택 실패, 첫 번째 몬스터 반환");
        return monsters[0];
    }

    /// <summary>
    /// 이벤트 효과 적용
    /// </summary>
    public void ApplyEventEffects()
    {
        Debug.Log("[DungeonManager] ━━━ 이벤트 효과 적용 시작 ━━━");

        if (currentEvent == null)
        {
            Debug.LogError("[DungeonManager] ? currentEvent가 null입니다!");
            return;
        }

        foreach (var effect in currentEvent.effects)
        {
            Debug.Log($"[DungeonManager] 효과 적용: {effect.effectType}, 값: {effect.value}");

            switch (effect.effectType)
            {
                case EventEffectType.Buff:
                    ApplyBuffToParty(effect);
                    break;

                case EventEffectType.Debuff:
                    ApplyDebuffToParty(effect);
                    break;

                case EventEffectType.Heal:
                    HealParty(effect.value);
                    break;

                case EventEffectType.Damage:
                    DamageParty(effect.value);
                    break;

                case EventEffectType.GoldReward:
                    RewardGold(effect.value);
                    break;

                case EventEffectType.ItemReward:
                    RewardItem(effect.rewardItem, effect.itemAmount);
                    break;
            }
        }

        Debug.Log("[DungeonManager] ? 이벤트 효과 적용 완료");
    }

    /// <summary>
    /// 파티에 버프 적용
    /// </summary>
    private void ApplyBuffToParty(EventEffect effect)
    {
        Debug.Log($"[DungeonManager] 파티 버프 적용: {effect.targetStat} +{effect.value} (지속: {effect.duration}턴)");

        // TODO: MercenaryManager와 연동하여 파티 전체에 버프 적용
        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[DungeonManager] ? MercenaryManager.Instance가 null입니다!");
            return;
        }

        //var party = MercenaryManager.Instance.GetPartyMembers();
        //foreach (var mercenary in party)
        //{
        //    Debug.Log($"[DungeonManager] {mercenary.mercenaryName}에게 버프 적용");
        //    // 실제 스탯 적용 로직 (예: mercenary.strength += effect.value)
        //}
    }

    /// <summary>
    /// 파티에 디버프 적용
    /// </summary>
    private void ApplyDebuffToParty(EventEffect effect)
    {
        Debug.Log($"[DungeonManager] 파티 디버프 적용: {effect.targetStat} -{effect.value} (지속: {effect.duration}턴)");

        // TODO: 버프와 동일한 방식으로 디버프 적용
    }

    /// <summary>
    /// 파티 체력 회복
    /// </summary>
    private void HealParty(int amount)
    {
        Debug.Log($"[DungeonManager] 파티 전체 체력 회복: +{amount}");

        // TODO: 파티 전체 HP 증가
    }

    /// <summary>
    /// 파티 체력 감소
    /// </summary>
    private void DamageParty(int amount)
    {
        Debug.Log($"[DungeonManager] 파티 전체 피해: -{amount}");

        // TODO: 파티 전체 HP 감소
    }

    /// <summary>
    /// 골드 보상
    /// </summary>
    private void RewardGold(int amount)
    {
        Debug.Log($"[DungeonManager] 골드 보상: +{amount}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(amount);
            Debug.Log("[DungeonManager] ? 골드 지급 완료");
        }
        else
        {
            Debug.LogError("[DungeonManager] ? GameManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 아이템 보상
    /// </summary>
    private void RewardItem(ItemDataSO item, int amount)
    {
        Debug.Log($"[DungeonManager] 아이템 보상: {item?.itemName ?? "null"} x{amount}");

        if (item == null)
        {
            Debug.LogError("[DungeonManager] ? rewardItem이 null입니다!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            //InventoryManager.Instance.AddItem(item.itemID, amount);
            Debug.Log("[DungeonManager] ? 아이템 지급 완료");
        }
        else
        {
            Debug.LogError("[DungeonManager] ? InventoryManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 던전 클리어 체크
    /// </summary>
    public bool IsDungeonCleared()
    {
        bool isCleared = currentRoomIndex >= totalRooms;
        Debug.Log($"[DungeonManager] 던전 클리어 여부: {isCleared} ({currentRoomIndex}/{totalRooms})");
        return isCleared;
    }

    /// <summary>
    /// 현재 방 타입 가져오기
    /// </summary>
    public DungeonRoomType GetCurrentRoomType()
    {
        return currentRoomType;
    }

    /// <summary>
    /// 스폰된 몬스터 리스트 가져오기
    /// </summary>
    public List<MonsterSpawnData> GetSpawnedMonsters()
    {
        return spawnedMonsters;
    }

    /// <summary>
    /// 현재 던전 데이터 가져오기
    /// </summary>
    public DungeonDataSO GetCurrentDungeon()
    {
        return currentDungeon;
    }

    /// <summary>
    /// 던전 완전 클리어 처리
    /// </summary>
    public void CompleteDungeon()
    {
        Debug.Log("[DungeonManager] ━━━ 던전 완전 클리어! ━━━");

        // 클리어 보상 지급
        RewardDungeonClear();

        // ✅ 던전 퇴장 → OnDungeonExited 이벤트 발생
        //    → GameSceneManager가 자동으로 ShowTownUI() 호출
        ExitDungeon();

        Debug.Log("[DungeonManager] ✅ 던전 퇴장 완료 (이벤트 발생)");
    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
        Debug.Log("[DungeonManager] ━━━ 다음 방으로 이동 ━━━");

        // ✅ 아무것도 호출하지 않음
        //    → CombatManager.EndCombat()에서 이미 GameSceneManager.OnCombatEnded() 호출
        //    → GameSceneManager가 자동으로 ShowCorridorUI() 호출

        Debug.Log("[DungeonManager] ✅ 다음 방 대기 (GameSceneManager가 처리)");
    }

    /// <summary>
    /// 던전 클리어 보상 지급
    /// </summary>
    private void RewardDungeonClear()
    {
        Debug.Log("[DungeonManager] 던전 클리어 보상 계산 중...");

        if (currentDungeon == null)
        {
            Debug.LogWarning("[DungeonManager] ⚠️ currentDungeon이 null입니다!");
            return;
        }

        // 클리어 보상 (골드 + 경험치)
        int goldReward = UnityEngine.Random.Range(100, 500); // 100~500 골드
        int expReward = UnityEngine.Random.Range(200, 1000); // 200~1000 경험치

        Debug.Log($"[DungeonManager] 클리어 보상 - 골드: {goldReward}, 경험치: {expReward}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);
            Debug.Log($"[DungeonManager] ✅ 골드 {goldReward} 지급");
        }

        // TODO: 경험치 시스템 구현 시 추가
        Debug.Log($"[DungeonManager] ✅ 경험치 {expReward} 지급 (미구현)");
    }
}

/// <summary>
/// 던전 방 타입
/// </summary>  
public enum DungeonRoomType
{
    Event,   // 이벤트
    Combat,  // 일반 전투
    Boss     // 보스 전투
}
