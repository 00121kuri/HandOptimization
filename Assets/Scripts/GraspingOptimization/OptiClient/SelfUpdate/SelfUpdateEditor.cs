#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GraspingOptimization;

namespace GraspingOptimization
{
    [CustomEditor(typeof(SelfUpdate))]
    public class SelfUpdateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Export UpdateInfo"))
            {
                SelfUpdate selfUpdate = target as SelfUpdate;
                selfUpdate.OnClickExportUpdateInfo();
            }
        }
    }
}
#endif