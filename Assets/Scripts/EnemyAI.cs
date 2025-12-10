using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        WaitingForEnemyTurn,
        TackingTurn,
        Busy,
    }
    private State state;
    private float timer;

    private void Awake()
    {
        state = State.WaitingForEnemyTurn;
    }
    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TackingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        // 적들 액션 불가
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }

    }

    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TackingTurn;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TackingTurn;
            timer = 2f;
        }
    }

    private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
    {
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if (TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
            {
                return true;
            }
        }
        return false;
    }

    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                Debug.Log($"[{enemyUnit.name}] {baseAction.GetActionName()} - Not enough action points");
                continue;
            }

            EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();

            Debug.Log($"[{enemyUnit.name}] {baseAction.GetActionName()} - BestScore: {(testEnemyAIAction != null ? testEnemyAIAction.actionValue.ToString() : "NULL")}");

            if (bestEnemyAIAction == null)
            {
                bestEnemyAIAction = testEnemyAIAction;
                bestBaseAction = baseAction;
            }
            else if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
            {
                bestEnemyAIAction = testEnemyAIAction;
                bestBaseAction = baseAction;
            }
        }

        Debug.Log($"[{enemyUnit.name}] === FINAL DECISION: {(bestBaseAction != null ? bestBaseAction.GetActionName() : "NULL")} with score {(bestEnemyAIAction != null ? bestEnemyAIAction.actionValue.ToString() : "NULL")} ===");

        if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
        {
            bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, onEnemyAIActionComplete);
            return true;
        }
        else
        {
            return false;
        }
    }
}
