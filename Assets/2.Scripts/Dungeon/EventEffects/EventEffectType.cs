/// <summary>
/// 이벤트 효과 타입
/// </summary>
public enum EventEffectType
{
    Buff,           // 스탯 증가 (STR, DEX, HP 회복 등)
    Debuff,         // 스탯 감소 (STR 감소, HP 피해 등)
    GoldReward,     // 골드 보상
    ItemReward      // 아이템 보상

    // 🗑️ 제거됨: Heal, Damage
    // → Buff/Debuff + StatType.HP로 대체
}