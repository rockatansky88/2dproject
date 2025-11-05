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
	[Tooltip("랜덤 생성 시 사용할 스탯 범위")]
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

	}

	/// <summary>
	/// 정의된 범위 내에서 랜덤 스탯 생성
	/// </summary>
	public void RandomizeStats()
	{
		Strength = strengthRange.GetRandomValue();
		Dexterity = dexterityRange.GetRandomValue();
		Wisdom = wisdomRange.GetRandomValue();
		Intelligence = intelligenceRange.GetRandomValue();
		Health = healthRange.GetRandomValue();
		Speed = speedRange.GetRandomValue();

		Debug.Log($"[CharacterStatsSO] 랜덤 스탯 생성 완료: {name}\n" +
				  $"STR: {Strength}, DEX: {Dexterity}, WIS: {Wisdom}, INT: {Intelligence}, HP: {Health}, SPD: {Speed}");
	}

	/// <summary>
	/// 런타임 인스턴스 생성 (원본 SO는 수정하지 않음)
	/// </summary>
	public CharacterStatsSO CreateRandomInstance()
	{
		CharacterStatsSO instance = Instantiate(this);
		instance.RandomizeStats();

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
		return Random.Range(min, max + 1); // max 포함
	}
}