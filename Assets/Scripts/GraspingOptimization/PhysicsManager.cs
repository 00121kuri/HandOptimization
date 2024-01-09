using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Physics.simulationMode = SimulationMode.Script;
        Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.simulationMode.ToString());
    }


    void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
