using UnityEngine;

public class Monster : MonoBehaviour
{
    public MonsterStatsSO monsterStats;
    public IntEventChannelSO onMonsterDefeated;

    void Start()
    {
        // �±� ����
        gameObject.tag = "Monster";
    }

    public void TakeDamage(int damage)
    {
        monsterStats.Health -= damage;
        Debug.Log(gameObject.name + "��(��) " + damage + " �������� �Ծ����ϴ�. ���� ü��: " + monsterStats.Health);

        if (monsterStats.Health <= 0)
        {
            Die();
        }
    }

    public void Attack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("���� ����� �����ϴ�.");
            return;
        }

        // ������ ��� (�ӽ�)
        int damage = monsterStats.Strength;
        target.GetComponent<PlayerExperience>().characterStats.Health -= damage;
        Debug.Log(gameObject.name + "��(��) " + target.name + "���� " + damage + " �������� �������ϴ�.");

        if (target.GetComponent<PlayerExperience>().characterStats.Health <= 0)
        {
            Debug.Log(target.name + "��(��) ���������ϴ�!");
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