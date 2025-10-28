//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SkillAction : CombatAction
//{
//    private SkillDataSO skillData;
//    private List<ICombatant> targets;

//    public SkillAction(ICombatant attacker, SkillDataSO skill, List<ICombatant> targets)
//    {
//        this.attacker = attacker;
//        this.skillData = skill;
//        this.targets = targets;
//    }

//    public override IEnumerator Execute()
//    {
//        // MP �Ҹ�
//        Character character = attacker as Character;
//        character.UseMP(skillData.manaCost);

//        // ��ų ȿ�� ����
//        foreach (var target in targets)
//        {
//            if (skillData.type == SkillType.Attack)
//            {
//                int damage = DamageCalculator.CalculateSkillDamage(
//                    character, target as Character, skillData
//                );
//                target.TakeDamage(damage);
//            }
//            else if (skillData.type == SkillType.Heal)
//            {
//                target.Heal((int)(skillData.damageMultiplier * character.Stats.Attack));
//            }

//            // ����/����� ����
//            foreach (var buff in skillData.buffs)
//            {
//                target.ApplyBuff(buff);
//            }
//        }

//        yield return new WaitForSeconds(1f);
//    }

//    public override bool CanExecute()
//    {
//        Character character = attacker as Character;
//        return character.CurrentMP >= skillData.manaCost;
//    }
//}
