using UnityEngine;

[CreateAssetMenu(fileName = "New Room Event", menuName = "Game/Room Event")]
public class RoomEventDataSO : ScriptableObject
{
    public string eventID;
    public string eventName;
    public Sprite eventImage;
    public string description;

    // �̺�Ʈ ȿ���� (���� �� ����)
    public EventEffect[] effects;
}

[System.Serializable]
public class EventEffect
{
    //public EventEffectType type; // Buff, Debuff, Reward, Damage, Heal
    public int value;
    public string targetStat; // HP, MP, Gold ��
    public ItemDataSO rewardItem;
    public int itemAmount;
}