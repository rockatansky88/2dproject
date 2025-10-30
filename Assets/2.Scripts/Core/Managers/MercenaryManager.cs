using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// �뺴 �ý����� �ٽ� ������
/// - �뺴 ���� ��� ���� (8��)
/// - ���� �뺴 ���� (�ִ� 4��)
/// - �뺴 ���/�߹� ó��
/// </summary>
public class MercenaryManager : MonoBehaviour
{
    public static MercenaryManager Instance { get; private set; }

    [Header("Mercenary Templates")]
    [SerializeField] private List<MercenaryDataSO> mercenaryTemplates = new List<MercenaryDataSO>();

    [Header("Shop Settings")]
    [SerializeField] private int shopMercenaryCount = 8; // ������ ǥ���� �뺴 ��

    [Header("Party Settings")]
    [SerializeField] private int maxPartySize = 4; // �ִ� ��� ���� �ο�
    [SerializeField] private MercenaryDataSO starterMercenary; // ���� ���� �� �⺻ �뺴

    // ���� ������ ǥ�õǴ� �뺴 ��� (���� ������ �ν��Ͻ�)
    private List<MercenaryInstance> currentShopMercenaries = new List<MercenaryInstance>();

    // ���� �뺴 ���
    private List<MercenaryInstance> recruitedMercenaries = new List<MercenaryInstance>();

    // �̺�Ʈ
    public event Action OnShopRefreshed;
    public event Action OnPartyChanged;

    // ������Ƽ
    public List<MercenaryInstance> ShopMercenaries => currentShopMercenaries;
    public List<MercenaryInstance> RecruitedMercenaries => recruitedMercenaries;
    public int CurrentPartySize => recruitedMercenaries.Count;
    public int MaxPartySize => maxPartySize;
    public bool IsPartyFull => recruitedMercenaries.Count >= maxPartySize;

    /// <summary>
    /// ���� ���� ������ ��Ƽ (RecruitedMercenaries�� ����) 
    /// </summary>
    public List<MercenaryInstance> CurrentParty => recruitedMercenaries;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MercenaryManager] ? Instance ������");
        }
        else
        {
            Debug.Log("[MercenaryManager] �ߺ� Instance ����");
            Destroy(gameObject);
            return;
        }

        // ��ȿ�� �˻�
        if (mercenaryTemplates == null || mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[MercenaryManager] ? mercenaryTemplates�� ����ֽ��ϴ�! Inspector���� �������ּ���.");
        }
    }

    private void Start()
    {
        // ���� ���� �� �⺻ �뺴 �߰�
        AddStarterMercenary();

        // ���� �ʱ�ȭ
        RefreshShop();
    }

    /// <summary>
    /// ���� ���� �� �⺻ �뺴 �߰�
    /// </summary>
    private void AddStarterMercenary()
    {
        if (starterMercenary == null)
        {
            Debug.LogWarning("[MercenaryManager] ?? starterMercenary�� �������� �ʾҽ��ϴ�. �⺻ �뺴�� �߰����� �ʽ��ϴ�.");
            return;
        }

        MercenaryInstance starter = starterMercenary.CreateRandomInstance();
        starter.isRecruited = true;
        recruitedMercenaries.Add(starter);

        Debug.Log($"[MercenaryManager] ? ���� �뺴 �߰�: {starter.mercenaryName} (Lv.{starter.level})");
        OnPartyChanged?.Invoke();
    }

    /// <summary>
    /// ���� �뺴 ��� ���ΰ�ħ (���� ����)
    /// </summary>
    public void RefreshShop()
    {
        Debug.Log("[MercenaryManager] ������ ���� �뺴 ��� ���ΰ�ħ ���� ������");

        currentShopMercenaries.Clear();

        if (mercenaryTemplates.Count == 0)
        {
            Debug.LogError("[MercenaryManager] ? mercenaryTemplates�� ����ֽ��ϴ�!");
            return;
        }

        // 8���� ���� �뺴 ����
        for (int i = 0; i < shopMercenaryCount; i++)
        {
            // ���� ���ø� ����
            MercenaryDataSO template = mercenaryTemplates[UnityEngine.Random.Range(0, mercenaryTemplates.Count)];

            // ���� �ν��Ͻ� ����
            MercenaryInstance instance = template.CreateRandomInstance();
            currentShopMercenaries.Add(instance);

            Debug.Log($"[MercenaryManager] ���� �뺴 {i + 1}/8 ����: {instance.mercenaryName} (Lv.{instance.level})");
        }

        Debug.Log($"[MercenaryManager] ? ���� �뺴 {currentShopMercenaries.Count}�� ���� �Ϸ�");
        OnShopRefreshed?.Invoke();
    }

    /// <summary>
    /// �뺴 ���
    /// </summary>
    public bool RecruitMercenary(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryManager] ������ �뺴 ��� �õ�: {mercenary.mercenaryName} ������");

        // ��Ƽ�� ���� á���� Ȯ��
        if (IsPartyFull)
        {
            Debug.LogWarning($"[MercenaryManager] ? ��Ƽ�� ���� á���ϴ�! ({recruitedMercenaries.Count}/{maxPartySize})");
            return false;
        }

        // �̹� ���Ǿ����� Ȯ��
        if (mercenary.isRecruited)
        {
            Debug.LogWarning($"[MercenaryManager] ? �̹� ���� �뺴�Դϴ�: {mercenary.mercenaryName}");
            return false;
        }

        // ��� Ȯ�� �� ����
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MercenaryManager] ? GameManager.Instance�� null�Դϴ�!");
            return false;
        }

        Debug.Log($"[MercenaryManager] ��� ���: {mercenary.recruitCost} ���");
        Debug.Log($"[MercenaryManager] ���� ���: {GameManager.Instance.Gold} ���");

        if (!GameManager.Instance.SpendGold(mercenary.recruitCost))
        {
            Debug.LogWarning("[MercenaryManager] ? ��尡 �����մϴ�!");
            return false;
        }

        // ��� ó��
        mercenary.isRecruited = true;
        recruitedMercenaries.Add(mercenary);

        // ���� ��Ͽ��� ����
        if (currentShopMercenaries.Contains(mercenary))
        {
            currentShopMercenaries.Remove(mercenary);
            Debug.Log($"[MercenaryManager] ���� ��Ͽ��� ���ŵ�");
        }

        Debug.Log($"[MercenaryManager] ??? �뺴 ��� �Ϸ�: {mercenary.mercenaryName}");
        Debug.Log($"[MercenaryManager] ���� ��Ƽ �ο�: {recruitedMercenaries.Count}/{maxPartySize}");

        OnPartyChanged?.Invoke();
        OnShopRefreshed?.Invoke(); // ���� UI�� ����

        return true;
    }

    /// <summary>
    /// �뺴 �߹�
    /// </summary>
    public bool DismissMercenary(MercenaryInstance mercenary)
    {
        Debug.Log($"[MercenaryManager] ������ �뺴 �߹� �õ�: {mercenary.mercenaryName} ������");

        if (!recruitedMercenaries.Contains(mercenary))
        {
            Debug.LogWarning($"[MercenaryManager] ? ������ ���� �뺴�Դϴ�: {mercenary.mercenaryName}");
            return false;
        }

        // ������ �� ���� �߹� �Ұ� (�ɼ�)
        if (recruitedMercenaries.Count <= 1)
        {
            Debug.LogWarning("[MercenaryManager] ? �ּ� 1���� �뺴�� �����ؾ� �մϴ�!");
            return false;
        }

        recruitedMercenaries.Remove(mercenary);
        mercenary.isRecruited = false;

        Debug.Log($"[MercenaryManager] ? �뺴 �߹� �Ϸ�: {mercenary.mercenaryName}");
        Debug.Log($"[MercenaryManager] ���� ��Ƽ �ο�: {recruitedMercenaries.Count}/{maxPartySize}");

        OnPartyChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// Ư�� �뺴�� ���Ǿ����� Ȯ��
    /// </summary>
    public bool IsMercenaryRecruited(MercenaryInstance mercenary)
    {
        return recruitedMercenaries.Contains(mercenary);
    }

    /// <summary>
    /// �����: ��� ���� �뺴 ���
    /// </summary>
    [ContextMenu("Debug: Print Recruited Mercenaries")]
    public void DebugPrintRecruitedMercenaries()
    {
        Debug.Log($"[MercenaryManager] ������ ���� �뺴 ��� ({recruitedMercenaries.Count}��) ������");
        for (int i = 0; i < recruitedMercenaries.Count; i++)
        {
            var merc = recruitedMercenaries[i];
            Debug.Log($"{i + 1}. {merc.mercenaryName} (Lv.{merc.level}) - HP:{merc.health} STR:{merc.strength}");
        }
    }

    internal IEnumerable<object> GetPartyMembers()
    {
        throw new NotImplementedException();
        // ��Ƽ ��� ���� ��ȯ �߰� �ʿ�

    }
}