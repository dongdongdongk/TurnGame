using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    private CinemachineImpulseSource cinemachineImpulseSource;
    public static ScreenShake Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There is more than one UnitActionsSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float intensity = 1f)
    {
        cinemachineImpulseSource.GenerateImpulse(intensity);
    }
}
