using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

// 物体の接触点を物体上となるように求める
namespace GraspingOptimization
{
    public class JointManager : MonoBehaviour
    {
        [SerializeField]
        JointType jointType;

        public Joint joint = null;

        public Collision currentCollision;


        void Start()
        {
            joint = new Joint(this.gameObject, jointType);
        }

        void OnCollisionStay(Collision collision)
        {
            currentCollision = collision;
        }

        void OnCollisionExit(Collision collision)
        {
            currentCollision = null;
        }

        public void showContact()
        {
            if (currentCollision == null)
            {
                return;
            }
            foreach (ContactPoint contact in currentCollision.contacts)
            {
                Debug.DrawLine(contact.point, contact.point + contact.normal * currentCollision.impulse.magnitude / currentCollision.contacts.Length, Color.red);
            }
        }
    }
}