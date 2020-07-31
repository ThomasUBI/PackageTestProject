using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public BoidSettings Settings;
    public Transform Target;
    [SerializeField]
    private Transform boidContainer;

    public Boid boidPrefab;
    public int boidCount { get; set; }

    public List<Boid> boids { get; private set; }

    public bool IsSimulating { get; private set; }

    public void Begin(Transform spawn)
    {
        if(Target == null)
        {
            Target = GameObject.FindGameObjectWithTag("BoidTarget").transform;
            if(Target == null)
            {
                Debug.LogError("No boid target in scene, please assign one or tag a transform as one");
            }
        }

        foreach (Transform item in boidContainer)
            Destroy(item.gameObject);

        boids = new List<Boid>();
        for (int i = 0; i < Settings.startingBoidCount; i++)
        {
            Boid boid = Instantiate(boidPrefab, boidContainer);
            boid.transform.position = (spawn != null ? spawn.position : Vector3.zero) + new Vector3(Random.Range(-0.5f, 0.5f),Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            boid.Init(this, Settings, Target);
            boids.Add(boid);
        }

        boidCount = boids.Count;
        IsSimulating = true;

    }

    private void Update()
    {
        if (Settings.updateMode == BoidSettings.UpdateMode.Update)
        {
            Simulate();
        }
    }
    private void FixedUpdate()
    {
        if (Settings.updateMode == BoidSettings.UpdateMode.FixedUpdate)
        {
            Simulate();
        }
    }
    private void LateUpdate()
    {
        if (Settings.updateMode == BoidSettings.UpdateMode.LateUpdate)
        {
            Simulate();
        }

    }


    private void Simulate()
    {
        if (boids == null || !IsSimulating)
            return;

        BoidData[] boidData = new BoidData[boids.Count];
        
        // update boid data and do math with it
        for (int i = 0; i < boids.Count; i++)
        {
            boidData[i].position = boids[i].position;
            boidData[i].direction = boids[i].direction;
        }

        ProcessData(ref boidData, Settings, boids.Count);

        // apply boid data to boid objects
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].avgFlockHeading = boidData[i].flockHeading;
            boids[i].centreOfFlockmates = boidData[i].flockCentre;
            boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
            boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

            boids[i].UpdateBoid();
        }
    }


    private void ProcessData(ref BoidData[] data, BoidSettings settings, int count)
    {
        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < data.Length; j++)
            {
                if(j != i)
                {
                    Vector3 offset = data[j].position - data[i].position;
                    float sqrDist = Vector3.SqrMagnitude(offset);

                    if(sqrDist < settings.perceptionRadius * settings.perceptionRadius)
                    {
                        data[i].numFlockmates++;
                        data[i].flockHeading += data[j].direction;
                        data[i].flockCentre += data[i].position;

                        if(sqrDist < settings.avoidanceRadius * settings.avoidanceRadius)
                        {
                            data[i].avoidanceHeading -= offset / sqrDist;
                        }
                    }
                }
            }
        }
    }

    public void ChangeSettings(BoidSettings settings)
    {
        this.Settings = settings;
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].SetSettings(settings);
        }
    }


    public bool AddBoid(Boid boid, bool updateBoidCount = true)
    {
        if (boid == null || boids == null)
            return false;

        boid.transform.SetParent(boidContainer);
        boid.Init(this, Settings, Target);
        boids.Add(boid);

        if (updateBoidCount)
        {
            boidCount++;


        }

        return true;
    }

    /// <summary>
    /// Remove with events, use this when boid dies for example
    /// </summary>
    /// <param name="boid"></param>
    /// <param name="updateBoidCount"></param>
    /// <returns></returns>
    public bool RemoveBoid(Boid boid, bool updateBoidCount = true)
    {
        if (!Remove(boid))
            return false;

        if (updateBoidCount)
        {
            //Debug.Log("remove 1 boid");
            boidCount--;

        }

        return true;
    }

    /// <summary>
    ///  Simple remove
    /// </summary>
    /// <param name="boid"></param>
    /// <returns></returns>
    public bool Remove(Boid boid)
    {
        if (boid == null || boids == null)
            return false;

        boids.Remove(boid);
        Destroy(boid);
        return true;
    }

    public void CompleteRemove(Boid boid)
    {
        if (Remove(boid))
            Destroy(boid.gameObject);
    }


    public void StopSimulation()
    {
        IsSimulating = false;
    }
    public void ResumeSimulation()
    {
        IsSimulating = true;
    }

    public void Show(bool value)
    {
        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].gameObject.SetActive(value);
        }
    }

    public void RemoveAll()
    {
        for (int i = 0; i < boids.Count; i++)
        {
            CompleteRemove(boids[i]);
        }
    }

}

public struct BoidData
{
    public Vector3 position;
    public Vector3 direction;

    public Vector3 flockHeading;
    public Vector3 flockCentre;
    public Vector3 avoidanceHeading;
    public int numFlockmates;
}
