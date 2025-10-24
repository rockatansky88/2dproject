using UnityEngine;

/// <summary>
/// 씬에 있는 MerchantShop 오브젝트 클릭 감지
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MerchantShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryWindow inventoryWindow;

    [Header("Outline Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color outlineColor = new Color(1f, 0.3f, 0.3f, 1f); // 빨간색
    [SerializeField] private float outlineThickness = 0.05f;

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

        // 테두리 효과를 위해 약간 크게 설정
        outlineObject.transform.localScale = Vector3.one * (1f + outlineThickness);

        Debug.Log($"[MerchantShop] 테두리 오브젝트 생성 완료 - 색상: {outlineColor}, 크기: {outlineObject.transform.localScale}");
    }

    private void OnMouseEnter()
    {
        Debug.Log("[MerchantShop] ✅ 마우스 진입!");
        SetOutlineActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("[MerchantShop] ❌ 마우스 이탈!");
        SetOutlineActive(false);
    }

    private void OnMouseDown()
    {
        Debug.Log("[MerchantShop] 🖱️ 클릭됨!");
        OpenShop();
    }

    private void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
            Debug.Log($"[MerchantShop] 테두리 {(active ? "활성화" : "비활성화")}");
        }
        else
        {
            Debug.LogError("[MerchantShop] ❌ outlineObject가 null입니다!");
        }
    }

    private void OpenShop()
    {
        Debug.Log("[MerchantShop] 🏪 상점 열기 시도...");

        // Lazy Initialization: InventoryWindow를 처음 필요할 때 검색
        if (inventoryWindow == null)
        {
            Debug.Log("[MerchantShop] InventoryWindow 검색 중...");

            // 비활성화된 오브젝트도 검색
            InventoryWindow[] allWindows = Resources.FindObjectsOfTypeAll<InventoryWindow>();
            Debug.Log($"[MerchantShop] 찾은 InventoryWindow 개수: {allWindows.Length}");

            foreach (var window in allWindows)
            {
                // 씬에 있는 오브젝트만 선택 (Prefab 제외)
                if (window.gameObject.scene.IsValid())
                {
                    inventoryWindow = window;
                    Debug.Log($"[MerchantShop] ✅ InventoryWindow 찾음: {window.gameObject.name}");
                    break;
                }
            }

            if (inventoryWindow == null)
            {
                Debug.LogError("[MerchantShop] ❌ InventoryWindow를 찾을 수 없습니다!");
                return;
            }
        }

        Debug.Log("[MerchantShop] OpenShopMode() 호출...");
        inventoryWindow.OpenShopMode();
    }
}