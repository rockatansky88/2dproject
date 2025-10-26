using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Town ���� MercenaryShop �ǹ� Ŭ�� ����
/// MerchantShop�� ������ �������� �����մϴ�.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MercenaryShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MercenaryWindow mercenaryWindow;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = new Color(0f, 1f, 0.5f, 1f); // �ʷϻ�
    [SerializeField] private float outlineThickness = 0.1f;

    private SpriteRenderer spriteRenderer;
    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �׵θ� ������Ʈ ����
        CreateOutline();

        // �ʱ� ����: �׵θ� ��Ȱ��ȭ
        SetOutlineActive(false);

        Debug.Log("[MercenaryShop] Awake �Ϸ� - �׵θ� ������");
    }

    private void CreateOutline()
    {
        // �׵θ��� ������Ʈ ����
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;

        // SpriteRenderer �߰�
        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = spriteRenderer.sprite;
        outlineRenderer.color = outlineColor;
        outlineRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // ���� �ڿ� ǥ��

        // �׵θ� ȿ���� ���� �ణ ũ�� ����
        outlineObject.transform.localScale = Vector3.one * (1f + outlineThickness);

        Debug.Log($"[MercenaryShop] �׵θ� ������Ʈ ���� �Ϸ� - ����: {outlineColor}");
    }

    private void OnMouseEnter()
    {
        // UI�� ���� ������ ȣ�� ȿ�� ����
        if (IsUIOpen())
        {
            return;
        }

        Debug.Log("[MercenaryShop] ? ���콺 ����!");
        SetOutlineActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("[MercenaryShop] ���콺 ��Ż!");
        SetOutlineActive(false);
    }

    private void OnMouseDown()
    {
        // UI�� ���� �ְų� UI ���� Ŭ���ϸ� ����
        if (IsUIOpen() || EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[MercenaryShop] UI�� ���� �ְų� UI Ŭ�� ���̹Ƿ� ����");
            return;
        }

        Debug.Log("[MercenaryShop] ??? Ŭ����! �뺴 ���� ����");
        OpenMercenaryShop();
    }

    private void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
        }
    }

    /// <summary>
    /// ���� UI�� ���� �ִ��� Ȯ��
    /// </summary>
    private bool IsUIOpen()
    {
        // Lazy Initialization
        if (mercenaryWindow == null)
        {
            MercenaryWindow[] allWindows = Resources.FindObjectsOfTypeAll<MercenaryWindow>();

            foreach (var window in allWindows)
            {
                if (window.gameObject.scene.IsValid())
                {
                    mercenaryWindow = window;
                    break;
                }
            }
        }

        return mercenaryWindow != null && mercenaryWindow.IsOpen;
    }

    private void OpenMercenaryShop()
    {
        // Lazy Initialization: MercenaryWindow�� ó�� �ʿ��� �� �˻�
        if (mercenaryWindow == null)
        {
            // ��Ȱ��ȭ�� ������Ʈ�� �˻�
            MercenaryWindow[] allWindows = Resources.FindObjectsOfTypeAll<MercenaryWindow>();

            foreach (var window in allWindows)
            {
                // ���� �ִ� ������Ʈ�� ���� (Prefab ����)
                if (window.gameObject.scene.IsValid())
                {
                    mercenaryWindow = window;
                    break;
                }
            }

            if (mercenaryWindow == null)
            {
                Debug.LogError("[MercenaryShop] ? MercenaryWindow�� ã�� �� �����ϴ�!");
                return;
            }

            Debug.Log("[MercenaryShop] ? MercenaryWindow�� ã�ҽ��ϴ�!");
        }

        Debug.Log("[MercenaryShop] MercenaryWindow.Open() ȣ��");
        mercenaryWindow.Open();
    }
}