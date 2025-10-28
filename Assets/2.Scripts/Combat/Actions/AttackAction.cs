using System.Collections;
using UnityEngine;

public class AttackAction : CombatAction
{
    public AttackAction(ICombatant attacker, ICombatant target)
    {
        this.attacker = attacker;
        this.target = target;
    }

    public override IEnumerator Execute()
    {
        if (!CanExecute()) yield break;

        //int damage = DamageCalculator.CalculatePhysicalDamage(
        //    attacker as Character,
        //    target as Character
        //);

        // 애니메이션 재생
        // VFX 표시
        //target.TakeDamage(damage);

        yield return new WaitForSeconds(0.5f);
    }

    public override bool CanExecute()
    {
        return attacker.IsAlive && target.IsAlive;
    }
}