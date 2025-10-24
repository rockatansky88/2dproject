using UnityEngine;
using System;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private int maxSlots = 24; // 최대 슬롯 수

    // 아이템ID, 개수
    private Dictionary<string, int> inventory = new Dictionary<string, int>();

    // 인벤토리 변경 이벤트
    public event Action OnInventoryChanged;

    // 아이템 데이터 캐시   
    private Dictionary<string, ItemDataSO> itemDataCache = new Dictionary<string, ItemDataSO>();

    [Header("References")]
    [SerializeField] private InventoryWindow inventoryWindow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllItemData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // I 키로 인벤토리 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("[InventoryManager] I 키 눌림 감지!");
            ToggleInventory();
        }

        // ESC 키로 인벤토리 닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (inventoryWindow != null && inventoryWindow.IsOpen)
            {
                Debug.Log("[InventoryManager] ESC 키 눌림 감지!");
                inventoryWindow.CloseWindow();
            }
        }
    }

    /// <summary>
    /// Resources 폴더에서 모든 아이템 데이터 로드
    /// </summary>
    private void LoadAllItemData()
    {
        // Resources/Items 폴더에서 모든 ItemDataSO 로드
        ItemDataSO[] items = Resources.LoadAll<ItemDataSO>("Items");

        foreach (ItemDataSO item in items)
        {
            if (item != null && !itemDataCache.ContainsKey(item.itemID))
            {
                itemDataCache[item.itemID] = item;
                Debug.Log($"[InventoryManager] 아이템 캐시 등록: {item.itemID} ({item.itemName})");
            }
        }

        Debug.Log($"[InventoryManager] ✅ 아이템 데이터 로드 완료: {itemDataCache.Count}개");

        if (itemDataCache.Count == 0)
        {
            Debug.LogError("[InventoryManager] ❌ 아이템이 하나도 로드되지 않았습니다! Resources/Items 폴더를 확인하세요!");
        }
    }

    /// <summary>
    /// 아이템 데이터 가져오기
    /// </summary>
    public ItemDataSO GetItemData(string itemID)
    {
        if (itemDataCache.ContainsKey(itemID))
        {
            return itemDataCache[itemID];
        }

        Debug.LogWarning($"아이템 데이터를 찾을 수 없습니다: {itemID}");
        return null;
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(ItemDataSO item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogError("아이템이 null입니다!");
            return;
        }

        if (inventory.ContainsKey(item.itemID))
        {
            inventory[item.itemID] += amount;
        }
        else
        {
            inventory[item.itemID] = amount;
        }

        Debug.Log($"아이템 추가: {item.itemName} x{amount}");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(string itemID, int amount = 1)
    {
        if (!inventory.ContainsKey(itemID))
        {
            Debug.Log("해당 아이템이 없습니다!");
            return false;
        }

        if (inventory[itemID] < amount)
        {
            Debug.Log("아이템 수량이 부족합니다!");
            return false;
        }

        inventory[itemID] -= amount;

        if (inventory[itemID] <= 0)
        {
            inventory.Remove(itemID);
        }

        Debug.Log($"아이템 제거: {itemID} x{amount}");
        OnInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// 아이템 개수 확인
    /// </summary>
    public int GetItemCount(string itemID)
    {
        return inventory.ContainsKey(itemID) ? inventory[itemID] : 0;
    }

    /// <summary>
    /// 전체 인벤토리 반환
    /// </summary>
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(inventory);
    }

    /// <summary>
    /// 인벤토리 초기화 (테스트용)
    /// </summary>
    public void Clear()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke();
    }

    private void ToggleInventory()
    {
        if (inventoryWindow == null)
        {
            Debug.LogError("[InventoryManager] InventoryWindow가 설정되지 않았습니다!");
            return;
        }

        if (inventoryWindow.IsOpen)
        {
            inventoryWindow.CloseWindow();
        }
        else
        {
            inventoryWindow.OpenInventoryMode();
        }
    }
}