using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    [System.Serializable]
    public class Finger
    {
        public List<Joint> jointList;

        public FingerType fingerType;

        public Finger(List<GameObject> jointObjectList, FingerType fingerType)
        {
            this.jointList = new List<Joint>();
            foreach (GameObject jointObject in jointObjectList)
            {
                if (jointObject.GetComponent<JointManager>() == null)
                {
                    continue;
                }
                this.jointList.Add(jointObject.GetComponent<JointManager>().joint);
            }
            this.fingerType = fingerType;
        }


        private void getJoints(Transform parent, List<Joint> jointList)
        {
            if (parent.childCount == 0)
            {
                return;
            }
            foreach (Transform child in parent)
            {
                if (child.childCount == 0)
                {
                    continue;
                }
                jointList.Add(child.GetComponent<JointManager>().joint);
                getJoints(child, jointList);
            }
        }
    }
}
