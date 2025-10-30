using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전투 UI 통합 관리
/// - 기존 MercenaryPartySlot 활용 (HP/MP 바 포함)
/// - 기존 Monster UI 활용
/// - 스킬 슬롯 관리
/// - 현재 턴 표시 (빨간색 테두리)
/// - 타겟 표시 (화살표)
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("파티 슬롯 (기존)")]
    [SerializeField] private MercenaryPartySlot[] partySlots; // 기존 파티 슬롯 4개

    [Header("스킬 슬롯")]
    [SerializeField] private Transform skillSlotParent;
    [SerializeField] private GameObject skillSlotPrefab;

    [Header("타겟 표시")]
    [SerializeField] private GameObject targetArrow;

    [Header("전투 버튼")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defendButton;

    [Header("TPE 미니게임 UI")]
    [SerializeField] private GameObject tpeMinigamePanel;

    [Header("패링 미니게임 UI")]
    [SerializeField] private GameObject parryMinigamePanel;

    private List<SkillSlot> skillSlots = new List<SkillSlot>();
    private ICombatant currentTarget;
    private SkillDataSO selectedSkill;
    private MercenaryPartySlot currentTurnSlot;

    private void Awake()
    {
        Debug.Log("[CombatUI] 초기화");

        attackButton?.onClick.AddListener(OnAttackButtonClicked);
        defendButton?.onClick.AddListener(OnDefendButtonClicked);

        // 미니게임 UI 초기 숨김
        if (tpeMinigamePanel != null) tpeMinigamePanel.SetActive(false);
        if (parryMinigamePanel != null) parryMinigamePanel.SetActive(false);
    }

    /// <summary>
    /// 파티 슬롯을 전투 모드로 전환
    /// </summary>
    public void InitializePartyUI(List<Character> party)
    {
        Debug.Log($"[CombatUI] 파티 UI 초기화: {party.Count}명");

        for (int i = 0; i < partySlots.Length; i++)
        {
            if (i < party.Count)
            {
                // TODO: Character → MercenaryInstance 변환 필요
                // partySlots[i].Initialize(party[i].mercenaryData);
                partySlots[i].SetCombatMode(true);
            }
            else
            {
                partySlots[i].SetEmpty();
            }
        }
    }

    /// <summary>
    /// 스킬 슬롯 생성 (현재 턴 캐릭터의 스킬)
    /// </summary>
    public void InitializeSkillSlots(List<SkillDataSO> skills)
    {
        Debug.Log($"[CombatUI] 스킬 슬롯 생성: {skills.Count}개");

        // 기존 슬롯 제거
        foreach (var slot in skillSlots)
        {
            Destroy(slot.gameObject);
        }
        skillSlots.Clear();

        // 새 슬롯 생성
        foreach (var skill in skills)
        {
            GameObject slotObj = Instantiate(skillSlotPrefab, skillSlotParent);
            SkillSlot slot = slotObj.GetComponent<SkillSlot>();

            if (slot != null)
            {
                slot.Initialize(skill);
                slot.OnSkillClicked += OnSkillClicked;
                skillSlots.Add(slot);
                Debug.Log($"[CombatUI] {skill.skillName} 슬롯 생성");
            }
        }
    }

    /// <summary>
    /// 현재 캐릭터의 MP에 따라 스킬 슬롯 업데이트
    /// </summary>
    public void UpdateSkillSlotsByMP(Character character)
    {
        if (character == null)
        {
            Debug.LogWarning("[CombatUI] character가 null입니다!");
            return;
        }

        int currentMP = character.Stats.CurrentMP;

        Debug.Log($"[CombatUI] 스킬 슬롯 MP 업데이트: 현재 MP {currentMP}");

        foreach (var slot in skillSlots)
        {
            slot.UpdateManaCost(currentMP);
        }
    }

    /// <summary>
    /// 현재 턴 표시 업데이트 (빨간색 테두리)
    /// </summary>
    public void UpdateCurrentTurn(ICombatant combatant)
    {
        Debug.Log($"[CombatUI] 현재 턴 표시: {combatant.Name}");

        // 모든 슬롯의 턴 테두리 제거
        foreach (var slot in partySlots)
        {
            slot.SetTurnActive(false);
        }

        // 플레이어 턴이면 해당 슬롯에 테두리 표시
        if (combatant.IsPlayer)
        {
            Character character = combatant as Character;
            if (character != null)
            {
                MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
                    s.GetMercenary()?.mercenaryName == character.Name);

                if (targetSlot != null)
                {
                    targetSlot.SetTurnActive(true);
                    currentTurnSlot = targetSlot;

                    // 스킬 슬롯 위치를 해당 파티 슬롯 위로 이동
                    if (skillSlotParent != null)
                    {
                        skillSlotParent.position = new Vector3(
                            targetSlot.transform.position.x,
                            targetSlot.transform.position.y + 100f, // 위쪽으로 오프셋
                            skillSlotParent.position.z
                        );
                    }
                }
            }
        }
        else
        {
            // 몬스터 턴이면 스킬 슬롯 숨김
            if (skillSlotParent != null)
            {
                skillSlotParent.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 타겟 화살표 표시
    /// </summary>
    public void ShowTargetArrow(ICombatant target)
    {
        currentTarget = target;
        targetArrow?.SetActive(true);

        // 타겟 위치로 화살표 이동
        if (target is Monster monster && monster.UIAnchor != null)
        {
            targetArrow.transform.position = new Vector3(
                monster.UIAnchor.position.x,
                monster.UIAnchor.position.y + 50f, // 위쪽으로 오프셋
                targetArrow.transform.position.z
            );
        }
        else if (target is Character character)
        {
            MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
                s.GetMercenary()?.mercenaryName == character.Name);

            if (targetSlot != null)
            {
                targetArrow.transform.position = new Vector3(
                    targetSlot.transform.position.x,
                    targetSlot.transform.position.y + 50f,
                    targetArrow.transform.position.z
                );
            }
        }

        Debug.Log($"[CombatUI] 타겟 표시: {target.Name}");
    }

    /// <summary>
    /// 타겟 화살표 숨김
    /// </summary>
    public void HideTargetArrow()
    {
        targetArrow?.SetActive(false);
    }

    /// <summary>
    /// 스킬 클릭 이벤트
    /// </summary>
    private void OnSkillClicked(SkillDataSO skill)
    {
        selectedSkill = skill;

        // 모든 스킬 슬롯 테두리 제거
        foreach (var slot in skillSlots)
        {
            slot.SetSelected(false);
        }

        // 선택된 스킬 테두리 표시 (빨간색)
        SkillSlot selectedSlot = skillSlots.Find(s => s.Skill == skill);
        if (selectedSlot != null)
        {
            selectedSlot.SetSelected(true);
        }

        Debug.Log($"[CombatUI] 스킬 선택: {skill.skillName}");
    }

    /// <summary>
    /// 공격 버튼 클릭
    /// </summary>
    private void OnAttackButtonClicked()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[CombatUI] 스킬이 선택되지 않았습니다!");
            return;
        }

        if (currentTarget == null)
        {
            Debug.LogWarning("[CombatUI] 타겟이 선택되지 않았습니다!");
            return;
        }

        Debug.Log($"[CombatUI] 공격 버튼 클릭: {selectedSkill.skillName} -> {currentTarget.Name}");

        // TPE 미니게임 시작
        ShowTPEMinigame();
    }

    /// <summary>
    /// 방어 버튼 클릭
    /// </summary>
    private void OnDefendButtonClicked()
    {
        Debug.Log("[CombatUI] 방어 버튼 클릭");
        // TODO: 방어 처리
    }

    /// <summary>
    /// TPE 미니게임 UI 표시
    /// </summary>
    public void ShowTPEMinigame()
    {
        if (tpeMinigamePanel != null)
        {
            tpeMinigamePanel.SetActive(true);
            Debug.Log("[CombatUI] TPE 미니게임 UI 표시");
        }
    }

    /// <summary>
    /// TPE 미니게임 UI 숨김
    /// </summary>
    public void HideTPEMinigame()
    {
        if (tpeMinigamePanel != null)
        {
            tpeMinigamePanel.SetActive(false);
            Debug.Log("[CombatUI] TPE 미니게임 UI 숨김");
        }
    }

    /// <summary>
    /// 패링 미니게임 UI 표시
    /// </summary>
    public void ShowParryMinigame()
    {
        if (parryMinigamePanel != null)
        {
            parryMinigamePanel.SetActive(true);
            Debug.Log("[CombatUI] 패링 미니게임 UI 표시");
        }
    }

    /// <summary>
    /// 패링 미니게임 UI 숨김
    /// </summary>
    public void HideParryMinigame()
    {
        if (parryMinigamePanel != null)
        {
            parryMinigamePanel.SetActive(false);
            Debug.Log("[CombatUI] 패링 미니게임 UI 숨김");
        }
    }

    /// <summary>
    /// 파티 멤버 스탯 업데이트
    /// </summary>
    public void UpdatePartyMemberStats(Character character)
    {
        MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
            s.GetMercenary()?.mercenaryName == character.Name);

        if (targetSlot != null)
        {
            targetSlot.UpdateCombatStats(
                character.Stats.CurrentHP,
                character.Stats.MaxHP,
                character.Stats.CurrentMP,
                character.Stats.MaxMP
            );

            Debug.Log($"[CombatUI] {character.Name} 스탯 업데이트 - HP: {character.Stats.CurrentHP}/{character.Stats.MaxHP}, MP: {character.Stats.CurrentMP}/{character.Stats.MaxMP}");
        }
    }

    /// <summary>
    /// 몬스터 스탯 업데이트
    /// </summary>
    public void UpdateMonsterStats(Monster monster)
    {
        // 몬스터 HP 바 업데이트 (Monster 오브젝트 자체에 HealthBar 컴포넌트 있음)
        HealthBar healthBar = monster.GetComponentInChildren<HealthBar>();

        if (healthBar != null)
        {
            healthBar.UpdateHealth(monster.Stats.CurrentHP, monster.Stats.MaxHP);
            Debug.Log($"[CombatUI] {monster.Name} HP 업데이트 - {monster.Stats.CurrentHP}/{monster.Stats.MaxHP}");
        }
        else
        {
            Debug.LogWarning($"[CombatUI] ⚠️ {monster.Name}에 HealthBar 컴포넌트가 없습니다!");
        }
    }
}