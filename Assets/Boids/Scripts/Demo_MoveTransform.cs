using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_MoveTransform : MonoBehaviour
{
    public AdaptSpeedToTarget adapter;
    public Vector3 dir;
    public float speed = 10;
    public float acceleration = 0;

    public bool useSideMove;
    public Vector3 sideMoveDir = Vector3.up;
    public float period = 2;
    public float amplitude = 1;

    private void Update()
    {
        adapter.TargetSpeed = speed;
        speed += acceleration * Time.deltaTime;

        transform.position += dir * speed * Time.deltaTime;
        if (useSideMove)
        {
            transform.position += sideMoveDir * Mathf.Sin(6.28f/period * Time.time) * amplitude * Time.deltaTime;
        }


        
    }
}
