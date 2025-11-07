using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// TPE (Timed Precision Event) 미니게임
/// 공격 시 Success 영역을 맞추면 크리티컬 확률 +30%
/// 난이도 대폭 상승: Success Zone 크기 축소, 화살표 속도 증가
/// Collider2D 기반 감지로 해상도 무관 정확한 판정 제공
/// Screen Space - Overlay 모드 완벽 지원
/// </summary>
public class TPEMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Text resultText;

    [Header("Collider 참조 - 필수")]
    [SerializeField] private BoxCollider2D successZoneCollider; // SuccessZone의 BoxCollider2D
    [SerializeField] private BoxCollider2D arrowCollider;        // Arrow의 BoxCollider2D

    [Header("설정 - 난이도 상승")]
    [SerializeField] private float arrowSpeed = 800f;
    [SerializeField] private float barWidth = 600f;


    [SerializeField] private float normalSuccessZoneWidth = 40f;
    [SerializeField] private float eliteSuccessZoneWidth = 25f;
    [SerializeField] private float bossSuccessZoneWidth = 15f;

    [Header("Collider 크기 설정")]
    [SerializeField] private float colliderHeight = 50f;          // Collider 높이
    [SerializeField] private float arrowColliderWidth = 10f;      // Arrow Collider 너비

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float arrowDirection = 1f;

    public event Action<bool> OnMinigameComplete;

    private void Awake()
    {
        minigamePanel.SetActive(false);
        ValidateColliders();
    }

    /// <summary>
    /// Collider 컴포넌트 검증 및 자동 설정
    /// </summary>
    private void ValidateColliders()
    {
        // SuccessZone Collider 검증
        if (successZoneCollider == null && successZone != null)
        {
            successZoneCollider = successZone.GetComponent<BoxCollider2D>();
        }

        // Arrow Collider 검증
        if (arrowCollider == null && arrow != null)
        {
            arrowCollider = arrow.GetComponent<BoxCollider2D>();
        }

        // Collider가 없으면 경고
        if (successZoneCollider == null || arrowCollider == null)
        {
            Debug.LogError("[TPEMinigame] Collider 설정이 올바르지 않습니다! Unity 에디터에서 BoxCollider2D를 추가하세요.");
        }
    }

    /// <summary>
    /// 미니게임 시작
    /// </summary>
    public void StartMinigame(MonsterDifficulty difficulty)
    {
        float zoneWidth = difficulty switch
        {
            MonsterDifficulty.Elite => eliteSuccessZoneWidth,
            MonsterDifficulty.Boss => bossSuccessZoneWidth,
            _ => normalSuccessZoneWidth
        };

        // UI RectTransform 크기 설정
        successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);

        // Collider 크기 동기화 (RectTransform 크기와 일치)
        if (successZoneCollider != null)
        {
            successZoneCollider.size = new Vector2(zoneWidth, colliderHeight);
            successZoneCollider.offset = Vector2.zero; // Offset 초기화
            successZoneCollider.isTrigger = true;
        }

        // Arrow Collider 설정
        if (arrowCollider != null)
        {
            arrowCollider.size = new Vector2(arrowColliderWidth, colliderHeight);
            arrowCollider.offset = Vector2.zero; // Offset 초기화
            arrowCollider.isTrigger = true;
        }

        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        arrowDirection = 1f;

        minigamePanel.SetActive(true);
        resultText.text = "";

        StartCoroutine(MoveArrow());
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckSuccess();
        }
    }

    /// <summary>
    /// 화살표 이동 (0 ~ barWidth 사이를 왔다갔다)
    /// Collider는 자동으로 따라가므로 별도 동기화 불필요
    /// </summary>
    private IEnumerator MoveArrow()
    {
        while (isPlaying && !hasInput)
        {
            Vector2 currentPos = arrow.anchoredPosition;
            currentPos.x += arrowSpeed * arrowDirection * Time.deltaTime;

            if (currentPos.x >= barWidth)
            {
                currentPos.x = barWidth;
                arrowDirection = -1f;
            }
            else if (currentPos.x <= 0)
            {
                currentPos.x = 0;
                arrowDirection = 1f;
            }

            arrow.anchoredPosition = currentPos;

            yield return null;
        }
    }

    /// <summary>
    /// Success 영역 체크 (Collider2D 물리 기반 직접 체크)
    /// Physics2D.OverlapBox를 사용한 정확한 충돌 감지
    /// </summary>
    private void CheckSuccess()
    {
        if (arrowCollider == null || successZoneCollider == null)
        {
            Debug.LogError("[TPEMinigame] Collider가 null입니다!");
            isSuccess = false;
            StartCoroutine(ShowResult());
            return;
        }

        // 두 Collider의 Bounds가 겹치는지 체크
        Bounds arrowBounds = arrowCollider.bounds;
        Bounds zoneBounds = successZoneCollider.bounds;

        isSuccess = arrowBounds.Intersects(zoneBounds);

        StartCoroutine(ShowResult());
    }

    /// <summary>
    /// 결과 표시 후 종료
    /// </summary>
    private IEnumerator ShowResult()
    {
        resultText.text = isSuccess ? "SUCCESS!" : "FAIL";
        resultText.color = isSuccess ? Color.green : Color.red;

        yield return new WaitForSeconds(0.5f);

        isPlaying = false;
        minigamePanel.SetActive(false);

        OnMinigameComplete?.Invoke(isSuccess);
    }
}