using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 씬에 있는 MerchantShop 오브젝트 클릭 감지
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]  // SpriteRenderer 을 생성 
[RequireComponent(typeof(BoxCollider2D))]  // BoxCollider2D 을 생성
public class MerchantShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryWindow inventoryWindow;

    [Header("Outline Settings")]
    [SerializeField] private GameObject outlinePrefab; // 테두리용 별도 스프라이트
    [SerializeField] private Color outlineColor = new Color(1f, 0.5f, 0f, 1f); // 주황색
    [SerializeField] private float outlineThickness = 0.1f;

    private SpriteRenderer spriteRenderer;
    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 테두리 오브젝트 생성
        CreateOutline();

        // 초기 상태: 테두리 비활성화
        SetOutlineActive(false);

        Debug.Log("[MerchantShop] Awake 완료 - 테두리 생성됨");
    }

    private void CreateOutline()
    {
        // 테두리용 오브젝트 생성
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;

        // SpriteRenderer 추가
        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = spriteRenderer.sprite;
        outlineRenderer.color = outlineColor;
        outlineRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1; // 원본 뒤에 표시

        // 테두리 효과를 위해 약간 작게 설정 (안쪽으로 들어오게)
        outlineObject.transform.localScale = Vector3.one * (1f - outlineThickness);

        Debug.Log($"[MerchantShop] 테두리 오브젝트 생성 완료 - 색상: {outlineColor}, 크기: {outlineObject.transform.localScale}");
    }

    private void OnMouseEnter()
    {
        // UI가 열려 있으면 호버 효과 무시
        if (IsUIOpen())
        {
            return;
        }

        Debug.Log("[MerchantShop] ✅ 마우스 진입!");
        SetOutlineActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("[MerchantShop] 마우스 이탈!");
        SetOutlineActive(false);
    }

    private void OnMouseDown()
    {
        // UI가 열려 있거나 UI 위를 클릭하면 무시
        if (IsUIOpen() || EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[MerchantShop] UI가 열려 있거나 UI 클릭 중이므로 무시");
            return;
        }

        Debug.Log("[MerchantShop] 클릭됨!");
        OpenShop();
    }

    private void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
        }
    }

    /// <summary>
    /// 현재 UI가 열려 있는지 확인
    /// </summary>
    private bool IsUIOpen()
    {
        // Lazy Initialization
        if (inventoryWindow == null)
        {
            InventoryWindow[] allWindows = Resources.FindObjectsOfTypeAll<InventoryWindow>();

            foreach (var window in allWindows)
            {
                if (window.gameObject.scene.IsValid())
                {
                    inventoryWindow = window;
                    break;
                }
            }
        }

        return inventoryWindow != null && inventoryWindow.IsOpen;
    }

    private void OpenShop()
    {
        // Lazy Initialization: InventoryWindow를 처음 필요할 때 검색
        if (inventoryWindow == null)
        {
            // 비활성화된 오브젝트도 검색
            InventoryWindow[] allWindows = Resources.FindObjectsOfTypeAll<InventoryWindow>();

            foreach (var window in allWindows)
            {  // 씬에 있는 오브젝트만 선택 (Prefab 제외)

                if (window.gameObject.scene.IsValid())
                {
                    inventoryWindow = window;
                    break;
                }
            }

            if (inventoryWindow == null)
            {
                Debug.LogError("[MerchantShop] InventoryWindow를 찾을 수 없습니다!");
                return;
            }

            Debug.Log("[MerchantShop] ✅ InventoryWindow를 찾았습니다!");
        }

        inventoryWindow.OpenShopMode();
    }
}