using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 개별 보상 슬롯
/// 아이템 아이콘을 표시하고 클릭 시 인벤토리에 추가합니다.
/// 수령 완료 시 슬롯이 비활성화되며 오버레이가 표시됩니다.
/// </summary>
public class RewardSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject claimedOverlay; // 수령 완료 표시 (반투명 회색 패널)

    private ItemDataSO itemData;
    private bool isClaimed = false;

    public bool IsClaimed => isClaimed;
    public event Action<ItemDataSO> OnRewardClaimed;

    private void Awake()
    {
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnButtonClicked);
        }

        if (claimedOverlay != null)
        {
            claimedOverlay.SetActive(false);
        }
    }

    /// <summary>
    /// 슬롯 초기화
    /// 아이템 데이터를 받아서 UI를 설정합니다.
    /// </summary>
    public void Initialize(ItemDataSO item)
    {
        itemData = item;
        isClaimed = false;

        if (itemIcon != null && item != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
        }

        if (itemNameText != null && item != null)
        {
            itemNameText.text = item.itemName;
        }

        if (claimedOverlay != null)
        {
            claimedOverlay.SetActive(false);
        }

        if (claimButton != null)
        {
            claimButton.interactable = true;
        }
    }

    /// <summary>
    /// 보상 수령
    /// 인벤토리에 아이템을 추가하고 슬롯을 비활성화합니다.
    /// </summary>
    public void ClaimReward()
    {
        if (isClaimed || itemData == null) return;

        isClaimed = true;

        OnRewardClaimed?.Invoke(itemData);

        if (claimedOverlay != null)
        {
            claimedOverlay.SetActive(true);
        }

        if (claimButton != null)
        {
            claimButton.interactable = false;
        }
    }

    private void OnButtonClicked()
    {
        ClaimReward();
    }

    private void OnDestroy()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}