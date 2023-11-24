using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEngine.UI;

namespace GraspingOptimization
{
    public class OptiClientDisplay : MonoBehaviour
    {
        string sequenceDt = "";
        string sequenceId = "";
        int frameCount = -1;
        int stepCount = -1;

        private Text text;

        private void Start()
        {
            text = this.GetComponent<Text>();
            text.text = $"Sequence DateTime: {sequenceDt}\nSequence Id: {sequenceId}\nFrame Count: {frameCount}\nStep Count: {stepCount}";
        }

        public void UpdateDisplay(string sequenceDt, string sequenceId, int frameCount, int stepCount)
        {
            this.sequenceDt = sequenceDt;
            this.sequenceId = sequenceId;
            this.frameCount = frameCount;
            this.stepCount = stepCount;
            text.text = $"Sequence DateTime: {sequenceDt}\nSequence Id: {sequenceId}\nFrame Count: {frameCount}\nStep Count: {stepCount}";
        }

        public void WaitingDisplay()
        {
            text.text = $"Waiting for next sequence...";
        }
    }
}
