using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVController : MonoBehaviour
{
    public float smooth = 0.125f;
    private Camera cam;
    public float baseFOV { get; private set; }

    private float desiredFOV;

    public ParticleSystem speedFX;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseFOV = cam.fieldOfView;
    }

    private void Start()
    {
        desiredFOV = baseFOV;
    }

    private void LateUpdate()
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV, smooth);
    }

    public void ChangeFOV(float fov)
    {
        desiredFOV = fov;
    }

    public void SetFOV(float fov)
    {
        cam.fieldOfView = desiredFOV = fov;
    }
}
