using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전투 시스템 총괄 관리자
/// - 전투 초기화 및 종료
/// - 파티/몬스터 생성 및 배치
/// - 턴 시스템 연동
/// - UI 연동
/// - 승리/패배 보상 처리
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("전투 설정")]
    [SerializeField] private Transform partySpawnParent;
    [SerializeField] private Transform monsterSpawnParent;

    [Header("몬스터 프리팹")]
    [SerializeField] private GameObject monsterUISlotPrefab;

    [Header("컴포넌트 참조")]
    [SerializeField] private CombatUI combatUI;
    [SerializeField] private TurnController turnController;
    [SerializeField] private TPEMinigame tpeMinigame;
    [SerializeField] private ParryMinigame parryMinigame;
    [SerializeField] private RewardInventoryUI rewardInventoryUI; // 보상 UI 추가

    // 전투 상태
    private List<Character> currentParty = new List<Character>();
    private List<Monster> currentMonsters = new List<Monster>();
    private bool isCombatActive = false;
    private bool isBossFight = false;

    // 전투 결과
    private int totalGoldReward = 0;
    private int totalExpReward = 0;

    // TPE 미니게임 관련 변수
    private bool tpeSuccess = false;
    private SkillDataSO pendingSkill = null;
    private ICombatant pendingTarget = null;

    // 패링 미니게임 관련 변수
    private int pendingDamage = 0;
    private Character pendingDefender = null;

    // 이벤트
    public event Action<bool> OnCombatEnded; // true: 승리, false: 패배

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (combatUI == null) combatUI = FindObjectOfType<CombatUI>();
        if (turnController == null) turnController = GetComponent<TurnController>();
        if (tpeMinigame == null) tpeMinigame = FindObjectOfType<TPEMinigame>();
        if (parryMinigame == null) parryMinigame = FindObjectOfType<ParryMinigame>();
        if (rewardInventoryUI == null) rewardInventoryUI = FindObjectOfType<RewardInventoryUI>(); // 보상 UI 자동 연결
    }

    private void Start()
    {
        if (turnController != null)
        {
            turnController.OnTurnStart += OnTurnStarted;
            turnController.OnTurnEnd += OnTurnEnded;
            turnController.OnBattleEnd += OnBattleEnded;
        }

        if (tpeMinigame != null)
        {
            tpeMinigame.OnMinigameComplete += OnTPEComplete;
        }

        if (parryMinigame != null)
        {
            parryMinigame.OnParryComplete += OnParryComplete;
        }

        // 보상 UI 이벤트 연결
        if (rewardInventoryUI != null)
        {
            rewardInventoryUI.OnAllRewardsClaimed += OnRewardsClaimed;
        }
    }

    /// <summary>
    /// 전투 시작 (DungeonManager에서 호출)
    /// DungeonManager.Instance.StartCombat()에서 이 메서드를 호출합니다
    /// </summary>
    public void StartCombat(List<MonsterSpawnData> monsterDataList, bool isBoss)
    {
        isCombatActive = true;
        isBossFight = isBoss;

        SpawnParty();
        SpawnMonsters(monsterDataList);
        InitializeCombatUI();
        turnController.InitializeBattle(currentParty, currentMonsters);
    }

    /// <summary>
    /// 파티 생성 (MercenaryManager의 파티를 Character로 변환하고 UI에 연결)
    /// </summary>
    private void SpawnParty()
    {
        currentParty.Clear();

        if (MercenaryManager.Instance == null)
        {
            return;
        }

        List<MercenaryInstance> party = MercenaryManager.Instance.CurrentParty;

        if (party.Count == 0)
        {
            return;
        }

        if (combatUI == null)
        {
            return;
        }

        for (int i = 0; i < party.Count; i++)
        {
            MercenaryInstance mercData = party[i];

            GameObject charDataObj = new GameObject($"CharacterData_{mercData.mercenaryName}");
            charDataObj.transform.SetParent(partySpawnParent);
            charDataObj.transform.localPosition = Vector3.zero;

            Character character = charDataObj.AddComponent<Character>();

            List<SkillDataSO> skills = new List<SkillDataSO>(mercData.skills);

            MercenaryPartySlot uiSlot = combatUI.GetPartySlot(i);
            character.Initialize(mercData, skills, uiSlot);

            currentParty.Add(character);
        }
    }

    /// <summary>
    /// 몬스터 생성 (MonsterUISlot과 Monster 데이터 통합)
    /// </summary>
    private void SpawnMonsters(List<MonsterSpawnData> monsterDataList)
    {
        currentMonsters.Clear();

        if (combatUI == null)
        {
            return;
        }

        if (monsterUISlotPrefab == null)
        {
            return;
        }

        if (monsterSpawnParent != null)
        {
            int childCount = monsterSpawnParent.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = monsterSpawnParent.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < monsterDataList.Count; i++)
        {
            MonsterSpawnData data = monsterDataList[i];

            GameObject monsterUIObj = Instantiate(monsterUISlotPrefab, monsterSpawnParent);
            monsterUIObj.name = $"MonsterUISlot_{data.monsterName}_{i}";
            MonsterUISlot uiSlot = monsterUIObj.GetComponent<MonsterUISlot>();

            if (uiSlot == null)
            {
                Destroy(monsterUIObj);
                continue;
            }

            GameObject monsterDataObj = new GameObject($"MonsterData_{data.monsterName}_{i}");
            monsterDataObj.transform.SetParent(monsterUIObj.transform);
            monsterDataObj.transform.localPosition = Vector3.zero;

            Monster monster = monsterDataObj.AddComponent<Monster>();

            List<SkillDataSO> monsterSkills = LoadMonsterSkills(data);

            monster.Initialize(data, monsterSkills, uiSlot);

            uiSlot.Initialize(monster);

            currentMonsters.Add(monster);
        }

        combatUI.InitializeMonsterUI(currentMonsters);
    }

    /// <summary>
    /// 몬스터 스킬 로드
    /// </summary>
    private List<SkillDataSO> LoadMonsterSkills(MonsterSpawnData data)
    {
        List<SkillDataSO> skills = new List<SkillDataSO>();

        if (data.skills != null && data.skills.Length > 0)
        {
            skills.AddRange(data.skills);
        }

        return skills;
    }

    /// <summary>
    /// 전투 UI 초기화
    /// </summary>
    private void InitializeCombatUI()
    {
        if (combatUI == null)
        {
            return;
        }

        combatUI.InitializePartyUI(currentParty);
    }

    /// <summary>
    /// 턴 시작 이벤트 핸들러
    /// </summary>
    private void OnTurnStarted(ICombatant combatant)
    {
        combatUI.UpdateCurrentTurn(combatant);

        if (combatant.IsPlayer)
        {
            Character character = combatant as Character;
            combatUI.InitializeSkillSlots(character.Skills);
            combatUI.UpdateSkillSlotsByMP(character);
        }
    }

    /// <summary>
    /// 턴 종료 이벤트 핸들러
    /// </summary>
    private void OnTurnEnded(ICombatant combatant)
    {
        UpdateAllCombatantUI();
    }

    /// <summary>
    /// 전투 종료 이벤트 핸들러
    /// </summary>
    private void OnBattleEnded()
    {
        bool isVictory = CheckVictory();
        EndCombat(isVictory);
    }

    /// <summary>
    /// TPE 미니게임 완료 이벤트
    /// </summary>
    private void OnTPEComplete(bool success)
    {
        tpeSuccess = success;

        if (combatUI != null)
        {
            combatUI.HideTPEMinigame();
        }

        ExecutePlayerAttack();
    }

    /// <summary>
    /// 플레이어 공격 실행 (TPE 후)
    /// </summary>
    private void ExecutePlayerAttack()
    {
        if (pendingSkill == null || pendingTarget == null)
        {
            return;
        }

        Character player = turnController.GetCurrentCombatant() as Character;

        if (player == null)
        {
            return;
        }

        float critBonus = tpeSuccess ? 30f : 0f;
        bool isCritical = player.Stats.RollCritical(critBonus);

        player.UseSkill(pendingSkill, pendingTarget, isCritical);

        turnController.EndCurrentTurn();

        pendingSkill = null;
        pendingTarget = null;
    }

    /// <summary>
    /// 패링 미니게임 완료 이벤트
    /// </summary>
    private void OnParryComplete(bool success)
    {
        if (combatUI != null)
        {
            combatUI.HideParryMinigame();
        }

        if (!success)
        {
            pendingDefender.TakeDamage(pendingDamage);
        }

        turnController.EndCurrentTurn();

        pendingDamage = 0;
        pendingDefender = null;
    }

    /// <summary>
    /// 모든 전투자 UI 업데이트
    /// </summary>
    private void UpdateAllCombatantUI()
    {
        foreach (var character in currentParty)
        {
            combatUI.UpdatePartyMemberStats(character);
        }

        foreach (var monster in currentMonsters)
        {
            combatUI.UpdateMonsterStats(monster);
        }
    }

    /// <summary>
    /// 승리 확인
    /// </summary>
    private bool CheckVictory()
    {
        bool allMonstersDead = currentMonsters.All(m => !m.IsAlive);
        bool allPartyDead = currentParty.All(c => !c.IsAlive);

        if (allMonstersDead)
        {
            return true;
        }

        if (allPartyDead)
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// 전투 종료 처리
    /// 승리 시 보상 UI를 표시하고, 모든 보상 수령 후 OnCombatEnded 이벤트를 발생시킵니다.
    /// </summary>
    public void EndCombat(bool isVictory)
    {
        isCombatActive = false;

        if (isVictory)
        {
            CalculateRewards();
            GiveRewards();

            // 보상 UI 표시 (RewardInventoryUI.OnAllRewardsClaimed 이벤트 대기)
            if (rewardInventoryUI != null)
            {
                rewardInventoryUI.ShowRewardInventory();
            }
            else
            {
                // 보상 UI가 없으면 즉시 이벤트 발생
                OnCombatEnded?.Invoke(isVictory);
                CleanupCombat();
            }
        }
        else
        {
            OnCombatEnded?.Invoke(isVictory);
            CleanupCombat();
        }
    }

    /// <summary>
    /// 모든 보상 수령 완료 이벤트 핸들러
    /// 보상 수령이 완료되면 전투 종료 이벤트를 발생시킵니다.
    /// </summary>
    private void OnRewardsClaimed()
    {
        OnCombatEnded?.Invoke(true);
        CleanupCombat();
    }

    /// <summary>
    /// 보상 계산
    /// </summary>
    private void CalculateRewards()
    {
        totalGoldReward = 0;
        totalExpReward = 0;

        foreach (var monster in currentMonsters)
        {
            totalGoldReward += UnityEngine.Random.Range(10, 50);
            totalExpReward += UnityEngine.Random.Range(20, 100);
        }

        if (isBossFight)
        {
            totalGoldReward *= 2;
            totalExpReward *= 2;
        }
    }

    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveRewards()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(totalGoldReward);
        }
    }

    /// <summary>
    /// 전투 정리
    /// </summary>
    private void CleanupCombat()
    {
        foreach (var character in currentParty)
        {
            if (character != null)
            {
                Destroy(character.gameObject);
            }
        }
        currentParty.Clear();

        foreach (var monster in currentMonsters)
        {
            if (monster != null)
            {
                Destroy(monster.gameObject);
            }
        }
        currentMonsters.Clear();
    }

    private void OnDestroy()
    {
        if (turnController != null)
        {
            turnController.OnTurnStart -= OnTurnStarted;
            turnController.OnTurnEnd -= OnTurnEnded;
            turnController.OnBattleEnd -= OnBattleEnded;
        }

        if (tpeMinigame != null)
        {
            tpeMinigame.OnMinigameComplete -= OnTPEComplete;
        }

        if (parryMinigame != null)
        {
            parryMinigame.OnParryComplete -= OnParryComplete;
        }

        if (rewardInventoryUI != null)
        {
            rewardInventoryUI.OnAllRewardsClaimed -= OnRewardsClaimed;
        }
    }

    /// <summary>
    /// 플레이어 행동 요청 (CombatUI에서 호출)
    /// </summary>
    public void RequestPlayerAction(SkillDataSO skill, ICombatant target)
    {
        pendingSkill = skill;
        pendingTarget = target;

        if (tpeMinigame != null)
        {
            MonsterDifficulty difficulty = MonsterDifficulty.Normal;

            if (target is Monster monster)
            {
                difficulty = monster.GetDifficulty();
            }

            combatUI.ShowTPEMinigame();
            tpeMinigame.StartMinigame(difficulty);
        }
        else
        {
            ExecutePlayerAttack();
        }
    }

    /// <summary>
    /// AI 공격 실행 (TurnController에서 호출)
    /// </summary>
    public void ExecuteAIAttack(Monster monster, SkillDataSO skill, Character target)
    {
        bool isCritical = monster.Stats.RollCritical();

        int damage = skill.CalculateDamage(monster.Stats, isCritical);

        pendingDamage = damage;
        pendingDefender = target;

        if (parryMinigame != null)
        {
            combatUI.ShowParryMinigame();
            parryMinigame.StartMinigame();
        }
        else
        {
            target.TakeDamage(damage);
            turnController.EndCurrentTurn();
        }
    }

    /// <summary>
    /// AI 타겟 화살표 표시 (TurnController에서 호출)
    /// </summary>
    public void ShowTargetArrowForAI(Character target)
    {
        if (combatUI == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        combatUI.ShowTargetArrow(target);
    }
}