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
	[Tooltip("랜덤 생성 시 사용할 스탯 범위")]
	public StatRange strengthRange = new StatRange(5, 20);
	public StatRange dexterityRange = new StatRange(5, 20);
	public StatRange wisdomRange = new StatRange(5, 15);
	public StatRange intelligenceRange = new StatRange(5, 15);
	public StatRange healthRange = new StatRange(50, 150);
	public StatRange speedRange = new StatRange(5, 15);

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

		Debug.Log($"[MonsterStatsSO] 랜덤 스탯 생성: {name}\n" +
				  $"STR: {Strength}, DEX: {Dexterity}, WIS: {Wisdom}, INT: {Intelligence}, HP: {Health}, SPD: {Speed}");
	}

	/// <summary>
	/// 런타임 인스턴스 생성 (원본 SO는 수정하지 않음)
	/// </summary>
	public MonsterStatsSO CreateRandomInstance()
	{
		MonsterStatsSO instance = Instantiate(this);
		instance.RandomizeStats();

		return instance;
	}
}