using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ���� ������ �����ϴ� �Ŵ���
/// - ���� ����/����, �� ����, ���� ����, �̺�Ʈ ���� ���� ó���մϴ�.
/// </summary>
public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("���� ���� ����")]
    [SerializeField] private DungeonDataSO currentDungeon;

    [Header("���� ���� ����")]
    [SerializeField] private int currentRoomIndex = 0; // ���� �� ��ȣ (0~4)
    [SerializeField] private int totalRooms = 5;       // �� �� ����

    private DungeonRoomType currentRoomType;           // ���� �� Ÿ��
    private List<MonsterSpawnData> spawnedMonsters;    // ������ ���� ����Ʈ
    private RoomEventDataSO currentEvent;              // ���� �̺�Ʈ

    // ���� ���� �̺�Ʈ
    public event Action<DungeonDataSO> OnDungeonEntered;          // ���� ����
    public event Action OnDungeonExited;                           // ���� ����
    public event Action<int, int> OnRoomProgressed;                // �� ���� (�����, �ѹ��)
    public event Action<DungeonRoomType> OnRoomTypeSelected;       // �� Ÿ�� ���� �Ϸ�
    public event Action<List<MonsterSpawnData>> OnMonstersSpawned; // ���� ����
    public event Action<RoomEventDataSO> OnEventTriggered;         // �̺�Ʈ �߻�

    private void Awake()
    {
        Debug.Log("[DungeonManager] ������ Awake ���� ������");

        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[DungeonManager] �̱��� �ν��Ͻ� ������");
        }
        else
        {
            Debug.LogWarning("[DungeonManager] �ߺ� �ν��Ͻ� �ı���");
            Destroy(gameObject);
            return;
        }

        spawnedMonsters = new List<MonsterSpawnData>();

        Debug.Log("[DungeonManager] ? Awake �Ϸ�");
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void EnterDungeon(DungeonDataSO dungeon)
    {
        Debug.Log($"[DungeonManager] ������ ���� ����: {dungeon.dungeonName} ������");

        if (dungeon == null)
        {
            Debug.LogError("[DungeonManager] ? dungeon�� null�Դϴ�!");
            return;
        }

        currentDungeon = dungeon;
        currentRoomIndex = 0;
        totalRooms = dungeon.totalRooms;

        Debug.Log($"[DungeonManager] ���� ������ �ε� �Ϸ� - �� ��: {totalRooms}��");

        OnDungeonEntered?.Invoke(currentDungeon);

        Debug.Log($"[DungeonManager] ? OnDungeonEntered �̺�Ʈ �߻�");
    }

    /// <summary>
    /// ���� ���� (Ŭ���� �Ǵ� �й�)
    /// </summary>
    public void ExitDungeon()
    {
        Debug.Log("[DungeonManager] ������ ���� ���� ������");

        currentDungeon = null;
        currentRoomIndex = 0;
        spawnedMonsters.Clear();
        currentEvent = null;

        OnDungeonExited?.Invoke();

        Debug.Log("[DungeonManager] ? ���� ������ �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// �� ���� (3���� �� �ϳ� ����)
    /// </summary>
    public void SelectPath(int pathIndex)
    {
        Debug.Log($"[DungeonManager] ������ ��� ����: {pathIndex}�� (0~2) ������");

        if (currentDungeon == null)
        {
            Debug.LogError("[DungeonManager] ? currentDungeon�� null�Դϴ�!");
            return;
        }

        // �� Ÿ�� ���� ����
        currentRoomType = DecideRoomType();

        Debug.Log($"[DungeonManager] ������ �� Ÿ��: {currentRoomType}");

        // �� ��ȣ ����
        currentRoomIndex++;

        Debug.Log($"[DungeonManager] ���� �� ���൵: {currentRoomIndex}/{totalRooms}");

        OnRoomProgressed?.Invoke(currentRoomIndex, totalRooms);
        OnRoomTypeSelected?.Invoke(currentRoomType);

        // �� Ÿ�Կ� ���� ó��
        ProcessRoom();

        Debug.Log($"[DungeonManager] ? �� ���� �Ϸ�");
    }

    /// <summary>
    /// �� Ÿ�� ���� ����
    /// </summary>
    private DungeonRoomType DecideRoomType()
    {
        Debug.Log("[DungeonManager] �� Ÿ�� ���� ���� ��...");

        // ������ ���� ������ ����
        if (currentRoomIndex >= totalRooms - 1)
        {
            Debug.Log("[DungeonManager] ������ �� �� ������ Ȯ��");
            return DungeonRoomType.Boss;
        }

        // Ȯ��: �̺�Ʈ 20%, �Ϲ����� 60%, ���� 20%
        int randomValue = UnityEngine.Random.Range(0, 100);

        if (randomValue < 20)
        {
            Debug.Log("[DungeonManager] ���� ��� �� �̺�Ʈ�� (20%)");
            return DungeonRoomType.Event;
        }
        else if (randomValue < 80)
        {
            Debug.Log("[DungeonManager] ���� ��� �� �Ϲ����� (60%)");
            return DungeonRoomType.Combat;
        }
        else
        {
            Debug.Log("[DungeonManager] ���� ��� �� ������ (20%)");
            return DungeonRoomType.Boss;
        }
    }

    /// <summary>
    /// �� Ÿ�Կ� ���� ó��
    /// </summary>
    private void ProcessRoom()
    {
        Debug.Log($"[DungeonManager] ������ �� ó�� ����: {currentRoomType} ������");

        switch (currentRoomType)
        {
            case DungeonRoomType.Event:
                Debug.Log("[DungeonManager] �̺�Ʈ �߻� ó��...");
                TriggerRandomEvent();
                break;

            case DungeonRoomType.Combat:
                Debug.Log("[DungeonManager] �Ϲ� ���� ���� ó��...");
                SpawnNormalMonsters();
                break;

            case DungeonRoomType.Boss:
                Debug.Log("[DungeonManager] ���� ���� ���� ó��...");
                SpawnBossMonster();
                break;
        }

        Debug.Log("[DungeonManager] ? �� ó�� �Ϸ�");
    }

    /// <summary>
    /// ���� �̺�Ʈ �߻�
    /// </summary>
    private void TriggerRandomEvent()
    {
        Debug.Log("[DungeonManager] ������ ���� �̺�Ʈ ���� ��... ������");

        if (currentDungeon.possibleEvents == null || currentDungeon.possibleEvents.Length == 0)
        {
            Debug.LogWarning("[DungeonManager] ? �̺�Ʈ ����Ʈ�� ����ֽ��ϴ�!");
            return;
        }

        // �������� �̺�Ʈ ����
        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.possibleEvents.Length);
        currentEvent = currentDungeon.possibleEvents[randomIndex];

        Debug.Log($"[DungeonManager] ���õ� �̺�Ʈ: {currentEvent.eventName} (ID: {currentEvent.eventID})");

        OnEventTriggered?.Invoke(currentEvent);

        Debug.Log("[DungeonManager] ? OnEventTriggered �̺�Ʈ �߻�");
    }

    /// <summary>
    /// �Ϲ� ���� ���� (1~3���� ����)
    /// </summary>
    private void SpawnNormalMonsters()
    {
        Debug.Log("[DungeonManager] ������ �Ϲ� ���� ���� ���� ������");

        spawnedMonsters.Clear();

        if (currentDungeon.normalMonsters == null || currentDungeon.normalMonsters.Length == 0)
        {
            Debug.LogError("[DungeonManager] ? normalMonsters ����Ʈ�� ����ֽ��ϴ�!");
            return;
        }

        // �Ϲ�~���� ��޸� ���͸�
        var validMonsters = currentDungeon.normalMonsters
            .Where(m => m.rarity != MonsterRarity.Boss)
            .ToList();

        if (validMonsters.Count == 0)
        {
            Debug.LogError("[DungeonManager] ? ���� ������ �Ϲ� ���Ͱ� �����ϴ�!");
            return;
        }

        Debug.Log($"[DungeonManager] ���� ������ ���� ����: {validMonsters.Count}��");

        // 1~3���� ���� ����
        int monsterCount = UnityEngine.Random.Range(1, 4);
        Debug.Log($"[DungeonManager] ������ ���� ��: {monsterCount}����");

        for (int i = 0; i < monsterCount; i++)
        {
            // ����ġ ��� ���� ����
            MonsterSpawnData selectedMonster = GetWeightedRandomMonster(validMonsters);

            if (selectedMonster != null)
            {
                spawnedMonsters.Add(selectedMonster);
                Debug.Log($"[DungeonManager] ���� {i + 1} ����: {selectedMonster.monsterName} (���: {selectedMonster.rarity})");
            }
        }

        OnMonstersSpawned?.Invoke(spawnedMonsters);

        Debug.Log($"[DungeonManager] ? �� {spawnedMonsters.Count}���� ���� �Ϸ�");
    }

    /// <summary>
    /// ���� ���� ���� (1����)
    /// </summary>
    private void SpawnBossMonster()
    {
        Debug.Log("[DungeonManager] ������ ���� ���� ���� ���� ������");

        spawnedMonsters.Clear();

        if (currentDungeon.bossMonsters == null || currentDungeon.bossMonsters.Length == 0)
        {
            Debug.LogError("[DungeonManager] ? bossMonsters ����Ʈ�� ����ֽ��ϴ�!");
            return;
        }

        // �������� ���� 1���� ����
        int randomIndex = UnityEngine.Random.Range(0, currentDungeon.bossMonsters.Length);
        MonsterSpawnData selectedBoss = currentDungeon.bossMonsters[randomIndex];

        spawnedMonsters.Add(selectedBoss);

        Debug.Log($"[DungeonManager] ���� ����: {selectedBoss.monsterName}");

        OnMonstersSpawned?.Invoke(spawnedMonsters);

        Debug.Log("[DungeonManager] ? ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ����ġ ��� ���� ���� ����
    /// </summary>
    private MonsterSpawnData GetWeightedRandomMonster(List<MonsterSpawnData> monsters)
    {
        Debug.Log("[DungeonManager] ����ġ ��� ���� ���� ��...");

        // �� ����ġ ���
        int totalWeight = 0;
        foreach (var monster in monsters)
        {
            totalWeight += monster.spawnWeight;
        }

        Debug.Log($"[DungeonManager] �� ����ġ: {totalWeight}");

        // ���� �� ����
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        Debug.Log($"[DungeonManager] ���� ��: {randomValue}");

        // ����ġ ���� Ȯ��
        int cumulativeWeight = 0;
        foreach (var monster in monsters)
        {
            cumulativeWeight += monster.spawnWeight;
            if (randomValue < cumulativeWeight)
            {
                Debug.Log($"[DungeonManager] ���õ� ����: {monster.monsterName} (����ġ: {monster.spawnWeight})");
                return monster;
            }
        }

        Debug.LogWarning("[DungeonManager] ?? ���� ���� ����, ù ��° ���� ��ȯ");
        return monsters[0];
    }

    /// <summary>
    /// �̺�Ʈ ȿ�� ����
    /// </summary>
    public void ApplyEventEffects()
    {
        Debug.Log("[DungeonManager] ������ �̺�Ʈ ȿ�� ���� ���� ������");

        if (currentEvent == null)
        {
            Debug.LogError("[DungeonManager] ? currentEvent�� null�Դϴ�!");
            return;
        }

        foreach (var effect in currentEvent.effects)
        {
            Debug.Log($"[DungeonManager] ȿ�� ����: {effect.effectType}, ��: {effect.value}");

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

        Debug.Log("[DungeonManager] ? �̺�Ʈ ȿ�� ���� �Ϸ�");
    }

    /// <summary>
    /// ��Ƽ�� ���� ����
    /// </summary>
    private void ApplyBuffToParty(EventEffect effect)
    {
        Debug.Log($"[DungeonManager] ��Ƽ ���� ����: {effect.targetStat} +{effect.value} (����: {effect.duration}��)");

        // TODO: MercenaryManager�� �����Ͽ� ��Ƽ ��ü�� ���� ����
        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[DungeonManager] ? MercenaryManager.Instance�� null�Դϴ�!");
            return;
        }

        //var party = MercenaryManager.Instance.GetPartyMembers();
        //foreach (var mercenary in party)
        //{
        //    Debug.Log($"[DungeonManager] {mercenary.mercenaryName}���� ���� ����");
        //    // ���� ���� ���� ���� (��: mercenary.strength += effect.value)
        //}
    }

    /// <summary>
    /// ��Ƽ�� ����� ����
    /// </summary>
    private void ApplyDebuffToParty(EventEffect effect)
    {
        Debug.Log($"[DungeonManager] ��Ƽ ����� ����: {effect.targetStat} -{effect.value} (����: {effect.duration}��)");

        // TODO: ������ ������ ������� ����� ����
    }

    /// <summary>
    /// ��Ƽ ü�� ȸ��
    /// </summary>
    private void HealParty(int amount)
    {
        Debug.Log($"[DungeonManager] ��Ƽ ��ü ü�� ȸ��: +{amount}");

        // TODO: ��Ƽ ��ü HP ����
    }

    /// <summary>
    /// ��Ƽ ü�� ����
    /// </summary>
    private void DamageParty(int amount)
    {
        Debug.Log($"[DungeonManager] ��Ƽ ��ü ����: -{amount}");

        // TODO: ��Ƽ ��ü HP ����
    }

    /// <summary>
    /// ��� ����
    /// </summary>
    private void RewardGold(int amount)
    {
        Debug.Log($"[DungeonManager] ��� ����: +{amount}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(amount);
            Debug.Log("[DungeonManager] ? ��� ���� �Ϸ�");
        }
        else
        {
            Debug.LogError("[DungeonManager] ? GameManager.Instance�� null�Դϴ�!");
        }
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    private void RewardItem(ItemDataSO item, int amount)
    {
        Debug.Log($"[DungeonManager] ������ ����: {item?.itemName ?? "null"} x{amount}");

        if (item == null)
        {
            Debug.LogError("[DungeonManager] ? rewardItem�� null�Դϴ�!");
            return;
        }

        if (InventoryManager.Instance != null)
        {
            //InventoryManager.Instance.AddItem(item.itemID, amount);
            Debug.Log("[DungeonManager] ? ������ ���� �Ϸ�");
        }
        else
        {
            Debug.LogError("[DungeonManager] ? InventoryManager.Instance�� null�Դϴ�!");
        }
    }

    /// <summary>
    /// ���� Ŭ���� üũ
    /// </summary>
    public bool IsDungeonCleared()
    {
        bool isCleared = currentRoomIndex >= totalRooms;
        Debug.Log($"[DungeonManager] ���� Ŭ���� ����: {isCleared} ({currentRoomIndex}/{totalRooms})");
        return isCleared;
    }

    /// <summary>
    /// ���� �� Ÿ�� ��������
    /// </summary>
    public DungeonRoomType GetCurrentRoomType()
    {
        return currentRoomType;
    }

    /// <summary>
    /// ������ ���� ����Ʈ ��������
    /// </summary>
    public List<MonsterSpawnData> GetSpawnedMonsters()
    {
        return spawnedMonsters;
    }

    /// <summary>
    /// ���� ���� ������ ��������
    /// </summary>
    public DungeonDataSO GetCurrentDungeon()
    {
        return currentDungeon;
    }
}

/// <summary>
/// ���� �� Ÿ��
/// </summary>
public enum DungeonRoomType
{
    Event,   // �̺�Ʈ
    Combat,  // �Ϲ� ����
    Boss     // ���� ����
}
