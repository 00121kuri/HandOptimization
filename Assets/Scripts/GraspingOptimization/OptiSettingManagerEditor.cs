#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GraspingOptimization
{
    [CustomEditor(typeof(OptiSettingManager))]
    public class OptiSettingManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Export OptiSetting"))
            {
                OptiSettingManager optiSettingManager = target as OptiSettingManager;
                optiSettingManager.Export();
            }
            if (GUILayout.Button("Load OptiSetting"))
            {
                OptiSettingManager optiSettingManager = target as OptiSettingManager;
                optiSettingManager.Load();
            }
        }
    }
}
#endif