using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �뺴�� ���͸� ���� �������� �����ϴ� ���丮
/// </summary>
public class CharacterFactory : MonoBehaviour
{
    [Header("Template ScriptableObjects")]
    [SerializeField] private List<CharacterStatsSO> mercenaryTemplates;
    [SerializeField] private List<MonsterStatsSO> monsterTemplates;

    [Header("Stat Variation")]
    [SerializeField, Range(0f, 0.3f)] private float statVariation = 0.1f; // ��10% ����

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
    /// ���� �뺴 ����
    /// </summary>
    public CharacterStatsSO CreateRandomMercenary()
    {
        if (mercenaryTemplates == null || mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ? �뺴 ���ø��� �����ϴ�!");
            return null;
        }

        CharacterStatsSO template = mercenaryTemplates[Random.Range(0, mercenaryTemplates.Count)];
        CharacterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ? ���� �뺴 ����: {instance.name} (Lv.{instance.Level})");
        return instance;
    }

    /// <summary>
    /// ���� ���� ����
    /// </summary>
    public MonsterStatsSO CreateRandomMonster()
    {
        if (monsterTemplates == null || monsterTemplates.Count == 0)
        {
            Debug.LogError("[CharacterFactory] ? ���� ���ø��� �����ϴ�!");
            return null;
        }

        MonsterStatsSO template = monsterTemplates[Random.Range(0, monsterTemplates.Count)];
        MonsterStatsSO instance = template.CreateRandomInstance();

        Debug.Log($"[CharacterFactory] ? ���� ���� ����: {instance.name}");
        return instance;
    }

    /// <summary>
    /// Ư�� ���ø����� ���� �뺴 ����
    /// </summary>
    public CharacterStatsSO CreateMercenary(CharacterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ? ���ø��� null�Դϴ�!");
            return null;
        }

        CharacterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ? �뺴 ����: {instance.name} (���ø�: {template.name})");
        return instance;
    }

    /// <summary>
    /// Ư�� ���ø����� ���� ���� ����
    /// </summary>
    public MonsterStatsSO CreateMonster(MonsterStatsSO template)
    {
        if (template == null)
        {
            Debug.LogError("[CharacterFactory] ? ���ø��� null�Դϴ�!");
            return null;
        }

        MonsterStatsSO instance = template.CreateRandomInstance();
        Debug.Log($"[CharacterFactory] ? ���� ����: {instance.name} (���ø�: {template.name})");
        return instance;
    }

    /// <summary>
    /// ������ ���� ���� ���� (�ɼ�)
    /// </summary>
    public void ApplyLevelScaling(CharacterStatsSO stats, int targetLevel)
    {
        if (targetLevel <= 1) return;

        float multiplier = 1f + (targetLevel - 1) * 0.1f; // ������ 10% ����

        stats.Strength = Mathf.RoundToInt(stats.Strength * multiplier);
        stats.Dexterity = Mathf.RoundToInt(stats.Dexterity * multiplier);
        stats.Wisdom = Mathf.RoundToInt(stats.Wisdom * multiplier);
        stats.Intelligence = Mathf.RoundToInt(stats.Intelligence * multiplier);
        stats.Health = Mathf.RoundToInt(stats.Health * multiplier);
        stats.Speed = Mathf.RoundToInt(stats.Speed * multiplier);
        stats.Level = targetLevel;

        Debug.Log($"[CharacterFactory] ���� �����ϸ� ����: Lv.{targetLevel} (����: {multiplier:F2}x)");
    }
}