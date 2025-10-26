using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Town 씬의 MercenaryShop 건물 클릭 감지
/// MerchantShop과 동일한 패턴으로 동작합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class MercenaryShop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MercenaryWindow mercenaryWindow;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = new Color(0f, 1f, 0.5f, 1f); // 초록색
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

        Debug.Log("[MercenaryShop] Awake 완료 - 테두리 생성됨");
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

        Debug.Log($"[MercenaryShop] 테두리 오브젝트 생성 완료 - 색상: {outlineColor}");
    }

    private void OnMouseEnter()
    {
        // UI가 열려 있으면 호버 효과 무시
        if (IsUIOpen())
        {
            return;
        }

        Debug.Log("[MercenaryShop] ? 마우스 진입!");
        SetOutlineActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("[MercenaryShop] 마우스 이탈!");
        SetOutlineActive(false);
    }

    private void OnMouseDown()
    {
        // UI가 열려 있거나 UI 위를 클릭하면 무시
        if (IsUIOpen() || EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[MercenaryShop] UI가 열려 있거나 UI 클릭 중이므로 무시");
            return;
        }

        Debug.Log("[MercenaryShop] ??? 클릭됨! 용병 상점 열기");
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
    /// 현재 UI가 열려 있는지 확인
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
        // Lazy Initialization: MercenaryWindow를 처음 필요할 때 검색
        if (mercenaryWindow == null)
        {
            // 비활성화된 오브젝트도 검색
            MercenaryWindow[] allWindows = Resources.FindObjectsOfTypeAll<MercenaryWindow>();

            foreach (var window in allWindows)
            {
                // 씬에 있는 오브젝트만 선택 (Prefab 제외)
                if (window.gameObject.scene.IsValid())
                {
                    mercenaryWindow = window;
                    break;
                }
            }

            if (mercenaryWindow == null)
            {
                Debug.LogError("[MercenaryShop] ? MercenaryWindow를 찾을 수 없습니다!");
                return;
            }

            Debug.Log("[MercenaryShop] ? MercenaryWindow를 찾았습니다!");
        }

        Debug.Log("[MercenaryShop] MercenaryWindow.Open() 호출");
        mercenaryWindow.Open();
    }
}