using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

// 物体の接触点を物体上となるように求める
namespace GraspingOptimization
{
    public class JointManager : MonoBehaviour
    {
        /*
        public bool isCollision = false;
        public Transform oldContactPoint = null; // 接触点
        public Transform newContactPoint = null; // 物体上の接触点
        public float minSeparation = 0.001f; // 最もseparationが小さい接触点のseparation
        */

        public List<Contact> contactList = new();
        //[SerializeField]
        //Vector3 minRotation;
        //[SerializeField]
        //Vector3 maxRotation;

        [SerializeField]
        JointType jointType;

        public Joint joint = null;
        //public GameObject HandManagerObject;




        void Start()
        {
            joint = new Joint(this.gameObject, jointType);
            /*
            oldContactPoint = new GameObject("oldContactPoint").transform;
            oldContactPoint.SetParent(this.transform);
            newContactPoint = new GameObject("newContactPoint").transform;
            newContactPoint.SetParent(this.transform);
            */
        }

        void OnCollisionStay(Collision collision)
        {
            // 初期化
            /*
            minSeparation = 0.001f;
            oldContactPoint.position = Vector3.zero;
            newContactPoint.position = Vector3.zero;
            isCollision = true;
            */

            /* 現在は不要のため削除
            DeleteContancts(joint);
            //Debug.Log("OnCollisionStay");
            float forcePerContact = collision.impulse.magnitude / collision.contactCount;

            foreach (ContactPoint contactPoint in collision.contacts)
            {
                Contact contact = new Contact(
                    new GameObject("contactPointObject"),
                    contactPoint.point,
                    contactPoint.normal,
                    forcePerContact,
                    contactPoint.separation
                );
                contact.contactPointObject.transform.SetParent(this.transform);
                contact.contactPointObject.transform.position = contactPoint.point;
                joint.contactList.Add(contact);
            }
            */
        }

        void OnCollisionExit(Collision collision)
        {
            //DeleteContancts(joint);
        }

        private void DeleteContancts(Joint joint)
        {
            // 接触点を削除
            foreach (Contact contact in joint.contactList)
            {
                Destroy(contact.contactPointObject);
            }
            joint.contactList.Clear();
        }
    }
}