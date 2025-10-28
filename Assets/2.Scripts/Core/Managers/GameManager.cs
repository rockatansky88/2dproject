using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Resources")]
    [SerializeField] private int startingGold = 500;

    private int currentGold;
    public int Gold => currentGold;

    // °ñµå º¯°æ ÀÌº¥Æ®
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        // ½Ì±ÛÅæ ¼³Á¤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentGold = startingGold;
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            Debug.Log($"°ñµå ¼Òºñ: -{amount} (ÀÜ¾×: {currentGold})");
            return true;
        }

        Debug.Log("°ñµå°¡ ºÎÁ·ÇÕ´Ï´Ù!");
        return false;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"°ñµå È¹µæ: +{amount} (ÀÜ¾×: {currentGold})");
    }
}