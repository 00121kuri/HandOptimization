#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEditor;

namespace GraspingOptimization
{
    [CustomEditor(typeof(SettingHashSender))]
    public class SettingHashSenderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add"))
            {
                SettingHashSender settingHashSender = target as SettingHashSender;
                settingHashSender.Add();
            }
            if (GUILayout.Button("Clear"))
            {
                SettingHashSender settingHashSender = target as SettingHashSender;
                settingHashSender.Clear();
            }
        }
    }
}
#endif