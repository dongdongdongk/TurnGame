#define USE_NEW_INPT_SYSTEM
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one InputManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

       playerInputAction = new PlayerInputAction();
       playerInputAction.Player.Enable();
    }
    public Vector2 GetMouseScreenPostion()
    {
        #if USE_NEW_INPT_SYSTEM
            return Mouse.current.position.ReadValue();
        #else
            return Input.mousePosition;
        #endif
    }

    public bool IsMouseButtonDownThisFrame()
    {
        #if USE_NEW_INPT_SYSTEM
            return playerInputAction.Player.Click.WasPressedThisFrame();
        #else
        return Input.GetMouseButtonDown(0);
        #endif
    }

    public Vector2 GetCameraMoveVector()
    {
        #if USE_NEW_INPT_SYSTEM
        return playerInputAction.Player.CameraMovement.ReadValue<Vector2>();
        #else
        Vector2 inputMoveDir = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            inputMoveDir.y = +1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMoveDir.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputMoveDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMoveDir.x = +1;
        }

        return inputMoveDir;
        #endif
    }

    public float GetCameraRotateAmount()
    {
        #if USE_NEW_INPT_SYSTEM
        return playerInputAction.Player.CameraRotate.ReadValue<float>();       
        #else
        float rotateAmount = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotateAmount = +1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotateAmount = -1f;
        }

        return rotateAmount;
        #endif
    }

    public float GetCameraZoomAmount()
    {
        #if USE_NEW_INPT_SYSTEM
        return playerInputAction.Player.CameraZoom.ReadValue<float>();
        #else
        float zoomAmount = 0f;

        if (Input.mouseScrollDelta.y > 0)
        {
            zoomAmount = -1f;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            zoomAmount = +1f;
        }

        return zoomAmount;
        #endif
    }
    
}
