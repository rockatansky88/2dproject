using UnityEngine;
using System;

/// <summary>
/// ���� ���� ���� ���� Ŭ����
/// - ĳ���Ϳ� ���� ��� ���
/// - ���� ��ȭ �� �ڵ����� �Ļ� ���� ����
/// </summary>
[System.Serializable]
public class CombatStats
{
    [Header("�⺻ ����")]
    public int Strength;       // �� - ���� ���ݷ�, HP ����
    public int Dexterity;      // ��ø - ũ��Ƽ�� Ȯ��
    public int Intelligence;   // ���� - ���� ���ݷ�
    public int Wisdom;         // ���� - MP ����
    public int Speed;          // �ӵ� - �� ����

    [Header("�Ļ� ���� (�ڵ� ���)")]
    public int MaxHP;          // �ִ� HP
    public int CurrentHP;      // ���� HP
    public int MaxMP;          // �ִ� MP
    public int CurrentMP;      // ���� MP
    public float CriticalChance; // ũ��Ƽ�� Ȯ�� (%)

    // ���� ��ȭ �̺�Ʈ
    public event Action OnStatsChanged;
    public event Action<int, int> OnHPChanged;  // (���� HP, �ִ� HP)
    public event Action<int, int> OnMPChanged;  // (���� MP, �ִ� MP)

    /// <summary>
    /// �ʱ�ȭ
    /// </summary>
    public void Initialize(int str, int dex, int intel, int wis, int spd, float baseCritChance = 5f)
    {
        Strength = str;
        Dexterity = dex;
        Intelligence = intel;
        Wisdom = wis;
        Speed = spd;

        // �Ļ� ���� ���
        RecalculateDerivedStats(baseCritChance);

        // ü��/���� Ǯ ����
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        Debug.Log($"[CombatStats] ? �ʱ�ȭ �Ϸ�\n" +
                  $"STR: {Strength}, DEX: {Dexterity}, INT: {Intelligence}, WIS: {Wisdom}, SPD: {Speed}\n" +
                  $"HP: {CurrentHP}/{MaxHP}, MP: {CurrentMP}/{MaxMP}, Crit: {CriticalChance}%");
    }

    /// <summary>
    /// �Ļ� ���� ����
    /// </summary>
    public void RecalculateDerivedStats(float baseCritChance = 5f)
    {
        // HP = 100 + (STR * 5)
        // ��: STR 10 -> HP 150
        MaxHP = 100 + (Strength * 5);

        // MP = 50 + (WIS * 3)
        // ��: WIS 10 -> MP 80
        MaxMP = 50 + (Wisdom * 3);

        // ũ��Ƽ�� Ȯ�� = �⺻ Ȯ�� + (DEX * 0.5%)
        // ��: �⺻ 5% + DEX 10 -> 10%
        CriticalChance = baseCritChance + (Dexterity * 0.5f);

        Debug.Log($"[CombatStats] �Ļ� ���� ��� �Ϸ� - MaxHP: {MaxHP}, MaxMP: {MaxMP}, Crit: {CriticalChance}%");

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ������ �ޱ�
    /// </summary>
    public void TakeDamage(int damage)
    {
        int oldHP = CurrentHP;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        Debug.Log($"[CombatStats] ?? ������ {damage} ����: {oldHP} -> {CurrentHP}");

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>
    /// ȸ��
    /// </summary>
    public void Heal(int amount)
    {
        int oldHP = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);

        Debug.Log($"[CombatStats] ?? ȸ�� {amount}: {oldHP} -> {CurrentHP}");

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>
    /// ���� �Ҹ�
    /// </summary>
    public bool ConsumeMana(int amount)
    {
        if (CurrentMP < amount)
        {
            Debug.LogWarning($"[CombatStats] ? ���� ����: {CurrentMP}/{amount}");
            return false;
        }

        int oldMP = CurrentMP;
        CurrentMP -= amount;

        Debug.Log($"[CombatStats] ?? ���� �Ҹ� {amount}: {oldMP} -> {CurrentMP}");

        OnMPChanged?.Invoke(CurrentMP, MaxMP);
        return true;
    }

    /// <summary>
    /// ���� ȸ��
    /// </summary>
    public void RestoreMana(int amount)
    {
        int oldMP = CurrentMP;
        CurrentMP = Mathf.Min(MaxMP, CurrentMP + amount);

        Debug.Log($"[CombatStats] ?? ���� ȸ�� {amount}: {oldMP} -> {CurrentMP}");

        OnMPChanged?.Invoke(CurrentMP, MaxMP);
    }

    /// <summary>
    /// ũ��Ƽ�� ����
    /// </summary>
    public bool RollCritical(float bonusChance = 0f)
    {
        float totalChance = CriticalChance + bonusChance;
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool isCrit = roll < totalChance;

        Debug.Log($"[CombatStats] ?? ũ��Ƽ�� ����: {roll:F1} < {totalChance:F1}% => {(isCrit ? "����!" : "����")}");

        return isCrit;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public bool IsAlive => CurrentHP > 0;

    /// <summary>
    /// ���� ����/����� ����
    /// </summary>
    public void ApplyStatModifier(int strMod, int dexMod, int intMod, int wisMod, int spdMod)
    {
        Strength += strMod;
        Dexterity += dexMod;
        Intelligence += intMod;
        Wisdom += wisMod;
        Speed += spdMod;

        Debug.Log($"[CombatStats] ?? ���� ���� ����: STR {strMod:+0;-#}, DEX {dexMod:+0;-#}, INT {intMod:+0;-#}, WIS {wisMod:+0;-#}, SPD {spdMod:+0;-#}");

        RecalculateDerivedStats();
    }
}