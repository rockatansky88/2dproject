using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 용병의 기본 템플릿 데이터
/// 실제 용병 생성 시 이 데이터를 기반으로 랜덤 스탯 인스턴스가 생성됩니다.
/// </summary>
[CreateAssetMenu(fileName = "New Mercenary", menuName = "Game/Mercenary Data")]
public class MercenaryDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string mercenaryID;
    public string mercenaryName;
    public Sprite portrait;
    public Sprite fullBodySprite;
    public GameObject prefab;

    [Header("Recruitment")]
    public int recruitCost = 100;

    [Header("Stat Ranges - 랜덤 생성 범위")]
    [Tooltip("레벨 범위")]
    public StatRange levelRange = new StatRange(1, 3);

    [Tooltip("체력 범위")]
    public StatRange healthRange = new StatRange(80, 120);

    [Tooltip("마나 범위")]
    public StatRange manaRange = new StatRange(50, 100);

    [Tooltip("힘 범위")]
    public StatRange strengthRange = new StatRange(8, 15);

    [Tooltip("민첩 범위")]
    public StatRange dexterityRange = new StatRange(8, 15);

    [Tooltip("지혜 범위")]
    public StatRange wisdomRange = new StatRange(8, 15);

    [Tooltip("지능 범위")]
    public StatRange intelligenceRange = new StatRange(8, 15);

    [Tooltip("속도 범위")]
    public StatRange speedRange = new StatRange(8, 12);

    [Header("Skills")]
    [Tooltip("이 용병이 사용할 스킬 목록 (기본 공격 포함, 최대 5개)")]
    public SkillDataSO[] availableSkills = new SkillDataSO[0];

    /// <summary>
    /// 랜덤 스탯을 가진 용병 인스턴스 생성
    /// HP/MP는 스탯 기반으로 자동 계산됩니다.
    /// </summary>
    public MercenaryInstance CreateRandomInstance()
    {
        MercenaryInstance instance = new MercenaryInstance();
        instance.sourceData = this;
        instance.mercenaryID = mercenaryID;
        instance.mercenaryName = mercenaryName;
        instance.portrait = portrait;
        instance.fullBodySprite = fullBodySprite;
        instance.prefab = prefab;
        instance.recruitCost = recruitCost;

        instance.level = levelRange.GetRandomValue();
        instance.health = healthRange.GetRandomValue();
        instance.strength = strengthRange.GetRandomValue();
        instance.dexterity = dexterityRange.GetRandomValue();
        instance.wisdom = wisdomRange.GetRandomValue();
        instance.intelligence = intelligenceRange.GetRandomValue();
        instance.speed = speedRange.GetRandomValue();

        instance.maxHP = instance.health + (instance.strength * 5);
        instance.currentHP = instance.maxHP;

        int baseMana = manaRange.GetRandomValue();
        instance.maxMP = baseMana + (instance.wisdom * 3);
        instance.currentMP = instance.maxMP;

        instance.criticalChance = 5f + (instance.dexterity * 0.5f);

        instance.skills = new List<SkillDataSO>();
        if (availableSkills != null && availableSkills.Length > 0)
        {
            instance.skills.AddRange(availableSkills);
        }
        else
        {
            Debug.LogWarning($"[MercenaryDataSO] ⚠️ {mercenaryName}에 스킬이 설정되지 않았습니다!");
        }

        Debug.Log($"[MercenaryDataSO] ✅ 랜덤 용병 인스턴스 생성: {mercenaryName}\n" +
                  $"Lv.{instance.level} | BaseHP: {instance.health} → MaxHP: {instance.maxHP} (STR+{instance.strength * 5}) | " +
                  $"MP: {instance.currentMP}/{instance.maxMP} | " +
                  $"STR: {instance.strength} | DEX: {instance.dexterity} | WIS: {instance.wisdom} | INT: {instance.intelligence} | " +
                  $"SPD: {instance.speed} | Crit: {instance.criticalChance:F1}% | 스킬: {instance.skills.Count}개");

        return instance;
    }
}

/// <summary>
/// 런타임에 생성되는 용병 인스턴스 (랜덤 스탯 적용)
/// 이벤트 버프/디버프를 관리하며, 던전이 끝날 때까지 임시 스탯이 적용됩니다.
/// </summary>
[System.Serializable]
public class MercenaryInstance
{
    public MercenaryDataSO sourceData;
    public string mercenaryID;
    public string mercenaryName;
    public Sprite portrait;
    public Sprite fullBodySprite;
    public GameObject prefab;
    public int recruitCost;

    // 🔑 기본 스탯 (버프 적용 전 원본 값)
    public int level;
    public int health;
    public int strength;
    public int dexterity;
    public int wisdom;
    public int intelligence;
    public int speed;

    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP;
    public float criticalChance;
    public List<SkillDataSO> skills = new List<SkillDataSO>();
    public bool isRecruited = false;

    // 🆕 추가: 이벤트 버프 리스트 (던전 동안 유지)
    [System.NonSerialized]
    public List<EventBuffData> activeBuffs = new List<EventBuffData>();

    /// <summary>
    /// 이벤트 버프 적용
    /// 던전 이벤트에서 획득한 버프를 용병에게 추가합니다.
    /// </summary>
    public void ApplyEventBuff(EventBuffData buff)
    {
        if (buff == null)
        {
            Debug.LogError($"[MercenaryInstance] ❌ {mercenaryName}: buff가 null입니다!");
            return;
        }

        // 중복 버프 체크 (같은 ID의 버프가 있으면 덮어쓰기)
        EventBuffData existingBuff = activeBuffs.Find(b => b.buffID == buff.buffID);
        if (existingBuff != null)
        {
            Debug.Log($"[MercenaryInstance] {mercenaryName}: 기존 버프 '{buff.buffName}' 갱신");
            activeBuffs.Remove(existingBuff);
        }

        activeBuffs.Add(buff);

        Debug.Log($"[MercenaryInstance] ✅ {mercenaryName}: 버프 '{buff.buffName}' 적용\n" +
                  $"STR {buff.strengthModifier:+0;-#}, DEX {buff.dexterityModifier:+0;-#}, INT {buff.intelligenceModifier:+0;-#}, " +
                  $"WIS {buff.wisdomModifier:+0;-#}, SPD {buff.speedModifier:+0;-#}");
    }

    /// <summary>
    /// 모든 이벤트 버프 제거 (던전 퇴장 시 호출)
    /// </summary>
    public void ClearEventBuffs()
    {
        if (activeBuffs.Count > 0)
        {
            Debug.Log($"[MercenaryInstance] {mercenaryName}: 이벤트 버프 {activeBuffs.Count}개 제거");
            activeBuffs.Clear();
        }
    }

    /// <summary>
    /// 버프가 적용된 최종 스탯 계산 (전투에서 사용)
    /// 기본 스탯 + 모든 활성 버프의 합산
    /// </summary>
    public int GetModifiedStrength()
    {
        int total = strength;
        foreach (var buff in activeBuffs)
        {
            if (buff.IsActive()) total += buff.strengthModifier;
        }
        return total;
    }

    public int GetModifiedDexterity()
    {
        int total = dexterity;
        foreach (var buff in activeBuffs)
        {
            if (buff.IsActive()) total += buff.dexterityModifier;
        }
        return total;
    }

    public int GetModifiedIntelligence()
    {
        int total = intelligence;
        foreach (var buff in activeBuffs)
        {
            if (buff.IsActive()) total += buff.intelligenceModifier;
        }
        return total;
    }

    public int GetModifiedWisdom()
    {
        int total = wisdom;
        foreach (var buff in activeBuffs)
        {
            if (buff.IsActive()) total += buff.wisdomModifier;
        }
        return total;
    }

    public int GetModifiedSpeed()
    {
        int total = speed;
        foreach (var buff in activeBuffs)
        {
            if (buff.IsActive()) total += buff.speedModifier;
        }
        return total;
    }

    /// <summary>
    /// HP 회복 (최대 HP 초과 불가)
    /// </summary>
    public void Heal(int amount)
    {
        int before = currentHP;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"[MercenaryInstance] 💚 {mercenaryName} HP 회복 +{amount}: {before} → {currentHP}/{maxHP}");
    }

    /// <summary>
    /// HP 감소 (0 이하로 내려가지 않음)
    /// </summary>
    public void TakeDamage(int damage)
    {
        int before = currentHP;
        currentHP = Mathf.Max(0, currentHP - damage);
        Debug.Log($"[MercenaryInstance] 🩸 {mercenaryName} HP 감소 -{damage}: {before} → {currentHP}/{maxHP}");
    }

    /// <summary>
    /// HP를 퍼센트로 감소 (이벤트용)
    /// </summary>
    public void TakeDamagePercent(int percent)
    {
        int damage = Mathf.RoundToInt(maxHP * (percent / 100f));
        TakeDamage(damage);
    }

    /// <summary>
    /// MP 회복 (최대 MP 초과 불가)
    /// </summary>
    public void RestoreMana(int amount)
    {
        int before = currentMP;
        currentMP = Mathf.Min(maxMP, currentMP + amount);
        Debug.Log($"[MercenaryInstance] 🔵 {mercenaryName} MP 회복 +{amount}: {before} → {currentMP}/{maxMP}");
    }

    /// <summary>
    /// MP 감소 (0 이하로 내려가지 않음)
    /// </summary>
    public void ConsumeMana(int amount)
    {
        int before = currentMP;
        currentMP = Mathf.Max(0, currentMP - amount);
        Debug.Log($"[MercenaryInstance] 💙 {mercenaryName} MP 소모 -{amount}: {before} → {currentMP}/{maxMP}");
    }

    /// <summary>
    /// 생존 여부
    /// </summary>
    public bool IsAlive()
    {
        return currentHP > 0;
    }
}