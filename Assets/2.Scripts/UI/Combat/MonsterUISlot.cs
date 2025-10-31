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
            Debug.Log($"[MonsterUISlot] ✅ 스프라이트 설정: {monster.spawnData.monsterSprite.name}");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ 스프라이트 설정 실패:\n" +
                           $"  - monsterImage null: {monsterImage == null}\n" +
                           $"  - spawnData null: {monster.spawnData == null}\n" +
                           $"  - sprite null: {(monster.spawnData != null ? (monster.spawnData.monsterSprite == null).ToString() : "N/A")}");
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

        // 🔧 5. 선택 표시 숨김 (null 체크 강화)
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(false);
            Debug.Log("[MonsterUISlot] ✅ 선택 표시 초기화 (비활성)");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ selectionIndicator가 null입니다! (오브젝트: {gameObject.name})");
        }

        // 🔧 6. 버튼 이벤트 연결
        if (selectButton != null)
        {
            // 기존 리스너 제거 후 재등록 (중복 방지)
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnButtonClicked);
            Debug.Log("[MonsterUISlot] ✅ 버튼 클릭 이벤트 연결 완료");
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
    }

    /// <summary>
    /// 선택 표시
    /// </summary>
    public void SetSelected(bool selected)
    {
        // 🔧 null 체크 강화
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(selected);
            Debug.Log($"[MonsterUISlot] {(monster != null ? monster.Name : "Unknown")} 선택 표시: {selected}");
        }
        else
        {
            Debug.LogWarning($"[MonsterUISlot] ⚠️ selectionIndicator가 null입니다! SetSelected({selected}) 호출 무시\n" +
                           $"  - 오브젝트: {gameObject.name}\n" +
                           $"  - Monster: {(monster != null ? monster.Name : "null")}");
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