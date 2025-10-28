using UnityEngine;

/// <summary>
/// ���� �̺�Ʈ ������
/// - ��Ƽ ��ü�� ������ �ִ� �̺�Ʈ (����, �����, ����, ���� ��)
/// </summary>
[CreateAssetMenu(fileName = "New Room Event", menuName = "Game/Dungeon/Room Event")]
public class RoomEventDataSO : ScriptableObject
{
    [Header("�̺�Ʈ �⺻ ����")]
    public string eventID;

    [Tooltip("�̺�Ʈ ���� (��: �ź��� ����)")]
    public string eventName;

    [Tooltip("�̺�Ʈ ��� �̹���")]
    public Sprite eventImage;

    [Tooltip("�̺�Ʈ ���� �ؽ�Ʈ")]
    [TextArea(3, 10)]
    public string description;

    [Header("�̺�Ʈ ȿ��")]
    [Tooltip("�̺�Ʈ ȿ�� ����Ʈ (���� ȿ�� ���� ���� ����)")]
    public EventEffect[] effects;
}

/// <summary>
/// �̺�Ʈ ȿ�� ������
/// </summary>
[System.Serializable]
public class EventEffect
{
    [Tooltip("ȿ�� Ÿ��")]
    public EventEffectType effectType;

    [Tooltip("ȿ�� �� (ü�� ��ȭ��, ��� �� ��)")]
    public int value;

    [Tooltip("��� ���� (HP, Strength, Dexterity ��)")]
    public string targetStat;

    [Tooltip("���� ������ (ItemReward�� ���� ���)")]
    public ItemDataSO rewardItem;

    [Tooltip("������ ����")]
    public int itemAmount = 1;

    [Tooltip("ȿ�� ���� �ð� (���� ��, 0 = ����)")]
    public int duration = 0;
}