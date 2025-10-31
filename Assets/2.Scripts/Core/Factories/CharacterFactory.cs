using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 용병과 몬스터를 랜덤 스탯으로 생성하는 팩토리
/// </summary>
public class CharacterFactory : MonoBehaviour
{
    [Header("Template ScriptableObjects")]
    [SerializeField] private List<CharacterStatsSO> mercenaryTemplates;
    [SerializeField] private List<MonsterStatsSO> monsterTemplates;

    [Header("Stat Variation")]
    [SerializeField, Range(0f, 0.3f)] private float statVariation = 0.1f; // ±10% 변동

    private static CharacterFactory instance;
    public static CharacterFactory Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 랜덤 용병 생성
    /// </summary>
    public CharacterStatsSO CreateRandomMercenary()
    {
        if (mercenaryTemplates == null || mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ? 용병 템플릿이 없습니다!");
            return null;
        }

        CharacterStatsSO template = mercenaryTemplates[Random.Range(0, mercenaryTemplates.Count)];
        CharacterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ? 랜덤 용병 생성: {instance.name} (Lv.{instance.Level})");
        return instance;
    }

    /// <summary>
    /// 랜덤 몬스터 생성
    /// </summary>
    public MonsterStatsSO CreateRandomMonster()
    {
        if (monsterTemplates == null || monsterTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ? 몬스터 템플릿이 없습니다!");
            return null;
        }

        MonsterStatsSO template = monsterTemplates[Random.Range(0, monsterTemplates.Count)];
        MonsterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ? 랜덤 몬스터 생성: {instance.name}");
        return instance;
    }

    /// <summary>
    /// 특정 템플릿으로 랜덤 용병 생성
    /// </summary>
    public CharacterStatsSO CreateMercenary(CharacterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ? 템플릿이 null입니다!");
            return null;
        }

        CharacterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ? 용병 생성: {instance.name} (템플릿: {template.name})");
        return instance;
    }

    /// <summary>
    /// 특정 템플릿으로 랜덤 몬스터 생성
    /// </summary>
    public MonsterStatsSO CreateMonster(MonsterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ? 템플릿이 null입니다!");
            return null;
        }

        MonsterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ? 몬스터 생성: {instance.name} (템플릿: {template.name})");
        return instance;
    }

    /// <summary>
    /// 전투용 Character 오브젝트 생성
    /// </summary>
    public static Character CreateCharacter(MercenaryInstance mercenaryData, Transform parent)
    {
        if (mercenaryData == null)
        {
            Debug.LogError("[CharacterFactory] ❌ mercenaryData가 null입니다!");
            return null;
        }

        Debug.Log($"[CharacterFactory] Character 생성 시작: {mercenaryData.mercenaryName}");

        // GameObject 생성
        GameObject charObj = new GameObject($"Character_{mercenaryData.mercenaryName}");
        charObj.transform.SetParent(parent);
        charObj.transform.localPosition = Vector3.zero;

        // Character 컴포넌트 추가
        Character character = charObj.AddComponent<Character>();

        // MercenaryInstance → Character 초기화
        CharacterStatsSO characterStats = ScriptableObject.CreateInstance<CharacterStatsSO>();
        characterStats.Strength = mercenaryData.strength;
        characterStats.Dexterity = mercenaryData.dexterity;
        characterStats.Wisdom = mercenaryData.wisdom;
        characterStats.Intelligence = mercenaryData.intelligence;
        characterStats.Health = mercenaryData.health;
        characterStats.Speed = mercenaryData.speed;
        characterStats.Level = mercenaryData.level;

        // 스킬 로드
        List<SkillDataSO> skills = LoadMercenarySkills(mercenaryData);

        // Character 초기화
        character.Initialize(characterStats, skills);

        Debug.Log($"[CharacterFactory] ✅ {mercenaryData.mercenaryName} 생성 완료");

        return character;
    }

    /// <summary>
    /// 용병 스킬 로드
    /// </summary>
    private static List<SkillDataSO> LoadMercenarySkills(MercenaryInstance mercenary)
    {
        List<SkillDataSO> skills = new List<SkillDataSO>();

        // TODO: Resources 폴더에서 스킬 로드
        // 임시로 기본 공격 스킬만 추가
        SkillDataSO basicAttack = ScriptableObject.CreateInstance<SkillDataSO>();
        basicAttack.skillName = "기본 공격";
        basicAttack.baseDamageMin = 5;      // ✅ 수정
        basicAttack.baseDamageMax = 10;     // ✅ 수정
        basicAttack.manaCost = 0;
        basicAttack.isBasicAttack = true;
        basicAttack.damageType = SkillDamageType.Physical;
        basicAttack.targetType = SkillTargetType.Single;
        basicAttack.statScaling = 0.5f;

        skills.Add(basicAttack);

        Debug.Log($"[CharacterFactory] {mercenary.mercenaryName} 스킬 로드: {skills.Count}개");

        return skills;
    }

    /// <summary>
    /// 레벨에 따른 스탯 보정 (옵션)
    /// </summary>
    public void ApplyLevelScaling(CharacterStatsSO stats, int targetLevel)
    {
        if (targetLevel <= 1) return;

        float multiplier = 1f + (targetLevel - 1) * 0.1f; // 레벨당 10% 증가

        stats.Strength = Mathf.RoundToInt(stats.Strength * multiplier);
        stats.Dexterity = Mathf.RoundToInt(stats.Dexterity * multiplier);
        stats.Wisdom = Mathf.RoundToInt(stats.Wisdom * multiplier);
        stats.Intelligence = Mathf.RoundToInt(stats.Intelligence * multiplier);
        stats.Health = Mathf.RoundToInt(stats.Health * multiplier);
        stats.Speed = Mathf.RoundToInt(stats.Speed * multiplier);
        stats.Level = targetLevel;

        Debug.Log($"[CharacterFactory] 레벨 스케일링 적용: Lv.{targetLevel} (배율: {multiplier:F2}x)");
    }
}