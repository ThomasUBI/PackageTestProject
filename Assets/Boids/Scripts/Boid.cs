using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private BoidSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 direction;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;


    Transform cachedTransform;
    Transform target;

    private Vector3[] _obstacleRays;
    private Vector3[] ObstacleRays
    {
        get
        {
            if (_obstacleRays == null)
            {
                float fov = 210f;
                _obstacleRays = new Vector3[11];
                int step = 0;
                for (int i = 0; i < _obstacleRays.Length; i++)
                {
                    if ((i % 2) == 1)
                        step++;

                    float angle = (i % 2 == 0 ? 1 : -1) * fov / (_obstacleRays.Length - 1) * step;
                    Vector2 dir = Mathf.Cos(angle * Mathf.Deg2Rad) * Vector2.right + Mathf.Sin(angle * Mathf.Deg2Rad) * Vector2.up;
                    _obstacleRays[i] = dir;
                }                
            }
            return _obstacleRays;
        }
    }

    private Flock flock;

    void Awake()
    {
        cachedTransform = transform;
    }
    public void Remove()
    {
        if(flock)
            flock.RemoveBoid(this);
    }

    public void SetSettings(BoidSettings settings)
    {
        this.settings = settings;
    }
    public void SetSettings(BoidSettings settings, bool force)
    {
        if (force)
            this.settings = settings;
        else SetSettings(settings);
    }

    public void Init(Flock flock, BoidSettings settings, Transform target)
    {
        this.settings = settings;
        this.target = target;
        this.flock = flock;

        position = cachedTransform.position;
        direction = cachedTransform.up;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.up * startSpeed;
    }

    public void UpdateBoid()
    {
        if(cachedTransform == null)
        {
            Remove();
            return;
        }

        Vector3 acceleration = Vector3.zero;

        Vector3 offsetToTarget = Vector3.zero;
        if (target != null)
        {
            offsetToTarget = target.position - position;
            if (offsetToTarget.sqrMagnitude < settings.targetRadius * settings.targetRadius)
                offsetToTarget = Vector3.zero;

            //Debug.Log("magnitude" + offsetToTarget.sqrMagnitude);
            float lerp = Mathf.InverseLerp(settings.SqrDistanceRange.x, settings.SqrDistanceRange.y, offsetToTarget.sqrMagnitude);
            //Debug.Log("lerp" + lerp);
            Vector3 toTarget = offsetToTarget.normalized;


            acceleration += SteerTowards(toTarget) * Mathf.Lerp(3, settings.targetWeight, lerp);
        }
        

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards(avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards(avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (IsHeadingForCollision() && settings.useObstacles)
        {
            Vector3 collisionAvoidDir = GetObstacleDirection();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, settings.minSpeed, settings.maxSpeed);
        //Debug.Log("speed" + speed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        position = cachedTransform.position;
        direction = dir;
    }

    private Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, settings.maxSteerForce);
    }

    public void ChangeTarget(Transform target)
    {
        this.target = target;
    }

   

    #region RAY BASED COLLISION

    bool IsHeadingForCollision()
    {
        RaycastHit2D hit = Physics2D.CircleCast(position, settings.boundsRadius, direction, settings.collisionAvoidDst, settings.obstacleMask);
        if (hit.collider)
        {
            return true;
        }
        else { }
        return false;
    }


    Vector3 GetObstacleDirection()
    {
        Ray2D[] rays = new Ray2D[ObstacleRays.Length];
        for (int i = 0; i < rays.Length; i++)
        {
            rays[i] = new Ray2D(cachedTransform.position, cachedTransform.TransformVector(ObstacleRays[i]));
        }

        for (int i = 0; i < rays.Length; i++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(rays[i].origin, settings.boundsRadius, rays[i].direction, settings.collisionAvoidDst, settings.obstacleMask);
            if (!hit.collider)
            {
                //Debug.DrawRay(rays[i].origin, rays[i].direction * settings.collisionAvoidDst, Color.green, 1f);
                return rays[i].direction;
            }
            //Debug.DrawRay(rays[i].origin, rays[i].direction * settings.collisionAvoidDst, Color.red, 1f);
        }

        return direction;
    }

    #endregion
}
