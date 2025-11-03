using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 턴 관리 컨트롤러
/// - 속도 기반 턴 순서 결정
/// - 현재 턴 전투자 추적
/// </summary>
public class TurnController : MonoBehaviour
{
    private Queue<ICombatant> turnQueue;
    private ICombatant currentCombatant;
    private bool isProcessingTurn;

    // 이벤트
    public event Action<ICombatant> OnTurnStart;  // 턴 시작 시
    public event Action<ICombatant> OnTurnEnd;    // 턴 종료 시
    public event Action OnBattleEnd;              // 전투 종료 시

    private List<Character> party;
    private List<Monster> enemies;

    /// <summary>
    /// 전투 초기화 - 파티와 적 리스트 받아서 턴 큐 생성
    /// </summary>
    public void InitializeBattle(List<Character> partyList, List<Monster> enemyList)
    {
        Debug.Log($"[TurnController] ━━━ 전투 초기화: 파티 {partyList.Count}명, 적 {enemyList.Count}마리 ━━━");

        party = partyList;
        enemies = enemyList;

        turnQueue = new Queue<ICombatant>();

        // 모든 전투자를 하나의 리스트로 합침
        List<ICombatant> allCombatants = new List<ICombatant>();
        allCombatants.AddRange(party.Cast<ICombatant>());
        allCombatants.AddRange(enemies.Cast<ICombatant>());

        // 속도 기반 정렬 (내림차순)
        // 속도가 같으면 랜덤 순서
        allCombatants = allCombatants
            .OrderByDescending(c => c.Speed)
            .ThenBy(c => UnityEngine.Random.value)
            .ToList();

        // 턴 큐에 추가
        foreach (var combatant in allCombatants)
        {
            turnQueue.Enqueue(combatant);
            Debug.Log($"[TurnController] 턴 큐 추가: {combatant.Name} (속도: {combatant.Speed})");
        }

        Debug.Log($"[TurnController] ✅ 턴 큐 생성 완료 - 총 {turnQueue.Count}명");

        // 첫 턴 시작
        ProcessNextTurn();
    }

    /// <summary>
    /// 다음 턴 처리
    /// </summary>
    public void ProcessNextTurn()
    {
        if (isProcessingTurn)
        {
            Debug.LogWarning("[TurnController] 이미 턴 처리 중입니다.");
            return;
        }

        // 전투 종료 확인
        if (CheckBattleEnd())
        {
            Debug.Log("[TurnController] ━━━ 전투 종료 ━━━");
            OnBattleEnd?.Invoke();
            return;
        }

        // 죽은 캐릭터 스킵
        while (turnQueue.Count > 0 && !turnQueue.Peek().IsAlive)
        {
            ICombatant dead = turnQueue.Dequeue();
            Debug.Log($"[TurnController] {dead.Name}은(는) 사망하여 턴 스킵");
        }

        // 턴 큐가 비었으면 재생성
        if (turnQueue.Count == 0)
        {
            Debug.Log("[TurnController] 턴 큐 비어있음 - 재생성 필요");
            RebuildTurnQueue();
        }

        // 현재 턴 전투자 꺼내기
        currentCombatant = turnQueue.Dequeue();
        isProcessingTurn = true;

        Debug.Log($"[TurnController] ━━━ {currentCombatant.Name}의 턴 시작 (속도: {currentCombatant.Speed}) ━━━");

        OnTurnStart?.Invoke(currentCombatant);

        // AI 또는 플레이어 턴 처리
        if (currentCombatant.IsPlayer)
        {
            Debug.Log($"[TurnController] 플레이어 {currentCombatant.Name} - 입력 대기 중...");
            // 플레이어 입력 대기 (UI에서 처리)
        }
        else
        {
            Debug.Log($"[TurnController] AI {currentCombatant.Name} - 자동 행동 처리");
            // AI 자동 행동
            StartCoroutine(ProcessAITurn(currentCombatant as Monster));
        }
    }

    /// <summary>
    /// AI 턴 처리
    /// </summary>
    private IEnumerator ProcessAITurn(Monster monster)
    {
        Debug.Log($"[TurnController] 🤖 AI {monster.Name} 행동 결정 중...");

        yield return new WaitForSeconds(1f); // AI 사고 시간

        // AI가 스킬 선택
        SkillDataSO skill = monster.DecideAction();

        if (skill == null)
        {
            Debug.LogWarning($"[TurnController] ⚠️ {monster.Name}의 스킬이 없어 턴 스킵");
            EndCurrentTurn();
            yield break;
        }

        // 타겟 선택 (살아있는 플레이어 중 랜덤)
        Character target = SelectRandomAlivePlayer();

        if (target == null)
        {
            Debug.LogWarning($"[TurnController] ⚠️ 살아있는 플레이어가 없어 턴 스킵");
            EndCurrentTurn();
            yield break;
        }

        Debug.Log($"[TurnController] 🎯 AI {monster.Name}이(가) {target.Name}을(를) 타겟으로 {skill.skillName} 사용");


        // 타겟 화살표 표시 (용병 위에 화살표 표시)

        if (CombatManager.Instance != null)
        {
            // CombatUI를 통해 타겟 화살표 표시
            CombatManager.Instance.ShowTargetArrowForAI(target);
            Debug.Log($"[TurnController] ✅ {target.Name} 위에 타겟 화살표 표시");
        }

        yield return new WaitForSeconds(0.5f); // 애니메이션 대기

        // CombatManager에게 AI 공격 요청
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ExecuteAIAttack(monster, skill, target);
        }
    }

    /// <summary>
    /// 살아있는 플레이어 중 랜덤 선택
    /// </summary>
    private Character SelectRandomAlivePlayer()
    {
        List<Character> alivePlayers = party.Where(c => c.IsAlive).ToList();

        if (alivePlayers.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, alivePlayers.Count);
        return alivePlayers[randomIndex];
    }

    /// <summary>
    /// 플레이어 행동 실행
    /// </summary>
    public void ExecutePlayerAction(SkillDataSO skill, ICombatant target)
    {
        Debug.Log($"[TurnController] 플레이어 행동 실행: {skill.skillName} -> {target.Name}");

        StartCoroutine(ExecutePlayerActionCoroutine(skill, target));
    }

    private IEnumerator ExecutePlayerActionCoroutine(SkillDataSO skill, ICombatant target)
    {
        Character player = currentCombatant as Character;

        if (player == null)
        {
            Debug.LogError("[TurnController] 현재 턴이 플레이어가 아닙니다!");
            yield break;
        }

        // 크리티컬 판정 (TPE 성공 시 +30% 보너스 추가 가능)
        bool isCritical = player.Stats.RollCritical();

        // 스킬 사용
        bool success = player.UseSkill(skill, target, isCritical);

        if (!success)
        {
            Debug.LogWarning($"[TurnController] {player.Name}의 스킬 사용 실패!");
            yield break;
        }

        yield return new WaitForSeconds(0.5f); // 애니메이션 대기

        EndTurn();
    }

    /// <summary>
    /// 턴 종료
    /// </summary>
    public void EndTurn()
    {
        Debug.Log($"[TurnController] {currentCombatant.Name}의 턴 종료");

        OnTurnEnd?.Invoke(currentCombatant);

        // 살아있으면 턴 큐 뒤에 다시 추가
        if (currentCombatant.IsAlive)
        {
            turnQueue.Enqueue(currentCombatant);
        }

        isProcessingTurn = false;

        // 다음 턴 처리
        ProcessNextTurn();
    }

    /// <summary>
    /// 턴 큐 재생성 (한 라운드 종료 시)
    /// </summary>
    private void RebuildTurnQueue()
    {
        Debug.Log("[TurnController] 턴 큐 재생성 중...");

        // TODO: 살아있는 전투자들로 턴 큐 재생성
        // 현재는 빈 큐로 놔두고, 전투 종료로 처리
    }

    /// <summary>
    /// 전투 종료 확인
    /// </summary>
    private bool CheckBattleEnd()
    {
        // 파티 전멸 확인
        bool allPartyDead = party.All(c => !c.IsAlive);

        // 몬스터 전멸 확인
        bool allMonstersDead = enemies.All(m => !m.IsAlive);

        if (allPartyDead)
        {
            Debug.Log("[TurnController] ❌ 파티 전멸! 전투 패배");
            return true;
        }

        if (allMonstersDead)
        {
            Debug.Log("[TurnController] ✅ 몬스터 전멸! 전투 승리");
            
            StartCoroutine(DelayedBattleEnd());
            return false; // 아직 종료 안 함 (대기 중)
        }

        return false;
    }

    /// <summary>
    /// 현재 턴 전투자 가져오기
    /// </summary>
    public ICombatant GetCurrentCombatant()
    {
        return currentCombatant;
    }

    /// <summary>
    /// 현재 턴 강제 종료 (public)
    /// </summary>
    public void EndCurrentTurn()
    {
        if (!isProcessingTurn)
        {
            Debug.LogWarning("[TurnController] 처리 중인 턴이 없습니다.");
            return;
        }

        EndTurn();
    }

    /// <summary>
    /// 페이드아웃 대기 후 전투 종료 이벤트 발생
    /// </summary>
    private IEnumerator DelayedBattleEnd()
    {
        Debug.Log("[TurnController] 몬스터 페이드아웃 대기 중... (2초)");
        
        // 페이드아웃 시간(1.5초) + 여유(0.5초) = 2초
        yield return new WaitForSeconds(2f);
        
        Debug.Log("[TurnController] ✅ 페이드아웃 완료 - 전투 종료 이벤트 발생");
        OnBattleEnd?.Invoke();
    }
}