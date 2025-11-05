using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 캐릭터 (용병)
/// MercenaryInstance의 이벤트 버프를 전투 시작 시 적용합니다.
/// </summary>
public class Character : MonoBehaviour, ICombatant
{
    [Header("캐릭터 데이터")]
    public MercenaryInstance mercenaryData;

    [Header("전투 스탯")]
    public CombatStats Stats;

    [Header("스킬")]
    public List<SkillDataSO> Skills = new List<SkillDataSO>();

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

        Stats = new CombatStats();
        Stats.Initialize(
            characterStats.Strength,
            characterStats.Dexterity,
            characterStats.Intelligence,
            characterStats.Wisdom,
            characterStats.Speed,
            characterStats.Health,
            baseCritChance: Random.Range(5f, 15f)
        );

        Debug.Log($"[Character] ✅ {Name} 초기화 완료 (CharacterStatsSO) - HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");
    }

    /// <summary>
    /// 초기화 (UI 슬롯 연결)
    /// MercenaryInstance의 HP/MP를 전투 중 실시간으로 동기화하고, 이벤트 버프를 적용합니다.
    /// </summary>
    public void Initialize(MercenaryInstance data, List<SkillDataSO> skills, MercenaryPartySlot slot = null)
    {
        Debug.Log($"[Character] ━━━ {data.mercenaryName} 초기화 시작 ━━━");

        mercenaryData = data;
        Skills = skills;
        uiSlot = slot;

        // 1. CombatStats 초기화 (MercenaryInstance에서 기본 스탯 로드)
        Stats = new CombatStats();
        Stats.InitializeFromMercenary(data);

        Debug.Log($"[Character] 1단계: 기본 스탯 로드 완료\n" +
                  $"  STR: {Stats.Strength}, DEX: {Stats.Dexterity}, INT: {Stats.Intelligence}, WIS: {Stats.Wisdom}, SPD: {Stats.Speed}\n" +
                  $"  HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}");

        // 🆕 2. 이벤트 버프 적용
        ApplyEventBuffs(data);

        // 3. HP/MP 변경 시 MercenaryInstance에 역반영
        Stats.OnHPChanged += (currentHP, maxHP) =>
        {
            if (mercenaryData != null)
            {
                mercenaryData.currentHP = currentHP;
            }

            if (uiSlot != null)
            {
                uiSlot.UpdateCombatStats(currentHP, maxHP, Stats.CurrentMP, Stats.MaxMP);
            }
        };

        Stats.OnMPChanged += (currentMP, maxMP) =>
        {
            if (mercenaryData != null)
            {
                mercenaryData.currentMP = currentMP;
            }

            if (uiSlot != null)
            {
                uiSlot.UpdateCombatStats(Stats.CurrentHP, Stats.MaxHP, currentMP, maxMP);
            }
        };

        // 4. 초기 HP/MP UI 업데이트
        if (uiSlot != null)
        {
            uiSlot.UpdateCombatStats(Stats.CurrentHP, Stats.MaxHP, Stats.CurrentMP, Stats.MaxMP);
        }

        Debug.Log($"[Character] ✅ {Name} 초기화 완료\n" +
                  $"  최종 스탯: STR {Stats.Strength}, DEX {Stats.Dexterity}, INT {Stats.Intelligence}, WIS {Stats.Wisdom}, SPD {Stats.Speed}\n" +
                  $"  HP: {Stats.CurrentHP}/{Stats.MaxHP}, MP: {Stats.CurrentMP}/{Stats.MaxMP}\n" +
                  $"  UI 연결: {(uiSlot != null ? "O" : "X")}");
    }

    /// <summary>
    /// 이벤트 버프를 CombatStats에 적용
    /// MercenaryInstance의 activeBuffs를 순회하여 스탯 보너스를 합산합니다.
    /// </summary>
    private void ApplyEventBuffs(MercenaryInstance data)
    {
        if (data.activeBuffs == null || data.activeBuffs.Count == 0)
        {
            Debug.Log($"[Character] {Name}: 활성 이벤트 버프 없음");
            return;
        }

        Debug.Log($"[Character] ━━ {Name} 이벤트 버프 적용 시작 ({data.activeBuffs.Count}개) ━━");

        int totalStr = 0, totalDex = 0, totalInt = 0, totalWis = 0, totalSpd = 0;

        foreach (var buff in data.activeBuffs)
        {
            if (!buff.IsActive())
            {
                Debug.LogWarning($"[Character] ⚠️ 버프 '{buff.buffName}'가 비활성 상태 (duration: {buff.remainingDuration})");
                continue;
            }

            totalStr += buff.strengthModifier;
            totalDex += buff.dexterityModifier;
            totalInt += buff.intelligenceModifier;
            totalWis += buff.wisdomModifier;
            totalSpd += buff.speedModifier;

            Debug.Log($"[Character] 버프 '{buff.buffName}' 적용:\n" +
                      $"  STR {buff.strengthModifier:+0;-#}, DEX {buff.dexterityModifier:+0;-#}, INT {buff.intelligenceModifier:+0;-#}, " +
                      $"WIS {buff.wisdomModifier:+0;-#}, SPD {buff.speedModifier:+0;-#}");
        }

        // 합산된 버프를 CombatStats에 적용
        if (totalStr != 0 || totalDex != 0 || totalInt != 0 || totalWis != 0 || totalSpd != 0)
        {
            Stats.ApplyStatModifier(totalStr, totalDex, totalInt, totalWis, totalSpd);

            Debug.Log($"[Character] ✅ {Name} 이벤트 버프 적용 완료:\n" +
                      $"  총 보너스: STR {totalStr:+0;-#}, DEX {totalDex:+0;-#}, INT {totalInt:+0;-#}, WIS {totalWis:+0;-#}, SPD {totalSpd:+0;-#}\n" +
                      $"  최종 스탯: STR {Stats.Strength}, DEX {Stats.Dexterity}, INT {Stats.Intelligence}, WIS {Stats.Wisdom}, SPD {Stats.Speed}");
        }
        else
        {
            Debug.Log($"[Character] {Name}: 버프 보너스 없음 (모든 값 0)");
        }
    }

    /// <summary>
    /// 스킬 사용
    /// </summary>
    public bool UseSkill(SkillDataSO skill, ICombatant target, bool isCritical)
    {
        if (!skill.isBasicAttack && !Stats.ConsumeMana(skill.manaCost))
        {
            Debug.LogWarning($"[Character] {Name} - 마나 부족으로 {skill.skillName} 사용 불가");
            return false;
        }

        int damage = skill.CalculateDamage(Stats, isCritical);
        target.TakeDamage(damage);

        if (target is Monster monster && monster.uiSlot != null)
        {
            monster.uiSlot.ShowDamage(damage, isCritical);
        }

        Debug.Log($"[Character] {Name}이(가) {skill.skillName} 사용 -> {target.Name}에게 {damage} 데미지{(isCritical ? " (크리티컬!)" : "")}!");

        return true;
    }

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