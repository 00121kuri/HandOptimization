using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    [System.Serializable]
    public class UpdateInfo
    {
        public string appDirectory;
        public string appVersion;

        public void ExportJson(string path)
        {
            string json = JsonUtility.ToJson(this);
            System.IO.File.WriteAllText(path, json);
        }
    }
}