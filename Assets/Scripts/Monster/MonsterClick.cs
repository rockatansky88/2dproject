using UnityEngine;

public class MonsterClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        TargetSelector.Instance.SetTarget(gameObject);
    }
}