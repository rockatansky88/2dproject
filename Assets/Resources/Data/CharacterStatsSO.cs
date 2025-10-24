using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Data/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Level Information")]
    public int Level = 1;
    public int Experience = 0;
    public int ExperienceRequired = 100;

    [Header("Current Stats")]
    public int Strength;
    public int Dexterity;
    public int Wisdom;
    public int Intelligence;
    public int Health;
    public int Speed;

    [Header("Random Stat Range (Min/Max)")]
    [Tooltip("���� ���� �� ����� ���� ����")]
    public StatRange strengthRange = new StatRange(8, 15);
    public StatRange dexterityRange = new StatRange(8, 15);
    public StatRange wisdomRange = new StatRange(8, 15);
    public StatRange intelligenceRange = new StatRange(8, 15);
    public StatRange healthRange = new StatRange(80, 120);
    public StatRange speedRange = new StatRange(8, 12);

    public void ResetStats()
    {
        Level = 1;
        Experience = 0;
        ExperienceRequired = 100;
        Strength = 10;
        Dexterity = 10;
        Wisdom = 10;
        Intelligence = 10;
        Health = 100;
        Speed = 10;

        Debug.Log($"[CharacterStatsSO] ���� �ʱ�ȭ �Ϸ�: {name}");
    }

    /// <summary>
    /// ���ǵ� ���� ������ ���� ���� ����
    /// </summary>
    public void RandomizeStats()
    {
        Strength = strengthRange.GetRandomValue();
        Dexterity = dexterityRange.GetRandomValue();
        Wisdom = wisdomRange.GetRandomValue();
        Intelligence = intelligenceRange.GetRandomValue();
        Health = healthRange.GetRandomValue();
        Speed = speedRange.GetRandomValue();

        Debug.Log($"[CharacterStatsSO] ���� ���� ���� �Ϸ�: {name}\n" +
                  $"STR: {Strength}, DEX: {Dexterity}, WIS: {Wisdom}, INT: {Intelligence}, HP: {Health}, SPD: {Speed}");
    }

    /// <summary>
    /// ��Ÿ�� �ν��Ͻ� ���� (���� SO�� �������� ����)
    /// </summary>
    public CharacterStatsSO CreateRandomInstance()
    {
        CharacterStatsSO instance = Instantiate(this);
        instance.RandomizeStats();

        Debug.Log($"[CharacterStatsSO] �� �ν��Ͻ� ����: {name} �� ���� ���� �����");
        return instance;
    }
}

[System.Serializable]
public struct StatRange
{
    public int min;
    public int max;

    public StatRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int GetRandomValue()
    {
        return Random.Range(min, max + 1); // max ����
    }
}