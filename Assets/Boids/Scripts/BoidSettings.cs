using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BoidSettings : ScriptableObject
{

    [Header("At start")]
    public int startingBoidCount = 5;

    // Settings
    public enum UpdateMode { Update, FixedUpdate, LateUpdate }
    [Header("Update")]
    public UpdateMode updateMode;


    [Header("Main")]
    public float minSpeed = 2;
    public float maxSpeed = 5;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3;
    public Vector2 SqrDistanceRange = new Vector2(1, 200);

    [Header("Weights")]
    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    [Header("Target")]
    public float targetRadius;
    public float targetWeight = 1;

    [Header("Collisions")]
    public bool useObstacles;
    public LayerMask obstacleMask;
    public float boundsRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;


}
