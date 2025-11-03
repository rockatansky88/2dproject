using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 캐릭터 (용병)
/// </summary>
public class Character : MonoBehaviour, ICombatant
{
    [Header("캐릭터 데이터")]
    public MercenaryInstance mercenaryData;

    [Header("전투 스탯")]
    public CombatStats Stats;

    [Header("스킬")]
    public List<SkillDataSO> Skills = new List<SkillDataSO>(); // 기본 공격 + 스킬 4개

    // ICombatant 구현
    public string Name => mercenaryData?.mercenaryName ?? "Unknown";
    public int Speed => Stats.Speed;
    public bool IsAlive => Stats.IsAlive;
    public bool IsPlayer => true;

    [Header("UI 참조")]
    public Transform UIAnchor; // HP/MP 바 위치
    public MercenaryPartySlot uiSlot; // 🆕 추가: 연결된 파티 슬롯

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(MercenaryInstance data, List<SkillDataSO> skills)
    {
        mercenaryData = data;
        Skills = skills;

        // 스탯 초기화
        Stats = new CombatStats();
        Stats.Initialize(
            data.strength,
            data.dexterity,
            data.intelligence,
            data.wisdom,
            data.speed,
            baseCritChance: Random.Range(5f, 15f) // 기본 크리티컬 5~15%
        );

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");
    }

    /// <summary>
    /// 초기화 (CharacterStatsSO 사용)
    /// </summary>
    public void Initialize(CharacterStatsSO characterStats, List<SkillDataSO> skills)
    {
        if (characterStats == null)
        {
            Debug.LogError("[Character] ❌ characterStats가 null입니다!");
            return;
        }

        // 임시 MercenaryInstance 생성
        mercenaryData = new MercenaryInstance
        {
            mercenaryName = "전투용_캐릭터",
            strength = characterStats.Strength,
            dexterity = characterStats.Dexterity,
            intelligence = characterStats.Intelligence,
            wisdom = characterStats.Wisdom,
            health = characterStats.Health,
            speed = characterStats.Speed,
            level = characterStats.Level
        };

        Skills = skills;

        // 스탯 초기화
        Stats = new CombatStats();
        Stats.Initialize(
            characterStats.Strength,
            characterStats.Dexterity,
            characterStats.Intelligence,
            characterStats.Wisdom,
            characterStats.Speed,
            baseCritChance: Random.Range(5f, 15f)
        );

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 (CharacterStatsSO) - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");
    }

    /// <summary>
    /// 초기화 (UI 슬롯 연결)
    /// </summary>
    public void Initialize(MercenaryInstance data, List<SkillDataSO> skills, MercenaryPartySlot slot = null)
    {
        mercenaryData = data;
        Skills = skills;
        uiSlot = slot; // 🆕 UI 슬롯 연결

        // 스탯 초기화
        Stats = new CombatStats();
        Stats.Initialize(
            data.strength,
            data.dexterity,
            data.intelligence,
            data.wisdom,
            data.speed,
            baseCritChance: Random.Range(5f, 15f)
        );

        // 🆕 추가: HP/MP 이벤트 구독 → UI 업데이트
        if (uiSlot != null)
        {
            Stats.OnHPChanged += (currentHP, maxHP) =>
            {
                uiSlot.UpdateCombatStats(currentHP, maxHP, Stats.CurrentMP, Stats.MaxMP);
                Debug.Log($"[Character] {Name} HP 변경 → UI 업데이트: {currentHP}/{maxHP}");
            };

            Stats.OnMPChanged += (currentMP, maxMP) =>
            {
                uiSlot.UpdateCombatStats(Stats.CurrentHP, Stats.MaxHP, currentMP, maxMP);
                Debug.Log($"[Character] {Name} MP 변경 → UI 업데이트: {currentMP}/{maxMP}");
            };

            // 초기 HP/MP UI 업데이트
            uiSlot.UpdateCombatStats(Stats.CurrentHP, Stats.MaxHP, Stats.CurrentMP, Stats.MaxMP);
        }

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}, UI 연결: {(uiSlot != null ? "O" : "X")}");
    }

    /// <summary>
    /// 스킬 사용
    /// </summary>
    public bool UseSkill(SkillDataSO skill, ICombatant target, bool isCritical)
    {
        // 마나 체크
        if (!skill.isBasicAttack && !Stats.ConsumeMana(skill.manaCost))
        {
            Debug.LogWarning($"[Character] {Name} - 마나 부족으로 {skill.skillName} 사용 불가");
            return false;
        }

        // 데미지 계산
        int damage = skill.CalculateDamage(Stats, isCritical);

        // 타겟에게 데미지
        target.TakeDamage(damage);

        if (target is Monster monster && monster.uiSlot != null)
        {
            monster.uiSlot.ShowDamage(damage, isCritical);
        }

        Debug.Log($"[Character] {Name}이(가) {skill.skillName} 사용 -> {target.Name}에게 {damage} 데미지{(isCritical ? " (크리티컬!)" : "")}!");

        return true;
    }


    // ICombatant 구현
    public void TakeDamage(int damage)
    {
        Stats.TakeDamage(damage);

        // 🆕 추가: 피격 UI 표시 (크리티컬 판정은 공격자가 결정하므로 false)
        if (uiSlot != null)
        {
            uiSlot.ShowDamage(damage, isCritical: false);
        }

        Debug.Log($"[Character] {Name} 피격 - {damage} 데미지, 남은 HP: {Stats.CurrentHP}/{Stats.MaxHP}");
    }

    public void Heal(int amount)
    {
        Stats.Heal(amount);
    }
}