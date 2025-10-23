using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    public static TargetSelector Instance { get; private set; }
    public GameObject currentTarget; // ���� ���õ� Ÿ��

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

    // Ÿ�� ���� �Լ�
    public void SetTarget(GameObject target)
    {
        currentTarget = target;
        // Target Indicator ��ġ ������Ʈ
        transform.position = target.transform.position + new Vector3(0, 1, 0);
    }
}