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
        public bool isCollision;

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
            this.isCollision = false;
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


        public (int joint, int contact) minSeparationIndex()
        {
            float minSeparation = 0.001f;
            int minSeparationJoint = -1;
            int minSeparationContact = -1;
            for (int i = 0; i < this.jointList.Count; i++)
            {
                for (int j = 0; j < this.jointList[i].contactList.Count; j++)
                {
                    if (minSeparation > this.jointList[i].contactList[j].separation)
                    {
                        minSeparation = this.jointList[i].contactList[j].separation;
                        minSeparationJoint = i;
                        minSeparationContact = j;
                        this.isCollision = true;
                    }
                }
            }
            return (minSeparationJoint, minSeparationContact);
        }
    }

}
