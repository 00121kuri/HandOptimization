using UnityEngine;
using System.IO;

[System.Serializable]
public class Parameters
{
    public int stepsPerOneFrame;
    public int framesPerEpisode;
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
        }
        else
        {
            Debug.LogError($"JSON file not found at {filePath}");
            LoadedParameters = new Parameters { stepsPerOneFrame = 1, framesPerEpisode = 50 };
        }
    }
}