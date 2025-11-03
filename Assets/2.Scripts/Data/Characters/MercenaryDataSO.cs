using UnityEngine;
using System.Collections.Generic; // 스킬 배열 생성을 위한 네임스페이스 추가

/// <summary>
/// 용병의 기본 템플릿 데이터
/// 실제 용병 생성 시 이 데이터를 기반으로 랜덤 스탯 인스턴스가 생성됩니다.
/// </summary>
[CreateAssetMenu(fileName = "New Mercenary", menuName = "Game/Mercenary Data")]
public class MercenaryDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string mercenaryID;           // 고유 ID (예: "warrior_001")
    public string mercenaryName;         // 이름 (예: "전사 로렌")
    public Sprite portrait;              // 초상화
    public Sprite fullBodySprite;        // 전신 이미지 (상세 패널용)
    public GameObject prefab;            // 전투용 프리팹

    [Header("Recruitment")]
    public int recruitCost = 100;        // 고용 비용

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

    // 🆕 추가: 스킬 정보
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

        // 랜덤 기본 스탯 생성
        instance.level = levelRange.GetRandomValue();
        instance.health = healthRange.GetRandomValue(); // 🔑 기본 체력 (80~120)
        instance.strength = strengthRange.GetRandomValue();
        instance.dexterity = dexterityRange.GetRandomValue();
        instance.wisdom = wisdomRange.GetRandomValue();
        instance.intelligence = intelligenceRange.GetRandomValue();
        instance.speed = speedRange.GetRandomValue();

        // HP 계산: MaxHP = 기본 체력 + (STR * 5)
        // 예: health 100 + STR 10 → MaxHP 150
        instance.maxHP = instance.health + (instance.strength * 5);
        instance.currentHP = instance.maxHP; // 초기에는 풀 HP

        // MP 계산: MaxMP = 기본 마나 + (WIS * 3)
        int baseMana = manaRange.GetRandomValue(); // 기본 마나 (50~100)
        instance.maxMP = baseMana + (instance.wisdom * 3);
        instance.currentMP = instance.maxMP; // 초기에는 풀 MP

        // 크리티컬 확률 계산 (기본 5% + DEX * 0.5%)
        instance.criticalChance = 5f + (instance.dexterity * 0.5f);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        // 스킬 복사
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
/// 이 클래스는 실제 게임에서 사용되는 용병 데이터입니다.
/// </summary>
[System.Serializable]
public class MercenaryInstance
{
    // 원본 데이터 참조
    public MercenaryDataSO sourceData;

    // 기본 정보
    public string mercenaryID;
    public string mercenaryName;
    public Sprite portrait;
    public Sprite fullBodySprite;
    public GameObject prefab;
    public int recruitCost;

    // 랜덤 생성된 스탯
    public int level;
    public int health;
    public int strength;
    public int dexterity;
    public int wisdom;
    public int intelligence;
    public int speed;

    // HP와 MP
    public int maxHP;
    public int currentHP;
    public int maxMP;
    public int currentMP; // 전투 중 소모되는 현재 마나

    // 크리티컬 확률
    public float criticalChance;

    // 스킬
    public List<SkillDataSO> skills = new List<SkillDataSO>();

    // 고용 여부 추적
    public bool isRecruited = false;
}