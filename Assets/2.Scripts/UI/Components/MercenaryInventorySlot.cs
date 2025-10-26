using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// �κ��丮�� ǥ�õǴ� �뺴 ����
/// Ŭ���ϸ� StatsPanel�� �ش� �뺴�� ������ ǥ�õ˴ϴ�.
/// </summary>
public class MercenaryInventorySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;      // �ʻ�ȭ
    [SerializeField] private Text nameText;            // �̸�
    [SerializeField] private Button slotButton;        // Ŭ�� ��ư

    private MercenaryInstance mercenaryData;

    // �̺�Ʈ (InventoryWindow�� ����)
    public event Action<MercenaryInstance> OnSlotClicked;

    private void Awake()
    {
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnClicked);
            Debug.Log("[MercenaryInventorySlot] ���� ��ư ������ ��ϵ�");
        }
        else
        {
            Debug.LogError("[MercenaryInventorySlot] ? slotButton�� null�Դϴ�!");
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ (�뺴 ������ ����)
    /// </summary>
    public void Initialize(MercenaryInstance mercenary)
    {
        mercenaryData = mercenary;

        Debug.Log($"[MercenaryInventorySlot] Initialize - �뺴: {mercenary?.mercenaryName ?? "null"}");

        if (mercenary == null)
        {
            Debug.LogError("[MercenaryInventorySlot] ? mercenary�� null�Դϴ�!");
            return;
        }

        // �ʻ�ȭ
        if (portraitImage != null)
        {
            portraitImage.sprite = mercenary.portrait;
            portraitImage.enabled = mercenary.portrait != null;
            Debug.Log($"[MercenaryInventorySlot] �ʻ�ȭ ����: {mercenary.portrait?.name ?? "null"}");
        }

        // �̸�
        if (nameText != null)
        {
            nameText.text = mercenary.mercenaryName;
        }

        Debug.Log($"[MercenaryInventorySlot] ? �ʱ�ȭ �Ϸ�: {mercenary.mercenaryName}");
    }

    /// <summary>
    /// ���� Ŭ�� �ڵ鷯
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"[MercenaryInventorySlot] ??? ���� Ŭ����: {mercenaryData?.mercenaryName ?? "null"}");

        if (mercenaryData != null)
        {
            Debug.Log($"[MercenaryInventorySlot] OnSlotClicked �̺�Ʈ �߻�");
            OnSlotClicked?.Invoke(mercenaryData);
        }
        else
        {
            Debug.LogError("[MercenaryInventorySlot] ? mercenaryData�� null�Դϴ�!");
        }
    }

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnClicked);
        }
    }
}