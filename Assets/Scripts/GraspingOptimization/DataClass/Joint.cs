using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    /// <summary>
    /// Joint class
    /// 関節の情報を格納するクラス
    /// </summary>
    [System.Serializable]
    public class Joint
    {
        public List<Contact> contactList;
        public GameObject jointObject;
        public JointManager jointManager;
        public FollowTarget followTarget;
        public Rigidbody rb;
        //public Vector3 minRotation;
        //public Vector3 maxRotation;
        public JointType jointType;

        public Joint(GameObject jointObject, JointType jointType)
        {
            this.jointObject = jointObject;
            this.jointManager = jointObject.GetComponent<JointManager>();
            if (jointObject.GetComponent<FollowTarget>() != null)
            {
                this.followTarget = jointObject.GetComponent<FollowTarget>();
            }
            if (jointObject.GetComponent<Rigidbody>() != null)
            {
                this.rb = jointObject.GetComponent<Rigidbody>();
            }
            this.contactList = this.jointManager.contactList;
            //this.minRotation = minRotation;
            //this.maxRotation = maxRotation;
            this.jointType = jointType;
        }
    }
}
