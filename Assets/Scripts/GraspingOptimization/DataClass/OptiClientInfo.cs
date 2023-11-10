using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GraspingOptimization;

namespace GraspingOptimization
{
    [System.Serializable]
    public class OptiClientInfo
    {
        public ClientState clientState;
        public float stepsPerSecond;

        public OptiClientInfo(ClientState clientState, float stepsPerSecond)
        {
            this.clientState = clientState;
            this.stepsPerSecond = stepsPerSecond;
        }

        public OptiClientInfo(ClientState clientState)
        {
            this.clientState = clientState;
            this.stepsPerSecond = 0;
        }
    }

    [System.Serializable]
    public enum ClientState
    {
        Connected,
        Disconnected,
        Waiting,
        Running,
        Error
    }
}