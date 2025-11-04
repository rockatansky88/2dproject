using UnityEngine;

public enum ItemType { Weapon, Armor, Potion, Material }
public enum EquipSlot { Weapon, Helmet, Armor, Boots }

/// <summary>
/// 아이템 데이터 ScriptableObject
/// - 장비, 소모품(포션), 재료, 포탈 스크롤 등을 정의합니다.
/// - healAmount: HP 회복량
/// - manaAmount: MP 회복량 (신규 추가)
/// - isTownPortalScroll: 마을 귀환 스크롤 여부 (신규 추가)
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string itemID;
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public int buyPrice;
    public int sellPrice;
    [TextArea(2, 4)]
    public string description;

    [Header("장비 전용")]
    [Tooltip("장비 슬롯 타입 (장비 아이템만 사용)")]
    public EquipSlot equipSlot;
    [Tooltip("스탯 보너스 (장비 아이템만 사용)")]
    public StatModifier[] statModifiers;

    [Header("소모품 전용")]
    [Tooltip("HP 회복량 (0이면 회복 안함)")]
    public int healAmount;

    [Tooltip("MP 회복량 (0이면 회복 안함) - 신규 추가")]
    public int manaAmount;

    [Tooltip("마을 귀환 스크롤 여부 - 신규 추가")]
    public bool isTownPortalScroll = false;
}

[System.Serializable]
public class StatModifier
{
    //public StatType statType;
    public int value;
    public bool isPercentage;
}