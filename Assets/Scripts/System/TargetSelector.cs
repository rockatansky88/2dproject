using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance { get; private set; }
    public GameObject currentTarget; // 현재 선택된 타겟

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 타겟 설정 함수
    public void SetTarget(GameObject target)
    {
        currentTarget = target;
        // Target Indicator 위치 업데이트
        transform.position = target.transform.position + new Vector3(0, 1, 0);
    }
}