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

        Debug.Log($"[MonsterUISlot] ━━━ {monster.Name} UI 슬롯 초기화 시작 ━━━");

        // 1. 기본 정보 설정 - 몬스터 이름
        if (nameText != null)
        {
            nameText.text = monster.Name;
            Debug.Log($"[MonsterUISlot] ✅ 이름 설정: {monster.Name}");
        }

        // 2. 몬스터 스프라이트 설정
        if (monsterImage != null && monster.spawnData != null && monster.spawnData.monsterSprite != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
            monsterImage.preserveAspect = true;
            monsterImage.color = Color.white;

            Debug.Log($"[MonsterUISlot] ✅ 스프라이트 설정: {monster.spawnData.monsterSprite.name}");
        }

        // 3. Outline 컴포넌트 자동 생성
        if (monsterImage != null)
        {
            turnOutline = monsterImage.GetComponent<Outline>();

            if (turnOutline == null)
            {
                turnOutline = monsterImage.gameObject.AddComponent<Outline>();
                Debug.Log($"[MonsterUISlot] ✅ Outline 컴포넌트 자동 생성");
            }

            turnOutline.effectColor = new Color(1f, 0f, 0f, 1f);
            turnOutline.effectDistance = new Vector2(5f, 5f);
            turnOutline.enabled = false;

            Debug.Log($"[MonsterUISlot] ✅ Outline 초기화 완료 (빨간색, 두께 5)");
        }

        // 4. 스탯 변화 이벤트 구독
        if (monster.Stats != null)
        {
            monster.Stats.OnHPChanged += UpdateHPBar;
            Debug.Log("[MonsterUISlot] ✅ HP 변경 이벤트 구독 완료");
        }

        // 5. 초기 HP 바 업데이트
        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        // 6. 선택 표시 숨김
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
            Debug.Log("[MonsterUISlot] ✅ 선택 표시 초기화 (비활성)");
        }

        // 7. 버튼 이벤트 연결
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnButtonClicked);

            Debug.Log($"[MonsterUISlot] ✅ 버튼 클릭 이벤트 연결 완료\n" +
                     $"  - Interactable: {selectButton.interactable}\n" +
                     $"  - Raycast Target: {(selectButton.GetComponent<Image>()?.raycastTarget ?? false)}\n" +
                     $"  - RectTransform Size: {selectButton.GetComponent<RectTransform>()?.rect.size}");
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
            Debug.Log($"[MonsterUISlot] ✅ {monster.Name} 턴 표시 활성화 (빨간색 외곽선 깜빡임 시작)");
        }
        else
        {
            turnOutline.enabled = false;
            Debug.Log($"[MonsterUISlot] {(monster != null ? monster.Name : "Unknown")} 턴 표시 비활성화");
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

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: 클릭 가능 여부 설정 (턴 제어)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 몬스터 클릭 가능 여부 설정
    /// 플레이어 턴에만 클릭 가능하도록 제어
    /// </summary>
    /// <param name="interactable">true: 클릭 가능 (플레이어 턴), false: 클릭 불가 (몬스터 턴)</param>
    public void SetInteractable(bool interactable)
    {
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 🔧 수정: selectButton null 체크 추가
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        if (selectButton == null)
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ {gameObject.name}: selectButton이 null입니다!\n" +
                           $"  - Monster: {(monster != null ? monster.Name : "null")}\n" +
                           $"  - SetInteractable({interactable}) 호출 무시\n" +
                           $"  - Inspector에서 SelectButton을 할당해주세요");
            return;
        }

        selectButton.interactable = interactable;
        Debug.Log($"[MonsterUISlot] {(monster != null ? monster.Name : "Unknown")} 클릭 가능 여부: {interactable}");
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

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 🆕 추가: 버튼이 비활성화 상태면 클릭 무시
        // (Unity Button 컴포넌트가 이미 처리하지만 추가 안전장치)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        if (selectButton != null && !selectButton.interactable)
        {
            Debug.LogWarning("[MonsterUISlot] ⚠️ 현재 몬스터를 선택할 수 없는 턴입니다!");
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