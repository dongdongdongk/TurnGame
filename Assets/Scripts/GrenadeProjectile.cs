using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    public static event EventHandler OnAnyGrenadeExploded;

    [SerializeField] private Transform grenadeExplodeVfxPrefeb;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private AnimationCurve arcYAnimationCurve;
    [SerializeField] private LayerMask obstacleLayerMask; // 충돌 감지할 레이어 설정

    private Vector3 targetPostion;
    private Action onGrenadeBehaviorComplete;
    private float totalDistance;
    private Vector3 positionXZ;
    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void Update()
    {
        Vector3 moveDir = ( targetPostion - positionXZ ).normalized;

        float moveSpeed = 15f;
        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(positionXZ, targetPostion);
        float distanceNormalized = 1 - distance / totalDistance;

        float maxHeight = totalDistance / 4f;
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;
        Vector3 newPosition = new Vector3(positionXZ.x, positionY, positionXZ.z);

        // 이전 프레임 위치에서 현재 위치까지 장애물 체크
        Vector3 moveDirection = newPosition - previousPosition;
        float moveDistance = moveDirection.magnitude;

        if (Physics.SphereCast(previousPosition, 0.2f, moveDirection.normalized, 
            out RaycastHit hit, moveDistance, obstacleLayerMask))
        {
            // 장애물에 부딪혔으면 그 지점에서 폭발
            Explode(hit.point);
            return;
        }

        transform.position = newPosition;
        previousPosition = newPosition;

        float reachedTargetDistance = .2f;
        if(Vector3.Distance(positionXZ, targetPostion) < reachedTargetDistance)
        {
            Explode(targetPostion);
        }
    }

    private void Explode(Vector3 explodePosition)
    {
        float damageRadius = 4f;
        Collider[] colliderArray = Physics.OverlapSphere(explodePosition, damageRadius);

        foreach(Collider collider in colliderArray)
        {
            if (collider.TryGetComponent<Unit>(out Unit targetUnit))
            {
                targetUnit.Damage(30);
            }

            if (collider.TryGetComponent<DestructibleCrate>(out DestructibleCrate destructibleCrate))
            {
                destructibleCrate.Damage();
            }
        }
        
        OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);

        trailRenderer.transform.parent = null;

        Instantiate(grenadeExplodeVfxPrefeb, explodePosition + Vector3.up * 1f, Quaternion.identity);

        Destroy(gameObject);

        onGrenadeBehaviorComplete();
    }

    public void Setup(GridPosition targetGridPosition , Action onGrenadeBehaviorComplete)
    {
        this.onGrenadeBehaviorComplete = onGrenadeBehaviorComplete;
        targetPostion = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        positionXZ = transform.position;
        positionXZ.y = 0;

        totalDistance = Vector3.Distance(positionXZ, targetPostion);
        
        previousPosition = transform.position;
    }
}