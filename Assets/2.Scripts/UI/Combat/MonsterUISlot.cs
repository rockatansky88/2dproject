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

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 수정: Image의 Outline 컴포넌트 사용
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    private Outline turnOutline; // 몬스터 이미지의 Outline 컴포넌트
    private Coroutine turnBlinkCoroutine; // 깜빡임 코루틴 참조

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

        Debug.Log($"[MonsterUISlot] ━━━ {monster.Name} UI 슬롯 초기화 시작 ━━━");

        // 🔧 1. 기본 정보 설정 - 몬스터 이름
        if (nameText != null)
        {
            nameText.text = monster.Name;
            Debug.Log($"[MonsterUISlot] ✅ 이름 설정: {monster.Name}");
        }
        else
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ nameText가 null입니다!");
        }

        // 🔧 2. 몬스터 스프라이트 설정
        if (monsterImage != null && monster.spawnData != null && monster.spawnData.monsterSprite != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
            monsterImage.preserveAspect = true;
            monsterImage.color = Color.white;

            Debug.Log($"[MonsterUISlot] ✅ 스프라이트 설정: {monster.spawnData.monsterSprite.name}");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ 스프라이트 설정 실패");
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 🆕 추가: Outline 컴포넌트 자동 생성 또는 가져오기
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        if (monsterImage != null)
        {
            // 기존 Outline 컴포넌트가 있는지 확인
            turnOutline = monsterImage.GetComponent<Outline>();

            // 없으면 새로 추가
            if (turnOutline == null)
            {
                turnOutline = monsterImage.gameObject.AddComponent<Outline>();
                Debug.Log($"[MonsterUISlot] ✅ Outline 컴포넌트 자동 생성");
            }

            // 초기 설정: 빨간색, 두께 5, 비활성화
            turnOutline.effectColor = new Color(1f, 0f, 0f, 1f); // 빨간색
            turnOutline.effectDistance = new Vector2(5f, 5f); // 외곽선 두께
            turnOutline.enabled = false; // 초기엔 비활성화

            Debug.Log($"[MonsterUISlot] ✅ Outline 초기화 완료 (빨간색, 두께 5)");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ monsterImage가 null이어서 Outline을 생성할 수 없습니다!");
        }

        // 🔧 3. 스탯 변화 이벤트 구독
        if (monster.Stats != null)
        {
            monster.Stats.OnHPChanged += UpdateHPBar;
            Debug.Log("[MonsterUISlot] ✅ HP 변경 이벤트 구독 완료");
        }
        else
        {
            Debug.LogError("[MonsterUISlot] ❌ monster.Stats가 null입니다!");
        }

        // 🔧 4. 초기 HP 바 업데이트
        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        // 🔧 5. 선택 표시 숨김
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
            Debug.Log("[MonsterUISlot] ✅ 선택 표시 초기화 (비활성)");
        }

        // 🔧 6. 버튼 이벤트 연결
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnButtonClicked);

            // 🆕 추가: 버튼 상태 확인
            Debug.Log($"[MonsterUISlot] ✅ 버튼 클릭 이벤트 연결 완료\n" +
                     $"  - Interactable: {selectButton.interactable}\n" +
                     $"  - Raycast Target: {(selectButton.GetComponent<Image>()?.raycastTarget ?? false)}\n" +
                     $"  - RectTransform Size: {selectButton.GetComponent<RectTransform>()?.rect.size}");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ selectButton이 null입니다! (오브젝트: {gameObject.name})");
        }

        Debug.Log($"[MonsterUISlot] ✅ {monster.Name} UI 슬롯 초기화 완료");
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

            Debug.Log($"[MonsterUISlot] {monster.Name} HP 바 업데이트: {currentHP}/{maxHP} ({fillAmount:P0})");
        }

        if (hpText != null)
        {
            hpText.text = $"{currentHP}/{maxHP}";
        }

        // 사망 시 비활성화
        if (currentHP <= 0)
        {
            OnMonsterDeath();
        }
    }

    /// <summary>
    /// 몬스터 사망 처리
    /// </summary>
    private void OnMonsterDeath()
    {
        Debug.Log($"[MonsterUISlot] {monster.Name} 사망 - UI 비활성화");

        // 버튼 비활성화
        if (selectButton != null)
        {
            selectButton.interactable = false;
        }

        // 이미지 반투명 처리
        if (monsterImage != null)
        {
            Color color = monsterImage.color;
            color.a = 0.5f;
            monsterImage.color = color;
        }

        // 🆕 추가: 턴 외곽선 비활성화
        SetTurnActive(false);
    }

    /// <summary>
    /// 선택 표시
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
            Debug.Log($"[MonsterUISlot] {(monster != null ? monster.Name : "Unknown")} 선택 표시: {selected}");
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 수정: Outline 사용 방식으로 변경
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 몬스터 턴 표시 활성화/비활성화
    /// 몬스터 이미지의 Outline 컴포넌트를 사용하여 빨간색 외곽선 깜빡임
    /// </summary>
    public void SetTurnActive(bool active)
    {
        // 🔧 수정: Outline null 체크
        if (turnOutline == null)
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ {gameObject.name}: Outline 컴포넌트가 없습니다!\n" +
                           $"  - Monster: {(monster != null ? monster.Name : "null")}\n" +
                           $"  - SetTurnActive({active}) 호출 무시\n" +
                           $"  - Outline은 Initialize()에서 자동 생성됩니다");
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
            Debug.Log($"[MonsterUISlot] ✅ {monster.Name} 턴 표시 활성화 (빨간색 외곽선 깜빡임 시작)");
        }
        else
        {
            // 턴 표시 비활성화
            turnOutline.enabled = false;
            Debug.Log($"[MonsterUISlot] {(monster != null ? monster.Name : "Unknown")} 턴 표시 비활성화");
        }
    }

    /// <summary>
    /// 빨간색 외곽선 깜빡임 효과
    /// Outline의 알파값을 0.5 ~ 1.0 사이에서 반복
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
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        Debug.Log($"[MonsterUISlot] 🖱️ 버튼 클릭 감지! (Monster: {(monster != null ? monster.Name : "null")})");

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

        Debug.Log($"[MonsterUISlot] ✅ 몬스터 클릭 이벤트 발생: {monster.Name}");
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
        // 이벤트 구독 해제
        if (monster != null && monster.Stats != null)
        {
            monster.Stats.OnHPChanged -= UpdateHPBar;
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnButtonClicked);
        }

        // 🆕 추가: 코루틴 정리
        if (turnBlinkCoroutine != null)
        {
            StopCoroutine(turnBlinkCoroutine);
            turnBlinkCoroutine = null;
        }
    }
}