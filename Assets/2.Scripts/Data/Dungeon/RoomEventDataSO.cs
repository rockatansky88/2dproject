using UnityEngine;

/// <summary>
/// 던전 이벤트 데이터
/// 파티 전체에 영향을 주는 이벤트 (버프, 디버프, 보상 등)
/// </summary>
[CreateAssetMenu(fileName = "New Room Event", menuName = "Game/Dungeon/Room Event")]
public class RoomEventDataSO : ScriptableObject
{
    [Header("이벤트 기본 정보")]
    public string eventID;

    [Tooltip("이벤트 이름 (예: 신비한 샘물)")]
    public string eventName;

    [Tooltip("이벤트 일러스트 이미지")]
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
/// Inspector에서 설정 가능한 개별 효과 단위입니다.
/// 
/// 사용 예시:
/// - HP 회복: Buff, HP, +50
/// - HP 피해: Debuff, HP, 30 (음수 자동 변환)
/// - STR 버프: Buff, Strength, +10, duration 0 (던전 종료까지)
/// - DEX 디버프: Debuff, Dexterity, 5, duration 3 (3턴간)
/// - 골드 보상: GoldReward, None, 200
/// </summary>
[System.Serializable]
public class EventEffect
{
    [Tooltip("효과 타입 (Buff: 증가, Debuff: 감소, GoldReward/ItemReward: 보상)")]
    public EventEffectType effectType;

    [Tooltip("대상 스탯 (HP, MP, Strength 등)\n" +
             "GoldReward/ItemReward는 None 선택")]
    public StatType targetStat = StatType.None;

    [Tooltip("효과 값 (증가/감소량, 골드 양 등)\n" +
             "Debuff는 자동으로 음수 처리됩니다.")]
    public int value;

    [Tooltip("보상 아이템 (ItemReward일 경우 사용)")]
    public ItemDataSO rewardItem;

    [Tooltip("아이템 개수")]
    public int itemAmount = 1;

    [Tooltip("효과 지속 시간 (턴 수)\n" +
             "0 = 던전 종료까지\n" +
             "HP/MP 즉시 효과는 duration 무시")]
    public int duration = 0;
}