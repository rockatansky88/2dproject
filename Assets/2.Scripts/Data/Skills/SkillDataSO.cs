using UnityEngine;

/// <summary>
/// 스킬 타입 - 물리 / 마법
/// </summary>
public enum SkillDamageType
{
	Physical,  // 물리 - STR 기반
	Magical    // 마법 - INT 기반
}

/// <summary>
/// 타겟 타입 - 단일 / 다중
/// </summary>
public enum SkillTargetType
{
	Single,    // 단일 대상
	All        // 전체 대상
}

/// <summary>
/// 스킬 데이터 ScriptableObject
/// - 기본 공격 스킬 + 특수 스킬 구현
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

	/// <summary>
	/// 최종 데미지 계산
	/// </summary>
	/// <param name="attackerStats">공격자 스탯</param>
	/// <param name="isCritical">크리티컬 여부</param>
	/// <returns>최종 데미지</returns>
	public int CalculateDamage(CombatStats attackerStats, bool isCritical)
	{
		// 기본 데미지 랜덤 계산
		int baseDamage = Random.Range(baseDamageMin, baseDamageMax + 1);

		// 스탯 기반 추가 데미지
		int statBonus = 0;
		if (damageType == SkillDamageType.Physical)
		{
			// 물리 공격 = STR 기반
			statBonus = Mathf.RoundToInt(attackerStats.Strength * statScaling);
		}
		else if (damageType == SkillDamageType.Magical)
		{
			// 마법 공격 = INT 기반
			statBonus = Mathf.RoundToInt(attackerStats.Intelligence * statScaling);
		}

		int totalDamage = baseDamage + statBonus;

		// 크리티컬 적용 (1.5배)
		if (isCritical)
		{
			totalDamage = Mathf.RoundToInt(totalDamage * 1.5f);
		}

		return totalDamage;
	}
}