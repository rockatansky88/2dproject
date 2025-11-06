using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// TPE (Timed Precision Event) 미니게임
/// 공격 시 Success 영역을 맞추면 크리티컬 확률 +30%
/// 난이도 대폭 상승: Success Zone 크기 축소, 화살표 속도 증가
/// </summary>
public class TPEMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Text resultText;

    [Header("설정 - 난이도 상승")]
    [SerializeField] private float arrowSpeed = 800f; // 화살표 속도 증가 (500 → 800)
    [SerializeField] private float barWidth = 600f;

    // Success Zone 크기 대폭 축소
    [SerializeField] private float normalSuccessZoneWidth = 40f;  // 70 → 40
    [SerializeField] private float eliteSuccessZoneWidth = 25f;   // 40 → 25
    [SerializeField] private float bossSuccessZoneWidth = 15f;    // 20 → 15

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float arrowDirection = 1f;

    public event Action<bool> OnMinigameComplete;

    private void Awake()
    {
        minigamePanel.SetActive(false);
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

        successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);

        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        arrowDirection = 1f;

        arrow.anchoredPosition = new Vector2(0, arrow.anchoredPosition.y);

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
    /// Success 영역 체크
    /// </summary>
    private void CheckSuccess()
    {
        float arrowX = arrow.anchoredPosition.x;

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