using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ���� �뺴 ��Ƽ UI (�׻� ǥ�õ�)
/// ���� ���� �� �ڵ����� �ʱ�ȭ�Ǹ�, MercenaryManager�� ��������� �����մϴ�.
/// </summary>
public class MercenaryParty : MonoBehaviour
{
    [Header("Party Slots")]
    [SerializeField] private Transform partySlotContainer; // ��Ƽ ���Ե��� �θ�
    [SerializeField] private GameObject mercenaryPartySlotPrefab; // ��Ƽ �뺴 ���� ������
    [SerializeField] private int maxPartySlots = 4;

    [Header("Detail Popup")]
    [SerializeField] private MercenaryDetailPopup detailPopup; // Canvas���� ����

    // ���� ���
    private List<MercenaryPartySlot> partySlots = new List<MercenaryPartySlot>();

    private void Awake()
    {
        Debug.Log("[MercenaryParty] Awake ����");

        // ��Ƽ ���� �ʱ�ȭ (4�� ����)
        InitializePartySlots();
    }

    private void Start()
    {
        Debug.Log("[MercenaryParty] Start ����");

        // MercenaryManager �̺�Ʈ ����
        if (MercenaryManager.Instance != null)
        {
            MercenaryManager.Instance.OnPartyChanged += RefreshPartySlots;
            Debug.Log("[MercenaryParty] MercenaryManager �̺�Ʈ ���� �Ϸ�");

            // �ʱ� ��Ƽ ����
            RefreshPartySlots();
        }
        else
        {
            Debug.LogWarning("[MercenaryParty] ?? MercenaryManager.Instance�� null�Դϴ�. ���߿� ��õ��մϴ�.");
            // MercenaryManager�� ���� ������ ���߿� ��õ�
            Invoke(nameof(DelayedStart), 0.1f);
        }
    }

    private void DelayedStart()
    {
        if (MercenaryManager.Instance != null)
        {
            MercenaryManager.Instance.OnPartyChanged += RefreshPartySlots;
            RefreshPartySlots();
            Debug.Log("[MercenaryParty] ? ������ �ʱ�ȭ ����");
        }
        else
        {
            Debug.LogError("[MercenaryParty] ? MercenaryManager�� ������ null�Դϴ�!");
        }
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (MercenaryManager.Instance != null)
        {
            MercenaryManager.Instance.OnPartyChanged -= RefreshPartySlots;
        }

        // ���� �̺�Ʈ ����
        foreach (var slot in partySlots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnPartySlotClicked;
            }
        }
    }

    /// <summary>
    /// ��Ƽ ���� �ʱ�ȭ (4�� ����)
    /// </summary>
    private void InitializePartySlots()
    {
        Debug.Log("[MercenaryParty] ������ ��Ƽ ���� �ʱ�ȭ ���� ������");

        // ���� ���� ���� (�ߺ� ����)
        foreach (var slot in partySlots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        partySlots.Clear();

        // 4���� ��Ƽ ���� ����
        for (int i = 0; i < maxPartySlots; i++)
        {
            GameObject slotObj = Instantiate(mercenaryPartySlotPrefab, partySlotContainer);
            MercenaryPartySlot slot = slotObj.GetComponent<MercenaryPartySlot>();

            if (slot != null)
            {
                slot.SetEmpty(); // �� �������� �ʱ�ȭ
                slot.OnSlotClicked += OnPartySlotClicked;
                partySlots.Add(slot);

                Debug.Log($"[MercenaryParty] ��Ƽ ���� {i + 1} ���� �Ϸ�");
            }
            else
            {
                Debug.LogError($"[MercenaryParty] ? ���� {i + 1}�� MercenaryPartySlot ������Ʈ�� �����ϴ�!");
            }
        }

        Debug.Log($"[MercenaryParty] ? ��Ƽ ���� {partySlots.Count}�� �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// ��Ƽ ���� ���� (�뺴 ������ �ݿ�)
    /// </summary>
    public void RefreshPartySlots()
    {
        Debug.Log("[MercenaryParty] ������ ��Ƽ ���� ���� ���� ������");

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[MercenaryParty] ? MercenaryManager.Instance�� null�Դϴ�!");
            return;
        }

        List<MercenaryInstance> recruited = MercenaryManager.Instance.RecruitedMercenaries;
        Debug.Log($"[MercenaryParty] ���� �뺴 ��: {recruited.Count}");

        // ���� ����
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < recruited.Count)
            {
                partySlots[i].Initialize(recruited[i]);
                Debug.Log($"[MercenaryParty] ��Ƽ ���� {i + 1} ����: {recruited[i].mercenaryName}");
            }
            else
            {
                partySlots[i].SetEmpty();
                Debug.Log($"[MercenaryParty] ��Ƽ ���� {i + 1} ���");
            }
        }

        Debug.Log("[MercenaryParty] ? ��Ƽ ���� ���� �Ϸ�");
    }

    /// <summary>
    /// ��Ƽ ���� Ŭ�� �� �� �˾� ǥ�� (�߹� ���)
    /// </summary>
    private void OnPartySlotClicked(MercenaryInstance mercenary)
    {
        if (mercenary == null)
        {
            Debug.Log("[MercenaryParty] �� ��Ƽ ���� Ŭ���� - ����");
            return;
        }

        Debug.Log($"[MercenaryParty] ??? ��Ƽ ���� Ŭ��: {mercenary.mercenaryName}");

        // DetailPopup�� ������ ã��
        if (detailPopup == null)
        {
            detailPopup = FindObjectOfType<MercenaryDetailPopup>();

            if (detailPopup == null)
            {
                Debug.LogError("[MercenaryParty] ? MercenaryDetailPopup�� ã�� �� �����ϴ�!");
                return;
            }
        }

        detailPopup.ShowDismissMode(mercenary);
    }

    /// <summary>
    /// ������ ��� ���� (������������ HP/MP ǥ��)
    /// </summary>
    public void SetCombatMode(bool isCombat)
    {
        Debug.Log($"[MercenaryParty] ���� ��� ����: {isCombat}");

        foreach (var slot in partySlots)
        {
            if (slot != null)
            {
                slot.SetCombatMode(isCombat);
            }
        }
    }
}