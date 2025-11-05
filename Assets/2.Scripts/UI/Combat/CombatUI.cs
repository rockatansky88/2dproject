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

    [Header("스킬 슬롯")]
    [SerializeField] private Transform skillSlotParent; // SkillSlotParent (빈 컨테이너)
    [SerializeField] private GameObject skillContainerPrefab; // 🆕 SkillContainer 프리팹
    [SerializeField] private float skillContainerOffsetY = 60f; // 용병 위로 올라갈 Y 오프셋

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

        // 파티 슬롯 찾기
        if (partySlots.Count == 0 && mercenaryPartyRoot != null)
        {
            partySlots.AddRange(mercenaryPartyRoot.GetComponentsInChildren<MercenaryPartySlot>(true));
        }

        // 전투 모드 활성화
        for (int i = 0; i < partySlots.Count; i++)
        {
            if (i < party.Count)
            {
                partySlots[i].SetCombatMode(true);
            }
            else
            {
                partySlots[i].SetEmpty();
            }
        }
    }


    /// <summary>
    /// 몬스터 슬롯 찾기 및 클릭 이벤트 연결
    /// 기능:
    /// 1. MonsterSpawnParent 밑의 모든 MonsterUISlot 찾기
    /// 2. 각 슬롯의 OnMonsterClicked 이벤트에 타겟 선택 핸들러 연결
    /// 3. 타겟 선택 시 화살표 표시 및 다른 슬롯 선택 해제
    /// </summary>
    public void InitializeMonsterUI(List<Monster> monsters)
    {

        // 🔧 수정: 몬스터 슬롯 새로 찾기 (이전 참조 제거)
        monsterSlots.Clear();

        if (monsterSpawnParent != null)
        {
            monsterSlots.AddRange(monsterSpawnParent.GetComponentsInChildren<MonsterUISlot>(true));
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

        }

    }


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
        }

        // 🔧 3. 타겟 화살표 표시
        ShowTargetArrow(monster);

        // 🔧 4. 현재 타겟으로 설정
        currentTarget = monster;
    }

    /// <summary>
    /// 스킬 슬롯에 스킬 데이터 할당 (SkillContainer 내부 슬롯 재사용)
    /// </summary>
    public void InitializeSkillSlots(List<SkillDataSO> skills)
    {

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

    }


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


    /// <summary>
    /// 현재 턴 표시
    /// </summary>
    public void UpdateCurrentTurn(ICombatant combatant)
    {

        // 1. 모든 파티 슬롯 턴 표시 제거
        foreach (var slot in partySlots)
        {
            if (slot != null) slot.SetTurnActive(false);
        }

        // 2. 모든 몬스터 슬롯 턴 표시 제거
        foreach (var slot in monsterSlots)
        {
            if (slot != null) slot.SetTurnActive(false);
        }

        selectedSkill = null;

        if (combatant.IsPlayer)
        {
            SetMonsterSlotsInteractable(true);

            Character character = combatant as Character;
            if (character != null)
            {

                MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
                    s != null &&
                    s.GetMercenary() != null &&
                    s.GetMercenary().instanceID == character.mercenaryData.instanceID); // ✅ ID로 비교

                if (targetSlot != null)
                {
                    targetSlot.SetTurnActive(true);

                    MoveSkillContainerToMercenary(targetSlot);
                }
                else
                {
                    Debug.LogWarning($"[CombatUI] ⚠️ {character.mercenaryData.GetDisplayName()} (instanceID: {character.mercenaryData.instanceID})의 슬롯을 찾을 수 없습니다!");
                }

                if (skillSlotParent != null)
                {
                    skillSlotParent.gameObject.SetActive(true);
                }

                SelectFirstAliveMonster();
            }
        }
        else
        {
            SetMonsterSlotsInteractable(false);

            Monster monster = combatant as Monster;
            if (monster != null)
            {
                MonsterUISlot targetSlot = monsterSlots.FirstOrDefault(s =>
                    s != null && s.GetMonster() != null && s.GetMonster() == monster);

                if (targetSlot != null)
                {
                    targetSlot.SetTurnActive(true);
                }
                else
                {
                    Debug.LogWarning($"[CombatUI] ⚠️ {monster.Name}의 슬롯을 찾을 수 없습니다!");
                }
            }

            if (skillSlotParent != null)
            {
                skillSlotParent.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 모든 몬스터 슬롯의 클릭 가능 여부 설정
    /// 플레이어 턴에만 몬스터를 선택할 수 있도록 제어
    /// </summary>
    /// <param name="interactable">true: 클릭 가능, false: 클릭 불가</param>
    private void SetMonsterSlotsInteractable(bool interactable)
    {


        if (monsterSlots == null || monsterSlots.Count == 0)
        {
            Debug.LogWarning("[CombatUI] ⚠️ monsterSlots가 비어있습니다! InitializeMonsterUI()를 먼저 호출하세요");
            return;
        }

        foreach (var slot in monsterSlots)
        {
            if (slot == null)
            {
                Debug.LogWarning("[CombatUI] ⚠️ monsterSlots에 null 슬롯이 있습니다!");
                continue;
            }

            // 사망한 몬스터는 항상 클릭 불가
            Monster monster = slot.GetMonster();
            if (monster != null && !monster.IsAlive)
            {
                slot.SetInteractable(false);
                continue;
            }

            slot.SetInteractable(interactable);
        }
    }

    /// <summary>
    /// 용병 턴 시작 시 첫 번째 살아있는 몬스터를 자동으로 타겟 지정
    /// </summary>
    public void SelectFirstAliveMonster()
    {

        // 살아있는 몬스터 슬롯 찾기
        MonsterUISlot firstAliveSlot = monsterSlots.FirstOrDefault(s =>
            s != null && s.GetMonster() != null && s.GetMonster().IsAlive);

        if (firstAliveSlot != null)
        {
            firstAliveSlot.gameObject.SetActive(true);
            Monster firstMonster = firstAliveSlot.GetMonster();

            // 몬스터 클릭 핸들러 호출 (수동 선택과 동일한 로직)
            OnMonsterSlotClicked(firstMonster);
        }
        else
        {
            Debug.LogWarning("[CombatUI] ⚠️ 살아있는 몬스터가 없습니다!");
        }
    }


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

    }

    /// <summary>
    /// 타겟 화살표 표시
    /// MercenaryInstance의 instanceID를 기반으로 정확한 슬롯 위치를 찾습니다.
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
            }
            else
            {
                Debug.LogWarning($"[CombatUI] ⚠️ 몬스터 {target.Name}의 슬롯을 찾을 수 없습니다!");
            }
        }
        else if (target is Character character)
        {

            if (character.mercenaryData == null)
            {
                Debug.LogWarning($"[CombatUI] ⚠️ {character.Name}의 mercenaryData가 null입니다!");
                return;
            }

            MercenaryPartySlot slot = partySlots.FirstOrDefault(s =>
                s != null &&
                s.GetMercenary() != null &&
                s.GetMercenary().instanceID == character.mercenaryData.instanceID); // ✅ ID로 비교

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
            }
            else
            {
                Debug.LogWarning($"[CombatUI] ⚠️ {character.mercenaryData.GetDisplayName()} (instanceID: {character.mercenaryData.instanceID})의 슬롯을 찾을 수 없습니다!");
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
        }
    }

    /// <summary>
    /// 파티 멤버 스탯 업데이트
    /// MercenaryInstance의 instanceID를 기반으로 정확한 슬롯을 찾습니다.
    /// </summary>
    public void UpdatePartyMemberStats(Character character)
    {
        if (character == null || character.mercenaryData == null)
        {
            Debug.LogWarning("[CombatUI] ⚠️ character 또는 mercenaryData가 null입니다!");
            return;
        }

        MercenaryPartySlot targetSlot = partySlots.FirstOrDefault(s =>
            s != null &&
            s.GetMercenary() != null &&
            s.GetMercenary().instanceID == character.mercenaryData.instanceID); //  ID로 비교

        if (targetSlot != null)
        {
            targetSlot.UpdateCombatStats(
                character.Stats.CurrentHP,
                character.Stats.MaxHP,
                character.Stats.CurrentMP,
                character.Stats.MaxMP
            );

        }
        else
        {
            Debug.LogWarning($"[CombatUI] ⚠️ {character.mercenaryData.GetDisplayName()} (instanceID: {character.mercenaryData.instanceID})의 슬롯을 찾을 수 없습니다!");
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