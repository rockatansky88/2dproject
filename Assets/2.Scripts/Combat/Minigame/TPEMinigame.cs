using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// TPE (Timed Precision Event) 미니게임
/// - 공격 시: Success 영역 맞추면 크리티컬 확률 +30%
/// - 방어 시: Success 영역 맞추면 데미지 0 (회피)
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
    [SerializeField] private float barWidth = 600f; // 바의 전체 너비 (0 ~ 600)
    [SerializeField] private float normalSuccessZoneWidth = 70f; // 일반 몬스터 Success 영역 크기
    [SerializeField] private float eliteSuccessZoneWidth = 40f;   // 엘리트 영역
    [SerializeField] private float bossSuccessZoneWidth = 20f;    // 보스 영역

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float arrowDirection = 1f; // 1 또는 -1 (좌우 이동)

    public event Action<bool> OnMinigameComplete; // true: 성공, false: 실패

    private void Awake()
    {
        minigamePanel.SetActive(false);
    }

    /// <summary>
    /// 미니게임 시작
    /// </summary>
    /// <param name="difficulty">난이도 (Normal, Elite, Boss)</param>
    public void StartMinigame(MonsterDifficulty difficulty)
    {

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

        // 화살표 시작 위치를 0으로 설정 (왼쪽 끝)
        arrow.anchoredPosition = new Vector2(0, arrow.anchoredPosition.y);


        // UI 활성화
        minigamePanel.SetActive(true);
        resultText.text = "";

        StartCoroutine(MoveArrow());

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
    /// 화살표 이동 (0 ~ barWidth 사이를 왔다갔다)
    /// </summary>
    private IEnumerator MoveArrow()
    {
        while (isPlaying && !hasInput)
        {
            // 화살표 이동
            Vector2 currentPos = arrow.anchoredPosition;
            currentPos.x += arrowSpeed * arrowDirection * Time.deltaTime;

            //  0 ~ barWidth 범위로 제한하고 방향 반전

            if (currentPos.x >= barWidth)
            {
                currentPos.x = barWidth;
                arrowDirection = -1f; // 왼쪽으로
            }
            else if (currentPos.x <= 0)
            {
                currentPos.x = 0;
                arrowDirection = 1f; // 오른쪽으로
            }

            arrow.anchoredPosition = currentPos;

            yield return null;
        }
    }

    /// <summary>
    /// Success 영역 체크
    /// </summary>
    private void CheckSuccess()
    {
        float arrowX = arrow.anchoredPosition.x;

        //  successZone의 위치도 0 ~ barWidth 기준으로 계산
        float zoneLeft = successZone.anchoredPosition.x - (successZone.rect.width / 2f);
        float zoneRight = successZone.anchoredPosition.x + (successZone.rect.width / 2f);

        isSuccess = arrowX >= zoneLeft && arrowX <= zoneRight;


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