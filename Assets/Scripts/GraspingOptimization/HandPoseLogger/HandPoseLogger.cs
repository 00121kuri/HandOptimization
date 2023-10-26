using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;  //AssetDatabaseを使うために追加
using System.IO;  //StreamWriterなどを使うために追加
using System.Linq;  //Selectを使うために追加


namespace GraspingOptimization
{
    public class HandPoseLogger : MonoBehaviour
    {
        public List<Hand> hands;

        [SerializeField]
        List<GameObject> handObjectList;

        string dt;

        HandPoseDataList handPoseDataList = new HandPoseDataList();

        int frameCount = 0;
        bool isExported = true;


        // Start is called before the first frame update
        void Start()
        {
            dt = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.Add(hand);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                GetHandPoseData(dt);
                isExported = false;
                frameCount++;
            }
            else
            {
                if (!isExported)
                {
                    string json = JsonUtility.ToJson(handPoseDataList);
                    string filePath = $"hand-pose-log/{dt}.json";
                    ExportJson(json, filePath);
                    isExported = true;
                    handObjectList.Clear();
                    frameCount = 0;
                }
                dt = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            }
        }

        void GetHandPoseData(string dt)
        {
            string filePath = $"hand-pose-log/{dt}.json";
            HandPoseData handPoseData = new HandPoseData(dt);
            handPoseData.frameCount = frameCount;


            foreach (Hand hand in hands)
            {
                HandData handData = new HandData(hand.handType);

                // wrist, elbow
                JointData wristJoint = new JointData(
                    hand.wristObject.transform.position,
                    hand.wristObject.transform.rotation,
                    hand.wristObject.transform.localScale,
                    JointType.Meta
                );
                JointData elbowJoint = new JointData(
                    hand.elbowObject.transform.position,
                    hand.elbowObject.transform.rotation,
                    hand.elbowObject.transform.localScale,
                    JointType.Meta
                );

                handData.wristJoint = wristJoint;
                handData.elbowJoint = elbowJoint;

                // 関節の位置・回転をログに記録
                foreach (Finger finger in hand.fingerList)
                {
                    FingerData fingerData = new FingerData(finger.fingerType);
                    foreach (Joint joint in finger.jointList)
                    {
                        JointData jointData = new JointData(
                            joint.jointObject.transform.position,
                            joint.jointObject.transform.rotation,
                            joint.jointObject.transform.localScale,
                            joint.jointType
                        );

                        fingerData.jointDataList.Add(jointData);
                    }
                    handData.fingerDataList.Add(fingerData);
                }
                handPoseData.handDataList.Add(handData);
            }

            handPoseDataList.data.Add(handPoseData);
        }

        void ExportJson(string json, string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            Debug.Log($"Exported hand pose data to {filePath}");
        }
    }
}
