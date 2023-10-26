using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Physics.autoSimulation = false;
        Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
    }


    void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
