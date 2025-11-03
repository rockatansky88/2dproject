using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 고용된 용병을 표시하는 파티 슬롯 (최대 4명)
/// 클릭하면 상세 팝업이 열립니다 (추방 모드).
/// 전투씬일 때는 HP/MP를 표시합니다.
/// 턴 표시: 용병 초상화 이미지에 빨간색 Outline 깜빡임
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

    [SerializeField] private Image hpFillImage;        // HP Fill Image
    [SerializeField] private Image mpFillImage;        // MP Fill Image

    [Header("Damage Display")]
    [SerializeField] private Text damageText;          // 데미지 표시 텍스트
    [SerializeField] private float damageFloatSpeed = 50f; // 위로 올라가는 속도
    [SerializeField] private float damageFadeDuration = 1f; // 사라지는 시간

    private Outline turnOutline; // 용병 초상화 이미지의 Outline 컴포넌트
    private Coroutine turnBlinkCoroutine; // 깜빡임 코루틴 참조

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

        InitializeTurnOutline();

        // 🆕 추가: 데미지 텍스트 초기 숨김
        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Outline 컴포넌트 초기화 또는 자동 생성
    /// 용병 초상화 이미지(portraitImage)에 Outline을 추가하여 턴 표시에 사용
    /// </summary>
    private void InitializeTurnOutline()
    {
        if (portraitImage == null)
        {
            Debug.LogWarning("[MercenaryPartySlot] ⚠️ portraitImage가 null이어서 Outline을 생성할 수 없습니다!");
            return;
        }

        // 기존 Outline 컴포넌트가 있는지 확인
        turnOutline = portraitImage.GetComponent<Outline>();

        // 없으면 새로 추가
        if (turnOutline == null)
        {
            turnOutline = portraitImage.gameObject.AddComponent<Outline>();
            Debug.Log($"[MercenaryPartySlot] ✅ Outline 컴포넌트 자동 생성");
        }

        // 초기 설정: 빨간색, 두께 5, 비활성화
        turnOutline.effectColor = new Color(1f, 0f, 0f, 1f); // 빨간색
        turnOutline.effectDistance = new Vector2(5f, 5f); // 외곽선 두께
        turnOutline.enabled = false; // 초기엔 비활성화

        Debug.Log($"[MercenaryPartySlot] ✅ Outline 초기화 완료 (빨간색, 두께 5)");
    }

    /// <summary>
    /// 슬롯 초기화 (용병 데이터 설정)
    /// 전투/비전투 상관없이 스탯 기반 HP/MP를 사용합니다.
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


        //  스탯 기반 HP/MP 사용 (maxHP/maxMP)
        // 전투씬일 경우에만 표시하지만, 값은 스탯 기반으로 사용

        if (isCombatScene)
        {
            UpdateCombatStats(mercenary.currentHP, mercenary.maxHP, mercenary.currentMP, mercenary.maxMP);
            Debug.Log($"[MercenaryPartySlot] 전투 스탯 초기화: HP {mercenary.currentHP}/{mercenary.maxHP}, MP {mercenary.currentMP}/{mercenary.maxMP}");
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

        // 턴 외곽선 비활성화
        SetTurnActive(false);
    }

    /// <summary>
    /// 전투씬 모드 설정
    /// 전투 시작 시 스탯 기반 HP/MP를 즉시 표시합니다.
    /// </summary>
    public void SetCombatMode(bool isCombat)
    {
        isCombatScene = isCombat;
        SetCombatStatsVisible(isCombat && mercenaryData != null);

        Debug.Log($"[MercenaryPartySlot] 전투 모드 설정: {isCombat}");

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 🔧 수정: 스탯 기반 HP/MP 사용 (maxHP/maxMP)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        if (isCombat && mercenaryData != null)
        {
            // ❌ 기존 코드: UpdateCombatStats(mercenaryData.health, mercenaryData.health, 50, 50);

            // ✅ 수정: 스탯 기반 maxHP/maxMP 사용
            UpdateCombatStats(mercenaryData.currentHP, mercenaryData.maxHP, mercenaryData.currentMP, mercenaryData.maxMP);

            Debug.Log($"[MercenaryPartySlot] 전투 스탯 표시 - {mercenaryData.mercenaryName}: HP {mercenaryData.currentHP}/{mercenaryData.maxHP}, MP {mercenaryData.currentMP}/{mercenaryData.maxMP}");
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

        if (hpFillImage != null)
        {
            float hpFill = maxHp > 0 ? (float)currentHp / maxHp : 0f;
            hpFillImage.fillAmount = hpFill;
            Debug.Log($"[MercenaryPartySlot] HP Fill 업데이트: {hpFill:P0} ({currentHp}/{maxHp})");
        }
        else
        {
            Debug.LogWarning("[MercenaryPartySlot] ⚠️ hpFillImage가 null입니다!");
        }

        if (mpFillImage != null)
        {
            float mpFill = maxMp > 0 ? (float)currentMp / maxMp : 0f;
            mpFillImage.fillAmount = mpFill;
            Debug.Log($"[MercenaryPartySlot] MP Fill 업데이트: {mpFill:P0} ({currentMp}/{maxMp})");
        }
        else
        {
            Debug.LogWarning("[MercenaryPartySlot] ⚠️ mpFillImage가 null입니다!");
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
    /// 피격 데미지 표시
    /// 빨간색 텍스트가 위로 올라가면서 서서히 사라지는 애니메이션
    /// </summary>
    /// <param name="damage">피해량</param>
    /// <param name="isCritical">크리티컬 여부 (크리티컬 시 노란색 + 크기 확대)</param>
    public void ShowDamage(int damage, bool isCritical = false)
    {
        if (damageText == null)
        {
            Debug.LogWarning("[MercenaryPartySlot] ⚠️ damageText가 null입니다! Inspector에서 할당해주세요");
            return;
        }

        // 🔧 데미지 텍스트 설정
        damageText.text = $"-{damage}";

        // 🔧 크리티컬 여부에 따른 스타일 변경
        if (isCritical)
        {
            damageText.color = new Color(1f, 0.8f, 0f, 1f); // 오렌지색
            damageText.fontSize = 24; // 크기 확대
        }
        else
        {
            damageText.color = new Color(1f, 0f, 0f, 1f); // 빨간색
            damageText.fontSize = 18; // 기본 크기
        }

        // 🔧 애니메이션 시작
        StartCoroutine(FloatingDamageAnimation());

        Debug.Log($"[MercenaryPartySlot] ✅ {mercenaryData.mercenaryName} 피격 표시: -{damage} (크리티컬: {isCritical})");
    }

    /// <summary>
    /// 데미지 텍스트 애니메이션 (위로 떠오르면서 사라짐)
    /// </summary>
    private IEnumerator FloatingDamageAnimation()
    {
        damageText.gameObject.SetActive(true);

        // 초기 위치 저장
        Vector3 startPosition = damageText.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < damageFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // 위로 이동
            float yOffset = damageFloatSpeed * Time.deltaTime;
            damageText.transform.localPosition += new Vector3(0, yOffset, 0);

            // 알파값 감소 (서서히 사라짐)
            Color color = damageText.color;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / damageFadeDuration);
            damageText.color = color;

            yield return null;
        }

        // 애니메이션 종료 후 숨김 및 위치 초기화
        damageText.gameObject.SetActive(false);
        damageText.transform.localPosition = startPosition;
    }


    // Outline 기반 턴 표시로 변경
    /// <summary>
    /// 현재 턴 표시 활성화/비활성화
    /// 용병 초상화 이미지의 Outline 컴포넌트를 사용하여 빨간색 외곽선 깜빡임
    /// </summary>
    public void SetTurnActive(bool active)
    {
        // Outline null 체크
        if (turnOutline == null)
        {
            Debug.LogWarning($"[MercenaryPartySlot] ⚠️ {gameObject.name}: Outline 컴포넌트가 없습니다!\n" +
                           $"  - Mercenary: {(mercenaryData != null ? mercenaryData.mercenaryName : "null")}\n" +
                           $"  - SetTurnActive({active}) 호출 무시\n" +
                           $"  - Outline은 Awake()에서 자동 생성됩니다");
            return;
        }

        // 기존 깜빡임 코루틴 중지
        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }

        if (active)
        {
            // 턴 표시 활성화 및 깜빡임 시작
            turnOutline.enabled = true;
            turnBlinkCoroutine = StartCoroutine(BlinkTurnOutline());
            Debug.Log($"[MercenaryPartySlot] ✅ {mercenaryData?.mercenaryName} 턴 표시 활성화 (빨간색 외곽선 깜빡임 시작)");
        }
        else
        {
            // 턴 표시 비활성화
            turnOutline.enabled = false;
            Debug.Log($"[MercenaryPartySlot] {(mercenaryData != null ? mercenaryData.mercenaryName : "Unknown")} 턴 표시 비활성화");
        }
    }

    /// <summary>
    /// 빨간색 외곽선 깜빡임 효과
    /// Outline의 알파값을 0.5 ~ 1.0 사이에서 반복하여 깜빡이는 효과 생성
    /// </summary>
    private IEnumerator BlinkTurnOutline()
    {
        float blinkSpeed = 2f; // 깜빡임 속도
        bool fadingOut = true;

        while (true)
        {
            Color color = turnOutline.effectColor;

            if (fadingOut)
            {
                // 투명하게
                color.a -= Time.deltaTime * blinkSpeed;
                if (color.a <= 0.5f)
                {
                    color.a = 0.5f;
                    fadingOut = false;
                }
            }
            else
            {
                // 불투명하게
                color.a += Time.deltaTime * blinkSpeed;
                if (color.a >= 1f)
                {
                    color.a = 1f;
                    fadingOut = true;
                }
            }

            turnOutline.effectColor = color;
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

        // 코루틴 정리
        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }
    }
}