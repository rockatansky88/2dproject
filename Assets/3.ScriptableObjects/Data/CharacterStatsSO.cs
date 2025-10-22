using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Data/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    public int Level = 1;
    public int Experience = 0;
    public int ExperienceRequired = 100;

    public int Strength;
    public int Dexterity;
    public int Wisdom;
    public int Intelligence;
    public int Health;

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
    }
}