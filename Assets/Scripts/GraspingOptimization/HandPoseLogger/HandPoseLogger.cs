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

        string filePath;

        // Start is called before the first frame update
        void Start()
        {
            filePath = $"hand-pose-log/{System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.json";
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.Add(hand);
            }
        }

        // Update is called once per frame
        void Update()
        {
            HandPoseData handPoseData = new HandPoseData();
            handPoseData.frameCount = Time.frameCount;

            foreach (Hand hand in hands)
            {
                HandData handData = new HandData(hand.handType);

                // 関節の位置・回転をログに記録
                foreach (Finger finger in hand.fingerList)
                {
                    FingerData fingerData = new FingerData(finger.fingerType);
                    foreach (Joint joint in finger.jointList)
                    {
                        JointData jointData = new JointData(
                            joint.jointObject.transform.localPosition,
                            joint.jointObject.transform.localRotation,
                            joint.jointType
                        );

                        fingerData.jointDataList.Add(jointData);
                    }
                    handData.fingerDataList.Add(fingerData);
                }
                handPoseData.handDataList.Add(handData);
            }

            string json = JsonUtility.ToJson(handPoseData);
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
        }
    }
}
