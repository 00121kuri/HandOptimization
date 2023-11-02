using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System.IO;

namespace GraspingOptimization
{
    [System.Serializable]
    public class EnvSetting
    {
        public GameObject virtualObject;
        public Vector3 objectPosOffset;
        public Quaternion objectRotOffset;
        public Vector3 objectScale;

        public EnvSetting()
        {

        }

        public EnvSetting(GameObject virtualObject)
        {
            this.virtualObject = virtualObject;
            objectPosOffset = virtualObject.transform.localPosition;
            objectRotOffset = virtualObject.transform.localRotation;
            objectScale = virtualObject.transform.localScale;
        }

        public void LoadEnvSetting(string dataDir, string hash)
        {
            StreamReader sr = new StreamReader($"{dataDir}/env-setting/{hash}.json");
            string json = sr.ReadToEnd();
            JsonUtility.FromJsonOverwrite(json, this);
            sr.Close();
        }

        public string ExportOptiSetting(string dataDir)
        {
            string json = JsonUtility.ToJson(this);
            string hash = Helper.GetHash(json);
            StreamWriter sw = new StreamWriter($"{dataDir}/env-setting/{hash}.json", false);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            return hash;
        }
    }
}

