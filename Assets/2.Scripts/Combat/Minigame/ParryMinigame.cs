using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// �и� �̴ϰ���
/// - ���� ���� �� Ÿ�ֿ̹� ���� Space �Է�
/// - ����: ������ 50% ���� + �ݰ� ������ 50%
/// - ����: �Ϲ� ������ ����
/// </summary>
public class ParryMinigame : MonoBehaviour
{
    [Header("UI ����")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private RectTransform attackIndicator; // ���� Ÿ�̹� ǥ�� ��
    [SerializeField] private RectTransform parryZone;       // �и� ���� ����
    [SerializeField] private Text resultText;
    [SerializeField] private Text instructionText;

    [Header("����")]
    [SerializeField] private float shrinkSpeed = 200f;      // ���� �پ��� �ӵ�
    [SerializeField] private float parryZoneSize = 50f;     // �и� ���� ���� ũ��
    [SerializeField] private float timeLimitSeconds = 2f;   // ���� �ð�

    private bool isPlaying = false;
    private bool hasInput = false;
    private bool isSuccess = false;
    private float currentSize;
    private float timer;

    public event Action<bool> OnParryComplete; // true: ����, false: ����

    private void Awake()
    {
        minigamePanel.SetActive(false);
        Debug.Log("[ParryMinigame] �ʱ�ȭ �Ϸ�");
    }

    /// <summary>
    /// �̴ϰ��� ����
    /// </summary>
    public void StartMinigame()
    {
        Debug.Log("[ParryMinigame] ������ �и� �̴ϰ��� ���� ������");

        // �ʱ�ȭ
        isPlaying = true;
        hasInput = false;
        isSuccess = false;
        timer = 0f;

        // �ʱ� ũ�� ����
        currentSize = 300f;
        attackIndicator.sizeDelta = new Vector2(currentSize, currentSize);
        parryZone.sizeDelta = new Vector2(parryZoneSize, parryZoneSize);

        // UI Ȱ��ȭ
        minigamePanel.SetActive(true);
        resultText.text = "";
        instructionText.text = "Space Ű�� ���� �и�!";

        StartCoroutine(ShrinkIndicator());

        Debug.Log($"[ParryMinigame] ���� �ð�: {timeLimitSeconds}��, �и� ����: {parryZoneSize}px");
    }

    private void Update()
    {
        if (!isPlaying || hasInput) return;

        // �����̽� �Է�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            hasInput = true;
            CheckParrySuccess();
        }
    }

    /// <summary>
    /// ���� Ÿ�̹� �� �پ���
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

        // �Է� ������ �ڵ� ����
        if (!hasInput)
        {
            hasInput = true;
            isSuccess = false;
            Debug.Log("[ParryMinigame] Ÿ�Ӿƿ� - �ڵ� ����");
            StartCoroutine(ShowResult());
        }
    }

    /// <summary>
    /// �и� ���� ���� Ȯ��
    /// </summary>
    private void CheckParrySuccess()
    {
        // ���� ũ�Ⱑ �и� ������ ����ϸ� ����
        float sizeDifference = Mathf.Abs(currentSize - parryZoneSize);
        isSuccess = sizeDifference <= 30f; // ���� ���� ��30px

        Debug.Log($"[ParryMinigame] �и� ���� - ���� ũ��: {currentSize:F1}, ��ǥ ũ��: {parryZoneSize}, ����: {sizeDifference:F1} => {(isSuccess ? "����!" : "����")}");

        StartCoroutine(ShowResult());
    }

    /// <summary>
    /// ��� ǥ�� �� ����
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

        Debug.Log($"[ParryMinigame] ������ �и� �̴ϰ��� ����: {(isSuccess ? "����" : "����")} ������");
    }
}