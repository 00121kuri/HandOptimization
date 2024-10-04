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
        public GameObject jointObject;
        public JointManager jointManager;
        public FollowTarget followTarget;
        public Rigidbody rb;
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
            this.jointType = jointType;
        }

        public bool isCollision()
        {
            return this.jointManager.currentCollision != null && this.jointManager.currentCollision.contacts.Length > 0;
        }

        public Vector3 CalculateAverageContactPoint()
        {
            Collision collision = this.jointManager.currentCollision;
            if (collision == null)
            {
                Debug.Log("collision is null");
                return Vector3.zero;
            }
            Vector3 averageContactPoint = Vector3.zero;
            foreach (ContactPoint contactPoint in collision.contacts)
            {
                averageContactPoint += contactPoint.point;
            }
            averageContactPoint /= collision.contacts.Length;
            return averageContactPoint;
        }

        public Vector3 CalculateAverageContactNormal()
        {
            Collision collision = this.jointManager.currentCollision;
            if (collision == null)
            {
                Debug.Log("collision is null");
                return Vector3.zero;
            }
            Vector3 averageContactNormal = Vector3.zero;
            foreach (ContactPoint contactPoint in collision.contacts)
            {
                averageContactNormal += contactPoint.normal;
            }
            averageContactNormal /= collision.contacts.Length;
            return averageContactNormal;
        }

        public float GetImpulseMagnitude()
        {
            Collision collision = this.jointManager.currentCollision;
            if (collision == null)
            {
                Debug.Log("collision is null");
                return 0;
            }
            return collision.impulse.magnitude;
        }
    }
}
