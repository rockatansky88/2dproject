using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 몬스터 UI 슬롯
/// - HP 바 표시
/// - 몬스터 스프라이트
/// - 클릭 이벤트 (타겟 선택)
/// - 몬스터 턴 표시 (빨간색 외곽선 깜빡임)
/// - 턴 제어: 플레이어 턴에만 클릭 가능
/// - 사망 시 페이드아웃 효과
/// </summary>
public class MonsterUISlot : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Image monsterImage;
    [SerializeField] private Text nameText;

    [Header("HP 바")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Text hpText;

    [Header("선택 표시")]
    [SerializeField] private GameObject selectionIndicator;

    [Header("버튼")]
    [SerializeField] private Button selectButton;

    [Header("Damage Display")]
    [SerializeField] private Text damageText;
    [SerializeField] private float damageFloatSpeed = 50f;
    [SerializeField] private float damageFadeDuration = 1f;

    // 페이드아웃 설정
    [Header("Death FadeOut")]
    [SerializeField] private float fadeOutDuration = 1.5f; // 페이드아웃 시간
    [SerializeField] private CanvasGroup canvasGroup; // 전체 슬롯의 투명도 제어

    private Outline turnOutline;
    private Coroutine turnBlinkCoroutine;

    private Monster monster;

    // 클릭 이벤트
    public event Action<Monster> OnMonsterClicked;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialize(Monster target)
    {
        monster = target;

        if (monster == null)
        {
            Debug.LogError("[MonsterUISlot] monster가 null입니다!");
            return;
        }


        // 1. 기본 정보 설정 - 몬스터 이름
        if (nameText != null)
        {
            nameText.text = monster.Name;
        }

        // 2. 몬스터 스프라이트 설정
        if (monsterImage != null && monster.spawnData != null && monster.spawnData.monsterSprite != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
            monsterImage.preserveAspect = true;
            monsterImage.color = Color.white;

        }

        // 3. Outline 컴포넌트 자동 생성
        if (monsterImage != null)
        {
            turnOutline = monsterImage.GetComponent<Outline>();

            if (turnOutline == null)
            {
                turnOutline = monsterImage.gameObject.AddComponent<Outline>();
            }

            turnOutline.effectColor = new Color(1f, 0f, 0f, 1f);
            turnOutline.effectDistance = new Vector2(5f, 5f);
            turnOutline.enabled = false;

        }

        // 4. 스탯 변화 이벤트 구독
        if (monster.Stats != null)
        {
            monster.Stats.OnHPChanged += UpdateHPBar;
        }

        // 5. 초기 HP 바 업데이트
        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        // 6. 선택 표시 숨김
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }

        // 7. 버튼 이벤트 연결
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnButtonClicked);

        }

        // 8. 데미지 텍스트 초기 숨김
        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }

        // 
        //  CanvasGroup 없을경우 자동 생성 
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 초기 알파값 1 (완전 불투명)
        canvasGroup.alpha = 1f;

    }

    /// <summary>
    /// HP 바 업데이트
    /// </summary>
    private void UpdateHPBar(int currentHP, int maxHP)
    {
        if (hpFillImage != null)
        {
            float fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
            hpFillImage.fillAmount = fillAmount;

        }

        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        if (currentHP <= 0)
        {
            OnMonsterDeath();
        }
    }

    // 몬스터 사망- 페이드아웃 

    /// <summary>
    /// 몬스터 사망 처리 (페이드아웃 효과)
    /// </summary>
    private void OnMonsterDeath()
    {

        // 버튼 비활성화
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }

        // 턴 표시 비활성화
        SetTurnActive(false);

        // 페이드아웃 효과 시작
        StartCoroutine(FadeOutAndDestroy());
    }

    /// <summary>
    /// 페이드아웃 후 슬롯 비활성화
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ CanvasGroup이 없어서 즉시 비활성화");
            gameObject.SetActive(false);
            yield break;
        }

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        // 알파값을 1 → 0으로 서서히 감소
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // 완전히 투명하게
        canvasGroup.alpha = 0f;


        // 슬롯 비활성화 (파괴하지 않고 숨김)
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 선택 표시
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
        }
    }

    /// <summary>
    /// 몬스터 턴 표시 활성화/비활성화
    /// </summary>
    public void SetTurnActive(bool active)
    {
        if (turnOutline == null)
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ {gameObject.name}: Outline 컴포넌트가 없습니다!");
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
        }
        else
        {
            turnOutline.enabled = false;
        }
    }

    /// <summary>
    /// 빨간색 외곽선 깜빡임 효과
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
    /// 피격 데미지 표시
    /// 데미지 크기와 색상 설정, 애니메이션 재생
    /// </summary>
    /// <param name="damage">피해량</param>
    /// <param name="isCritical">크리티컬 여부 (크리티컬 시 노란색 + 크기 확대)</param>
    public void ShowDamage(int damage, bool isCritical = false)
    {
        if (damageText == null)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ damageText가 null입니다!");
            return;
        }

        if (isCritical)
        {
            damageText.text = $"CRITICAL!\n-{damage}";
            damageText.color = new Color(1f, 0.8f, 0f, 1f);
            damageText.fontSize = 24;
        }
        else
        {
            damageText.text = $"-{damage}";
            damageText.color = new Color(1f, 0f, 0f, 1f);
            damageText.fontSize = 18;
        }

        StartCoroutine(FloatingDamageAnimation());

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
    /// 몬스터 클릭 가능 여부 설정
    /// 플레이어 턴에만 클릭 가능하도록 제어
    /// </summary>
    /// <param name="interactable">true: 클릭 가능 (플레이어 턴), false: 클릭 불가 (몬스터 턴)</param>
    public void SetInteractable(bool interactable)
    {
        if (selectButton == null)
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ {gameObject.name}: selectButton이 null입니다!\n" +
                           $"  - Monster: {(monster != null ? monster.Name : "null")}\n" +
                           $"  - SetInteractable({interactable}) 호출 무시\n" +
                           $"  - Inspector에서 SelectButton을 할당해주세요");
            return;
        }

        selectButton.interactable = interactable;
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {

        if (monster == null)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ monster가 null입니다!");
            return;
        }

        if (!monster.IsAlive)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ 사망한 몬스터는 선택할 수 없습니다!");
            return;
        }

        // 버튼이 비활성화 상태면 클릭 무시
        // (Unity Button 컴포넌트가 이미 처리하지만 추가 안전장치)

        if (selectButton != null && !selectButton.interactable)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ 현재 몬스터를 선택할 수 없는 턴입니다!");
            return;
        }

        OnMonsterClicked?.Invoke(monster);
    }

    /// <summary>
    /// 몬스터 참조 반환
    /// </summary>
    public Monster GetMonster()
    {
        return monster;
    }

    private void OnDestroy()
    {
        if (monster != null && monster.Stats != null)
        {
            monster.Stats.OnHPChanged -= UpdateHPBar;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnButtonClicked);
        }

        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }
    }
}