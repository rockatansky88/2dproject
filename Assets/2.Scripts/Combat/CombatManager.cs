using UnityEngine;
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
    /// 파티 생성 (MercenaryManager에서 현재 파티 가져오기)
    /// </summary>
    private void SpawnParty()
    {
        Debug.Log("[CombatManager] 파티 생성 시작");

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

        //for (int i = 0; i < party.Count; i++)
        //{
        //    MercenaryInstance mercData = party[i];

        //    // CharacterFactory를 통해 생성
        //    Character character = CharacterFactory.CreateCharacter(mercData, partySpawnParent);

        //    if (character != null)
        //    {
        //        currentParty.Add(character);
        //        Debug.Log($"[CombatManager] ✅ {mercData.mercenaryName} 파티 배치 완료");
        //    }
        //}

        Debug.Log($"[CombatManager] 파티 생성 완료 - 총 {currentParty.Count}명");
    }

    /// <summary>
    /// 몬스터 생성 (씬에 배치)
    /// </summary>
    private void SpawnMonsters(List<MonsterSpawnData> monsterDataList)
    {
        Debug.Log("[CombatManager] ━━━ 몬스터 생성 시작 ━━━");

        currentMonsters.Clear();

        for (int i = 0; i < monsterDataList.Count; i++)
        {
            MonsterSpawnData data = monsterDataList[i];

            Debug.Log($"[CombatManager] 몬스터 {i + 1}/{monsterDataList.Count}: {data.monsterName}");

            // 몬스터 GameObject 생성
            GameObject monsterObj = new GameObject($"Monster_{data.monsterName}_{i}");
            monsterObj.transform.SetParent(monsterSpawnParent);
            monsterObj.transform.localPosition = new Vector3(i * 150f, 0f, 0f); // 간격 배치

            Debug.Log($"[CombatManager] GameObject 생성: {monsterObj.name}, 위치: {monsterObj.transform.localPosition}");

            // SpriteRenderer 추가 (몬스터 이미지 표시)
            SpriteRenderer spriteRenderer = monsterObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = data.monsterSprite;
            spriteRenderer.sortingOrder = 10;

            Debug.Log($"[CombatManager] SpriteRenderer 추가 완료");

            // Canvas + HealthBar UI 추가 (몬스터 위에 HP 바 표시)
            GameObject canvasObj = new GameObject("MonsterCanvas");
            canvasObj.transform.SetParent(monsterObj.transform);
            canvasObj.transform.localPosition = new Vector3(0f, 100f, 0f); // 몬스터 위쪽

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(100f, 20f);

            // HP 바 이미지
            GameObject hpBarObj = new GameObject("HealthBar");
            hpBarObj.transform.SetParent(canvasObj.transform);

            UnityEngine.UI.Image hpBarImage = hpBarObj.AddComponent<UnityEngine.UI.Image>();
            hpBarImage.color = Color.red;
            hpBarImage.type = UnityEngine.UI.Image.Type.Filled;
            hpBarImage.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;

            RectTransform hpBarRect = hpBarObj.GetComponent<RectTransform>();
            hpBarRect.anchorMin = Vector2.zero;
            hpBarRect.anchorMax = Vector2.one;
            hpBarRect.sizeDelta = Vector2.zero;

            // HealthBar 컴포넌트 추가
            HealthBar healthBar = hpBarObj.AddComponent<HealthBar>();
            // healthBar.Initialize() 필요 시 여기서 호출

            Debug.Log($"[CombatManager] HealthBar UI 생성 완료");

            // Monster 컴포넌트 추가
            Monster monster = monsterObj.AddComponent<Monster>();
            monster.UIAnchor = canvasObj.transform;

            // 스킬 로드 (임시)
            List<SkillDataSO> monsterSkills = LoadMonsterSkills(data);

            // Monster 초기화
            monster.Initialize(data, monsterSkills);
            currentMonsters.Add(monster);

            Debug.Log($"[CombatManager] ✅ {data.monsterName} 생성 완료 (HP: {monster.Stats.CurrentHP}/{monster.Stats.MaxHP})");
        }

        Debug.Log($"[CombatManager] ✅ 몬스터 생성 완료 - 총 {currentMonsters.Count}마리");
    }

    /// <summary>
    /// 몬스터 스킬 로드 (임시)
    /// </summary>
    private List<SkillDataSO> LoadMonsterSkills(MonsterSpawnData data)
    {
        Debug.Log($"[CombatManager] 몬스터 스킬 로드: {data.monsterName}");

        List<SkillDataSO> skills = new List<SkillDataSO>();

        // TODO: Resources 폴더에서 몬스터 스킬 로드
        Debug.LogWarning("[CombatManager] ⚠️ 몬스터 스킬 로드 미구현 - 빈 스킬 리스트 반환");

        return skills;
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
        Debug.Log($"[CombatManager] ━━━ {combatant.Name}의 턴 시작 ━━━");

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
        Debug.Log($"[CombatManager] TPE 미니게임 결과: {(success ? "성공" : "실패")}");

        // 크리티컬 보너스 적용
        float critBonus = success ? 30f : 0f;

        // TODO: 크리티컬 보너스를 스킬 데미지에 반영
        // 현재 턴 캐릭터의 행동 실행
        ICombatant currentCombatant = turnController.GetCurrentCombatant();

        if (currentCombatant is Character character)
        {
            // TODO: 선택된 스킬과 타겟으로 공격 실행
            Debug.Log($"[CombatManager] {character.Name} 공격 실행 (크리티컬 보너스: +{critBonus}%)");
        }
    }

    /// <summary>
    /// 패링 미니게임 완료 이벤트
    /// </summary>
    private void OnParryComplete(bool success)
    {
        Debug.Log($"[CombatManager] 패링 미니게임 결과: {(success ? "성공" : "실패")}");

        if (success)
        {
            // 데미지 50% 감소 + 반격 데미지 50%
            Debug.Log("[CombatManager] ✅ 패링 성공! 데미지 감소 + 반격!");
        }
        else
        {
            // 일반 데미지 적용
            Debug.Log("[CombatManager] ❌ 패링 실패! 일반 데미지 적용");
        }
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
}