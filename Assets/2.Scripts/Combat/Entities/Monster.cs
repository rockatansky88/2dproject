using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 몬스터
/// </summary>
public class Monster : MonoBehaviour, ICombatant
{
    [Header("몬스터 데이터")]
    public MonsterSpawnData spawnData;
    public MonsterStatsSO statsData;

    [Header("전투 스탯")]
    public CombatStats Stats;

    [Header("스킬")]
    public List<SkillDataSO> Skills = new List<SkillDataSO>(); // AI가 사용할 스킬

    [Header("UI 참조")]
    public Transform UIAnchor; // HP 바 위치

    // ICombatant 구현
    public string Name => spawnData?.monsterName ?? "Monster";
    public int Speed => Stats.Speed;
    public bool IsAlive => Stats.IsAlive;
    public bool IsPlayer => false;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(MonsterSpawnData data, List<SkillDataSO> skills)
    {
        spawnData = data;
        statsData = data.monsterStats.CreateRandomInstance(); // ✅ stats -> monsterStats로 수정
        Skills = skills;

        // 스탯 초기화
        Stats = new CombatStats();
        Stats.Initialize(
            statsData.Strength,
            statsData.Dexterity,
            statsData.Intelligence,
            statsData.Wisdom,
            statsData.Speed,
            baseCritChance: Random.Range(3f, 10f) // 몬스터 기본 크리티컬 3~10%
        );

        Debug.Log($"[Monster] ✅ {Name} 생성 완료 - HP: {Stats.CurrentHP}/{Stats.MaxHP}");
    }

    /// <summary>
    /// AI 행동 결정 (랜덤 스킬 선택)
    /// </summary>
    public SkillDataSO DecideAction()
    {
        // 마나가 있는 스킬 중 랜덤 선택
        List<SkillDataSO> usableSkills = Skills.FindAll(s => s.isBasicAttack || Stats.CurrentMP >= s.manaCost);

        if (usableSkills.Count == 0)
        {
            Debug.LogWarning($"[Monster] {Name} - 사용 가능한 스킬 없음, 기본 공격");
            return Skills[0]; // 기본 공격
        }

        SkillDataSO selectedSkill = usableSkills[Random.Range(0, usableSkills.Count)];
        Debug.Log($"[Monster] {Name} AI 선택: {selectedSkill.skillName}");

        return selectedSkill;
    }

    /// <summary>
    /// 스킬 사용
    /// </summary>
    public bool UseSkill(SkillDataSO skill, ICombatant target)
    {
        // 마나 소모
        if (!skill.isBasicAttack && !Stats.ConsumeMana(skill.manaCost))
        {
            return false;
        }

        // 크리티컬 판정
        bool isCritical = Stats.RollCritical();

        // 데미지 계산
        int damage = skill.CalculateDamage(Stats, isCritical);

        // 타겟에게 데미지
        target.TakeDamage(damage);

        Debug.Log($"[Monster] {Name}이(가) {skill.skillName} 사용 -> {target.Name}에게 {damage} 데미지!");

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