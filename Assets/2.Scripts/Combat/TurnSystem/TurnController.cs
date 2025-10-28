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

        // ��� �������� �ӵ� �������� ����
        List<ICombatant> allCombatants = new List<ICombatant>();
        //allCombatants.AddRange(party.Cast<ICombatant>());
        //allCombatants.AddRange(enemies.Cast<ICombatant>());

        // �ӵ� ���� ����, ������ ����
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

        // ���� ���� ���� üũ
        if (CheckBattleEnd())
        {
            OnBattleEnd?.Invoke();
            return;
        }

        // ���� ĳ���� ��ŵ
        while (turnQueue.Count > 0 && !turnQueue.Peek().IsAlive)
        {
            turnQueue.Dequeue();
        }

        if (turnQueue.Count == 0)
        {
            // �� ���� ����, ť �����
            //RebuildTurnQueue();
        }

        currentCombatant = turnQueue.Dequeue();
        isProcessingTurn = true;

        OnTurnStart?.Invoke(currentCombatant);

        // AI or Player �� ó��
        if (currentCombatant.IsPlayer)
        {
            // �÷��̾� �Է� ��� (UI���� ó��)
            //BattleManager.Instance.WaitForPlayerInput(currentCombatant as Character);
        }
        else
        {
            // AI �ڵ� �ൿ
            StartCoroutine(ProcessAITurn(currentCombatant as Monster));
        }
    }

    private IEnumerator ProcessAITurn(Monster monster)
    {
        yield return new WaitForSeconds(0.5f); // AI ��� �ð�

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
        // ����
        return false;
    }
}