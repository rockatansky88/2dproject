using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전투 UI 통합 관리
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("파티 슬롯")]
    [SerializeField] private Transform mercenaryPartyRoot; // MercenaryParty 루트
    private List<MercenaryPartySlot> partySlots = new List<MercenaryPartySlot>();

    [Header("몬스터 슬롯")]
    [SerializeField] private Transform monsterSpawnParent; // MonsterUI가 생성되는 부모
    private List<MonsterUISlot> monsterSlots = new List<MonsterUISlot>();

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: 스킬 컨테이너 참조 방식 변경
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    [Header("스킬 슬롯")]
    [SerializeField] private Transform skillSlotParent; // SkillSlotParent (빈 컨테이너)
    [SerializeField] private GameObject skillContainerPrefab; // 🆕 SkillContainer 프리팹
    [SerializeField] private float skillContainerOffsetY = 80f; // 용병 위로 올라갈 Y 오프셋

    private GameObject skillContainerInstance; // 🆕 생성된 SkillContainer 인스턴스
    private List<SkillSlot> skillSlots = new List<SkillSlot>(); // 내부 슬롯 참조

    [Header("타겟 표시")]
    [SerializeField] private GameObject targetArrow;

    [Header("전투 버튼")]
    [SerializeField] private Button attackButton;

    [Header("TPE 미니게임 UI")]
    [SerializeField] private GameObject tpeMinigamePanel;

    [Header("패링 미니게임 UI")]
    [SerializeField] private GameObject parryMinigamePanel;

    private ICombatant currentTarget;
    private SkillDataSO selectedSkill;

    private void Awake()
    {
        Debug.Log("[CombatUI] 초기화");

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
        }

        // 미니게임 UI 초기 숨김
        if (tpeMinigamePanel != null) tpeMinigamePanel.SetActive(false);
        if (parryMinigamePanel != null) parryMinigamePanel.SetActive(false);

        // 타겟 화살표 초기 숨김
        if (targetArrow != null) targetArrow.SetActive(false);

        // 🆕 추가: 스킬 슬롯 부모 초기 숨김
        if (skillSlotParent != null) skillSlotParent.gameObject.SetActive(false);

        // 🆕 추가: 스킬 컨테이너 생성
        CreateSkillContainer();
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: SkillContainer 생성 메서드
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// SkillContainer를 한 번만 생성하고 내부 슬롯 참조
    /// </summary>
    private void CreateSkillContainer()
    {
        if (skillContainerPrefab == null)
        {
            Debug.LogError("[CombatUI] ❌ skillContainerPrefab이 null입니다! Inspector에서 할당해주세요");
            return;
        }

        if (skillSlotParent == null)
        {
            Debug.LogError("[CombatUI] ❌ skillSlotParent가 null입니다!");
            return;
        }

        // 🔧 기존 자식이 있다면 제거 (중복 방지)
        foreach (Transform child in skillSlotParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("[CombatUI] SkillContainer 생성 시작");

        // 🆕 SkillContainer 인스턴스 생성
        skillContainerInstance = Instantiate(skillContainerPrefab, skillSlotParent);
        skillContainerInstance.name = "SkillContainer"; // (Clone) 제거

        // 🆕 내부 슬롯 찾기 (Slot1 ~ Slot5)
        skillSlots.Clear();
        SkillSlot[] foundSlots = skillContainerInstance.GetComponentsInChildren<SkillSlot>(true);

        if (foundSlots.Length == 0)
        {
            Debug.LogError("[CombatUI] ❌ SkillContainer 내부에 SkillSlot 컴포넌트가 없습니다!");
            return;
        }

        skillSlots.AddRange(foundSlots);
        Debug.Log($"[CombatUI] ✅ SkillContainer 생성 완료 - 내부 슬롯 {skillSlots.Count}개 발견");

        // 초기엔 모든 슬롯 비활성화
        foreach (var slot in skillSlots)
        {
            slot.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 파티 슬롯을 전투 모드로 전환
    /// </summary>
    public void InitializePartyUI(List<Character> party)
    {
        Debug.Log($"[CombatUI] 파티 UI 전투 모드 전환: {party.Count}명");

        // 파티 슬롯 찾기
        if (partySlots.Count == 0 && mercenaryPartyRoot != null)
        {
            partySlots.AddRange(mercenaryPartyRoot.GetComponentsInChildren<MercenaryPartySlot>(true));
            Debug.Log($"[CombatUI] 파티 슬롯 {partySlots.Count}개 발견");
        }

        // 전투 모드 활성화
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < party.Count)
            {
                partySlots[i].SetCombatMode(true);
                Debug.Log($"[CombatUI] 파티 슬롯 {i}: 전투 모드 활성화");
            }
            else
            {
                partySlots[i].SetEmpty();
            }
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: InitializeMonsterUI - 몬스터 클릭 이벤트 연결 및 타겟 선택 구현
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 몬스터 슬롯 찾기 및 클릭 이벤트 연결
    /// 기능:
    /// 1. MonsterSpawnParent 밑의 모든 MonsterUISlot 찾기
    /// 2. 각 슬롯의 OnMonsterClicked 이벤트에 타겟 선택 핸들러 연결
    /// 3. 타겟 선택 시 화살표 표시 및 다른 슬롯 선택 해제
    /// </summary>
    public void InitializeMonsterUI(List<Monster> monsters)
    {
        Debug.Log($"[CombatUI] ━━━ 몬스터 UI 초기화: {monsters.Count}마리 ━━━");

        // 🔧 수정: 몬스터 슬롯 새로 찾기 (이전 참조 제거)
        monsterSlots.Clear();

        if (monsterSpawnParent != null)
        {
            monsterSlots.AddRange(monsterSpawnParent.GetComponentsInChildren<MonsterUISlot>(true));
            Debug.Log($"[CombatUI] 몬스터 슬롯 {monsterSlots.Count}개 발견");
        }
        else
        {
            Debug.LogError("[CombatUI] ❌ monsterSpawnParent가 null입니다!");
            return;
        }

        // 🆕 클릭 이벤트 연결
        for (int i = 0; i < monsterSlots.Count; i++)
        {
            MonsterUISlot slot = monsterSlots[i];

            if (slot == null)
            {
                Debug.LogWarning($"[CombatUI] ⚠️ 몬스터 슬롯 {i}가 null입니다!");
                continue;
            }

            Monster monster = slot.GetMonster();

            if (monster == null)
            {
                Debug.LogWarning($"[CombatUI] ⚠️ 슬롯 {i}에 Monster 데이터가 없습니다!");
                continue;
            }

            // 🔧 이벤트 중복 등록 방지
            slot.OnMonsterClicked -= OnMonsterSlotClicked;
            slot.OnMonsterClicked += OnMonsterSlotClicked;

            Debug.Log($"[CombatUI] ✅ 몬스터 슬롯 {i}: {monster.Name} 클릭 이벤트 등록");
        }

        Debug.Log("[CombatUI] ✅ 몬스터 UI 초기화 완료");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: 몬스터 클릭 핸들러
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 몬스터 슬롯 클릭 핸들러
    /// 기능:
    /// 1. 현재 타겟으로 설정
    /// 2. 타겟 화살표 표시
    /// 3. 다른 몬스터 슬롯 선택 해제
    /// 4. 클릭한 슬롯만 선택 표시
    /// </summary>
    private void OnMonsterSlotClicked(Monster monster)
    {
        if (monster == null || !monster.IsAlive)
        {
            Debug.LogWarning("[CombatUI] ⚠️ 죽은 몬스터는 타겟으로 선택할 수 없습니다!");
            return;
        }

        Debug.Log($"[CombatUI] 🎯 몬스터 클릭: {monster.Name} - 타겟으로 설정");

        // 🔧 1. 모든 몬스터 슬롯 선택 해제
        foreach (var slot in monsterSlots)
        {
            if (slot != null)
            {
                slot.SetSelected(false);
            }
        }

        // 🔧 2. 클릭한 슬롯만 선택 표시
        MonsterUISlot clickedSlot = monsterSlots.FirstOrDefault(s => s != null && s.GetMonster() == monster);

        if (clickedSlot != null)
        {
            clickedSlot.SetSelected(true);
            Debug.Log($"[CombatUI] ✅ {monster.Name} 선택 표시 활성화");
        }

        // 🔧 3. 타겟 화살표 표시
        ShowTargetArrow(monster);

        // 🔧 4. 현재 타겟으로 설정
        currentTarget = monster;
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: InitializeSkillSlots - SkillContainer 내부 슬롯 재사용
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 스킬 슬롯에 스킬 데이터 할당 (SkillContainer 내부 슬롯 재사용)
    /// </summary>
    public void InitializeSkillSlots(List<SkillDataSO> skills)
    {
        Debug.Log($"[CombatUI] 스킬 슬롯 초기화: {skills?.Count ?? 0}개");

        if (skillSlots.Count == 0)
        {
            Debug.LogError("[CombatUI] ❌ 스킬 슬롯이 없습니다! CreateSkillContainer 실행 필요");
            return;
        }

        if (skills == null || skills.Count == 0)
        {
            Debug.LogWarning("[CombatUI] 스킬이 없습니다!");
            HideAllSkillSlots();
            return;
        }

        // 🔧 모든 슬롯 비활성화 후 필요한 만큼만 활성화
        HideAllSkillSlots();

        int skillCount = Mathf.Min(skills.Count, skillSlots.Count);

        for (int i = 0; i < skillCount; i++)
        {
            SkillSlot slot = skillSlots[i];
            SkillDataSO skill = skills[i];

            // 🔧 이벤트 중복 등록 방지
            slot.OnSkillClicked -= OnSkillClicked;
            slot.OnSkillClicked += OnSkillClicked;

            slot.Initialize(skill);
            slot.gameObject.SetActive(true);

            Debug.Log($"[CombatUI] 스킬 슬롯 {i}: {skill.skillName} 할당");
        }

        // 🆕 SkillContainer 활성화
        if (skillContainerInstance != null)
        {
            skillContainerInstance.SetActive(true);
        }

        if (skillSlotParent != null)
        {
            skillSlotParent.gameObject.SetActive(true);
        }

        Debug.Log($"[CombatUI] ✅ 스킬 슬롯 {skillCount}개 활성화 완료");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: 모든 스킬 슬롯 숨기기
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 모든 스킬 슬롯 비활성화
    /// </summary>
    private void HideAllSkillSlots()
    {
        foreach (var slot in skillSlots)
        {
            if (slot != null)
            {
                // 🔧 수정: 초기화된 슬롯만 SetSelected 호출
                if (slot.Skill != null)
                {
                    slot.SetSelected(false);
                }

                slot.gameObject.SetActive(false);
            }
        }

        // 🆕 추가: LayoutRebuilder로 레이아웃 즉시 재계산
        if (skillContainerInstance != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
                skillContainerInstance.GetComponent<RectTransform>()
            );
        }
    }

    /// <summary>
    /// MP에 따라 스킬 슬롯 업데이트
    /// </summary>
    public void UpdateSkillSlotsByMP(Character character)
    {
        if (character == null) return;

        int currentMP = character.Stats.CurrentMP;

        foreach (var slot in skillSlots)
        {
            if (slot.gameObject.activeSelf) // 활성화된 슬롯만 업데이트
            {
                slot.UpdateManaCost(currentMP);
            }
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: UpdateCurrentTurn - SkillContainer 위치 이동 추가
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 현재 턴 표시
    /// </summary>
    public void UpdateCurrentTurn(ICombatant combatant)
    {
        Debug.Log($"[CombatUI] 현재 턴 표시: {combatant.Name}");

        // 모든 파티 슬롯 턴 표시 제거
        foreach (var slot in partySlots)
        {
            if (slot != null) slot.SetTurnActive(false);
        }

        // 스킬 선택 초기화
        selectedSkill = null;

        if (combatant.IsPlayer)
        {
            Character character = combatant as Character;
            if (character != null)
            {
                // 해당 파티 슬롯 찾기
                MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
                    s != null && s.GetMercenary() != null && s.GetMercenary().mercenaryName == character.Name);

                if (targetSlot != null)
                {
                    targetSlot.SetTurnActive(true);
                    Debug.Log($"[CombatUI] {character.Name} 턴 표시");

                    // 🆕 추가: SkillContainer를 해당 용병 위로 이동
                    MoveSkillContainerToMercenary(targetSlot);
                }

                // 스킬 슬롯 활성화
                if (skillSlotParent != null)
                {
                    skillSlotParent.gameObject.SetActive(true);
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

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: SkillContainer를 용병 위치로 이동
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// SkillContainer(또는 Parent)를 턴인 용병 바로 위로 이동
    /// </summary>
    private void MoveSkillContainerToMercenary(MercenaryPartySlot targetSlot)
    {
        if (skillSlotParent == null || targetSlot == null)
        {
            Debug.LogWarning("[CombatUI] ⚠️ skillSlotParent 또는 targetSlot이 null입니다");
            return;
        }

        RectTransform containerRect = skillSlotParent.GetComponent<RectTransform>();
        RectTransform targetSlotRect = targetSlot.GetComponent<RectTransform>();

        if (containerRect == null || targetSlotRect == null)
        {
            Debug.LogWarning("[CombatUI] ⚠️ RectTransform을 찾을 수 없습니다");
            return;
        }

        // 🔧 수정: 용병 슬롯 위쪽으로 위치 이동
        Vector3 newPosition = new Vector3(
            targetSlotRect.position.x,
            targetSlotRect.position.y + skillContainerOffsetY, // Y축 오프셋
            containerRect.position.z
        );

        containerRect.position = newPosition;

        Debug.Log($"[CombatUI] ✅ SkillContainer를 {targetSlot.GetMercenary().mercenaryName} 위로 이동: {newPosition}");
    }

    /// <summary>
    /// 타겟 화살표 표시
    /// </summary>
    public void ShowTargetArrow(ICombatant target)
    {
        if (targetArrow == null)
        {
            Debug.LogError("[CombatUI] targetArrow가 null입니다!");
            return;
        }

        currentTarget = target;
        targetArrow.SetActive(true);

        // 타겟 위치로 이동
        if (target is Monster monster)
        {
            MonsterUISlot slot = monsterSlots.FirstOrDefault(s => s != null && s.GetMonster() == monster);

            if (slot != null)
            {
                RectTransform rt = targetArrow.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.position = new Vector3(
                        slot.transform.position.x,
                        slot.transform.position.y + 50f,
                        rt.position.z
                    );
                }
                Debug.Log($"[CombatUI] 타겟 화살표 표시: {target.Name}");
            }
        }
        else if (target is Character character)
        {
            MercenaryPartySlot slot = partySlots.FirstOrDefault(s =>
                s != null && s.GetMercenary() != null && s.GetMercenary().mercenaryName == character.Name);

            if (slot != null)
            {
                RectTransform rt = targetArrow.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.position = new Vector3(
                        slot.transform.position.x,
                        slot.transform.position.y + 50f,
                        rt.position.z
                    );
                }
                Debug.Log($"[CombatUI] 타겟 화살표 표시: {target.Name}");
            }
        }
    }

    /// <summary>
    /// 타겟 화살표 숨김
    /// </summary>
    public void HideTargetArrow()
    {
        if (targetArrow != null)
        {
            targetArrow.SetActive(false);
        }
    }

    /// <summary>
    /// 스킬 클릭
    /// </summary>
    private void OnSkillClicked(SkillDataSO skill)
    {
        selectedSkill = skill;

        foreach (var slot in skillSlots)
        {
            if (slot.gameObject.activeSelf)
            {
                slot.SetSelected(false);
            }
        }

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

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.RequestPlayerAction(selectedSkill, currentTarget);
        }
    }

    /// <summary>
    /// TPE 미니게임 표시
    /// </summary>
    public void ShowTPEMinigame()
    {
        if (tpeMinigamePanel != null)
        {
            tpeMinigamePanel.SetActive(true);
            Debug.Log("[CombatUI] TPE 미니게임 표시");
        }
    }

    /// <summary>
    /// TPE 미니게임 숨김
    /// </summary>
    public void HideTPEMinigame()
    {
        if (tpeMinigamePanel != null)
        {
            tpeMinigamePanel.SetActive(false);
            Debug.Log("[CombatUI] TPE 미니게임 숨김");
        }
    }

    /// <summary>
    /// 패링 미니게임 표시
    /// </summary>
    public void ShowParryMinigame()
    {
        if (parryMinigamePanel != null)
        {
            parryMinigamePanel.SetActive(true);
            Debug.Log("[CombatUI] 패링 미니게임 표시");
        }
    }

    /// <summary>
    /// 패링 미니게임 숨김
    /// </summary>
    public void HideParryMinigame()
    {
        if (parryMinigamePanel != null)
        {
            parryMinigamePanel.SetActive(false);
            Debug.Log("[CombatUI] 패링 미니게임 숨김");
        }
    }

    /// <summary>
    /// 파티 멤버 스탯 업데이트
    /// </summary>
    public void UpdatePartyMemberStats(Character character)
    {
        MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
            s != null && s.GetMercenary() != null && s.GetMercenary().mercenaryName == character.Name);

        if (targetSlot != null)
        {
            targetSlot.UpdateCombatStats(
                character.Stats.CurrentHP,
                character.Stats.MaxHP,
                character.Stats.CurrentMP,
                character.Stats.MaxMP
            );
        }
    }

    /// <summary>
    /// 몬스터 스탯 업데이트
    /// </summary>
    public void UpdateMonsterStats(Monster monster)
    {
        // MonsterUISlot이 자동으로 처리
    }

    /// <summary>
    /// 파티 슬롯 가져오기 (CombatManager용)
    /// </summary>
    public MercenaryPartySlot GetPartySlot(int index)
    {
        if (partySlots.Count == 0 && mercenaryPartyRoot != null)
        {
            partySlots.AddRange(mercenaryPartyRoot.GetComponentsInChildren<MercenaryPartySlot>(true));
        }

        if (index >= 0 && index < partySlots.Count)
        {
            return partySlots[index];
        }

        return null;
    }

    /// <summary>
    /// 몬스터 슬롯 가져오기 (CombatManager용)
    /// </summary>
    public MonsterUISlot GetMonsterSlot(int index)
    {
        if (index >= 0 && index < monsterSlots.Count)
        {
            return monsterSlots[index];
        }

        return null;
    }
}