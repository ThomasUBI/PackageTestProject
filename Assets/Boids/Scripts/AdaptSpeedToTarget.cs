using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Flock))]
public class AdaptSpeedToTarget : MonoBehaviour
{
    public float TargetSpeed;
    public float additionalSpeed = 5;

    private Flock _flock;
    private float _baseMaxSpeed;

    private void Awake()
    {
        _flock = GetComponent<Flock>();

        _baseMaxSpeed = _flock.Settings.maxSpeed;
    }

    private void OnDisable()
    {
        _flock.Settings.maxSpeed = _baseMaxSpeed;
    }

    private void Update()
    {
        _flock.Settings.maxSpeed = TargetSpeed + additionalSpeed;
    }

}
