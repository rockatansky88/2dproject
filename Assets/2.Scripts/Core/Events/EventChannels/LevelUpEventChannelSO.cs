using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Events/Level Up Event Channel")]
public class LevelUpEventChannelSO : ScriptableObject
{
    public event Action OnEventRaised;

    public void RaiseEvent()
    {
        if (OnEventRaised != null)
        {
            OnEventRaised.Invoke();
        }
    }
}