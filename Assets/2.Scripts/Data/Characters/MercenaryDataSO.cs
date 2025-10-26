using UnityEngine;

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

    /// <summary>
    /// 랜덤 스탯을 가진 용병 인스턴스 생성
    /// 이 메서드는 원본 SO를 수정하지 않고 새로운 런타임 인스턴스를 생성합니다.
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

        // 랜덤 스탯 생성
        instance.level = levelRange.GetRandomValue();
        instance.health = healthRange.GetRandomValue();
        instance.strength = strengthRange.GetRandomValue();
        instance.dexterity = dexterityRange.GetRandomValue();
        instance.wisdom = wisdomRange.GetRandomValue();
        instance.intelligence = intelligenceRange.GetRandomValue();
        instance.speed = speedRange.GetRandomValue();

        Debug.Log($"[MercenaryDataSO] ✅ 랜덤 용병 인스턴스 생성: {mercenaryName}\n" +
                  $"Lv.{instance.level} | HP: {instance.health} | STR: {instance.strength} | DEX: {instance.dexterity} | " +
                  $"WIS: {instance.wisdom} | INT: {instance.intelligence} | SPD: {instance.speed}");

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

    // 고용 여부 추적
    public bool isRecruited = false;
}