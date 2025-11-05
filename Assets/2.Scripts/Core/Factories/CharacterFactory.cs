using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 캐릭터 생성 팩토리 (레거시)
/// 현재는 CombatManager에서 직접 Character를 생성하므로 사용되지 않습니다.
/// 향후 확장 시 사용 가능하도록 보관합니다.
/// </summary>
public class CharacterFactory : MonoBehaviour
{
    [Header("Template ScriptableObjects")]
    [SerializeField] private List<CharacterStatsSO> mercenaryTemplates;
    [SerializeField] private List<MonsterStatsSO> monsterTemplates;

    [Header("Stat Variation")]
    [SerializeField, Range(0f, 0.3f)] private float statVariation = 0.1f;

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
    /// 랜덤 용병 생성 (레거시)
    /// </summary>
    public CharacterStatsSO CreateRandomMercenary()
    {
        if (mercenaryTemplates == null || mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ❌ 용병 템플릿이 없습니다!");
            return null;
        }

        CharacterStatsSO template = mercenaryTemplates[Random.Range(0, mercenaryTemplates.Count)];
        CharacterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ✅ 랜덤 용병 생성: {instance.name} (Lv.{instance.Level})");
        return instance;
    }

    /// <summary>
    /// 랜덤 몬스터 생성 (레거시)
    /// </summary>
    public MonsterStatsSO CreateRandomMonster()
    {
        if (monsterTemplates == null || monsterTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ❌ 몬스터 템플릿이 없습니다!");
            return null;
        }

        MonsterStatsSO template = monsterTemplates[Random.Range(0, monsterTemplates.Count)];
        MonsterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ✅ 랜덤 몬스터 생성: {instance.name}");
        return instance;
    }

    /// <summary>
    /// 특정 템플릿으로 랜덤 용병 생성 (레거시)
    /// </summary>
    public CharacterStatsSO CreateMercenary(CharacterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ❌ 템플릿이 null입니다!");
            return null;
        }

        CharacterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ✅ 용병 생성: {instance.name} (템플릿: {template.name})");
        return instance;
    }

    /// <summary>
    /// 특정 템플릿으로 랜덤 몬스터 생성 (레거시)
    /// </summary>
    public MonsterStatsSO CreateMonster(MonsterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ❌ 템플릿이 null입니다!");
            return null;
        }

        MonsterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ✅ 몬스터 생성: {instance.name} (템플릿: {template.name})");
        return instance;
    }

    /// <summary>
    /// 레벨에 따른 스탯 보정 (옵션)
    /// </summary>
    public void ApplyLevelScaling(CharacterStatsSO stats, int targetLevel)
    {
        if (targetLevel <= 1) return;

        float multiplier = 1f + (targetLevel - 1) * 0.1f;

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