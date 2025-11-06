using UnityEngine;

public enum SkillDamageType
{
    Physical,
    Magical
}

public enum SkillTargetType
{
    Single,
    All
}

/// <summary>
/// 스킬 데이터 ScriptableObject
/// 기본 공격 스킬 + 특수 스킬 구현
/// 스킬별 사운드 효과 및 스프라이트 애니메이션 포함
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Combat/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("스킬 고유 ID")]
    public string skillID;

    [Tooltip("스킬 이름 (예: 파이어볼, 이중 타격)")]
    public string skillName;

    [Tooltip("스킬 아이콘")]
    public Sprite skillIcon;

    [Tooltip("스킬 설명")]
    [TextArea(2, 4)]
    public string description;

    [Header("스킬 타입")]
    [Tooltip("데미지 타입 (Physical=STR, Magical=INT)")]
    public SkillDamageType damageType;

    [Tooltip("타겟 타입 (Single=단일, All=전체)")]
    public SkillTargetType targetType;

    [Header("데미지 정보")]
    [Tooltip("기본 데미지 최소값")]
    public int baseDamageMin = 5;

    [Tooltip("기본 데미지 최대값")]
    public int baseDamageMax = 10;

    [Tooltip("스탯 계수 (STR 또는 INT의 몇 % 적용) - 예: 0.5 = 50%")]
    [Range(0f, 2f)]
    public float statScaling = 0.5f;

    [Header("마나 소모")]
    [Tooltip("마나 소모량 (0 = 기본 공격)")]
    public int manaCost = 0;

    [Header("기본 공격 여부")]
    [Tooltip("기본 공격인지 여부 (true면 마나 소모 없음)")]
    public bool isBasicAttack = false;

    [Header("사운드 효과")]
    [Tooltip("스킬 사용 시 재생될 효과음")]
    public AudioClip skillSound;

    [Header("스프라이트 애니메이션")] // ✅ 추가
    [Tooltip("스킬 이펙트 스프라이트 시퀀스 (타겟 위치에 재생)")]
    public Sprite[] effectSprites;

    [Tooltip("이펙트 프레임 속도 (초)")]
    public float effectFrameRate = 0.1f;

    [Tooltip("이펙트 스프라이트 크기 (기본: 1)")]
    public float effectScale = 1f;

    /// <summary>
    /// 최종 데미지 계산
    /// </summary>
    public int CalculateDamage(CombatStats attackerStats, bool isCritical)
    {
        int baseDamage = Random.Range(baseDamageMin, baseDamageMax + 1);

        int statBonus = 0;
        if (damageType == SkillDamageType.Physical)
        {
            statBonus = Mathf.RoundToInt(attackerStats.Strength * statScaling);
        }
        else if (damageType == SkillDamageType.Magical)
        {
            statBonus = Mathf.RoundToInt(attackerStats.Intelligence * statScaling);
        }

        int totalDamage = baseDamage + statBonus;

        if (isCritical)
        {
            totalDamage = Mathf.RoundToInt(totalDamage * 1.5f);
        }

        return totalDamage;
    }
}