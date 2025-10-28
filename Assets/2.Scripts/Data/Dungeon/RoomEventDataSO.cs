using UnityEngine;

/// <summary>
/// 던전 이벤트 데이터
/// - 파티 전체에 영향을 주는 이벤트 (버프, 디버프, 보상, 피해 등)
/// </summary>
[CreateAssetMenu(fileName = "New Room Event", menuName = "Game/Dungeon/Room Event")]
public class RoomEventDataSO : ScriptableObject
{
    [Header("이벤트 기본 정보")]
    public string eventID;

    [Tooltip("이벤트 제목 (예: 신비한 샘물)")]
    public string eventName;

    [Tooltip("이벤트 배경 이미지")]
    public Sprite eventImage;

    [Tooltip("이벤트 설명 텍스트")]
    [TextArea(3, 10)]
    public string description;

    [Header("이벤트 효과")]
    [Tooltip("이벤트 효과 리스트 (여러 효과 동시 적용 가능)")]
    public EventEffect[] effects;
}

/// <summary>
/// 이벤트 효과 데이터
/// </summary>
[System.Serializable]
public class EventEffect
{
    [Tooltip("효과 타입")]
    public EventEffectType effectType;

    [Tooltip("효과 값 (체력 변화량, 골드 양 등)")]
    public int value;

    [Tooltip("대상 스탯 (HP, Strength, Dexterity 등)")]
    public string targetStat;

    [Tooltip("보상 아이템 (ItemReward일 때만 사용)")]
    public ItemDataSO rewardItem;

    [Tooltip("아이템 개수")]
    public int itemAmount = 1;

    [Tooltip("효과 지속 시간 (전투 수, 0 = 영구)")]
    public int duration = 0;
}