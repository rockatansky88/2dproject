using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public void Attack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("���� ����� �����ϴ�.");
            return;
        }

        // ������ ��� (�ӽ�)
        int damage = GetComponent<PlayerExperience>().characterStats.Strength;
        target.GetComponent<Monster>().TakeDamage(damage);
        TurnManager.Instance.EndTurn();
    }
}