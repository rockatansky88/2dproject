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

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: UI 슬롯 참조 필드
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Header("UI 참조")]
    public Transform UIAnchor; // HP 바 위치
    public MonsterUISlot uiSlot; // 🆕 추가: 연결된 몬스터 슬롯

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

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: Initialize 메서드 - UI 이벤트 연결
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 초기화 (UI 슬롯 연결)
    /// </summary>
    public void Initialize(MonsterSpawnData data, List<SkillDataSO> skills, MonsterUISlot slot = null)
    {
        spawnData = data;
        statsData = data.monsterStats.CreateRandomInstance();
        Skills = skills;
        uiSlot = slot; // 🆕 UI 슬롯 연결

        // 스탯 초기화
        Stats = new CombatStats();
        Stats.Initialize(
            statsData.Strength,
            statsData.Dexterity,
            statsData.Intelligence,
            statsData.Wisdom,
            statsData.Speed,
            baseCritChance: Random.Range(3f, 10f)
        );

        // 🆕 추가: HP 이벤트 구독 → UI 업데이트
        if (uiSlot != null)
        {
            Stats.OnHPChanged += (currentHP, maxHP) =>
            {
                // MonsterUISlot의 HP 바 업데이트는 자체 이벤트로 처리됨
                Debug.Log($"[Monster] {Name} HP 변경 → {currentHP}/{maxHP}");
            };

            // 초기 HP UI 업데이트 (MonsterUISlot.Initialize에서 자동 처리)
        }

        Debug.Log($"[Monster] ✅ {Name} 생성 완료 - HP: {Stats.CurrentHP}/{Stats.MaxHP}, UI 연결: {(uiSlot != null ? "O" : "X")}");
    }

    /// <summary>
    /// AI 행동 결정 (랜덤 스킬 선택)
    /// </summary>
    public SkillDataSO DecideAction()
    {
        // 스킬 리스트가 비어있으면 null 반환
        if (Skills == null || Skills.Count == 0)
        {
            Debug.LogError($"[Monster] ❌ {Name}에 스킬이 없습니다!");
            return null;
        }

        // 마나가 있는 스킬 중 랜덤 선택
        List<SkillDataSO> usableSkills = Skills.FindAll(s => s.isBasicAttack || Stats.CurrentMP >= s.manaCost);

        if (usableSkills.Count == 0)
        {
            Debug.LogWarning($"[Monster] {Name} - 사용 가능한 스킬 없음, 기본 공격");

            // 기본 공격 찾기
            SkillDataSO basicAttack = Skills.Find(s => s.isBasicAttack);

            if (basicAttack != null)
            {
                return basicAttack;
            }
            else
            {
                // 기본 공격도 없으면 첫 번째 스킬 사용
                Debug.LogWarning($"[Monster] ⚠️ {Name}에 기본 공격이 없어서 첫 번째 스킬 사용");
                return Skills[0];
            }
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


        //피격 UI 표시
        if (target is Character character && character.uiSlot != null)
        {
            character.uiSlot.ShowDamage(damage, isCritical);
        }

        Debug.Log($"[Monster] {Name}이(가) {skill.skillName} 사용 -> {target.Name}에게 {damage} 데미지{(isCritical ? " (크리티컬!)" : "")}!");

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

        Debug.Log($"[Monster] {Name} 피격 - {damage} 데미지, 남은 HP: {Stats.CurrentHP}/{Stats.MaxHP}");
    }

    public void Heal(int amount)
    {
        Stats.Heal(amount);
    }

    /// <summary>
    /// 몬스터 난이도 반환 (TPE 미니게임용)
    /// </summary>
    public MonsterDifficulty GetDifficulty()
    {
        return spawnData?.difficulty ?? MonsterDifficulty.Normal;
    }
}