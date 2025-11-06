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
        // 던전 매니저 싱글톤 인스턴스 설정

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        spawnedMonsters = new List<MonsterSpawnData>();

    }

    /// <summary>
    /// 던전 입장
    /// </summary>
    public void EnterDungeon(DungeonDataSO dungeon)
    {

        if (dungeon == null)
        {
            return;
        }

        currentDungeon = dungeon;
        currentRoomIndex = 0;
        totalRooms = dungeon.totalRooms;


        OnDungeonEntered?.Invoke(currentDungeon);

    }

    /// <summary>
    /// 던전 퇴장 (클리어 또는 패배)
    /// 마을로 귀환 시 모든 용병의 HP/MP를 완전 회복하고 이벤트 버프를 제거합니다.
    /// </summary>
    public void ExitDungeon()
    {

        // 🆕 추가: 모든 용병의 이벤트 버프 제거
        ClearAllEventBuffs();

        // 모든 용병 HP/MP 완전 회복
        RestoreAllMercenaries();

        currentDungeon = null;
        currentRoomIndex = 0;
        spawnedMonsters.Clear();
        currentEvent = null;

        OnDungeonExited?.Invoke();

    }

    /// <summary>
    /// 모든 용병의 이벤트 버프를 제거합니다.
    /// 던전 퇴장 시 호출되어 스탯을 원래대로 되돌립니다.
    /// </summary>
    private void ClearAllEventBuffs()
    {

        if (MercenaryManager.Instance == null)
        {
            return;
        }

        List<MercenaryInstance> allMercenaries = MercenaryManager.Instance.RecruitedMercenaries;

        foreach (var mercenary in allMercenaries)
        {
            mercenary.ClearEventBuffs();
        }

    }

    /// <summary>
    /// 모든 용병의 HP/MP를 완전 회복합니다.
    /// 던전 퇴장 시 호출됩니다.
    /// </summary>
    private void RestoreAllMercenaries()
    {

        if (MercenaryManager.Instance == null)
        {
            return;
        }

        List<MercenaryInstance> allMercenaries = MercenaryManager.Instance.RecruitedMercenaries;

        foreach (var mercenary in allMercenaries)
        {
            int beforeHP = mercenary.currentHP;
            int beforeMP = mercenary.currentMP;

            mercenary.currentHP = mercenary.maxHP;
            mercenary.currentMP = mercenary.maxMP;

        }

    }

    /// <summary>
    /// 방 선택 (3갈래 중 하나 선택)
    /// </summary>
    public void SelectPath(int pathIndex)
    {
        if (currentDungeon == null)
        {
            return;
        }

        currentRoomIndex++;
        OnRoomProgressed?.Invoke(currentRoomIndex, totalRooms);
        currentRoomType = DecideRoomType();
        OnRoomTypeSelected?.Invoke(currentRoomType);
        ProcessRoom();
    }

    /// <summary>
    /// 방 타입 랜덤 결정
    /// </summary>
    private DungeonRoomType DecideRoomType()
    {
        if (currentRoomIndex >= totalRooms)
        {
            return DungeonRoomType.Boss;
        }

        int randomValue = UnityEngine.Random.Range(0, 100);

        if (randomValue < 20)
        {
            return DungeonRoomType.Event;
        }
        else if (randomValue < 80)
        {
            return DungeonRoomType.Combat;
        }
        else
        {
            return DungeonRoomType.Boss;
        }
    }

    /// <summary>
    /// 방 타입에 따른 처리
    /// </summary>
    private void ProcessRoom()
    {

        switch (currentRoomType)
        {
            case DungeonRoomType.Event:
                TriggerRandomEvent();
                break;

            case DungeonRoomType.Combat:
                SpawnNormalMonsters();
                break;

            case DungeonRoomType.Boss:
                SpawnBossMonster();
                break;
        }

    }

    /// <summary>
    /// 랜덤 이벤트 발생
    /// </summary>
    private void TriggerRandomEvent()
    {

        if (currentDungeon.possibleEvents == null || currentDungeon.possibleEvents.Length == 0)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.possibleEvents.Length);
        currentEvent = currentDungeon.possibleEvents[randomIndex];


        OnEventTriggered?.Invoke(currentEvent);

    }

    /// <summary>
    /// 일반 몬스터 스폰 (1~3마리 랜덤)
    /// </summary>
    private void SpawnNormalMonsters()
    {

        spawnedMonsters.Clear();

        if (currentDungeon.normalMonsters == null || currentDungeon.normalMonsters.Length == 0)
        {
            return;
        }

        var validMonsters = currentDungeon.normalMonsters
            .Where(m => m.rarity != MonsterRarity.Boss)
            .ToList();

        if (validMonsters.Count == 0)
        {
            return;
        }


        int monsterCount = UnityEngine.Random.Range(1, 4);

        for (int i = 0; i < monsterCount; i++)
        {
            MonsterSpawnData selectedMonster = GetWeightedRandomMonster(validMonsters);

            if (selectedMonster != null)
            {
                spawnedMonsters.Add(selectedMonster);
            }
        }

        OnMonstersSpawned?.Invoke(spawnedMonsters);

    }

    /// <summary>
    /// 보스 몬스터 스폰 (1마리)
    /// </summary>
    private void SpawnBossMonster()
    {

        spawnedMonsters.Clear();

        if (currentDungeon.bossMonsters == null || currentDungeon.bossMonsters.Length == 0)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.bossMonsters.Length);
        MonsterSpawnData selectedBoss = currentDungeon.bossMonsters[randomIndex];

        spawnedMonsters.Add(selectedBoss);


        OnMonstersSpawned?.Invoke(spawnedMonsters);

    }

    /// <summary>
    /// 가중치 기반 랜덤 몬스터 선택
    /// </summary>
    private MonsterSpawnData GetWeightedRandomMonster(List<MonsterSpawnData> monsters)
    {

        int totalWeight = 0;
        foreach (var monster in monsters)
        {
            totalWeight += monster.spawnWeight;
        }


        int randomValue = UnityEngine.Random.Range(0, totalWeight);

        int cumulativeWeight = 0;
        foreach (var monster in monsters)
        {
            cumulativeWeight += monster.spawnWeight;
            if (randomValue < cumulativeWeight)
            {
                return monster;
            }
        }

        return monsters[0];
    }

    /// <summary>
    /// 이벤트 효과 적용
    /// GameSceneManager.ShowEventUI()에서 자동으로 호출되어 파티 전체에 효과를 적용합니다.
    /// </summary>
    public void ApplyEventEffects()
    {

        if (currentEvent == null)
        {
            return;
        }

        if (MercenaryManager.Instance == null)
        {
            return;
        }

        List<MercenaryInstance> party = MercenaryManager.Instance.RecruitedMercenaries;

        if (party == null || party.Count == 0)
        {
            return;
        }


        foreach (var effect in currentEvent.effects)
        {

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
                    break;
            }
        }

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

    }

    /// <summary>
    /// MP 즉시 변경 (회복 또는 소모)
    /// </summary>
    private void ApplyImmediateMPChange(int amount, List<MercenaryInstance> party)
    {

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
                    break;
                case StatType.MaxMP:
                    maxMP = modifiedValue;
                    break;
                case StatType.None:
                    continue;
                default:
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

    }

    /// <summary>
    /// 골드 보상
    /// </summary>
    private void RewardGold(int amount)
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(amount);
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

        if (item == null)
        {
            return;
        }

        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(item, amount);
            if (success)
            {
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
    /// 5번째 방(보스)을 완료했는지 확인합니다.
    /// </summary>
    public bool IsDungeonCleared()
    {
        // 5번째 방을 완료하면 currentRoomIndex = 5이므로
        bool isCleared = currentRoomIndex >= totalRooms;
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

        RewardDungeonClear();
        ExitDungeon();

    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void MoveToNextRoom()
    {
    }

    /// <summary>
    /// 던전 클리어 보상 지급
    /// </summary>
    private void RewardDungeonClear()
    {

        if (currentDungeon == null)
        {
            return;
        }

        int goldReward = UnityEngine.Random.Range(100, 500);
        int expReward = UnityEngine.Random.Range(200, 1000);


        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(goldReward);
        }

    }
}

public enum DungeonRoomType
{
    Event,
    Combat,
    Boss
}
