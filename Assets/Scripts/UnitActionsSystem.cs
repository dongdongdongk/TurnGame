using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

public class UnitActionsSystem : MonoBehaviour
{
    public event EventHandler OnSelectedUnitChanged;
    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (TryHnadleUnitSelection())
            {
                return;
            }
            selectedUnit.Move(MouseWorld.GetPosition());
        }
    }

    private bool TryHnadleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
            {
                SetSelectedUnit(unit);
                return true;
            }

        }
        return false;
    }

    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }
}
