using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// TPE (Timed Precision Event) 미니게임
/// - 공격 시: Success 영역 맞추면 크리티컬 확률 +30%
/// - 방어 시: Success 영역 맞추면 데미지 0 (패링)
/// </summary>
public class TPEMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Text resultText;

    [Header("설정")]
    [SerializeField] private float arrowSpeed = 500f; // 화살표 이동 속도
    [SerializeField] private float normalSuccessZoneWidth = 100f; // 일반 몬스터 Success 영역 크기
    [SerializeField] private float eliteSuccessZoneWidth = 70f;   // 정예 몬스터
    [SerializeField] private float bossSuccessZoneWidth = 50f;    // 보스 몬스터

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float arrowDirection = 1f; // 1 또는 -1 (좌우 이동)

    public event Action<bool> OnMinigameComplete; // true: 성공, false: 실패

    private void Awake()
    {
        minigamePanel.SetActive(false);
        Debug.Log("[TPEMinigame] 초기화 완료");
    }

    /// <summary>
    /// 미니게임 시작
    /// </summary>
    /// <param name="difficulty">난이도 (Normal, Elite, Boss)</param>
    public void StartMinigame(MonsterDifficulty difficulty)
    {
        Debug.Log($"[TPEMinigame] ━━━ 미니게임 시작 (난이도: {difficulty}) ━━━");

        // Success 영역 크기 설정
        float zoneWidth = difficulty switch
        {
            MonsterDifficulty.Elite => eliteSuccessZoneWidth,
            MonsterDifficulty.Boss => bossSuccessZoneWidth,
            _ => normalSuccessZoneWidth
        };

        successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);

        // 초기화
        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        arrowDirection = 1f;

        // 화살표 초기 위치 (좌측 끝)
        arrow.anchoredPosition = new Vector2(-barBackground.rect.width / 2f, 0);

        // UI 활성화
        minigamePanel.SetActive(true);
        resultText.text = "";

        StartCoroutine(MoveArrow());

        Debug.Log($"[TPEMinigame] Success 영역 크기: {zoneWidth}px");
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        // 스페이스바 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckSuccess();
        }
    }

    /// <summary>
    /// 화살표 이동
    /// </summary>
    private IEnumerator MoveArrow()
    {
        float barWidth = barBackground.rect.width;
        float halfWidth = barWidth / 2f;

        while (isPlaying && !hasInput)
        {
            // 화살표 이동
            Vector2 currentPos = arrow.anchoredPosition;
            currentPos.x += arrowSpeed * arrowDirection * Time.deltaTime;

            // 끝에 도달하면 방향 전환
            if (currentPos.x >= halfWidth)
            {
                currentPos.x = halfWidth;
                arrowDirection = -1f;
            }
            else if (currentPos.x <= -halfWidth)
            {
                currentPos.x = -halfWidth;
                arrowDirection = 1f;
            }

            arrow.anchoredPosition = currentPos;

            yield return null;
        }
    }

    /// <summary>
    /// Success 영역 판정
    /// </summary>
    private void CheckSuccess()
    {
        float arrowX = arrow.anchoredPosition.x;
        float zoneLeft = successZone.anchoredPosition.x - (successZone.rect.width / 2f);
        float zoneRight = successZone.anchoredPosition.x + (successZone.rect.width / 2f);

        isSuccess = arrowX >= zoneLeft && arrowX <= zoneRight;

        Debug.Log($"[TPEMinigame] 판정 - 화살표 위치: {arrowX:F1}, Success 영역: [{zoneLeft:F1}, {zoneRight:F1}] => {(isSuccess ? "성공!" : "실패")}");

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

        Debug.Log($"[TPEMinigame] ━━━ 미니게임 종료: {(isSuccess ? "성공" : "실패")} ━━━");
    }
}