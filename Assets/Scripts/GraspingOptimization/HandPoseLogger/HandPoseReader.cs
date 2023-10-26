using System.Collections;
using System.Collections.Generic;
using GraspingOptimization;
using UnityEngine;

using UnityEditor;  //AssetDatabaseを使うために追加
using System.IO;  //StreamWriterなどを使うために追加
using System.Linq;  //Selectを使うために追加

namespace GraspingOptimization
{
    public class HandPoseReader : MonoBehaviour
    {
        public List<Hand> hands;

        [SerializeField]
        List<GameObject> handObjectList;

        [SerializeField]
        GameObject realObject;

        [SerializeField]
        string dataDir;
        [SerializeField]
        string fileName;

        [SerializeField]
        int frameCount;

        public HandPoseDataList handPoseDataList = new HandPoseDataList();

        void Start()
        {
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.Add(hand);
            }
            handPoseDataList = ReadHandPoseDataList(dataDir, fileName);
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {

                HandPoseData handPoseData = ReadHandPoseData(frameCount);
                if (handPoseData != null)
                {
                    Debug.Log($"Frame: {handPoseData.frameCount}");
                    SetHandPose(handPoseData);
                    frameCount++;
                }
                else
                {
                    Debug.Log("End of data");
                    frameCount = 0;
                }

            }
        }

        public HandPoseData ReadHandPoseData(int frameCount)
        {
            return handPoseDataList.data.Find(x => x.frameCount == frameCount);
        }

        HandPoseDataList ReadHandPoseDataList(string dataDir, string fileName)
        {
            string filePath = $"{dataDir}/input/{fileName}.json";
            string json = File.ReadAllText(filePath);
            handPoseDataList = JsonUtility.FromJson<HandPoseDataList>(json);
            return handPoseDataList;
        }

        public void SetHandPose(HandPoseData handPoseData)
        {
            // オブジェクトの位置と回転を設定
            realObject.transform.position = handPoseData.objectData.position;
            realObject.transform.rotation = handPoseData.objectData.rotation;

            // 手のデータを設定
            foreach (HandData handData in handPoseData.handDataList)
            {
                Hand hand = hands.Find(x => x.handType == handData.handType);
                // elbow, wrist
                hand.elbowObject.transform.position = handData.elbowJoint.position;
                hand.elbowObject.transform.rotation = handData.elbowJoint.rotation;
                hand.elbowObject.transform.localScale = handData.elbowJoint.localScale;
                hand.wristObject.transform.position = handData.wristJoint.position;
                hand.wristObject.transform.rotation = handData.wristJoint.rotation;
                hand.wristObject.transform.localScale = handData.wristJoint.localScale;

                // fingers
                foreach (FingerData fingerData in handData.fingerDataList)
                {
                    Finger finger = hand.fingerList.Find(x => x.fingerType == fingerData.fingerType);
                    foreach (JointData jointData in fingerData.jointDataList)
                    {
                        Joint joint = finger.jointList.Find(x => x.jointType == jointData.jointType);
                        joint.jointObject.transform.position = jointData.position;
                        joint.jointObject.transform.rotation = jointData.rotation;
                        joint.jointObject.transform.localScale = jointData.localScale;
                    }
                }
            }
        }
    }
}
