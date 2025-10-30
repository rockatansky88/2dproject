using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 몬스터 UI 슬롯
/// - HP 바 표시
/// - 몬스터 스프라이트
/// - 클릭 이벤트 (타겟 선택)
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

        // 기본 정보 설정
        if (nameText != null)
        {
            nameText.text = monster.Name;
        }

        if (monsterImage != null && monster.spawnData != null)
        {
            monsterImage.sprite = monster.spawnData.monsterSprite;
        }

        // 스탯 변화 이벤트 구독
        monster.Stats.OnHPChanged += UpdateHPBar;

        // 초기 HP 바 업데이트
        UpdateHPBar(monster.Stats.CurrentHP, monster.Stats.MaxHP);

        // 선택 표시 숨김
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
        }

        // 버튼 이벤트 연결
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnButtonClicked);
        }

        Debug.Log($"[MonsterUISlot] {monster.Name} UI 슬롯 초기화 완료");
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
    }

    /// <summary>
    /// 선택 표시
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
            Debug.Log($"[MonsterUISlot] {monster.Name} 선택 표시: {selected}");
        }
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        if (monster == null || !monster.IsAlive)
        {
            Debug.LogWarning("[MonsterUISlot] 사망한 몬스터는 선택할 수 없습니다!");
            return;
        }

        Debug.Log($"[MonsterUISlot] 몬스터 클릭: {monster.Name}");
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
    }
}