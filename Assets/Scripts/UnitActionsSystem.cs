using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class UnitActionsSystem : MonoBehaviour
{
    public static UnitActionsSystem Instance { get; private set; }
    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;
    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy = false;

    private void Start()
    {
        SetSelectedUnit(selectedUnit);

        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one UnitActionsSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (isBusy)
        {
            return;
        }

        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        // UI 클릭 체크 추가
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (TryHnadleUnitSelection())
        {
            return;
        }

        HandleSelectedAction();


    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDown())
        {

            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                return;
            }
            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
                return;
            }

            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);

            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetBusy()
    {
        isBusy = true;

        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged?.Invoke(this, isBusy);
    }


    private bool TryHnadleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPostion());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        // 이미 선택된 유닛
                        return false;
                    }

                    if (unit.IsEnemy())
                    {
                        // 적 유닛 선택 불가
                        return false;
                    }
                    SetSelectedUnit(unit);
                    return true;
                }

            }
        }
        return false;
    }

    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        SetSelectedAction(unit.GetAction<MoveAction>());

        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;

        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit deadUnit = sender as Unit;

        // 죽은 유닛이 선택된 유닛인지 확인
        if (selectedUnit == deadUnit)
        {
            // UnitManager에서 살아있는 아군 찾기
            List<Unit> friendlyUnits = UnitManager.Instance.GetFriendlyUnitList();

            if (friendlyUnits.Count > 0)
            {
                // 첫 번째 살아있는 아군 선택
                SetSelectedUnit(friendlyUnits[0]);
            }
            else
            {
                // 아군이 모두 죽음
                selectedUnit = null;
            }
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지
        Unit.OnAnyUnitDead -= Unit_OnAnyUnitDead;
    }

}
