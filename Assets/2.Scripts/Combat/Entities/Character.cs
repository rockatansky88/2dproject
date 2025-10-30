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

    [Header("UI 참조")]
    public Transform UIAnchor; // HP/MP 바 위치

    // ICombatant 구현
    public string Name => mercenaryData?.mercenaryName ?? "Unknown";
    public int Speed => Stats.Speed;
    public bool IsAlive => Stats.IsAlive;
    public bool IsPlayer => true;

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

        Debug.Log($"[Character] {Name}이(가) {skill.skillName} 사용 -> {target.Name}에게 {damage} 데미지!");

        return true;
    }

    // ICombatant 구현
    public void TakeDamage(int damage)
    {
        Stats.TakeDamage(damage);
    }

    public void Heal(int amount)
    {
        Stats.Heal(amount);
    }
}