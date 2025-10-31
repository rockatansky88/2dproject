﻿using UnityEngine;
using System;  // ✅ 추가!
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
    [SerializeField] private Transform partySpawnParent;    // 파티 생성 위치
    [SerializeField] private Transform monsterSpawnParent;  // 몬스터 생성 위치
    [SerializeField] private GameObject characterPrefab;     // 캐릭터 프리팹

    // 🆕 추가: MonsterUISlot 프리팹 참조 (Inspector에서 할당)
    [SerializeField] private GameObject monsterUISlotPrefab; // MonsterUISlot 프리팹

    [Header("컴포넌트 참조")]
    [SerializeField] private CombatUI combatUI;
    [SerializeField] private TurnController turnController;
    [SerializeField] private TPEMinigame tpeMinigame;
    [SerializeField] private ParryMinigame parryMinigame;

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
            Debug.Log("[CombatManager] 싱글톤 인스턴스 생성");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 컴포넌트 자동 연결
        if (combatUI == null) combatUI = FindObjectOfType<CombatUI>();
        if (turnController == null) turnController = GetComponent<TurnController>();
        if (tpeMinigame == null) tpeMinigame = FindObjectOfType<TPEMinigame>();
        if (parryMinigame == null) parryMinigame = FindObjectOfType<ParryMinigame>();
    }

    private void Start()
    {
        // 턴 컨트롤러 이벤트 연결
        if (turnController != null)
        {
            turnController.OnTurnStart += OnTurnStarted;
            turnController.OnTurnEnd += OnTurnEnded;
            turnController.OnBattleEnd += OnBattleEnded;
            Debug.Log("[CombatManager] TurnController 이벤트 연결 완료");
        }

        // TPE 미니게임 이벤트 연결
        if (tpeMinigame != null)
        {
            tpeMinigame.OnMinigameComplete += OnTPEComplete;
            Debug.Log("[CombatManager] TPEMinigame 이벤트 연결 완료");
        }

        // 패링 미니게임 이벤트 연결
        if (parryMinigame != null)
        {
            parryMinigame.OnParryComplete += OnParryComplete;
            Debug.Log("[CombatManager] ParryMinigame 이벤트 연결 완료");
        }
    }

    /// <summary>
    /// 전투 시작 (DungeonManager에서 호출)
    /// </summary>
    public void StartCombat(List<MonsterSpawnData> monsterDataList, bool isBoss)
    {
        Debug.Log($"[CombatManager] ━━━━━━ 전투 시작 ━━━━━━");
        Debug.Log($"[CombatManager] 몬스터 수: {monsterDataList.Count}, 보스전: {isBoss}");

        isCombatActive = true;
        isBossFight = isBoss;

        // 1. 파티 생성
        SpawnParty();

        // 2. 몬스터 생성
        SpawnMonsters(monsterDataList);

        // 3. UI 초기화
        InitializeCombatUI();

        // 4. 턴 시스템 시작
        turnController.InitializeBattle(currentParty, currentMonsters);

        Debug.Log($"[CombatManager] ✅ 전투 초기화 완료 - 파티: {currentParty.Count}, 몬스터: {currentMonsters.Count}");
    }

    /// <summary>
    /// 파티 생성 (MercenaryManager의 파티를 Character로 변환하고 UI에 연결)
    /// </summary>
    private void SpawnParty()
    {
        Debug.Log("[CombatManager] ━━━ 파티 생성 시작 ━━━");

        currentParty.Clear();

        if (MercenaryManager.Instance == null)
        {
            Debug.LogError("[CombatManager] ❌ MercenaryManager.Instance가 null입니다!");
            return;
        }

        List<MercenaryInstance> party = MercenaryManager.Instance.CurrentParty;

        if (party.Count == 0)
        {
            Debug.LogError("[CombatManager] ❌ 파티가 비어있습니다!");
            return;
        }

        // 🔧 기존 파티 슬롯 UI 가져오기
        if (combatUI == null)
        {
            Debug.LogError("[CombatManager] ❌ CombatUI가 null입니다!");
            return;
        }

        // 파티 멤버 수만큼 Character 데이터 생성
        for (int i = 0; i < party.Count; i++)
        {
            MercenaryInstance mercData = party[i];

            Debug.Log($"[CombatManager] 파티 멤버 {i + 1}/{party.Count}: {mercData.mercenaryName}");

            // 🆕 GameObject를 생성하지 않고, 데이터만 담는 경량 오브젝트 생성
            GameObject charDataObj = new GameObject($"CharacterData_{mercData.mercenaryName}");
            charDataObj.transform.SetParent(partySpawnParent);
            charDataObj.transform.localPosition = Vector3.zero;

            Character character = charDataObj.AddComponent<Character>();

            // 스킬은 MercenaryInstance에서 가져옴
            List<SkillDataSO> skills = new List<SkillDataSO>(mercData.skills);

            // 🆕 UI 슬롯과 함께 초기화
            MercenaryPartySlot uiSlot = combatUI.GetPartySlot(i); // 기존 파티 슬롯 가져오기
            character.Initialize(mercData, skills, uiSlot);

            currentParty.Add(character);

            Debug.Log($"[CombatManager] ✅ {mercData.mercenaryName} 파티 배치 완료 (UI 슬롯 {i} 연결)");
        }

        Debug.Log($"[CombatManager] ✅ 파티 생성 완료 - 총 {currentParty.Count}명");
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🔧 수정: SpawnMonsters - 기존 빈 슬롯 제거 후 생성
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 몬스터 생성 (MonsterUISlot과 Monster 데이터 통합)
    /// 로직:
    /// 0. MonsterSpawnParent 밑의 기존 자식 오브젝트 모두 제거 (빈 슬롯 정리)
    /// 1. MonsterUISlot 프리팹을 MonsterSpawnParent 밑에 생성
    /// 2. Monster 데이터 오브젝트를 MonsterUISlot 밑에 생성
    /// 3. Monster.Initialize()에서 UI 슬롯과 연결
    /// 4. MonsterUISlot.Initialize()에서 Monster 데이터 연결
    /// 5. DungeonSO의 스프라이트와 스탯을 MonsterUISlot에 자동 반영
    /// </summary>
    private void SpawnMonsters(List<MonsterSpawnData> monsterDataList)
    {
        Debug.Log("[CombatManager] ━━━ 몬스터 생성 시작 ━━━━━");

        currentMonsters.Clear();

        if (combatUI == null)
        {
            Debug.LogError("[CombatManager] ❌ CombatUI가 null입니다!");
            return;
        }

        // 🔧 수정: Inspector에서 할당된 프리팹 사용
        if (monsterUISlotPrefab == null)
        {
            Debug.LogError("[CombatManager] ❌ MonsterUISlot 프리팹이 할당되지 않았습니다! Inspector에서 할당해주세요.");
            return;
        }

        // 🆕 0단계: MonsterSpawnParent 밑의 기존 자식 오브젝트 모두 제거 (빈 슬롯 정리)
        if (monsterSpawnParent != null)
        {
            int childCount = monsterSpawnParent.childCount;
            Debug.Log($"[CombatManager] 🧹 기존 몬스터 슬롯 {childCount}개 정리 중...");

            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = monsterSpawnParent.GetChild(i);
                Debug.Log($"[CombatManager] 제거: {child.name}");
                Destroy(child.gameObject);
            }

            Debug.Log("[CombatManager] ✅ 기존 몬스터 슬롯 정리 완료");
        }

        for (int i = 0; i < monsterDataList.Count; i++)
        {
            MonsterSpawnData data = monsterDataList[i];

            Debug.Log($"[CombatManager] 몬스터 {i + 1}/{monsterDataList.Count}: {data.monsterName} 생성 중...");
            Debug.Log($"[CombatManager] 📊 몬스터 데이터 확인:\n" +
                     $"  - 이름: {data.monsterName}\n" +
                     $"  - 스프라이트: {(data.monsterSprite != null ? data.monsterSprite.name : "null")}\n" +
                     $"  - 스탯SO: {(data.monsterStats != null ? data.monsterStats.name : "null")}\n" +
                     $"  - 난이도: {data.difficulty}");

            // 🆕 1단계: MonsterUISlot 생성 (MonsterSpawnParent 밑에)
            GameObject monsterUIObj = Instantiate(monsterUISlotPrefab, monsterSpawnParent);
            monsterUIObj.name = $"MonsterUISlot_{data.monsterName}_{i}"; // 이름 정리
            MonsterUISlot uiSlot = monsterUIObj.GetComponent<MonsterUISlot>();

            if (uiSlot == null)
            {
                Debug.LogError($"[CombatManager] ❌ MonsterUISlot 컴포넌트를 찾을 수 없습니다! (오브젝트: {monsterUIObj.name})");
                Destroy(monsterUIObj);
                continue;
            }

            // 🆕 2단계: Monster 데이터 오브젝트 생성 (MonsterUISlot 밑에 숨김)
            GameObject monsterDataObj = new GameObject($"MonsterData_{data.monsterName}_{i}");
            monsterDataObj.transform.SetParent(monsterUIObj.transform); // UI 슬롯 밑에 배치
            monsterDataObj.transform.localPosition = Vector3.zero;

            Monster monster = monsterDataObj.AddComponent<Monster>();

            // 🆕 3단계: 스킬 로드
            List<SkillDataSO> monsterSkills = LoadMonsterSkills(data);

            // 🆕 4단계: Monster 초기화 (UI 슬롯 연결)
            // DungeonSO의 스프라이트와 스탯이 MonsterSpawnData에 포함되어 있으므로
            // Monster.Initialize()에서 자동으로 처리됨
            monster.Initialize(data, monsterSkills, uiSlot);

            // 🆕 5단계: MonsterUISlot 초기화 (Monster 데이터 연결)
            // 이 단계에서 MonsterUISlot이 Monster의 스프라이트와 스탯을 받아서 UI에 표시
            uiSlot.Initialize(monster);

            currentMonsters.Add(monster);

            Debug.Log($"[CombatManager] ✅ {data.monsterName} 생성 완료\n" +
                     $"  - UI 슬롯: {uiSlot.name}\n" +
                     $"  - 데이터: {monster.name}\n" +
                     $"  - HP: {monster.Stats.CurrentHP}/{monster.Stats.MaxHP}\n" +
                     $"  - 스프라이트 연결: {(data.monsterSprite != null ? "O" : "X")}");
        }

        // 🆕 6단계: CombatUI에 몬스터 슬롯 연결 및 클릭 이벤트 등록
        combatUI.InitializeMonsterUI(currentMonsters);

        Debug.Log($"[CombatManager] ✅ 몬스터 생성 완료 - 총 {currentMonsters.Count}마리");
    }

    /// <summary>
    /// 몬스터 스킬 로드   (임시)
    /// </summary>
    private List<SkillDataSO> LoadMonsterSkills(MonsterSpawnData data)
    {
        Debug.Log($"[CombatManager] 몬스터 스킬 로드: {data.monsterName}");

        List<SkillDataSO> skills = new List<SkillDataSO>();

        // MonsterSpawnData에서 스킬 배열 가져오기
        if (data.skills != null && data.skills.Length > 0)
        {
            skills.AddRange(data.skills);
            Debug.Log($"[CombatManager] ✅ {data.monsterName} 스킬 {skills.Count}개 로드됨");
        }
        else
        {
            Debug.LogWarning($"[CombatManager] ⚠️ {data.monsterName}에 스킬이 없습니다! 기본 공격 생성 중...");
        }
        return skills;
    }

    /// <summary>
    /// 기본 공격 스킬 생성 (임시)
    /// </summary>
    private SkillDataSO CreateDefaultBasicAttack()
    {
        SkillDataSO basicAttack = ScriptableObject.CreateInstance<SkillDataSO>();
        basicAttack.skillName = "기본 공격";
        basicAttack.baseDamageMin = 5;
        basicAttack.baseDamageMax = 10;
        basicAttack.manaCost = 0;
        basicAttack.isBasicAttack = true;
        basicAttack.damageType = SkillDamageType.Physical;
        basicAttack.targetType = SkillTargetType.Single;
        basicAttack.statScaling = 0.5f;

        Debug.Log("[CombatManager] 기본 공격 스킬 임시 생성");

        return basicAttack;
    }

    /// <summary>
    /// 전투 UI 초기화
    /// </summary>
    private void InitializeCombatUI()
    {
        Debug.Log("[CombatManager] 전투 UI 초기화");

        if (combatUI == null)
        {
            Debug.LogError("[CombatManager] ❌ CombatUI가 null입니다!");
            return;
        }

        // 파티 UI 초기화
        combatUI.InitializePartyUI(currentParty);

        Debug.Log("[CombatManager] ✅ 전투 UI 초기화 완료");
    }

    /// <summary>
    /// 턴 시작 이벤트 핸들러
    /// </summary>
    private void OnTurnStarted(ICombatant combatant)
    {
        Debug.Log($"[CombatManager] ━━━ {combatant.Name}의 턴 시작 ━━━━━");

        // UI 업데이트
        combatUI.UpdateCurrentTurn(combatant);

        // 플레이어 턴이면 스킬 슬롯 표시
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
        Debug.Log($"[CombatManager] {combatant.Name}의 턴 종료");

        // HP/MP UI 업데이트
        UpdateAllCombatantUI();
    }

    /// <summary>
    /// 전투 종료 이벤트 핸들러
    /// </summary>
    private void OnBattleEnded()
    {
        Debug.Log("[CombatManager] ━━━━━━ 전투 종료 ━━━━━━");

        bool isVictory = CheckVictory();

        EndCombat(isVictory);
    }

    /// <summary>
    /// TPE 미니게임 완료 이벤트
    /// </summary>
    private void OnTPEComplete(bool success)
    {
        Debug.Log($"[CombatManager] 🎯 TPE 미니게임 결과: {(success ? "성공 (크리티컬 +30%)" : "실패")}");

        tpeSuccess = success;

        // UI 숨김
        if (combatUI != null)
        {
            combatUI.HideTPEMinigame();
        }

        // 실제 공격 실행
        ExecutePlayerAttack();
    }

    /// <summary>
    /// 플레이어 공격 실행 (TPE 후)
    /// </summary>
    private void ExecutePlayerAttack()
    {
        if (pendingSkill == null || pendingTarget == null)
        {
            Debug.LogError("[CombatManager] ❌ 대기 중인 스킬 또는 타겟이 없습니다!");
            return;
        }

        Character player = turnController.GetCurrentCombatant() as Character;

        if (player == null)
        {
            Debug.LogError("[CombatManager] ❌ 현재 턴이 플레이어가 아닙니다!");
            return;
        }

        Debug.Log($"[CombatManager] ⚔️ {player.Name}이(가) {pendingTarget.Name}에게 {pendingSkill.skillName} 사용!");

        // 크리티컬 판정 (TPE 성공 시 +30% 보너스)
        float critBonus = tpeSuccess ? 30f : 0f;
        bool isCritical = player.Stats.RollCritical(critBonus);

        Debug.Log($"[CombatManager] 크리티컬 판정: {(isCritical ? "크리티컬!" : "일반 공격")} (보너스: +{critBonus}%)");

        // 스킬 사용
        player.UseSkill(pendingSkill, pendingTarget, isCritical);

        // 턴 종료
        turnController.EndCurrentTurn();

        // 초기화
        pendingSkill = null;
        pendingTarget = null;
    }

    /// <summary>
    /// 패링 미니게임 완료 이벤트
    /// </summary>
    private void OnParryComplete(bool success)
    {
        Debug.Log($"[CombatManager] 🛡️ 패링 미니게임 결과: {(success ? "성공 (데미지 0)" : "실패 (일반 데미지)")}");

        // UI 숨김
        if (combatUI != null)
        {
            combatUI.HideParryMinigame();
        }

        if (success)
        {
            // 패링 성공: 데미지 0
            Debug.Log($"[CombatManager] ✅ {pendingDefender.Name} 패링 성공! 데미지 0");
        }
        else
        {
            // 패링 실패: 일반 데미지 적용
            Debug.Log($"[CombatManager] ❌ {pendingDefender.Name} 패링 실패! 데미지 {pendingDamage} 적용");
            pendingDefender.TakeDamage(pendingDamage);
        }

        // 턴 종료
        turnController.EndCurrentTurn();

        // 초기화
        pendingDamage = 0;
        pendingDefender = null;
    }

    /// <summary>
    /// 모든 전투자 UI 업데이트
    /// </summary>
    private void UpdateAllCombatantUI()
    {
        // 파티 HP/MP 업데이트
        foreach (var character in currentParty)
        {
            combatUI.UpdatePartyMemberStats(character);
        }

        // 몬스터 HP 업데이트
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
            Debug.Log("[CombatManager] ✅ 승리! 모든 몬스터 처치");
            return true;
        }

        if (allPartyDead)
        {
            Debug.Log("[CombatManager] ❌ 패배! 파티 전멸");
            return false;
        }

        return false;
    }

    /// <summary>
    /// 전투 종료 처리
    /// </summary>
    public void EndCombat(bool isVictory)
    {
        Debug.Log($"[CombatManager] ━━━ 전투 종료: {(isVictory ? "승리" : "패배")} ━━━");

        isCombatActive = false;

        if (isVictory)
        {
            // 보상 계산
            CalculateRewards();

            // 보상 지급
            GiveRewards();

            Debug.Log($"[CombatManager] ✅ 보상 지급 완료 - 골드: {totalGoldReward}, 경험치: {totalExpReward}");

            // 다음 방으로 이동
            if (DungeonManager.Instance != null)
            {
                if (DungeonManager.Instance.IsDungeonCleared())
                {
                    Debug.Log("[CombatManager] ✅ 던전 클리어!");
                    DungeonManager.Instance.CompleteDungeon();
                }
                else
                {
                    Debug.Log("[CombatManager] 다음 방으로 이동");
                    DungeonManager.Instance.MoveToNextRoom();
                }
            }
        }
        else
        {
            // 패배 처리
            Debug.Log("[CombatManager] ❌ 패배! 던전 퇴장");

            if (DungeonManager.Instance != null)
            {
                DungeonManager.Instance.ExitDungeon();
            }
        }

        // 전투 종료 이벤트 발생
        OnCombatEnded?.Invoke(isVictory);

        // 정리
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

        // 보스전이면 보상 2배
        if (isBossFight)
        {
            totalGoldReward *= 2;
            totalExpReward *= 2;
            Debug.Log("[CombatManager] 보스전 보상 2배 적용!");
        }

        Debug.Log($"[CombatManager] 보상 계산 완료 - 골드: {totalGoldReward}, 경험치: {totalExpReward}");
    }

    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveRewards()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(totalGoldReward);
            Debug.Log($"[CombatManager] ✅ 골드 {totalGoldReward} 지급");
        }

        // TODO: 경험치 시스템 구현 시 추가
        Debug.Log($"[CombatManager] ✅ 경험치 {totalExpReward} 지급 (미구현)");
    }

    /// <summary>
    /// 전투 정리
    /// </summary>
    private void CleanupCombat()
    {
        Debug.Log("[CombatManager] 전투 정리 시작");

        // 파티 오브젝트 제거
        foreach (var character in currentParty)
        {
            if (character != null)
            {
                Destroy(character.gameObject);
            }
        }
        currentParty.Clear();

        // 몬스터 오브젝트 제거
        foreach (var monster in currentMonsters)
        {
            if (monster != null)
            {
                Destroy(monster.gameObject);
            }
        }
        currentMonsters.Clear();

        Debug.Log("[CombatManager] ✅ 전투 정리 완료");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
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
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: 플레이어 행동 요청 (CombatUI에서 호출)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// 플레이어 행동 요청 (스킬 + 타겟 선택 완료 시)
    /// </summary>
    public void RequestPlayerAction(SkillDataSO skill, ICombatant target)
    {
        Debug.Log($"[CombatManager] 🎯 플레이어 행동 요청: {skill.skillName} -> {target.Name}");

        pendingSkill = skill;
        pendingTarget = target;

        // TPE 미니게임 시작
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
            Debug.LogWarning("[CombatManager] ⚠️ TPEMinigame가 없어서 바로 공격 실행");
            ExecutePlayerAttack();
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 🆕 추가: AI 공격 실행 (TurnController에서 호출)
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    /// <summary>
    /// AI 공격 실행
    /// </summary>
    public void ExecuteAIAttack(Monster monster, SkillDataSO skill, Character target)
    {
        Debug.Log($"[CombatManager] 🤖 AI 공격: {monster.Name} -> {target.Name} (스킬: {skill.skillName})");

        // 데미지 계산
        int damage = CalculateDamage(monster, skill, target);

        Debug.Log($"[CombatManager] 계산된 데미지: {damage}");

        // 패링 미니게임 표시
        pendingDamage = damage;
        pendingDefender = target;

        if (parryMinigame != null)
        {
            combatUI.ShowParryMinigame();
            parryMinigame.StartMinigame();
        }
        else
        {
            Debug.LogWarning("[CombatManager] ⚠️ ParryMinigame가 없어서 바로 데미지 적용");
            target.TakeDamage(damage);
            turnController.EndCurrentTurn();
        }
    }

    /// <summary>
    /// 데미지 계산 (임시)
    /// </summary>
    private int CalculateDamage(Monster attacker, SkillDataSO skill, Character defender)
    {
        // 크리티컬 판정
        bool isCritical = attacker.Stats.RollCritical();

        // 스킬 데미지 계산
        int damage = skill.CalculateDamage(attacker.Stats, isCritical);

        Debug.Log($"[CombatManager] 데미지 계산: {damage} (크리티컬: {isCritical})");

        return damage;
    }
}