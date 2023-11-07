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
        //public GameObject virtualObject;
        public string objectPath;
        public Vector3 objectPosOffset;
        public Quaternion objectRotOffset;
        public Vector3 objectScale;

        public EnvSetting()
        {

        }

        public EnvSetting(GameObject virtualObject)
        {
            this.objectPath = virtualObject.name;

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

        public GameObject LoadObjectInstance()
        {
            // ResourcesからGameObjectをロードします。
            GameObject loadedObject = Resources.Load<GameObject>(this.objectPath);

            GameObject instance = GameObject.Instantiate(loadedObject);
            instance.transform.localPosition = objectPosOffset;
            instance.transform.localRotation = objectRotOffset;
            instance.transform.localScale = objectScale;

            return instance;
        }

        public GameObject LoadObjectPrefab()
        {
            // ResourcesからGameObjectをロードします。
            GameObject loadedObject = Resources.Load<GameObject>(this.objectPath);
            return loadedObject;
        }
    }
}

