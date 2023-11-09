using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

using UnityEditor;  //AssetDatabaseを使うために追加
using System.IO;  //StreamWriterなどを使うために追加
using System.Linq;  //Selectを使うために追加
using System;
using MongoDB.Bson;
using MongoDB.Driver;


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
        string sequenceId = "inputdata";


        int frameCount = 0;

        private IMongoCollection<BsonDocument> collection;

        // MongoDBに接続するための接続文字列
        private string connectionString = "mongodb://localhost:27017";


        // Start is called before the first frame update
        void Start()
        {
            dt = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.Add(hand);
            }

            // MongoDBクライアントの初期化
            if (dataType == DataType.Input)
            {
                collection = MongoDB.GetCollection("opti-data", "input");
            }
            else if (dataType == DataType.Output)
            {
                collection = MongoDB.GetCollection("opti-data", "output");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                HandPoseData handPoseData = GetHandPoseData(sequenceId, dt, frameCount);
                string fileName;
                if (dataType == DataType.Input)
                {
                    fileName = $"{dataDir}/input/{dt}.jsonl";
                }
                else
                {
                    fileName = $"{dataDir}/output/{dt}.jsonl";
                }
                //ExportJson(JsonUtility.ToJson(handPoseData), fileName);
                ExportDB(JsonUtility.ToJson(handPoseData));
                frameCount++;
            }
            else
            {
                dt = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                frameCount = 0;
            }
        }

        public HandPoseData GetHandPoseData(string sequenceId, string dt, int frameCount)
        {
            HandPoseData handPoseData = new HandPoseData(sequenceId, dt, frameCount);

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

        public void ExportDB(string json)
        {
            // MongoDBにデータを送信
            var document = BsonDocument.Parse(json);
            collection.InsertOne(document);
            Debug.Log("Export to MongoDB");
        }

        public void SetLogObject(GameObject setObject)
        {
            logObject = setObject;
        }
    }
}
