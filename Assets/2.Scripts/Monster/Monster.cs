using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterStatsSO monsterStats;
    public IntEventChannelSO onMonsterDefeated;

    void Start()
    {
        // 태그 설정
        gameObject.tag = "Monster";
    }

    public void TakeDamage(int damage)
    {
        monsterStats.Health -= damage;
        Debug.Log(gameObject.name + "이(가) " + damage + " 데미지를 입었습니다. 남은 체력: " + monsterStats.Health);

        if (monsterStats.Health <= 0)
        {
            Die();
        }
    }

    public void Attack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("공격 대상이 없습니다.");
            return;
        }

        // 데미지 계산 (임시)
        int damage = monsterStats.Strength;
        target.GetComponent<PlayerExperience>().characterStats.Health -= damage;
        Debug.Log(gameObject.name + "이(가) " + target.name + "에게 " + damage + " 데미지를 입혔습니다.");

        if (target.GetComponent<PlayerExperience>().characterStats.Health <= 0)
        {
            Debug.Log(target.name + "이(가) 쓰러졌습니다!");
        }
    }

    public void Die()
    {
        onMonsterDefeated.RaiseEvent(50);
        Destroy(gameObject);
        TurnManager.Instance.monsters.Remove(gameObject);
        TurnManager.Instance.UpdateTurnOrder();
    }
}