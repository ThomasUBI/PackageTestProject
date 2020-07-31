using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoManager : MonoBehaviour
{
    public Flock flock;
    public Transform target;

    private void Start()
    {
        flock.Begin(flock.transform);
    }
}
