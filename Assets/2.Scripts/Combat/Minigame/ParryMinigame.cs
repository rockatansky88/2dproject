using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 패링 미니게임
/// 몬스터 공격 시 타이밍 맞추고 Space 입력
/// 성공: 데미지 무효화, 실패: 일반 데미지 적용
/// </summary>
public class ParryMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform attackIndicator;
    [SerializeField] private RectTransform parryZone;
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    [Header("설정 - 난이도 극상")]
    [SerializeField] private float shrinkSpeed = 1200f;
    [SerializeField] private float parryZoneSize = 40f;
    [SerializeField] private float timeLimitSeconds = 1.0f;
    [SerializeField] private float parryTolerance = 8f;

    [Header("추가 난이도 옵션")]
    [SerializeField] private bool useAcceleration = true;          // 가속 사용 여부
    [SerializeField] private float accelerationRate = 100f;        // 시간에 따른 가속도
    [SerializeField] private bool usePerfectTimingOnly = false;    // 완벽한 타이밍만 인정 (5 이하)
    [SerializeField] private float perfectTolerance = 5f;          // 완벽한 타이밍 범위

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float currentSize;
    private float currentShrinkSpeed;
    private float timer;

    public event Action<bool> OnParryComplete;

    private void Awake()
    {
        minigamePanel.SetActive(false);
    }

    /// <summary>
    /// 미니게임 시작
    /// </summary>
    public void StartMinigame()
    {
        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        timer = 0f;

        currentSize = 300f;
        currentShrinkSpeed = shrinkSpeed;

        attackIndicator.sizeDelta = new Vector2(currentSize, currentSize);
        parryZone.sizeDelta = new Vector2(parryZoneSize, parryZoneSize);

        minigamePanel.SetActive(true);
        resultText.text = "";
        instructionText.text = "Space 키로 정확히 패링!";

        StartCoroutine(ShrinkIndicator());
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckParrySuccess();
        }
    }

    /// <summary>
    /// 공격 타이밍 바 축소 (가속 적용)
    /// </summary>
    private IEnumerator ShrinkIndicator()
    {
        while (isPlaying && !hasInput && timer < timeLimitSeconds)
        {
            timer += Time.deltaTime;

            // 가속 적용 - 시간이 지날수록 더 빨라짐
            if (useAcceleration)
            {
                currentShrinkSpeed = shrinkSpeed + (accelerationRate * timer);
            }

            currentSize -= currentShrinkSpeed * Time.deltaTime;

            if (currentSize < 0f)
            {
                currentSize = 0f;
            }

            attackIndicator.sizeDelta = new Vector2(currentSize, currentSize);

            yield return null;
        }

        if (!hasInput)
        {
            hasInput = true;
            isSuccess = false;
            StartCoroutine(ShowResult());
        }
    }

    /// <summary>
    /// 패링 성공 여부 확인
    /// 판정 범위를 극도로 엄격하게 설정
    /// </summary>
    private void CheckParrySuccess()
    {
        float sizeDifference = Mathf.Abs(currentSize - parryZoneSize);

        if (usePerfectTimingOnly)
        {
            // 완벽한 타이밍만 인정하는 모드
            isSuccess = sizeDifference <= perfectTolerance;
        }
        else
        {
            // 일반 모드 (여전히 매우 어려움)
            isSuccess = sizeDifference <= parryTolerance;
        }

        StartCoroutine(ShowResult());
    }

    /// <summary>
    /// 결과 표시 후 종료
    /// </summary>
    private IEnumerator ShowResult()
    {
        resultText.text = isSuccess ? "PARRY SUCCESS!" : "PARRY FAILED";
        resultText.color = isSuccess ? Color.cyan : Color.red;
        instructionText.text = "";

        yield return new WaitForSeconds(0.5f);

        isPlaying = false;
        minigamePanel.SetActive(false);

        OnParryComplete?.Invoke(isSuccess);
    }
}