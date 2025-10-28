using UnityEngine;

/// <summary>
/// 던전 기본 정보를 담는 ScriptableObject
/// - 던전별 배경 이미지, 출몰 몬스터 리스트, 이벤트 리스트를 관리합니다.
/// </summary>
[CreateAssetMenu(fileName = "New Dungeon", menuName = "Game/Dungeon/Dungeon Data")]
public class DungeonDataSO : ScriptableObject
{
    [Header("던전 기본 정보")]
    [Tooltip("던전 고유 ID (예: dungeon_forest_01)")]
    public string dungeonID;

    [Tooltip("던전 이름 (예: 어둠의 숲)")]
    public string dungeonName;

    [Tooltip("던전 입구 배경 이미지")]
    public Sprite entranceSprite;

    [Tooltip("던전 내부 통로 배경 이미지 (3갈래 선택 화면)")]
    public Sprite corridorSprite;

    [Tooltip("일반 전투 배경 이미지")]
    public Sprite combatBackgroundSprite;

    [Tooltip("보스 전투 배경 이미지")]
    public Sprite bossBackgroundSprite;

    [Tooltip("이벤트 맵 배경 이미지")]
    public Sprite eventBackgroundSprite;

    [Header("몬스터 스폰 설정")]
    [Tooltip("일반 전투에서 등장 가능한 몬스터 리스트 (일반~에픽)")]
    public MonsterSpawnData[] normalMonsters;

    [Tooltip("보스 전투에서 등장 가능한 보스 리스트")]
    public MonsterSpawnData[] bossMonsters;

    [Header("이벤트 설정")]
    [Tooltip("이 던전에서 등장 가능한 이벤트 리스트")]
    public RoomEventDataSO[] possibleEvents;

    [Header("던전 난이도")]
    [Tooltip("권장 레벨")]
    public int recommendedLevel = 1;

    [Tooltip("총 방 개수 (기본값: 5)")]
    public int totalRooms = 5;
}

/// <summary>
/// 몬스터 스폰 정보
/// </summary>
[System.Serializable]
public class MonsterSpawnData
{
    [Tooltip("몬스터 스탯 데이터")]
    public MonsterStatsSO monsterStats;

    [Tooltip("몬스터 스프라이트 (전투에서 표시)")]
    public Sprite monsterSprite;

    [Tooltip("몬스터 이름")]
    public string monsterName;

    [Tooltip("몬스터 등급 (Normal, Rare, Epic, Boss)")]
    public MonsterRarity rarity;

    [Tooltip("스폰 가중치 (높을수록 자주 등장)")]
    [Range(1, 100)]
    public int spawnWeight = 10;
}

/// <summary>
/// 몬스터 등급
/// </summary>
public enum MonsterRarity
{
    Normal,  // 일반
    Rare,    // 희귀
    Epic,    // 에픽
    Boss     // 보스
}
