using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStats", menuName = "Data/Monster Stats")]
public class MonsterStatsSO : ScriptableObject
{
    public int Strength;
    public int Dexterity;
    public int Wisdom;
    public int Intelligence;
    public int Health;
    public int Speed;
}