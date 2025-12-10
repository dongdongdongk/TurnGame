using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    private Vector3 targetPostion;
    private Action onGrenadeBehaviorComplete;

    private void Update()
    {
        Vector3 moveDir = ( targetPostion - transform.position ).normalized;

        float moveSpeed = 15f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        float reachedTargetDistance = .2f;
        if(Vector3.Distance(transform.position, targetPostion) < reachedTargetDistance)
        {
            float damageRadius = 4f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPostion, damageRadius);

            foreach(Collider collider in colliderArray)
            {
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(30);
                }
            }
            Destroy(gameObject);

            onGrenadeBehaviorComplete();
        }
    }

    public void Setup(GridPosition targetGridPosition , Action onGrenadeBehaviorComplete)
    {
        this.onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        targetPostion = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
    }
}
