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

        [SerializeField]
        GameObject logObject;

        [SerializeField]
        string dataDir;
        [SerializeField]
        DataType dataType;
        string dt;

        //HandPoseDataList handPoseDataList = new HandPoseDataList();

        int frameCount = 0;


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
                HandPoseData handPoseData = GetHandPoseData(dt);
                string fileName;
                if (dataType == DataType.Input)
                {
                    fileName = $"{dataDir}/input/{dt}.jsonl";
                }
                else
                {
                    fileName = $"{dataDir}/output/{dt}.jsonl";
                }
                ExportJson(JsonUtility.ToJson(handPoseData), fileName);
                frameCount++;
            }
            else
            {
                dt = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            }
        }

        public HandPoseData GetHandPoseData(string dt)
        {
            HandPoseData handPoseData = new HandPoseData(dt);
            handPoseData.frameCount = frameCount;

            // オブジェクトの位置・回転をログに記録
            ObjectData objectData = new ObjectData(
                logObject.transform.position,
                logObject.transform.rotation
            );
            handPoseData.objectData = objectData;


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
            //handPoseDataList.data.Add(handPoseData);
            return handPoseData;
        }

        public void ExportJson(string json, string filePath)
        {
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            //Debug.Log($"Exported hand pose data to {filePath}");
        }
    }
}
