using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Game/Character")]
public class CharacterDataSO : ScriptableObject
{
    public string characterID;
    public string characterName;
    public Sprite portrait;
    public GameObject prefab;

    // 기본 스탯
    public int baseHP;
    public int baseMP;
    public int baseAttack;
    public int baseDefense;
    public int baseSpeed;

    // 레벨업 성장치
    public int hpGrowth;
    public int mpGrowth;
    public int attackGrowth;
    public int defenseGrowth;

    // 스킬 슬롯 (최대 4개)
    //public List<SkillDataSO> defaultSkills;

    // 고용 비용
    public int recruitCost;
}
