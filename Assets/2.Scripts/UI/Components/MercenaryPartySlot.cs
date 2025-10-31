using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 고용된 용병을 표시하는 파티 슬롯 (최대 4명)
/// 클릭하면 상세 팝업이 열립니다 (추방 모드).
/// 전투씬일 때는 HP/MP를 표시합니다.
/// </summary>
public class MercenaryPartySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;      // 초상화
    [SerializeField] private Button slotButton;        // 클릭 버튼
    [SerializeField] private GameObject emptySlotIndicator; // 빈 슬롯 표시 (예: "Empty" 텍스트)

    [Header("Combat UI - 전투씬에서만 표시")]
    [SerializeField] private GameObject combatStatsPanel; // HP/MP UI를 담은 부모 오브젝트
    [SerializeField] private Text hpText;              // HP 텍스트
    [SerializeField] private Text mpText;              // MP 텍스트
    [SerializeField] private Slider hpSlider;          // HP 슬라이더 (옵션)
    [SerializeField] private Slider mpSlider;          // MP 슬라이더 (옵션)

    [Header("Turn Indicator - 현재 턴 표시")]
    [SerializeField] private Image turnBorder;         // 빨간색 테두리 이미지

    private MercenaryInstance mercenaryData;
    private bool isCombatScene = false;

    // 이벤트
    public event Action<MercenaryInstance> OnSlotClicked;

    private void Awake()
    {
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(OnClicked);
            Debug.Log("[MercenaryPartySlot] 슬롯 버튼 리스너 등록됨");
        }

        // 초기 상태: 전투 스탯 UI 숨김
        SetCombatStatsVisible(false);

        // 턴 테두리 숨김
        if (turnBorder != null)
        {
            turnBorder.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 슬롯 초기화 (용병 데이터 설정)
    /// </summary>
    public void Initialize(MercenaryInstance mercenary)
    {
        mercenaryData = mercenary;

        Debug.Log($"[MercenaryPartySlot] Initialize - 용병: {mercenary?.mercenaryName ?? "null"}");

        if (mercenary == null)
        {
            SetEmpty();
            return;
        }

        // 빈 슬롯 표시 비활성화
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(false);
        }

        // 초상화
        if (portraitImage != null)
        {
            portraitImage.sprite = mercenary.portrait;
            portraitImage.enabled = true;
            portraitImage.color = Color.white;
        }

        // 전투씬일 경우 HP/MP 업데이트
        if (isCombatScene)
        {
            UpdateCombatStats(mercenary.health, mercenary.health, 50, 50); // TODO: 현재HP, 최대HP, 현재MP, 최대MP
        }

        Debug.Log($"[MercenaryPartySlot] ✅ 초기화 완료: {mercenary.mercenaryName}");
    }

    /// <summary>
    /// 빈 슬롯으로 설정
    /// </summary>
    public void SetEmpty()
    {
        mercenaryData = null;

        Debug.Log("[MercenaryPartySlot] 빈 슬롯으로 설정");

        // 빈 슬롯 표시 활성화
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }

        // UI 요소 비활성화
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        // 전투 스탯 UI 숨김
        SetCombatStatsVisible(false);

        // 턴 테두리 숨김
        SetTurnActive(false);
    }

    /// <summary>
    /// 전투씬 모드 설정
    /// </summary>
    public void SetCombatMode(bool isCombat)
    {
        isCombatScene = isCombat;
        SetCombatStatsVisible(isCombat && mercenaryData != null);

        Debug.Log($"[MercenaryPartySlot] 전투 모드 설정: {isCombat}");

        // 전투씬이면서 용병이 있으면 스탯 업데이트
        if (isCombat && mercenaryData != null)
        {
            UpdateCombatStats(mercenaryData.health, mercenaryData.health, 50, 50);
        }
    }

    /// <summary>
    /// 전투 스탯 UI 업데이트
    /// </summary>
    public void UpdateCombatStats(int currentHp, int maxHp, int currentMp, int maxMp)
    {
        if (!isCombatScene) return;

        Debug.Log($"[MercenaryPartySlot] 전투 스탯 업데이트: HP {currentHp}/{maxHp}, MP {currentMp}/{maxMp}");

        // HP 텍스트
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHp}/{maxHp}";
        }

        // MP 텍스트
        if (mpText != null)
        {
            mpText.text = $"MP: {currentMp}/{maxMp}";
        }

        // HP 슬라이더
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        // MP 슬라이더
        if (mpSlider != null)
        {
            mpSlider.maxValue = maxMp;
            mpSlider.value = currentMp;
        }
    }

    /// <summary>
    /// 전투 스탯 UI 표시/숨김
    /// </summary>
    private void SetCombatStatsVisible(bool visible)
    {
        if (combatStatsPanel != null)
        {
            combatStatsPanel.SetActive(visible);
        }
    }

    /// <summary>
    /// 현재 턴 표시 (빨간색 테두리)
    /// </summary>
    private Coroutine turnBlinkCoroutine;

    public void SetTurnActive(bool active)
    {
        if (turnBorder == null) return;

        if (active)
        {
            turnBorder.gameObject.SetActive(true);

            // 깜빡임 시작
            if (turnBlinkCoroutine != null)
            {
                StopCoroutine(turnBlinkCoroutine);
            }

            turnBlinkCoroutine = StartCoroutine(BlinkTurnBorder());

            Debug.Log($"[MercenaryPartySlot] ✨ {mercenaryData?.mercenaryName} 턴 표시 시작 (깜빡임)");
        }
        else
        {
            turnBorder.gameObject.SetActive(false);

            // 깜빡임 중지
            if (turnBlinkCoroutine != null)
            {
                StopCoroutine(turnBlinkCoroutine);
                turnBlinkCoroutine = null;
            }

            Debug.Log($"[MercenaryPartySlot] {mercenaryData?.mercenaryName} 턴 표시 종료");
        }
    }

    /// <summary>
    /// 빨간색 테두리 깜빡임 효과
    /// </summary>
    private IEnumerator BlinkTurnBorder()
    {
        float minAlpha = 0.4f; // R값 40%
        float maxAlpha = 0.65f; // R값 65%
        float speed = 2f; // 깜빡임 속도

        while (true)
        {
            // 알파값을 40% ~ 65% 사이로 반복
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * speed, 1f));

            // 빨간색으로 설정 (R=1, G=0, B=0, A=alpha)
            turnBorder.color = new Color(1f, 0f, 0f, alpha);

            yield return null;
        }
    }

    /// <summary>
    /// 슬롯 클릭 핸들러
    /// </summary>
    private void OnClicked()
    {
        Debug.Log($"[MercenaryPartySlot] 🖱️ 파티 슬롯 클릭됨: {mercenaryData?.mercenaryName ?? "Empty"}");

        // 빈 슬롯 클릭 시 무시
        if (mercenaryData == null)
        {
            Debug.Log("[MercenaryPartySlot] 빈 슬롯이므로 무시");
            return;
        }

        Debug.Log($"[MercenaryPartySlot] OnSlotClicked 이벤트 발생");
        OnSlotClicked?.Invoke(mercenaryData);
    }

    /// <summary>
    /// 용병 데이터 반환
    /// </summary>
    public MercenaryInstance GetMercenary()
    {
        return mercenaryData;
    }

    private void OnDestroy()
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnClicked);
        }
    }
}