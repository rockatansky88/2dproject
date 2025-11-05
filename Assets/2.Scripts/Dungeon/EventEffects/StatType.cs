/// <summary>
/// 이벤트 버프/디버프가 적용될 수 있는 스탯 타입
/// RoomEventDataSO의 EventEffect에서 사용됩니다.
/// HP/MP 즉시 변경도 이 enum을 통해 처리합니다.
/// </summary>
public enum StatType
{
    None,           // 스탯 변화 없음 (GoldReward, ItemReward 전용)

    // 기본 스탯 (던전 종료까지 지속 가능)
    Strength,       // 힘 (물리 공격력, HP 증가)
    Dexterity,      // 민첩 (크리티컬 확률)
    Intelligence,   // 지능 (마법 공격력)
    Wisdom,         // 지혜 (MP 증가)
    Speed,          // 속도 (턴 순서)

    // HP/MP (즉시 적용, duration은 무시됨)
    HP,             // 현재 체력 (즉시 회복/피해)
    MP,             // 현재 마나 (즉시 회복/소모)
    MaxHP,          // 최대 체력 (던전 종료까지 지속, 매우 드물게 사용)
    MaxMP           // 최대 마나 (던전 종료까지 지속, 매우 드물게 사용)
}