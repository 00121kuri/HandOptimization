using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraspingOptimization
{
    [CustomEditor(typeof(EnvSettingManager))]
    public class EnvSettingManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Export EnvSetting"))
            {
                EnvSettingManager envSettingManager = target as EnvSettingManager;
                envSettingManager.Export();
            }
            if (GUILayout.Button("Load EnvSetting"))
            {
                EnvSettingManager envSettingManager = target as EnvSettingManager;
                envSettingManager.Load();
            }
        }
    }
}
