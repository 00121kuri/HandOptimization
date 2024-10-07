using System.Collections.Generic;
using UnityEngine;
using static TransformHelper;
using GraspingOptimization;
using static GraspingOptimization.Helper;

// 物体の接触点の調節後の接触点の情報から指の関節の回転を求める
namespace GraspingOptimization
{
    public class FingerManager : MonoBehaviour
    {
        [SerializeField] public Finger finger = null; // Fingerクラス

        [SerializeField]
        private List<GameObject> jointObjectList;
        [SerializeField]
        private FingerType fingerType;


        void Start()
        {
            finger = new Finger(jointObjectList, fingerType);
            Debug.Log("jointList.Count: " + finger.jointList.Count);

            foreach (Joint joint in finger.jointList)
            {
                Debug.Log(joint.jointObject.name);
            }
        }
    }
}
