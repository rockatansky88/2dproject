using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{
    private Queue<ICombatant> turnQueue;
    private ICombatant currentCombatant;
    private bool isProcessingTurn;

    public event Action<ICombatant> OnTurnStart;
    public event Action<ICombatant> OnTurnEnd;
    public event Action OnBattleEnd;

    public void InitializeBattle(List<Character> party, List<Monster> enemies)
    {
        turnQueue = new Queue<ICombatant>();

        // 모든 전투원을 속도 기준으로 정렬
        List<ICombatant> allCombatants = new List<ICombatant>();
        //allCombatants.AddRange(party.Cast<ICombatant>());
        //allCombatants.AddRange(enemies.Cast<ICombatant>());

        // 속도 기준 정렬, 같으면 랜덤
        //allCombatants = allCombatants
        //    .OrderByDescending(c => c.Speed)
        //    .ThenBy(c => Random.value)
        //    .ToList();

        //foreach (var combatant in allCombatants)
        //{
        //    turnQueue.Enqueue(combatant);
        //}

        ProcessNextTurn();
    }

    public void ProcessNextTurn()
    {
        if (isProcessingTurn) return;

        // 전투 종료 조건 체크
        if (CheckBattleEnd())
        {
            OnBattleEnd?.Invoke();
            return;
        }

        // 죽은 캐릭터 스킵
        while (turnQueue.Count > 0 && !turnQueue.Peek().IsAlive)
        {
            turnQueue.Dequeue();
        }

        if (turnQueue.Count == 0)
        {
            // 한 라운드 종료, 큐 재생성
            //RebuildTurnQueue();
        }

        currentCombatant = turnQueue.Dequeue();
        isProcessingTurn = true;

        OnTurnStart?.Invoke(currentCombatant);

        // AI or Player 턴 처리
        if (currentCombatant.IsPlayer)
        {
            // 플레이어 입력 대기 (UI에서 처리)
            //BattleManager.Instance.WaitForPlayerInput(currentCombatant as Character);
        }
        else
        {
            // AI 자동 행동
            StartCoroutine(ProcessAITurn(currentCombatant as Monster));
        }
    }

    private IEnumerator ProcessAITurn(Monster monster)
    {
        yield return new WaitForSeconds(0.5f); // AI 사고 시간

        //CombatAction action = monster.DecideAction();
        //yield return StartCoroutine(action.Execute());

        EndTurn();
    }

    public void ExecutePlayerAction(CombatAction action)
    {
        StartCoroutine(ExecutePlayerActionCoroutine(action));
    }

    private IEnumerator ExecutePlayerActionCoroutine(CombatAction action)
    {
        yield return StartCoroutine(action.Execute());
        EndTurn();
    }

    private void EndTurn()
    {
        OnTurnEnd?.Invoke(currentCombatant);
        isProcessingTurn = false;
        ProcessNextTurn();
    }

    private bool CheckBattleEnd()
    {
        // 구현
        return false;
    }
}