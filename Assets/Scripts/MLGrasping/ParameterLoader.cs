using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Parameters
{
    public int stepsPerOneFrame;
    public int framesPerEpisode;
    public List<string> dateTimeList;
}

public class ParameterLoader : MonoBehaviour
{
    public static Parameters LoadedParameters { get; private set; }

    [SerializeField]
    private string jsonFileName = "parameters.json";

    private void Awake()
    {
        LoadParameters();
    }

    private void LoadParameters()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            LoadedParameters = JsonUtility.FromJson<Parameters>(jsonContent);
            Debug.Log($"Parameters loaded: stepsPerOneFrame={LoadedParameters.stepsPerOneFrame}, framesPerEpisode={LoadedParameters.framesPerEpisode}");
            Debug.Log($"Loaded {LoadedParameters.dateTimeList.Count} date/time entries");
        }
        else
        {
            Debug.LogError($"JSON file not found at {filePath}");
            LoadedParameters = new Parameters
            {
                stepsPerOneFrame = 1,
                framesPerEpisode = 50,
                dateTimeList = new List<string>()
            };
        }
    }
}