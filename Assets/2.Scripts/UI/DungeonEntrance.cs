using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 마을에 있는 던전 입구 오브젝트
/// - 클릭 시 던전 입구 UI로 이동
/// - 호버 시 테두리 표시
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class DungeonEntrance : MonoBehaviour
{
    [Header("Dungeon Data")]
    [SerializeField] private DungeonDataSO dungeonData;

    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private float outlineThickness = 0.1f;

    private SpriteRenderer spriteRenderer;
    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;

    private void Awake()
    {
        Debug.Log("[DungeonEntrance] Awake 시작");

        spriteRenderer = GetComponent<SpriteRenderer>();

        // 테두리 생성
        CreateOutline();
        SetOutlineActive(false);

        Debug.Log("[DungeonEntrance] ✅ Awake 완료");
    }

    /// <summary>
    /// 테두리 오브젝트 생성
    /// </summary>
    private void CreateOutline()
    {
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;

        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = spriteRenderer.sprite;
        outlineRenderer.color = outlineColor;
        outlineRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        outlineRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

        outlineObject.transform.localScale = Vector3.one * (1f + outlineThickness);

        Debug.Log($"[DungeonEntrance] 테두리 생성 완료 (색상: {outlineColor})");
    }

    /// <summary>
    /// 테두리 표시/숨김
    /// </summary>
    private void SetOutlineActive(bool active)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(active);
        }
    }

    private void OnMouseEnter()
    {
        // UI 위에 있으면 무시
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Debug.Log("[DungeonEntrance] 마우스 진입");
        SetOutlineActive(true);
    }

    private void OnMouseExit()
    {
        Debug.Log("[DungeonEntrance] 마우스 이탈");
        SetOutlineActive(false);
    }

    private void OnMouseDown()
    {
        // UI 위에 있으면 무시
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Debug.Log($"[DungeonEntrance] 🖱️ 던전 입구 클릭: {dungeonData?.dungeonName ?? "null"}");

        if (dungeonData == null)
        {
            Debug.LogError("[DungeonEntrance] ❌ dungeonData가 null입니다!");
            return;
        }

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.ShowEntranceUI(dungeonData);
        }
        else
        {
            Debug.LogError("[DungeonEntrance] ❌ GameSceneManager.Instance가 null입니다!");
        }
    }
}