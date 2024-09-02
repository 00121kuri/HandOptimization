using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEngine.UI;

namespace GraspingOptimization
{
    public class OptiClientDisplay : MonoBehaviour
    {
        List<string> displayLines = new List<string>();

        private Text text;

        private void Start()
        {
            text = this.GetComponent<Text>();
            if (text == null)
            {
                Debug.LogError("Text component not found on this GameObject");
                return;
            }
            text.text = $"No data to display.";
        }

        public void UpdateDisplay(string sequenceDt = null, string sequenceId = null, int? frameCount = null, int? stepCount = null)
        {
            displayLines.Clear();

            if (sequenceDt != null)
                displayLines.Add($"Sequence DateTime: {sequenceDt}");

            if (sequenceId != null)
                displayLines.Add($"Sequence Id: {sequenceId}");

            if (frameCount != null)
                displayLines.Add($"Frame Count: {frameCount}");

            if (stepCount != null)
                displayLines.Add($"Step Count: {stepCount}");

            if (displayLines.Count == 0)
                text.text = "No data to display.";
            else
                text.text = string.Join("\n", displayLines);
        }

        public void WaitingDisplay()
        {
            text.text = $"Waiting for next sequence...";
        }
    }
}
