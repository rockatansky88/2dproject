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
    /// 전투용 Character 오브젝트 생성 (사용 안 함 - CombatManager에서 직접 생성)
    /// </summary>
    /*public static Character CreateCharacter(MercenaryInstance mercenaryData, Transform parent)
    {
        // 사용하지 않음
        Debug.LogWarning("[CharacterFactory] CreateCharacter는 더 이상 사용하지 않습니다!");
        return null;
    }*/
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