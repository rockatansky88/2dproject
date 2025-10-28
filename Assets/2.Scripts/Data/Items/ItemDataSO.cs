using UnityEngine;

public enum ItemType { Weapon, Armor, Potion, Material }
public enum EquipSlot { Weapon, Helmet, Armor, Boots }

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemDataSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int buyPrice;
    public int sellPrice;
    public string description;

    // 장비 전용
    public EquipSlot equipSlot;
    public StatModifier[] statModifiers;

    // 소모품 전용
    public int healAmount;
    //public BuffData[] buffs;
}

[System.Serializable]
public class StatModifier
{
    //public StatType statType;
    public int value;
    public bool isPercentage;
}