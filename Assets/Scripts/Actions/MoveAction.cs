using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    // [SerializeField] private Animator unitAnimator;
    [SerializeField] private int maxMoveDistance = 4;
    private List<Vector3> positionList;
    private int currentPositionIndex;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        float stoppingDistance = 0.1f;
        Vector3 targetPosition = positionList[currentPositionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float rotationSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection, Time.deltaTime * rotationSpeed);

        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * Time.deltaTime * moveSpeed;
        }
        else
        {
            currentPositionIndex++;
            if (currentPositionIndex >= positionList.Count)
            {
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }
        }

    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetGridPosition(), gridPosition, out int path);

        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }


    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();


        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (unitGridPosition == testGridPosition)
                {
                    // Same position
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Already occupied
                    continue;
                }

                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!Pathfinding.Instance.HasPath(unitGridPosition, testGridPosition))
                {
                    continue;
                }

                int pathfindingDistanceMultiplier = 10;
                if (Pathfinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathfindingDistanceMultiplier)
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<ShootAction>().GetTargetCountAtPosition(gridPosition);
        int pathLength = Pathfinding.Instance.GetPathLength(unit.GetGridPosition(), gridPosition);

        // 사격 가능하면 매우 높은 점수 (1000점대)
        if (targetCountAtGridPosition > 0)
        {
            int score = 1000 - pathLength;
            Debug.Log($"[SHOOT POSSIBLE] Pos:{gridPosition}, Score:{score}");
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = score,
            };
        }

        // 사격 불가능하면 플레이어 접근 (1~500점대)
        List<Unit> playerUnitList = UnitManager.Instance.GetFriendlyUnitList();

        if (playerUnitList.Count == 0)
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = 0,
            };
        }

        int shortestPathToPlayer = int.MaxValue;
        GridPosition closestPlayerPos = default;

        foreach (Unit playerUnit in playerUnitList)
        {
            GridPosition playerGridPosition = playerUnit.GetGridPosition();

            if (Pathfinding.Instance.HasPath(gridPosition, playerGridPosition))
            {
                int pathToPlayer = Pathfinding.Instance.GetPathLength(gridPosition, playerGridPosition);
                if (pathToPlayer < shortestPathToPlayer)
                {
                    shortestPathToPlayer = pathToPlayer;
                    closestPlayerPos = playerGridPosition;
                }
            }
        }

        if (shortestPathToPlayer == int.MaxValue)
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = 0,
            };
        }

        // 500점에서 경로 길이만큼 감점 (양수 보장)
        int actionValue = 500 - shortestPathToPlayer;

        Debug.Log($"[APPROACH] Pos:{gridPosition}, PathToPlayer:{shortestPathToPlayer}, Score:{actionValue}");

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = Mathf.Max(actionValue, 1), // 최소 1점
        };
    }
}
