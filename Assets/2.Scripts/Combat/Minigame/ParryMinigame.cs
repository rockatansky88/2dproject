using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 패링 미니게임
/// 몬스터 공격 시 타이밍 맞춰 Space 입력
/// 성공: 데미지 무효화, 실패: 일반 데미지 적용
/// 난이도 대폭 상승: 축소 속도 증가, 판정 범위 축소
/// </summary>
public class ParryMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform attackIndicator;
    [SerializeField] private RectTransform parryZone;
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    [Header("설정 - 난이도 상승")]
    [SerializeField] private float shrinkSpeed = 350f;      // 축소 속도 증가 (200 → 350)
    [SerializeField] private float parryZoneSize = 50f;
    [SerializeField] private float timeLimitSeconds = 1.5f; // 제한 시간 단축 (2초 → 1.5초)
    [SerializeField] private float parryTolerance = 15f;    // 판정 범위 축소 (30 → 15)

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float currentSize;
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
    /// 공격 타이밍 바 축소
    /// </summary>
    private IEnumerator ShrinkIndicator()
    {
        while (isPlaying && !hasInput && timer < timeLimitSeconds)
        {
            timer += Time.deltaTime;
            currentSize -= shrinkSpeed * Time.deltaTime;

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
    /// 판정 범위를 더 엄격하게 설정
    /// </summary>
    private void CheckParrySuccess()
    {
        float sizeDifference = Mathf.Abs(currentSize - parryZoneSize);
        isSuccess = sizeDifference <= parryTolerance; // 30 → 15

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