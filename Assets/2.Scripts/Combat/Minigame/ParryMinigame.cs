using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 패링 미니게임
/// - 몬스터 공격 시 타이밍에 맞춰 Space 입력
/// - 성공: 데미지 50% 감소 + 반격 데미지 50%
/// - 실패: 일반 데미지 적용
/// </summary>
public class ParryMinigame : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform attackIndicator; // 공격 타이밍 표시 원
    [SerializeField] private RectTransform parryZone;       // 패링 성공 구간
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    [Header("설정")]
    [SerializeField] private float shrinkSpeed = 200f;      // 원이 줄어드는 속도
    [SerializeField] private float parryZoneSize = 50f;     // 패링 성공 구간 크기
    [SerializeField] private float timeLimitSeconds = 2f;   // 제한 시간

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float currentSize;
    private float timer;

    public event Action<bool> OnParryComplete; // true: 성공, false: 실패

    private void Awake()
    {
        minigamePanel.SetActive(false);
        Debug.Log("[ParryMinigame] 초기화 완료");
    }

    /// <summary>
    /// 미니게임 시작
    /// </summary>
    public void StartMinigame()
    {
        Debug.Log("[ParryMinigame] ━━━ 패링 미니게임 시작 ━━━");

        // 초기화
        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        timer = 0f;

        // 초기 크기 설정
        currentSize = 300f;
        attackIndicator.sizeDelta = new Vector2(currentSize, currentSize);
        parryZone.sizeDelta = new Vector2(parryZoneSize, parryZoneSize);

        // UI 활성화
        minigamePanel.SetActive(true);
        resultText.text = "";
        instructionText.text = "Space 키를 눌러 패링!";

        StartCoroutine(ShrinkIndicator());

        Debug.Log($"[ParryMinigame] 제한 시간: {timeLimitSeconds}초, 패링 구간: {parryZoneSize}px");
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        // 스페이스 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckParrySuccess();
        }
    }

    /// <summary>
    /// 공격 타이밍 원 줄어들기
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

        // 입력 없으면 자동 실패
        if (!hasInput)
        {
            hasInput = true;
            isSuccess = false;
            Debug.Log("[ParryMinigame] 타임아웃 - 자동 실패");
            StartCoroutine(ShowResult());
        }
    }

    /// <summary>
    /// 패링 성공 여부 확인
    /// </summary>
    private void CheckParrySuccess()
    {
        // 원의 크기가 패링 구간과 비슷하면 성공
        float sizeDifference = Mathf.Abs(currentSize - parryZoneSize);
        isSuccess = sizeDifference <= 30f; // 오차 범위 ±30px

        Debug.Log($"[ParryMinigame] 패링 판정 - 현재 크기: {currentSize:F1}, 목표 크기: {parryZoneSize}, 오차: {sizeDifference:F1} => {(isSuccess ? "성공!" : "실패")}");

        StartCoroutine(ShowResult());
    }

    /// <summary>
    /// 결과 표시 및 종료
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

        Debug.Log($"[ParryMinigame] ━━━ 패링 미니게임 종료: {(isSuccess ? "성공" : "실패")} ━━━");
    }
}