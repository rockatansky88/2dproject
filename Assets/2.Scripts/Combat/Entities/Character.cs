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
    public List<SkillDataSO> Skills = new List<SkillDataSO>();

    // ICombatant 구현
    public string Name => mercenaryData?.mercenaryName ?? "Unknown";
    public int Speed => Stats.Speed;
    public bool IsAlive => Stats.IsAlive;
    public bool IsPlayer => true;

    [Header("UI 참조")]
    public Transform UIAnchor;
    public MercenaryPartySlot uiSlot;

    /// <summary>
    /// 초기화 (MercenaryInstance 사용 - 권장)
    /// </summary>
    public void Initialize(MercenaryInstance data, List<SkillDataSO> skills)
    {
        mercenaryData = data;
        Skills = skills;

        // 스탯 초기화
        Stats = new CombatStats();


        Stats.InitializeFromMercenary(data);

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");
    }

    /// <summary>
    /// 초기화 (CharacterStatsSO 사용 - 레거시)
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
            speed = characterStats.Speed,
            level = characterStats.Level
        };

        Skills = skills;

        // 스탯 초기화 (레거시 방법 - 실시간 계산)
        Stats = new CombatStats();

        Stats.Initialize(
            characterStats.Strength,
            characterStats.Dexterity,
            characterStats.Intelligence,
            characterStats.Wisdom,
            characterStats.Speed,
            characterStats.Health, // ← baseHealth 전달
            baseCritChance: Random.Range(5f, 15f)
        );

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 (CharacterStatsSO) - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");
    }

    /// <summary>
    /// 초기화 (UI 슬롯 연결)
    /// MercenaryInstance의 HP/MP를 전투 중 실시간으로 동기화합니다.
    /// </summary>
    public void Initialize(MercenaryInstance data, List<SkillDataSO> skills, MercenaryPartySlot slot = null)
    {
        mercenaryData = data;
        Skills = skills;
        uiSlot = slot;

        // 스탯 초기화
        Stats = new CombatStats();

        Stats.InitializeFromMercenary(data);

        // HP/MP 변경 시 MercenaryInstance에 역반영
        // 전투 중 HP/MP 변화가 원본 데이터에도 저장되어
        // 전투 종료 후에도 유지됩니다.
        Stats.OnHPChanged += (currentHP, maxHP) =>
        {
            // MercenaryInstance 업데이트
            if (mercenaryData != null)
            {
                mercenaryData.currentHP = currentHP;
                Debug.Log($"[Character] {Name} HP 변경 → MercenaryInstance 업데이트: {currentHP}/{maxHP}");
            }

            // UI 업데이트
            if (uiSlot != null)
            {
                uiSlot.UpdateCombatStats(currentHP, maxHP, Stats.CurrentMP, Stats.MaxMP);
            }
        };

        Stats.OnMPChanged += (currentMP, maxMP) =>
        {
            // MercenaryInstance 업데이트
            if (mercenaryData != null)
            {
                mercenaryData.currentMP = currentMP;
                Debug.Log($"[Character] {Name} MP 변경 → MercenaryInstance 업데이트: {currentMP}/{maxMP}");
            }

            // UI 업데이트
            if (uiSlot != null)
            {
                uiSlot.UpdateCombatStats(Stats.CurrentHP, Stats.MaxHP, currentMP, maxMP);
            }
        };

        // 초기 HP/MP UI 업데이트
        if (uiSlot != null)
        {
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