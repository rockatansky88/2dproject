using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 고용된 용병을 표시하는 파티 슬롯 (최대 4명)
/// 클릭하면 상세 팝업이 열립니다 (추방 모드).
/// HP/MP UI는 용병이 배치되면 항상 표시됩니다 (마을/던전/전투 모두 동일).
/// 턴 표시: 용병 초상화 이미지에 빨간색 Outline 깜빡임
/// </summary>
public class MercenaryPartySlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;      // 초상화
    [SerializeField] private Button slotButton;        // 클릭 버튼
    [SerializeField] private GameObject emptySlotIndicator; // 빈 슬롯 표시 (예: "Empty" 텍스트)

    [Header("Combat UI - HP/MP 항상 표시")]
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

        SetCombatStatsVisible(false);

        InitializeTurnOutline();

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

        turnOutline = portraitImage.GetComponent<Outline>();

        if (turnOutline == null)
        {
            turnOutline = portraitImage.gameObject.AddComponent<Outline>();
            Debug.Log($"[MercenaryPartySlot] ✅ Outline 컴포넌트 자동 생성");
        }

        turnOutline.effectColor = new Color(1f, 0f, 0f, 1f);
        turnOutline.effectDistance = new Vector2(5f, 5f);
        turnOutline.enabled = false;

        Debug.Log($"[MercenaryPartySlot] ✅ Outline 초기화 완료 (빨간색, 두께 5)");
    }

    /// <summary>
    /// 슬롯 초기화 (용병 데이터 설정)
    /// HP/MP 스탯은 MercenaryInstance의 파생 스탯(maxHP/maxMP)을 사용하여 항상 표시합니다.
    /// </summary>
    public void Initialize(MercenaryInstance mercenary)
    {
        mercenaryData = mercenary;

        Debug.Log($"[MercenaryPartySlot] Initialize - 슬롯: {gameObject.name}\n" +
                  $"  용병: {mercenary?.GetDisplayName() ?? "null"}\n" +
                  $"  InstanceID: {mercenary?.instanceID}");

        if (mercenary == null)
        {
            SetEmpty();
            return;
        }

        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(false);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = mercenary.portrait;
            portraitImage.enabled = true;
            portraitImage.color = Color.white;
        }

        SetCombatStatsVisible(true);
        UpdateCombatStats(mercenary.currentHP, mercenary.maxHP, mercenary.currentMP, mercenary.maxMP);

        Debug.Log($"[MercenaryPartySlot] ✅ 초기화 완료: {mercenary.GetDisplayName()}\n" +
                  $"  Slot: {gameObject.name}\n" +
                  $"  HP: {mercenary.currentHP}/{mercenary.maxHP}\n" +
                  $"  MP: {mercenary.currentMP}/{mercenary.maxMP}\n" +
                  $"  스탯 UI: 항상 표시");
    }

    /// <summary>
    /// 빈 슬롯으로 설정
    /// </summary>
    public void SetEmpty()
    {
        mercenaryData = null;

        Debug.Log("[MercenaryPartySlot] 빈 슬롯으로 설정");

        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }

        SetCombatStatsVisible(false);
        SetTurnActive(false);
    }

    /// <summary>
    /// 전투씬 모드 설정
    /// 전투 시작 시 호출되며, HP/MP UI는 이미 표시 중이므로 데이터만 갱신합니다.
    /// </summary>
    public void SetCombatMode(bool isCombat)
    {
        isCombatScene = isCombat;

        Debug.Log($"[MercenaryPartySlot] 전투 모드 설정: {isCombat}");

        if (mercenaryData != null)
        {
            UpdateCombatStats(mercenaryData.currentHP, mercenaryData.maxHP, mercenaryData.currentMP, mercenaryData.maxMP);

            Debug.Log($"[MercenaryPartySlot] 전투 스탯 갱신 - {mercenaryData.mercenaryName}: HP {mercenaryData.currentHP}/{mercenaryData.maxHP}, MP {mercenaryData.currentMP}/{mercenaryData.maxMP}");
        }
    }

    /// <summary>
    /// 전투 스탯 UI 업데이트
    /// MercenaryInstance의 파생 스탯(maxHP/maxMP)을 사용하여 HP/MP를 표시합니다.
    /// 이 값은 상세 팝업 및 인벤토리에서 표시되는 값과 동일합니다.
    /// </summary>
    public void UpdateCombatStats(int currentHp, int maxHp, int currentMp, int maxMp)
    {
        Debug.Log($"[MercenaryPartySlot] 전투 스탯 업데이트: HP {currentHp}/{maxHp}, MP {currentMp}/{maxMp}");

        if (hpText != null)
        {
            hpText.text = $"HP: {currentHp}/{maxHp}";
        }

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

        damageText.text = $"-{damage}";

        if (isCritical)
        {
            damageText.color = new Color(1f, 0.8f, 0f, 1f);
            damageText.fontSize = 24;
        }
        else
        {
            damageText.color = new Color(1f, 0f, 0f, 1f);
            damageText.fontSize = 18;
        }

        StartCoroutine(FloatingDamageAnimation());

        Debug.Log($"[MercenaryPartySlot] ✅ {mercenaryData.mercenaryName} 피격 표시: -{damage} (크리티컬: {isCritical})");
    }

    /// <summary>
    /// 데미지 텍스트 애니메이션 (위로 떠오르면서 사라짐)
    /// </summary>
    private IEnumerator FloatingDamageAnimation()
    {
        damageText.gameObject.SetActive(true);

        Vector3 startPosition = damageText.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < damageFadeDuration)
        {
            elapsedTime += Time.deltaTime;

            float yOffset = damageFloatSpeed * Time.deltaTime;
            damageText.transform.localPosition += new Vector3(0, yOffset, 0);

            Color color = damageText.color;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / damageFadeDuration);
            damageText.color = color;

            yield return null;
        }

        damageText.gameObject.SetActive(false);
        damageText.transform.localPosition = startPosition;
    }

    /// <summary>
    /// 현재 턴 표시 활성화/비활성화
    /// 용병 초상화 이미지의 Outline 컴포넌트를 사용하여 빨간색 외곽선 깜빡임
    /// </summary>
    public void SetTurnActive(bool active)
    {
        if (turnOutline == null)
        {
            Debug.LogWarning($"[MercenaryPartySlot] ⚠️ {gameObject.name}: Outline 컴포넌트가 없습니다!\n" +
                           $"  - Mercenary: {(mercenaryData != null ? mercenaryData.mercenaryName : "null")}\n" +
                           $"  - SetTurnActive({active}) 호출 무시\n" +
                           $"  - Outline은 Awake()에서 자동 생성됩니다");
            return;
        }

        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }

        if (active)
        {
            turnOutline.enabled = true;
            turnBlinkCoroutine = StartCoroutine(BlinkTurnOutline());
            Debug.Log($"[MercenaryPartySlot] ✅ {mercenaryData?.mercenaryName} 턴 표시 활성화 (빨간색 외곽선 깜빡임 시작)");
        }
        else
        {
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
        float blinkSpeed = 2f;
        bool fadingOut = true;

        while (true)
        {
            Color color = turnOutline.effectColor;

            if (fadingOut)
            {
                color.a -= Time.deltaTime * blinkSpeed;
                if (color.a <= 0.5f)
                {
                    color.a = 0.5f;
                    fadingOut = false;
                }
            }
            else
            {
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

        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }
    }

    /// <summary>
    /// 턴 하이라이트 강제 제거 (마을 귀환 시)
    /// </summary>
    public void ResetHighlight()
    {
        Debug.Log($"[MercenaryPartySlot] 하이라이트 제거: {mercenaryData?.mercenaryName ?? "Empty"}");

        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }

        if (turnOutline != null)
        {
            turnOutline.enabled = false;
        }
    }
}