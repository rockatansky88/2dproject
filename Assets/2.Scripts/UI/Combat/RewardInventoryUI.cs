using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전투 완료 후 표시되는 보상 인벤토리 UI
/// 랜덤 아이템을 최대 3개까지 표시하며, 클릭 시 인벤토리에 추가됩니다.
/// 모든 아이템을 수령하거나 "보상받기" 버튼 클릭 시 다음 단계로 진행합니다.
/// </summary>
public class RewardInventoryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Transform rewardSlotsContainer;
    [SerializeField] private GameObject rewardSlotPrefab;
    [SerializeField] private Button claimAllButton;
    [SerializeField] private Text titleText;

    [Header("Reward Settings")]
    [SerializeField] private int maxRewardSlots = 3;
    [SerializeField] private ItemDataSO[] rewardItemPool;

    private List<RewardSlot> rewardSlots = new List<RewardSlot>();
    private List<ItemDataSO> currentRewards = new List<ItemDataSO>();
    private int claimedCount = 0;

    public event System.Action OnAllRewardsClaimed;

    private void Awake()
    {
        if (claimAllButton != null)
        {
            claimAllButton.onClick.AddListener(OnClaimAllButtonClicked);
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 보상 인벤토리 표시
    /// 랜덤 아이템을 생성하고 슬롯에 배치합니다.
    /// </summary>
    public void ShowRewardInventory()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        claimedCount = 0;
        currentRewards.Clear();

        GenerateRandomRewards();
        CreateRewardSlots();

        if (titleText != null)
        {
            titleText.text = "전투 보상";
        }

        if (claimAllButton != null)
        {
            claimAllButton.interactable = true;
        }
    }

    /// <summary>
    /// 보상 인벤토리 숨기기
    /// </summary>
    public void HideRewardInventory()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        ClearRewardSlots();
    }

    /// <summary>
    /// 랜덤 보상 아이템 생성 (최대 3개)
    /// </summary>
    private void GenerateRandomRewards()
    {
        if (rewardItemPool == null || rewardItemPool.Length == 0)
        {
            return;
        }

        int rewardCount = Random.Range(1, maxRewardSlots + 1);

        for (int i = 0; i < rewardCount; i++)
        {
            int randomIndex = Random.Range(0, rewardItemPool.Length);
            ItemDataSO randomItem = rewardItemPool[randomIndex];

            if (randomItem != null)
            {
                currentRewards.Add(randomItem);
            }
        }
    }

    /// <summary>
    /// 보상 슬롯 생성 및 초기화
    /// </summary>
    private void CreateRewardSlots()
    {
        ClearRewardSlots();

        if (rewardSlotPrefab == null)
        {
            return;
        }

        for (int i = 0; i < currentRewards.Count; i++)
        {
            ItemDataSO item = currentRewards[i];
            GameObject slotObj = Instantiate(rewardSlotPrefab, rewardSlotsContainer);
            RewardSlot slot = slotObj.GetComponent<RewardSlot>();

            if (slot != null)
            {
                slot.Initialize(item);
                slot.OnRewardClaimed += OnRewardSlotClaimed;
                rewardSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// 보상 슬롯 클리어
    /// </summary>
    private void ClearRewardSlots()
    {
        foreach (var slot in rewardSlots)
        {
            if (slot != null)
            {
                slot.OnRewardClaimed -= OnRewardSlotClaimed;
                Destroy(slot.gameObject);
            }
        }

        rewardSlots.Clear();
    }

    /// <summary>
    /// 개별 보상 슬롯 클릭 처리
    /// 아이템을 인벤토리에 추가하고 슬롯을 비활성화합니다.
    /// </summary>
    private void OnRewardSlotClaimed(ItemDataSO item)
    {
        if (item == null) return;

        if (InventoryManager.Instance != null)
        {
            bool success = InventoryManager.Instance.AddItem(item, 1);

            if (success)
            {
                claimedCount++;
                CheckAllRewardsClaimed();
            }
        }
    }

    /// <summary>
    /// "보상받기" 버튼 클릭 처리
    /// 남은 모든 보상을 한 번에 수령합니다.
    /// Collection Modified 에러 방지를 위해 ToArray()로 복사본을 순회합니다.
    /// </summary>
    private void OnClaimAllButtonClicked()
    {
        // ToArray()로 복사본을 만들어서 순회 (Collection Modified 에러 방지)
        foreach (var slot in rewardSlots.ToArray())
        {
            if (slot != null && !slot.IsClaimed)
            {
                slot.ClaimReward();
            }
        }
    }

    /// <summary>
    /// 모든 보상 수령 완료 확인
    /// </summary>
    private void CheckAllRewardsClaimed()
    {
        if (claimedCount >= currentRewards.Count)
        {
            OnAllRewardsClaimed?.Invoke();
            HideRewardInventory();
        }
    }

    private void OnDestroy()
    {
        if (claimAllButton != null)
        {
            claimAllButton.onClick.RemoveListener(OnClaimAllButtonClicked);
        }

        ClearRewardSlots();
    }
}
