using UnityEngine;
using System;

/// <summary>
/// 전투 스탯 통합 관리 클래스
/// - 캐릭터와 몬스터 모두 사용
/// - 스탯 변화 시 자동으로 파생 스탯 재계산
/// </summary>
[System.Serializable]
public class CombatStats
{
    [Header("기본 스탯")]
    public int Strength;       // 힘 - 물리 공격력, HP 증가
    public int Dexterity;      // 민첩 - 크리티컬 확률
    public int Intelligence;   // 지능 - 마법 공격력
    public int Wisdom;         // 지혜 - MP 증가
    public int Speed;          // 속도 - 턴 순서

    [Header("파생 스탯 (자동 계산)")]
    public int MaxHP;          // 최대 HP
    public int CurrentHP;      // 현재 HP
    public int MaxMP;          // 최대 MP
    public int CurrentMP;      // 현재 MP
    public float CriticalChance; // 크리티컬 확률 (%)

    // 스탯 변화 이벤트
    public event Action OnStatsChanged;
    public event Action<int, int> OnHPChanged;  // (현재 HP, 최대 HP)
    public event Action<int, int> OnMPChanged;  // (현재 MP, 최대 MP)

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(int str, int dex, int intel, int wis, int spd, float baseCritChance = 5f)
    {
        Strength = str;
        Dexterity = dex;
        Intelligence = intel;
        Wisdom = wis;
        Speed = spd;

        // 파생 스탯 계산
        RecalculateDerivedStats(baseCritChance);

        // 체력/마나 풀 충전
        CurrentHP = MaxHP;
        CurrentMP = MaxMP;

        Debug.Log($"[CombatStats] ? 초기화 완료\n" +
                  $"STR: {Strength}, DEX: {Dexterity}, INT: {Intelligence}, WIS: {Wisdom}, SPD: {Speed}\n" +
                  $"HP: {CurrentHP}/{MaxHP}, MP: {CurrentMP}/{MaxMP}, Crit: {CriticalChance}%");
    }

    /// <summary>
    /// 파생 스탯 재계산
    /// </summary>
    public void RecalculateDerivedStats(float baseCritChance = 5f)
    {
        // HP = 100 + (STR * 5)
        // 예: STR 10 -> HP 150
        MaxHP = 100 + (Strength * 5);

        // MP = 50 + (WIS * 3)
        // 예: WIS 10 -> MP 80
        MaxMP = 50 + (Wisdom * 3);

        // 크리티컬 확률 = 기본 확률 + (DEX * 0.5%)
        // 예: 기본 5% + DEX 10 -> 10%
        CriticalChance = baseCritChance + (Dexterity * 0.5f);

        Debug.Log($"[CombatStats] 파생 스탯 계산 완료 - MaxHP: {MaxHP}, MaxMP: {MaxMP}, Crit: {CriticalChance}%");

        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(int damage)
    {
        int oldHP = CurrentHP;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        Debug.Log($"[CombatStats] ?? 데미지 {damage} 받음: {oldHP} -> {CurrentHP}");

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>
    /// 회복
    /// </summary>
    public void Heal(int amount)
    {
        int oldHP = CurrentHP;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);

        Debug.Log($"[CombatStats] ?? 회복 {amount}: {oldHP} -> {CurrentHP}");

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>
    /// 마나 소모
    /// </summary>
    public bool ConsumeMana(int amount)
    {
        if (CurrentMP < amount)
        {
            Debug.LogWarning($"[CombatStats] ? 마나 부족: {CurrentMP}/{amount}");
            return false;
        }

        int oldMP = CurrentMP;
        CurrentMP -= amount;

        Debug.Log($"[CombatStats] ?? 마나 소모 {amount}: {oldMP} -> {CurrentMP}");

        OnMPChanged?.Invoke(CurrentMP, MaxMP);
        return true;
    }

    /// <summary>
    /// 마나 회복
    /// </summary>
    public void RestoreMana(int amount)
    {
        int oldMP = CurrentMP;
        CurrentMP = Mathf.Min(MaxMP, CurrentMP + amount);

        Debug.Log($"[CombatStats] ?? 마나 회복 {amount}: {oldMP} -> {CurrentMP}");

        OnMPChanged?.Invoke(CurrentMP, MaxMP);
    }

    /// <summary>
    /// 크리티컬 판정
    /// </summary>
    public bool RollCritical(float bonusChance = 0f)
    {
        float totalChance = CriticalChance + bonusChance;
        float roll = UnityEngine.Random.Range(0f, 100f);
        bool isCrit = roll < totalChance;

        Debug.Log($"[CombatStats] ?? 크리티컬 판정: {roll:F1} < {totalChance:F1}% => {(isCrit ? "성공!" : "실패")}");

        return isCrit;
    }

    /// <summary>
    /// 생존 여부
    /// </summary>
    public bool IsAlive => CurrentHP > 0;

    /// <summary>
    /// 스탯 버프/디버프 적용
    /// </summary>
    public void ApplyStatModifier(int strMod, int dexMod, int intMod, int wisMod, int spdMod)
    {
        Strength += strMod;
        Dexterity += dexMod;
        Intelligence += intMod;
        Wisdom += wisMod;
        Speed += spdMod;

        Debug.Log($"[CombatStats] ?? 스탯 변경 적용: STR {strMod:+0;-#}, DEX {dexMod:+0;-#}, INT {intMod:+0;-#}, WIS {wisMod:+0;-#}, SPD {spdMod:+0;-#}");

        RecalculateDerivedStats();
    }
}