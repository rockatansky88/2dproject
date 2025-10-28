using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStats", menuName = "Data/Monster Stats")]
public class MonsterStatsSO : ScriptableObject
{
    [Header("Current Stats")]
    public int Strength;
    public int Dexterity;
    public int Wisdom;
    public int Intelligence;
    public int Health;
    public int Speed;

    [Header("Random Stat Range (Min/Max)")]
    [Tooltip("���� ���� �� ����� ���� ����")]
    public StatRange strengthRange = new StatRange(5, 20);
    public StatRange dexterityRange = new StatRange(5, 20);
    public StatRange wisdomRange = new StatRange(5, 15);
    public StatRange intelligenceRange = new StatRange(5, 15);
    public StatRange healthRange = new StatRange(50, 150);
    public StatRange speedRange = new StatRange(5, 15);

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

        Debug.Log($"[MonsterStatsSO] ���� ���� ����: {name}\n" +
                  $"STR: {Strength}, DEX: {Dexterity}, WIS: {Wisdom}, INT: {Intelligence}, HP: {Health}, SPD: {Speed}");
    }

    /// <summary>
    /// ��Ÿ�� �ν��Ͻ� ���� (���� SO�� �������� ����)
    /// </summary>
    public MonsterStatsSO CreateRandomInstance()
    {
        MonsterStatsSO instance = Instantiate(this);
        instance.RandomizeStats();

        Debug.Log($"[MonsterStatsSO] �� ���� �ν��Ͻ� ����: {name}");
        return instance;
    }
}