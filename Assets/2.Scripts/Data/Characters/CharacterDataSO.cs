using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Game/Character")]
public class CharacterDataSO : ScriptableObject
{
    public string characterID;
    public string characterName;
    public Sprite portrait;
    public GameObject prefab;

    // �⺻ ����
    public int baseHP;
    public int baseMP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;

    // ������ ����ġ
    public int hpGrowth;
    public int mpGrowth;
    public int attackGrowth;
    public int defenseGrowth;

    // ��ų ���� (�ִ� 4��)
    //public List<SkillDataSO> defaultSkills;

    // ��� ���
    public int recruitCost;
}
