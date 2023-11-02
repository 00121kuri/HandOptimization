using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class EnvSettingManager : MonoBehaviour
    {
        public EnvSetting envSetting;

        [SerializeField]
        private string dataDir;
        [SerializeField]
        private string settingHash;

        [SerializeField]
        GameObject virtualObject;


        public void Export()
        {
            envSetting = new EnvSetting(virtualObject);
            string hash = envSetting.ExportOptiSetting(dataDir);
            settingHash = hash;
            Debug.Log($"Exported EnvSetting: {hash}");
        }

        public void Load()
        {
            EnvSetting envSetting = new EnvSetting();
            envSetting.LoadEnvSetting(dataDir, settingHash);
            virtualObject = envSetting.virtualObject;
            virtualObject.transform.localPosition = envSetting.objectPosOffset;
            virtualObject.transform.localRotation = envSetting.objectRotOffset;
            virtualObject.transform.localScale = envSetting.objectScale;
            Debug.Log($"Loaded EnvSetting: {settingHash}");
        }
    }
}