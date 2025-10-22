using UnityEngine;

public class Monster : MonoBehaviour
{
    public int experienceToGive = 50;
    public IntEventChannelSO onMonsterDefeated;

    public void Die()
    {
        onMonsterDefeated.RaiseEvent(experienceToGive);
        Destroy(gameObject);
    }
}