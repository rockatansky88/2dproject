using UnityEngine;

/// <summary>
/// ���� �⺻ ������ ��� ScriptableObject
/// - ������ ��� �̹���, ��� ���� ����Ʈ, �̺�Ʈ ����Ʈ�� �����մϴ�.
/// </summary>
[CreateAssetMenu(fileName = "New Dungeon", menuName = "Game/Dungeon/Dungeon Data")]
public class DungeonDataSO : ScriptableObject
{
    [Header("���� �⺻ ����")]
    [Tooltip("���� ���� ID (��: dungeon_forest_01)")]
    public string dungeonID;

    [Tooltip("���� �̸� (��: ����� ��)")]
    public string dungeonName;

    [Tooltip("���� �Ա� ��� �̹���")]
    public Sprite entranceSprite;

    [Tooltip("���� ���� ��� ��� �̹��� (3���� ���� ȭ��)")]
    public Sprite corridorSprite;

    [Tooltip("�Ϲ� ���� ��� �̹���")]
    public Sprite combatBackgroundSprite;

    [Tooltip("���� ���� ��� �̹���")]
    public Sprite bossBackgroundSprite;

    [Tooltip("�̺�Ʈ �� ��� �̹���")]
    public Sprite eventBackgroundSprite;

    [Header("���� ���� ����")]
    [Tooltip("�Ϲ� �������� ���� ������ ���� ����Ʈ (�Ϲ�~����)")]
    public MonsterSpawnData[] normalMonsters;

    [Tooltip("���� �������� ���� ������ ���� ����Ʈ")]
    public MonsterSpawnData[] bossMonsters;

    [Header("�̺�Ʈ ����")]
    [Tooltip("�� �������� ���� ������ �̺�Ʈ ����Ʈ")]
    public RoomEventDataSO[] possibleEvents;

    [Header("���� ���̵�")]
    [Tooltip("���� ����")]
    public int recommendedLevel = 1;

    [Tooltip("�� �� ���� (�⺻��: 5)")]
    public int totalRooms = 5;
}

/// <summary>
/// ���� ���� ����
/// </summary>
[System.Serializable]
public class MonsterSpawnData
{
    [Tooltip("���� ���� ������")]
    public MonsterStatsSO monsterStats;

    [Tooltip("���� ��������Ʈ (�������� ǥ��)")]
    public Sprite monsterSprite;

    [Tooltip("���� �̸�")]
    public string monsterName;

    [Tooltip("���� ��� (Normal, Rare, Epic, Boss)")]
    public MonsterRarity rarity;

    [Tooltip("���� ����ġ (�������� ���� ����)")]
    [Range(1, 100)]
    public int spawnWeight = 10;
}

/// <summary>
/// ���� ���
/// </summary>
public enum MonsterRarity
{
    Normal,  // �Ϲ�
    Rare,    // ���
    Epic,    // ����
    Boss     // ����
}
