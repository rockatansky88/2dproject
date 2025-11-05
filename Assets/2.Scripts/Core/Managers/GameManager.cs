using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Resources")]
    [SerializeField] private int startingGold = 500;

    private int currentGold;
    public int Gold => currentGold;

    // ∞ÒµÂ ∫Ø∞Ê ¿Ã∫•∆Æ
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        // ΩÃ±€≈Ê º≥¡§
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
            return true;
        }

        return false;
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
    }
}