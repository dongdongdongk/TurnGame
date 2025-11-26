using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float MIN_FOLLOW_OFFSET_Y = 2f;
    private const float MAX_FOLLOW_OFFSET_Y = 12f;
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;
    private bool isZooming = false;

    private void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 inputMoveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            inputMoveDir.z = +1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMoveDir.z = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputMoveDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMoveDir.x = +1;
        }

        float moveSpeed = 10f;

        Vector3 moveVector = transform.forward * inputMoveDir.z + transform.right * inputMoveDir.x;

        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Vector3 rotationVector = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.Q))
        {
            rotationVector.y = -1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotationVector.y = +1;
        }

        // 입력이 있을 때만 회전
        if (rotationVector != Vector3.zero)
        {
            float rotationSpeed = 100f;
            transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;
        }
    }

    private void HandleZoom()
    {
        // Debug.Log(Input.mouseScrollDelta);

        float zoomAmount = 1f;
        if (Input.mouseScrollDelta.y > 0)
        {
            targetFollowOffset.y -= zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFollowOffset.y += zoomAmount;
        }
        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_OFFSET_Y, MAX_FOLLOW_OFFSET_Y);

        float zoomSpeed = 5f;
        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(
            cinemachineTransposer.m_FollowOffset,
            targetFollowOffset,
            Time.deltaTime * zoomSpeed
        );

        //  목표에 거의 도달하면 정확히 맞춤
        float threshold = 0.01f;
        if (Vector3.Distance(cinemachineTransposer.m_FollowOffset, targetFollowOffset) < threshold)
        {
            cinemachineTransposer.m_FollowOffset = targetFollowOffset;
        }
    }
}
