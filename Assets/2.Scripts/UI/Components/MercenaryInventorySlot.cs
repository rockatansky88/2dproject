using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 인벤토리에 표시되는 용병 슬롯
/// 클릭하면 StatsPanel에 해당 용병의 스탯이 표시됩니다.
/// </summary>
public class MercenaryInventorySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;      // 초상화
    [SerializeField] private Text nameText;            // 이름
    [SerializeField] private Button slotButton;        // 클릭 버튼

    private MercenaryInstance mercenaryData;

    // 이벤트 (InventoryWindow가 구독)
    public event Action<MercenaryInstance> OnSlotClicked;

    private void Awake()
    {
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnClicked);
            Debug.Log("[MercenaryInventorySlot] 슬롯 버튼 리스너 등록됨");
        }
        else
        {
            Debug.LogError("[MercenaryInventorySlot] ? slotButton이 null입니다!");
        }
    }

    /// <summary>
    /// 슬롯 초기화 (용병 데이터 설정)
    /// </summary>
    public void Initialize(MercenaryInstance mercenary)
    {
        mercenaryData = mercenary;

        Debug.Log($"[MercenaryInventorySlot] Initialize - 용병: {mercenary?.mercenaryName ?? "null"}");

        if (mercenary == null)
        {
            Debug.LogError("[MercenaryInventorySlot] ? mercenary가 null입니다!");
            return;
        }

        // 초상화
        if (portraitImage != null)
        {
            portraitImage.sprite = mercenary.portrait;
            portraitImage.enabled = mercenary.portrait != null;
            Debug.Log($"[MercenaryInventorySlot] 초상화 설정: {mercenary.portrait?.name ?? "null"}");
        }

        // 이름
        if (nameText != null)
        {
            nameText.text = mercenary.mercenaryName;
        }

        Debug.Log($"[MercenaryInventorySlot] ? 초기화 완료: {mercenary.mercenaryName}");
    }

    /// <summary>
    /// 슬롯 클릭 핸들러
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"[MercenaryInventorySlot] ??? 슬롯 클릭됨: {mercenaryData?.mercenaryName ?? "null"}");

        if (mercenaryData != null)
        {
            Debug.Log($"[MercenaryInventorySlot] OnSlotClicked 이벤트 발생");
            OnSlotClicked?.Invoke(mercenaryData);
        }
        else
        {
            Debug.LogError("[MercenaryInventorySlot] ? mercenaryData가 null입니다!");
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