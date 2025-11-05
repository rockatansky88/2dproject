using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 던전 진행을 관리하는 매니저
/// 던전 입장/퇴장, 방 선택, 몬스터 스폰, 이벤트 효과 적용을 처리합니다.
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("현재 던전 정보")]
    [SerializeField] private DungeonDataSO currentDungeon;

    [Header("던전 진행 상태")]
    [SerializeField] private int currentRoomIndex = 0;
    [SerializeField] private int totalRooms = 5;

    private DungeonRoomType currentRoomType;
    private List<MonsterSpawnData> spawnedMonsters;
    private RoomEventDataSO currentEvent;

    public event Action<DungeonDataSO> OnDungeonEntered;
    public event Action OnDungeonExited;
    public event Action<int, int> OnRoomProgressed;
    public event Action<DungeonRoomType> OnRoomTypeSelected;
    public event Action<List<MonsterSpawnData>> OnMonstersSpawned;
    public event Action<RoomEventDataSO> OnEventTriggered;

    public bool IsInDungeon => currentDungeon != null;

    private void Awake()
    {
        Debug.Log("[DungeonManager] ━━━ Awake 시작 ━━━");

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

        Debug.Log("[DungeonManager] ✅ Awake 완료");
    }

    /// <summary>
    /// 던전 입장
    /// </summary>
    public void EnterDungeon(DungeonDataSO dungeon)
    {
        Debug.Log($"[DungeonManager] ━━━ 던전 입장: {dungeon.dungeonName} ━━━");

        if (dungeon == null)
        {
            Debug.LogError("[DungeonManager] ❌ dungeon이 null입니다!");
            return;
        }

        currentDungeon = dungeon;
        currentRoomIndex = 0;
        totalRooms = dungeon.totalRooms;

        Debug.Log($"[DungeonManager] 던전 데이터 로드 완료 - 총 방: {totalRooms}개");

        OnDungeonEntered?.Invoke(currentDungeon);

        Debug.Log($"[DungeonManager] ✅ OnDungeonEntered 이벤트 발생");
    }

    /// <summary>
    /// 던전 퇴장 (클리어 또는 패배)
    /// 마을로 귀환 시 모든 용병의 HP/MP를 완전 회복하고 이벤트 버프를 제거합니다.
    /// </summary>
    public void ExitDungeon()
    {
        Debug.Log("[DungeonManager] ━━━ 던전 퇴장 ━━━");

        // 🆕 추가: 모든 용병의 이벤트 버프 제거
        ClearAllEventBuffs();

        // 모든 용병 HP/MP 완전 회복
        RestoreAllMercenaries();

        currentDungeon = null;
        currentRoomIndex = 0;
        spawnedMonsters.Clear();
        currentEvent = null;

        OnDungeonExited?.Invoke();

        Debug.Log("[DungeonManager] ✅ 던전 데이터 초기화 완료");
    }

    /// <summary>
    /// 모든 용병의 이벤트 버프를 제거합니다.
    /// 던전 퇴장 시 호출되어 스탯을 원래대로 되돌립니다.
    /// </summary>
    private void ClearAllEventBuffs()
    {
        Debug.Log("[DungeonManager] ━━━ 이벤트 버프 제거 시작 ━━━");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[DungeonManager] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> allMercenaries = MercenaryManager.Instance.RecruitedMercenaries;

        foreach (var mercenary in allMercenaries)
        {
            mercenary.ClearEventBuffs();
        }

        Debug.Log($"[DungeonManager] ✅ {allMercenaries.Count}명의 용병 이벤트 버프 제거 완료");
    }

    /// <summary>
    /// 모든 용병의 HP/MP를 완전 회복합니다.
    /// 던전 퇴장 시 호출됩니다.
    /// </summary>
    private void RestoreAllMercenaries()
    {
        Debug.Log("[DungeonManager] ━━━ 용병 HP/MP 회복 시작 ━━━");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[DungeonManager] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> allMercenaries = MercenaryManager.Instance.RecruitedMercenaries;

        foreach (var mercenary in allMercenaries)
        {
            int beforeHP = mercenary.currentHP;
            int beforeMP = mercenary.currentMP;

            mercenary.currentHP = mercenary.maxHP;
            mercenary.currentMP = mercenary.maxMP;

            Debug.Log($"[DungeonManager] {mercenary.mercenaryName} 회복: HP {beforeHP} → {mercenary.currentHP}, MP {beforeMP} → {mercenary.currentMP}");
        }

        Debug.Log("[DungeonManager] ✅ 모든 용병 HP/MP 회복 완료");
    }

    /// <summary>
    /// 방 선택 (3갈래 중 하나 선택)
    /// </summary>
    public void SelectPath(int pathIndex)
    {
        Debug.Log($"[DungeonManager] ━━━ 통로 선택: {pathIndex}번 (0~2) ━━━");

        if (currentDungeon == null)
        {
            Debug.LogError("[DungeonManager] ❌ currentDungeon이 null입니다!");
            return;
        }

        currentRoomType = DecideRoomType();
        Debug.Log($"[DungeonManager] 결정된 방 타입: {currentRoomType}");

        currentRoomIndex++;
        Debug.Log($"[DungeonManager] 현재 방 진행도: {currentRoomIndex}/{totalRooms}");

        OnRoomProgressed?.Invoke(currentRoomIndex, totalRooms);
        OnRoomTypeSelected?.Invoke(currentRoomType);

        ProcessRoom();

        Debug.Log($"[DungeonManager] ✅ 방 선택 완료");
    }

    /// <summary>
    /// 방 타입 랜덤 결정
    /// </summary>
    private DungeonRoomType DecideRoomType()
    {
        Debug.Log("[DungeonManager] 방 타입 랜덤 결정 중...");

        if (currentRoomIndex >= totalRooms - 1)
        {
            Debug.Log("[DungeonManager] 마지막 방 → 보스방 확정");
            return DungeonRoomType.Boss;
        }

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

        Debug.Log("[DungeonManager] ✅ 방 처리 완료");
    }

    /// <summary>
    /// 랜덤 이벤트 발생
    /// </summary>
    private void TriggerRandomEvent()
    {
        Debug.Log("[DungeonManager] ━━━ 랜덤 이벤트 선택 중... ━━━");

        if (currentDungeon.possibleEvents == null || currentDungeon.possibleEvents.Length == 0)
        {
            Debug.LogWarning("[DungeonManager] ⚠️ 이벤트 리스트가 비어있습니다!");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.possibleEvents.Length);
        currentEvent = currentDungeon.possibleEvents[randomIndex];

        Debug.Log($"[DungeonManager] 선택된 이벤트: {currentEvent.eventName} (ID: {currentEvent.eventID})");

        OnEventTriggered?.Invoke(currentEvent);

        Debug.Log("[DungeonManager] ✅ OnEventTriggered 이벤트 발생");
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

        var validMonsters = currentDungeon.normalMonsters
            .Where(m => m.rarity != MonsterRarity.Boss)
            .ToList();

        if (validMonsters.Count == 0)
        {
            Debug.LogError("[DungeonManager] ❌ 스폰 가능한 일반 몬스터가 없습니다!");
            return;
        }

        Debug.Log($"[DungeonManager] 스폰 가능한 몬스터 종류: {validMonsters.Count}개");

        int monsterCount = UnityEngine.Random.Range(1, 4);
        Debug.Log($"[DungeonManager] 스폰할 몬스터 수: {monsterCount}마리");

        for (int i = 0; i < monsterCount; i++)
        {
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
            Debug.LogError("[DungeonManager] ❌ bossMonsters 리스트가 비어있습니다!");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.bossMonsters.Length);
        MonsterSpawnData selectedBoss = currentDungeon.bossMonsters[randomIndex];

        spawnedMonsters.Add(selectedBoss);

        Debug.Log($"[DungeonManager] 보스 스폰: {selectedBoss.monsterName}");

        OnMonstersSpawned?.Invoke(spawnedMonsters);

        Debug.Log("[DungeonManager] ✅ 보스 스폰 완료");
    }

    /// <summary>
    /// 가중치 기반 랜덤 몬스터 선택
    /// </summary>
    private MonsterSpawnData GetWeightedRandomMonster(List<MonsterSpawnData> monsters)
    {
        Debug.Log("[DungeonManager] 가중치 기반 몬스터 선택 중...");

        int totalWeight = 0;
        foreach (var monster in monsters)
        {
            totalWeight += monster.spawnWeight;
        }

        Debug.Log($"[DungeonManager] 총 가중치: {totalWeight}");

        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log($"[DungeonManager] 랜덤 값: {randomValue}");

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

        Debug.LogWarning("[DungeonManager] ⚠️ 몬스터 선택 실패, 첫 번째 몬스터 반환");
        return monsters[0];
    }

    /// <summary>
    /// 이벤트 효과 적용
    /// GameSceneManager.ShowEventUI()에서 자동으로 호출되어 파티 전체에 효과를 적용합니다.
    /// </summary>
    public void ApplyEventEffects()
    {
        Debug.Log("[DungeonManager] ━━━━━━ 이벤트 효과 적용 시작 ━━━━━━");

        if (currentEvent == null)
        {
            Debug.LogError("[DungeonManager] ❌ currentEvent가 null입니다!");
            return;
        }

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[DungeonManager] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> party = MercenaryManager.Instance.RecruitedMercenaries;

        if (party == null || party.Count == 0)
        {
            Debug.LogWarning("[DungeonManager] ⚠️ 파티가 비어있습니다!");
            return;
        }

        Debug.Log($"[DungeonManager] 파티 인원: {party.Count}명, 효과 개수: {currentEvent.effects.Length}개");

        foreach (var effect in currentEvent.effects)
        {
            Debug.Log($"[DungeonManager] ━━ 효과 적용: {effect.effectType}, 스탯: {effect.targetStat}, 값: {effect.value} ━━");

            switch (effect.effectType)
            {
                case EventEffectType.Buff:
                    ApplyStatChange(effect, party, isPositive: true);
                    break;

                case EventEffectType.Debuff:
                    ApplyStatChange(effect, party, isPositive: false);
                    break;

                case EventEffectType.GoldReward:
                    RewardGold(effect.value);
                    break;

                case EventEffectType.ItemReward:
                    RewardItem(effect.rewardItem, effect.itemAmount);
                    break;

                default:
                    Debug.LogWarning($"[DungeonManager] ⚠️ 처리되지 않은 효과 타입: {effect.effectType}");
                    break;
            }
        }

        Debug.Log("[DungeonManager] ✅ 이벤트 효과 적용 완료");
    }

    /// <summary>
    /// 스탯 변경 적용 (Buff/Debuff 통합 처리)
    /// HP/MP 즉시 변경과 스탯 버프/디버프를 모두 처리합니다.
    /// </summary>
    /// <param name="effect">효과 데이터</param>
    /// <param name="party">파티 리스트</param>
    /// <param name="isPositive">true: 증가(Buff), false: 감소(Debuff)</param>
    private void ApplyStatChange(EventEffect effect, List<MercenaryInstance> party, bool isPositive)
    {
        string effectName = isPositive ? "버프" : "디버프";
        int modifiedValue = isPositive ? effect.value : -effect.value;

        Debug.Log($"[DungeonManager] ━━ {effectName} 적용 시작: {effect.targetStat} {modifiedValue:+0;-#} ━━");

        // HP/MP는 즉시 적용 (버프 시스템 사용 안함)
        if (effect.targetStat == StatType.HP)
        {
            ApplyImmediateHPChange(modifiedValue, party);
            return;
        }

        if (effect.targetStat == StatType.MP)
        {
            ApplyImmediateMPChange(modifiedValue, party);
            return;
        }

        // 나머지 스탯은 버프 시스템으로 처리 (던전 종료까지 유지)
        ApplyStatBuff(effect, party, modifiedValue);
    }

    /// <summary>
    /// HP 즉시 변경 (회복 또는 피해)
    /// </summary>
    private void ApplyImmediateHPChange(int amount, List<MercenaryInstance> party)
    {
        Debug.Log($"[DungeonManager] ━━ HP 즉시 변경: {amount:+0;-#} ━━");

        foreach (var mercenary in party)
        {
            if (amount > 0)
            {
                mercenary.Heal(amount);
            }
            else
            {
                mercenary.TakeDamage(-amount); // 음수를 양수로 변환
            }
        }

        Debug.Log($"[DungeonManager] ✅ HP 변경 완료: {party.Count}명에게 {amount:+0;-#}");
    }

    /// <summary>
    /// MP 즉시 변경 (회복 또는 소모)
    /// </summary>
    private void ApplyImmediateMPChange(int amount, List<MercenaryInstance> party)
    {
        Debug.Log($"[DungeonManager] ━━ MP 즉시 변경: {amount:+0;-#} ━━");

        foreach (var mercenary in party)
        {
            if (amount > 0)
            {
                mercenary.RestoreMana(amount);
            }
            else
            {
                mercenary.ConsumeMana(-amount); // 음수를 양수로 변환
            }
        }

        Debug.Log($"[DungeonManager] ✅ MP 변경 완료: {party.Count}명에게 {amount:+0;-#}");
    }

    /// <summary>
    /// 스탯 버프 적용 (던전 종료까지 지속)
    /// STR, DEX, INT, WIS, SPD, MaxHP, MaxMP를 처리합니다.
    /// </summary>
    private void ApplyStatBuff(EventEffect effect, List<MercenaryInstance> party, int modifiedValue)
    {
        string buffID = $"buff_{currentEvent.eventID}_{effect.targetStat}_{UnityEngine.Random.Range(1000, 9999)}";
        string buffName = $"{currentEvent.eventName} - {effect.targetStat} {(modifiedValue > 0 ? "증가" : "감소")}";

        foreach (var mercenary in party)
        {
            int str = 0, dex = 0, intel = 0, wis = 0, spd = 0;
            int maxHP = 0, maxMP = 0;

            switch (effect.targetStat)
            {
                case StatType.Strength:
                    str = modifiedValue;
                    break;
                case StatType.Dexterity:
                    dex = modifiedValue;
                    break;
                case StatType.Intelligence:
                    intel = modifiedValue;
                    break;
                case StatType.Wisdom:
                    wis = modifiedValue;
                    break;
                case StatType.Speed:
                    spd = modifiedValue;
                    break;
                case StatType.MaxHP:
                    maxHP = modifiedValue;
                    Debug.Log($"[DungeonManager] ⚠️ MaxHP 버프는 현재 미구현 (확장 예정)");
                    break;
                case StatType.MaxMP:
                    maxMP = modifiedValue;
                    Debug.Log($"[DungeonManager] ⚠️ MaxMP 버프는 현재 미구현 (확장 예정)");
                    break;
                case StatType.None:
                    Debug.LogWarning($"[DungeonManager] ⚠️ targetStat이 None입니다. Buff/Debuff에는 스탯을 지정해야 합니다.");
                    continue;
                default:
                    Debug.LogWarning($"[DungeonManager] ⚠️ 처리되지 않은 targetStat: {effect.targetStat}");
                    continue;
            }

            EventBuffData buff = new EventBuffData(
                buffID,
                buffName,
                modifiedValue > 0 ? EventEffectType.Buff : EventEffectType.Debuff,
                str, dex, intel, wis, spd,
                effect.duration
            );

            mercenary.ApplyEventBuff(buff);
        }

        Debug.Log($"[DungeonManager] ✅ 스탯 버프 적용 완료: {party.Count}명에게 {effect.targetStat} {modifiedValue:+0;-#}");
    }

    /// <summary>
    /// 골드 보상
    /// </summary>
    private void RewardGold(int amount)
    {
        Debug.Log($"[DungeonManager] ━━ 골드 보상: +{amount} ━━");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(amount);
            Debug.Log($"[DungeonManager] ✅ 골드 {amount} 지급 완료");
        }
        else
        {
            Debug.LogError("[DungeonManager] ❌ GameManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 아이템 보상
    /// </summary>
    private void RewardItem(ItemDataSO item, int amount)
    {
        Debug.Log($"[DungeonManager] ━━ 아이템 보상: {item?.itemName ?? "null"} x{amount} ━━");

        if (item == null)
        {
            Debug.LogError("[DungeonManager] ❌ rewardItem이 null입니다!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(item, amount);
            if (success)
            {
                Debug.Log($"[DungeonManager] ✅ 아이템 '{item.itemName}' x{amount} 지급 완료");
            }
            else
            {
                Debug.LogWarning($"[DungeonManager] ⚠️ 아이템 '{item.itemName}' 지급 실패 (인벤토리 가득 참?)");
            }
        }
        else
        {
            Debug.LogError("[DungeonManager] ❌ InventoryManager.Instance가 null입니다!");
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

    public DungeonRoomType GetCurrentRoomType()
    {
        return currentRoomType;
    }

    public List<MonsterSpawnData> GetSpawnedMonsters()
    {
        return spawnedMonsters;
    }

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

        RewardDungeonClear();
        ExitDungeon();

        Debug.Log("[DungeonManager] ✅ 던전 퇴장 완료 (이벤트 발생)");
    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
        Debug.Log("[DungeonManager] ━━━ 다음 방으로 이동 ━━━");
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

        int goldReward = UnityEngine.Random.Range(100, 500);
        int expReward = UnityEngine.Random.Range(200, 1000);

        Debug.Log($"[DungeonManager] 클리어 보상 - 골드: {goldReward}, 경험치: {expReward}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);
            Debug.Log($"[DungeonManager] ✅ 골드 {goldReward} 지급");
        }

        Debug.Log($"[DungeonManager] ✅ 경험치 {expReward} 지급 (미구현)");
    }
}

public enum DungeonRoomType
{
    Event,
    Combat,
    Boss
}
