using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    public CharacterStatsSO characterStats;
    public IntEventChannelSO onExperienceGained;
    public LevelUpEventChannelSO onLevelUp;

    private void OnEnable()
    {
        onExperienceGained.OnEventRaised += GainExperience;
    }

    private void OnDisable()
    {
        onExperienceGained.OnEventRaised -= GainExperience;
    }

    public void GainExperience(int amount)
    {
        characterStats.Experience += amount;
        Debug.Log("°æÇèÄ¡ È¹µæ: " + amount + ", ÇöÀç °æÇèÄ¡: " + characterStats.Experience);

        while (characterStats.Experience >= characterStats.ExperienceRequired)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        characterStats.Level++;
        characterStats.Experience -= characterStats.ExperienceRequired;
        characterStats.ExperienceRequired = Mathf.RoundToInt(characterStats.ExperienceRequired * 1.5f); // ÇÊ¿ä °æÇèÄ¡ Áõ°¡
        IncreaseStats();
        onLevelUp.RaiseEvent();

        Debug.Log("·¹º§¾÷! ÇöÀç ·¹º§: " + characterStats.Level);
    }

    private void IncreaseStats()
    {
        characterStats.Strength += Random.Range(1, 3);
        characterStats.Dexterity += Random.Range(1, 3);
        characterStats.Wisdom += Random.Range(1, 3);
        characterStats.Intelligence += Random.Range(1, 3);
        characterStats.Health += Random.Range(5, 10);

        Debug.Log("½ºÅÈ »ó½Â! Èû: " + characterStats.Strength + ", ¹ÎÃ¸: " + characterStats.Dexterity + ", ÁöÇı: " + characterStats.Wisdom + ", Áö´É: " + characterStats.Intelligence + ", Ã¼·Â: " + characterStats.Health);
    }
}