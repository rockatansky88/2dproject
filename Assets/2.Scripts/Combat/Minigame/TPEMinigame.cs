using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// TPE (Timed Precision Event) �̴ϰ���
/// - ���� ��: Success ���� ���߸� ũ��Ƽ�� Ȯ�� +30%
/// - ��� ��: Success ���� ���߸� ������ 0 (�и�)
/// </summary>
public class TPEMinigame : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform successZone;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Text resultText;

    [Header("����")]
    [SerializeField] private float arrowSpeed = 500f; // ȭ��ǥ �̵� �ӵ�
    [SerializeField] private float normalSuccessZoneWidth = 100f; // �Ϲ� ���� Success ���� ũ��
    [SerializeField] private float eliteSuccessZoneWidth = 70f;   // ���� ����
    [SerializeField] private float bossSuccessZoneWidth = 50f;    // ���� ����

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float arrowDirection = 1f; // 1 �Ǵ� -1 (�¿� �̵�)

    public event Action<bool> OnMinigameComplete; // true: ����, false: ����

    private void Awake()
    {
        minigamePanel.SetActive(false);
        Debug.Log("[TPEMinigame] �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// �̴ϰ��� ����
    /// </summary>
    /// <param name="difficulty">���̵� (Normal, Elite, Boss)</param>
    public void StartMinigame(MonsterDifficulty difficulty)
    {
        Debug.Log($"[TPEMinigame] ������ �̴ϰ��� ���� (���̵�: {difficulty}) ������");

        // Success ���� ũ�� ����
        float zoneWidth = difficulty switch
        {
            MonsterDifficulty.Elite => eliteSuccessZoneWidth,
            MonsterDifficulty.Boss => bossSuccessZoneWidth,
            _ => normalSuccessZoneWidth
        };

        successZone.sizeDelta = new Vector2(zoneWidth, successZone.sizeDelta.y);

        // �ʱ�ȭ
        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        arrowDirection = 1f;

        // ȭ��ǥ �ʱ� ��ġ (���� ��)
        arrow.anchoredPosition = new Vector2(-barBackground.rect.width / 2f, 0);

        // UI Ȱ��ȭ
        minigamePanel.SetActive(true);
        resultText.text = "";

        StartCoroutine(MoveArrow());

        Debug.Log($"[TPEMinigame] Success ���� ũ��: {zoneWidth}px");
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        // �����̽��� �Է�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckSuccess();
        }
    }

    /// <summary>
    /// ȭ��ǥ �̵�
    /// </summary>
    private IEnumerator MoveArrow()
    {
        float barWidth = barBackground.rect.width;
        float halfWidth = barWidth / 2f;

        while (isPlaying && !hasInput)
        {
            // ȭ��ǥ �̵�
            Vector2 currentPos = arrow.anchoredPosition;
            currentPos.x += arrowSpeed * arrowDirection * Time.deltaTime;

            // ���� �����ϸ� ���� ��ȯ
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
    /// Success ���� ����
    /// </summary>
    private void CheckSuccess()
    {
        float arrowX = arrow.anchoredPosition.x;
        float zoneLeft = successZone.anchoredPosition.x - (successZone.rect.width / 2f);
        float zoneRight = successZone.anchoredPosition.x + (successZone.rect.width / 2f);

        isSuccess = arrowX >= zoneLeft && arrowX <= zoneRight;

        Debug.Log($"[TPEMinigame] ���� - ȭ��ǥ ��ġ: {arrowX:F1}, Success ����: [{zoneLeft:F1}, {zoneRight:F1}] => {(isSuccess ? "����!" : "����")}");

        StartCoroutine(ShowResult());
    }

    /// <summary>
    /// ��� ǥ�� �� ����
    /// </summary>
    private IEnumerator ShowResult()
    {
        resultText.text = isSuccess ? "SUCCESS!" : "FAIL";
        resultText.color = isSuccess ? Color.green : Color.red;

        yield return new WaitForSeconds(0.5f);

        isPlaying = false;
        minigamePanel.SetActive(false);

        OnMinigameComplete?.Invoke(isSuccess);

        Debug.Log($"[TPEMinigame] ������ �̴ϰ��� ����: {(isSuccess ? "����" : "����")} ������");
    }
}