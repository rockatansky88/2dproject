using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public void Attack(GameObject target)
    {
        if (target == null)
        {
            Debug.LogWarning("공격 대상이 없습니다.");
            return;
        }

        // 데미지 계산 (임시)
        int damage = GetComponent<PlayerExperience>().characterStats.Strength;
        target.GetComponent<Monster>().TakeDamage(damage);
        TurnManager.Instance.EndTurn();
    }
}