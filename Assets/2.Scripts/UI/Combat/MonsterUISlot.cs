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
/// - 공격 애니메이션 재생 (SpriteAnimator 연동)
/// - 몬스터별 개별 애니메이션 클립 동적 설정
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

    [Header("Death FadeOut")]
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private SpriteAnimator spriteAnimator;

    private Outline turnOutline;
    private Coroutine turnBlinkCoroutine;

    private Monster monster;

    public event Action<Monster> OnMonsterClicked;

    /// <summary>
    /// 초기화
    /// Monster 데이터를 받아 UI 설정, SpriteAnimator 자동 생성 및 연결, 몬스터별 애니메이션 클립 설정
    /// </summary>
    public void Initialize(Monster target)
    {
        monster = target;

        if (monster == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = monster.Name;
        }

        if (monsterImage != null && monster.spawnData != null && monster.spawnData.monsterSprite != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
            monsterImage.preserveAspect = true;
            monsterImage.color = Color.white;
        }

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

        if (monster.Stats != null)
        {
            monster.Stats.OnHPChanged += UpdateHPBar;
        }

        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnButtonClicked);
        }

        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        canvasGroup.alpha = 1f;

        // SpriteAnimator 자동 생성 및 연결
        if (spriteAnimator == null && monsterImage != null)
        {
            spriteAnimator = monsterImage.GetComponent<SpriteAnimator>();
            if (spriteAnimator == null)
            {
                spriteAnimator = monsterImage.gameObject.AddComponent<SpriteAnimator>();
            }
        }

        // 몬스터별 애니메이션 클립 동적 설정
        if (spriteAnimator != null && monster.spawnData != null && monster.spawnData.animationClips != null)
        {
            SetupMonsterAnimations(monster.spawnData.animationClips);
        }
    }

    /// <summary>
    /// SpriteAnimator에 몬스터별 애니메이션 클립 동적 설정
    /// MonsterSpawnData의 animationClips 배열을 SpriteAnimator에 주입
    /// </summary>
    private void SetupMonsterAnimations(SpriteAnimationClip[] clips)
    {
        if (spriteAnimator == null || clips == null || clips.Length == 0)
        {
            return;
        }

        spriteAnimator.SetClips(clips);
    }

    /// <summary>
    /// Idle 애니메이션 재생 (루프)
    /// 몬스터가 대기 중일 때 반복 재생
    /// </summary>
    public void PlayIdleAnimation()
    {
        if (spriteAnimator != null)
        {
            spriteAnimator.Play("idle", loop: true);
        }
    }

    /// <summary>
    /// 공격 애니메이션 재생 (Coroutine 방식)
    /// 스킬에서 지정한 애니메이션 클립 이름을 받아 재생하고, 완료될 때까지 대기 후 콜백 호출
    /// </summary>
    /// <param name="clipName">재생할 애니메이션 클립 이름 (예: attack, cast, slash)</param>
    /// <param name="onComplete">애니메이션 완료 후 호출될 콜백</param>
    public IEnumerator PlayAttackAnimation(string clipName, System.Action onComplete = null)
    {
        if (spriteAnimator == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        spriteAnimator.Play(clipName, loop: false);

        yield return new WaitUntil(() => !spriteAnimator.IsPlaying(clipName));

        // 애니메이션 완료 후 idle로 복귀
        PlayIdleAnimation();

        onComplete?.Invoke();
    }

    /// <summary>
    /// HP 바 업데이트
    /// 현재 HP와 최대 HP를 받아 UI 갱신, HP가 0 이하일 때 사망 처리
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

    /// <summary>
    /// 몬스터 사망 처리 (페이드아웃 효과)
    /// 버튼 비활성화, 턴 표시 제거, 페이드아웃 후 슬롯 비활성화
    /// </summary>
    private void OnMonsterDeath()
    {
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }

        SetTurnActive(false);

        StartCoroutine(FadeOutAndDestroy());
    }

    /// <summary>
    /// 페이드아웃 후 슬롯 비활성화
    /// CanvasGroup alpha를 1 → 0으로 서서히 감소시켜 페이드아웃 효과 구현
    /// </summary>
    private IEnumerator FadeOutAndDestroy()
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 선택 표시
    /// 타겟팅 시 시각적 피드백 제공
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
    /// 빨간색 외곽선 깜빡임으로 현재 턴 표시
    /// </summary>
    public void SetTurnActive(bool active)
    {
        if (turnOutline == null)
        {
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
    /// alpha 값을 0.5 ↔ 1.0 사이에서 반복하여 깜빡이는 효과 구현
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
    /// 위치를 위로 이동시키면서 alpha를 0으로 감소시켜 페이드아웃 효과
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
            return;
        }

        selectButton.interactable = interactable;
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// 몬스터 선택 시 이벤트 발생
    /// </summary>
    private void OnButtonClicked()
    {
        if (monster == null)
        {
            return;
        }

        if (!monster.IsAlive)
        {
            return;
        }

        if (selectButton != null && !selectButton.interactable)
        {
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