using UnityEngine;
using System;

/// <summary>
/// 아이템 사용 처리 매니저
/// - HP/MP 포션 사용
/// - 마을 귀환 스크롤 사용
/// - 던전에서만 사용 가능하도록 제한
/// </summary>
public class ItemUsageManager : MonoBehaviour
{
    public static ItemUsageManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ItemUsageManager] 싱글톤 인스턴스 생성됨");
        }
        else
        {
            Debug.LogWarning("[ItemUsageManager] 중복 인스턴스 파괴됨");
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 아이템 사용 메인 함수
    /// </summary>
    public void UseItem(ItemDataSO item, MercenaryInstance targetMercenary = null)
    {
        if (item == null)
        {
            Debug.LogError("[ItemUsageManager] 아이템이 null입니다");
            return;
        }

        Debug.Log($"[ItemUsageManager] ━━━ 아이템 사용 시도: {item.itemName} ━━━");

        // 던전에서만 사용 가능
        if (!IsInDungeon())
        {
            Debug.LogWarning("[ItemUsageManager] 던전에서만 아이템을 사용할 수 있습니다");
            ShowMessage("던전에서만 사용 가능합니다");
            return;
        }

        // 아이템 타입에 따라 처리
        if (item.isTownPortalScroll)
        {
            UseTownPortalScroll(item);
        }
        else if (item.healAmount > 0 || item.manaAmount > 0)
        {
            UsePotion(item, targetMercenary);
        }
        else
        {
            Debug.LogWarning($"[ItemUsageManager] {item.itemName}은(는) 사용할 수 없는 아이템입니다");
            ShowMessage("사용할 수 없는 아이템입니다");
        }
    }

    /// <summary>
    /// HP/MP 포션 사용
    /// </summary>
    private void UsePotion(ItemDataSO item, MercenaryInstance targetMercenary)
    {
        if (targetMercenary == null)
        {
            Debug.LogWarning("[ItemUsageManager] 대상 용병이 선택되지 않았습니다");
            ShowMessage("사용할 용병을 선택해주세요");
            return;
        }

        Debug.Log($"[ItemUsageManager] 포션 사용: {item.itemName} → {targetMercenary.mercenaryName}");

        bool used = false;

        // HP 회복
        if (item.healAmount > 0)
        {
            int beforeHP = targetMercenary.currentHP;
            targetMercenary.currentHP = Mathf.Min(targetMercenary.currentHP + item.healAmount, targetMercenary.maxHP);
            int healedAmount = targetMercenary.currentHP - beforeHP;

            if (healedAmount > 0)
            {
                Debug.Log($"[ItemUsageManager] HP 회복: {targetMercenary.mercenaryName} +{healedAmount} HP ({beforeHP} → {targetMercenary.currentHP})");
                ShowMessage($"{targetMercenary.mercenaryName}의 HP가 {healedAmount} 회복되었습니다");
                used = true;
            }
            else
            {
                Debug.Log($"[ItemUsageManager] HP가 이미 최대입니다");
                ShowMessage("HP가 이미 최대입니다");
                return;
            }
        }

        // MP 회복
        if (item.manaAmount > 0)
        {
            int beforeMP = targetMercenary.currentMP;
            targetMercenary.currentMP = Mathf.Min(targetMercenary.currentMP + item.manaAmount, targetMercenary.maxMP);
            int restoredAmount = targetMercenary.currentMP - beforeMP;

            if (restoredAmount > 0)
            {
                Debug.Log($"[ItemUsageManager] MP 회복: {targetMercenary.mercenaryName} +{restoredAmount} MP ({beforeMP} → {targetMercenary.currentMP})");
                ShowMessage($"{targetMercenary.mercenaryName}의 MP가 {restoredAmount} 회복되었습니다");
                used = true;
            }
            else
            {
                Debug.Log($"[ItemUsageManager] MP가 이미 최대입니다");
                if (!used) // HP도 회복하지 않았으면
                {
                    ShowMessage("MP가 이미 최대입니다");
                    return;
                }
            }
        }

        // 아이템 소모
        if (used && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(item.itemID, 1);
            Debug.Log($"[ItemUsageManager] 아이템 소모: {item.itemName}");
        }

        // UI 갱신 (InventoryWindow)
        RefreshInventoryUI();
    }

    /// <summary>
    /// 마을 귀환 스크롤 사용
    /// itemID를 파라미터로 받아서 확실하게 소모 처리
    /// </summary>
    private void UseTownPortalScroll(ItemDataSO scrollItem)
    {
        Debug.Log("[ItemUsageManager] ━━━ 마을 귀환 스크롤 사용 시도 ━━━");

        if (ConfirmationPopup.Instance == null)
        {
            Debug.LogError("[ItemUsageManager] ❌ ConfirmationPopup.Instance가 null입니다!");
            return;
        }

        // 확인 팝업 표시
        ConfirmationPopup.Instance.Show(
            message: "던전에서 나가시겠습니까?",
            onConfirm: () =>
            {
                Debug.Log("[ItemUsageManager] ✅ 마을 귀환 승인됨");

                // 1. 아이템 먼저 소모
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.RemoveItem(scrollItem.itemID, 1);
                    Debug.Log($"[ItemUsageManager] ✅ 포탈 스크롤 소모: {scrollItem.itemName}");
                }

                // 2. 인벤토리 윈도우 닫기 (열려있다면)
                InventoryWindow inventoryWindow = FindObjectOfType<InventoryWindow>();
                if (inventoryWindow != null && inventoryWindow.IsOpen)
                {
                    inventoryWindow.CloseWindow();
                    Debug.Log("[ItemUsageManager] 인벤토리 윈도우 닫기");
                }

                // 3. 던전 퇴장 → GameSceneManager가 자동으로 ShowTownUI() 호출
                if (DungeonManager.Instance != null)
                {
                    DungeonManager.Instance.ExitDungeon();
                    Debug.Log("[ItemUsageManager] ✅ 던전 퇴장 완료");
                }

                ShowMessage("마을로 귀환합니다");
            },
            onCancel: () =>
            {
                Debug.Log("[ItemUsageManager] ❌ 마을 귀환 취소됨");
            },
            title: "마을 귀환"
        );
    }

    /// <summary>
    /// 메시지 표시 (디버그 로그)
    /// </summary>
    private void ShowMessage(string message)
    {
        Debug.Log($"[ItemUsageManager] 📢 메시지: {message}");
        // TODO: UI 토스트 메시지 시스템 구현 시 추가

    }

    /// <summary>
    /// 던전 내부인지 확인
    /// </summary>
    private bool IsInDungeon()
    {
        return DungeonManager.Instance != null && DungeonManager.Instance.IsInDungeon;
    }

    /// <summary>
    /// 인벤토리 UI 갱신
    /// </summary>
    private void RefreshInventoryUI()
    {
        InventoryWindow inventoryWindow = FindObjectOfType<InventoryWindow>();
        if (inventoryWindow != null && inventoryWindow.IsOpen)
        {
            // 현재 선택된 용병 스탯 재표시
            MercenaryInstance selectedMercenary = inventoryWindow.GetSelectedMercenary();
            if (selectedMercenary != null)
            {
                inventoryWindow.ShowMercenaryStats(selectedMercenary);
                Debug.Log("[ItemUsageManager] 인벤토리 UI 갱신 완료");
            }
        }
    }
}