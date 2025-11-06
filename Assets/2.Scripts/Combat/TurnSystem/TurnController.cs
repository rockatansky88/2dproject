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

    public event Action<ICombatant> OnTurnStart;
    public event Action<ICombatant> OnTurnEnd;
    public event Action OnBattleEnd;

    private List<Character> party;
    private List<Monster> enemies;
    private bool isBattleEnding = false;

    /// <summary>
    /// 전투 초기화 - 파티와 적 리스트 받아서 턴 큐 생성
    /// </summary>
    public void InitializeBattle(List<Character> partyList, List<Monster> enemyList)
    {
        party = partyList;
        enemies = enemyList;
        turnQueue = new Queue<ICombatant>();
        isProcessingTurn = false;
        isBattleEnding = false;

        List<ICombatant> allCombatants = new List<ICombatant>();
        allCombatants.AddRange(party.Cast<ICombatant>());
        allCombatants.AddRange(enemies.Cast<ICombatant>());

        allCombatants = allCombatants
            .OrderByDescending(c => c.Speed)
            .ThenBy(c => UnityEngine.Random.value)
            .ToList();

        foreach (var combatant in allCombatants)
        {
            turnQueue.Enqueue(combatant);
        }

        ProcessNextTurn();
    }

    /// <summary>
    /// 다음 턴 처리
    /// </summary>
    public void ProcessNextTurn()
    {
        if (isProcessingTurn)
        {
            return;
        }

        if (isBattleEnding)
        {
            return;
        }

        if (CheckBattleEnd())
        {
            return;
        }

        while (turnQueue.Count > 0 && !turnQueue.Peek().IsAlive)
        {
            ICombatant dead = turnQueue.Dequeue();
        }

        if (turnQueue.Count == 0)
        {
            RebuildTurnQueue();
        }

        currentCombatant = turnQueue.Dequeue();
        isProcessingTurn = true;

        OnTurnStart?.Invoke(currentCombatant);

        if (currentCombatant.IsPlayer)
        {
        }
        else
        {
            StartCoroutine(ProcessAITurn(currentCombatant as Monster));
        }
    }

    /// <summary>
    /// AI 턴 처리
    /// 몬스터가 스킬을 선택하고, 타겟을 선택한 후 스킬에 지정된 애니메이션 재생 → 실제 데미지 처리
    /// </summary>
    private IEnumerator ProcessAITurn(Monster monster)
    {
        yield return new WaitForSeconds(1f);

        SkillDataSO skill = monster.DecideAction();

        if (skill == null)
        {
            EndCurrentTurn();
            yield break;
        }

        Character target = SelectRandomAlivePlayer();

        if (target == null)
        {
            EndCurrentTurn();
            yield break;
        }

        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ShowTargetArrowForAI(target);
        }

        yield return new WaitForSeconds(0.5f);

        // 애니메이션 재생 (안전하게 처리)
        if (monster.uiSlot != null)
        {
            bool animationComplete = false;
            string clipName = "attack";

            // attackerAnimationClip 필드가 없어도 동작하도록 안전하게 처리
            try
            {
                var clipField = skill.GetType().GetField("attackerAnimationClip");
                if (clipField != null)
                {
                    string fieldValue = clipField.GetValue(skill) as string;
                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        clipName = fieldValue;
                    }
                }
            }
            catch
            {
                // 필드가 없으면 기본값 사용
            }

            StartCoroutine(monster.uiSlot.PlayAttackAnimation(clipName, () =>
            {
                animationComplete = true;
            }));

            yield return new WaitUntil(() => animationComplete);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

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
        StartCoroutine(ExecutePlayerActionCoroutine(skill, target));
    }

    private IEnumerator ExecutePlayerActionCoroutine(SkillDataSO skill, ICombatant target)
    {
        Character player = currentCombatant as Character;

        if (player == null)
        {
            yield break;
        }

        // 크리티컬 판정 (TPE 성공 시 +30% 보너스 추가 가능)
        bool isCritical = player.Stats.RollCritical();

        // 스킬 사용
        bool success = player.UseSkill(skill, target, isCritical);

        if (!success)
        {
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
    }

    /// <summary>
    /// 전투 종료 확인
    /// 즉시 OnBattleEnd 이벤트 발생
    /// </summary>
    private bool CheckBattleEnd()
    {
        bool allPartyDead = party.All(c => !c.IsAlive);
        bool allMonstersDead = enemies.All(m => !m.IsAlive);

        if (allPartyDead)
        {
            isBattleEnding = true;
            isProcessingTurn = false;

            Debug.Log("[TurnController] 파티 전멸! 전투 종료");
            OnBattleEnd?.Invoke();
            return true;
        }

        if (allMonstersDead)
        {
            isBattleEnding = true;
            isProcessingTurn = false;

            Debug.Log("[TurnController] 몬스터 전멸! 전투 종료");
            OnBattleEnd?.Invoke();
            return true;
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
            return;
        }

        EndTurn();
    }
}