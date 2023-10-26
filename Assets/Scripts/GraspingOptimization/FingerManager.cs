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
        // 第一関節から順番に
        //public List<Transform> jointList = null; // 指の関節のリスト
        [SerializeField] public Finger finger = null; // Fingerクラス

        /*
        public List<AdjustContactPoint> adjustContactPointList = null; // AdjustContactPointスクリプトの参照
        public List<FollowTarget> followTargetList = null; // FollowTargetスクリプトの参照
        public List<Quaternion> jointRotationList; // 指の関節の回転のリスト
        public List<Vector3> jointPositionList; // 指の関節の位置のリスト
        public List<GameObject> collObjectList; // 指のコライダーのリスト
        public List<Rigidbody> rbList; // 指の関節のRigidbodyのリスト
        */

        Vector3 jointMaxAngle; // 指の関節の最大回転


        //public AdjustContactPoint adjustContactPoint; // AdjustContactPointスクリプトの参照
        //public int totalIterations = 2; // 総計算回数

        private List<GameObject> contactSphereList = new List<GameObject>();
        private List<GameObject> targetSphereList = new List<GameObject>();

        //[SerializeField]
        //private GameObject contactSphere;
        //[SerializeField]
        //private GameObject targetSphere;
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

        public int getMinSeparationIndex(List<Contact> contactList)
        {
            float minSeparation = 0.001f;
            int minSeparationIndex = 0;
            for (int i = 0; i < contactList.Count; i++)
            {
                if (contactList[i].separation < minSeparation)
                {
                    minSeparation = contactList[i].separation;
                    minSeparationIndex = i;
                }
            }
            return minSeparationIndex;
        }




        // CCD-IK
        private void RotateTowardsTarget(Transform joint, Vector3 endEffectorPosition, Vector3 targetPosition, Vector3 jointMaxAngle)
        {
            // jointと接触点のベクトル
            Vector3 jointToContact = (endEffectorPosition - joint.position).normalized;
            // jointと新しい接触点のベクトル
            Vector3 jointToNewContact = (targetPosition - joint.position).normalized;

            // jointと接触点のベクトルとjointと新しい接触点のベクトルのなす角
            float angle = Vector3.Angle(jointToContact, jointToNewContact);
            //Debug.Log("angle: " + angle);
            // jointをjointToContactとjointToNewContactの外積を軸にしてangle度回転させる
            Quaternion jointRotation = Quaternion.AngleAxis(angle, Vector3.Cross(jointToContact, jointToNewContact)) * joint.rotation;

            Vector3 jointAngle = joint.transform.InverseTransformEulerAngles(jointRotation.eulerAngles);

            //Debug.Log("jointAngle no limit: " + jointAngle.ToString("F6"));
            // 角度制限
            jointAngle = new Vector3(
                ClampAngle(jointAngle.x, -jointMaxAngle.x, jointMaxAngle.x),
                ClampAngle(jointAngle.y, -jointMaxAngle.y, jointMaxAngle.x),
                ClampAngle(jointAngle.z, -jointMaxAngle.z, 40f)
            );
            //Debug.Log("jointAngle limited: " + jointAngle.ToString("F6"));
            joint.localRotation = Quaternion.Euler(jointAngle);
        }



        private List<GameObject> showFingerContact(Finger finger, List<GameObject> sphereList, GameObject spherePrefab)
        {
            DestroyObjList(sphereList);
            foreach (Joint joint in finger.jointList)
            {
                foreach (Contact contact in joint.contactList)
                {
                    GameObject sphere = GameObject.Instantiate(spherePrefab);
                    sphere.transform.position = contact.position;
                    sphereList.Add(sphere);
                }
            }
            return sphereList;
        }

        private List<GameObject> showFingerTarget(Finger finger, List<GameObject> sphereList, GameObject spherePrefab)
        {
            DestroyObjList(sphereList);
            foreach (Joint joint in finger.jointList)
            {
                foreach (Contact contact in joint.contactList)
                {
                    GameObject sphere = GameObject.Instantiate(spherePrefab);
                    sphere.transform.position = contact.targetPosition();
                    sphereList.Add(sphere);
                }
            }
            return sphereList;
        }

        private void DestroyObjList(List<GameObject> objList)
        {
            foreach (GameObject obj in objList)
            {
                Destroy(obj);
            }
            objList.Clear();
        }
    }
}
