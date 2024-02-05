using System.Collections;
using System.Collections.Generic;
using GraspingOptimization;
using UnityEngine;
using MongoDB.Bson;
using MongoDB.Driver;

using UnityEditor;  //AssetDatabaseを使うために追加
using System.IO;  //StreamWriterなどを使うために追加
using System.Linq;
using UnityEngineInternal;  //Selectを使うために追加

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
        string sequenceId;
        [SerializeField]
        string dateTime;

        [SerializeField]
        int frameCount = -1;
        [SerializeField]
        DataType dataType;

        [SerializeField]
        bool captureScreen = false;

        [SerializeField]
        private string captureName = null;

        GUIStyle guiStyle;

        [SerializeField]
        private GameObject handCamera;

        private Vector3 handCameraOffsetPos;

        //public HandPoseDataList handPoseDataList = new HandPoseDataList();
        private IMongoCollection<BsonDocument> collection;

        [SerializeField]
        private bool isDisplay = false;

        [SerializeField]
        private bool isViewer = false;

        private EnvSetting envSetting;

        private SettingHash settingHash;

        private string prevSequenceId;

        void Start()
        {
            // GUIの文字の設定
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 16;
            guiStyle.normal.textColor = Color.white;

            // カメラの位置と回転を保存
            handCameraOffsetPos = handCamera.transform.position;

            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.Add(hand);
            }
            //handPoseDataList = ReadHandPoseDataList(dataDir, fileName);
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

        void Update()
        {
            if (isViewer && Input.GetKey(KeyCode.Space))
            {
                if (prevSequenceId != sequenceId) // sequenceIdが変わったら更新
                {
                    frameCount = -1;

                    if (dataType == DataType.Output)
                    {
                        settingHash = OptiDB.FetchSettingHash(sequenceId);
                        envSetting = OptiDB.FetchEnvSetting(settingHash.envSettingHash);
                        dateTime = settingHash.sequenceDt;
                        Destroy(realObject);
                        realObject = envSetting.LoadObjectInstance();
                        try
                        {
                            Rigidbody rb = realObject.GetComponent<Rigidbody>();
                            rb.isKinematic = true;
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log(e);
                        }
                    }
                }

                frameCount++;
                //HandPoseData handPoseData = ReadHandPoseData(sequenceId, dateTime, frameCount);
                HandPoseData handPoseData = ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);
                if (handPoseData != null)
                {
                    if (frameCount == 0)
                    {
                        // カメラの位置と回転をシーケンスの最初のフレームに合わせる
                        handCamera.transform.position = handCameraOffsetPos + handPoseData.objectData.position;
                    }
                    Debug.Log($"Frame: {handPoseData.frameCount}");
                    SetHandPose(handPoseData);

                    if (captureScreen)
                    {
                        // ディレクトリの作成
                        string dirPath;
                        if (captureName == null)
                        {
                            dirPath = $"SequenceCapture/{dateTime}/{sequenceId}";
                        }
                        else
                        {
                            dirPath = $"SequenceCapture/{captureName}/{dateTime}/{sequenceId}";
                        }
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
                        string path = $"{dirPath}/{frameCount}.png";
                        ScreenCapture.CaptureScreenshot(path);
                    }
                }
                else
                {
                    Debug.Log("End of data");
                    frameCount = -1;
                }
                prevSequenceId = sequenceId;
            }
        }

        public HandPoseData ReadHandPoseData(string sequenceId, string dt, int frameCount)
        {
            //return handPoseDataList.data.Find(x => x.frameCount == frameCount);

            string filePath;
            if (dataType == DataType.Input)
            {
                filePath = $"{dataDir}/input/{dt}.jsonl";
            }
            else
            {
                filePath = $"{dataDir}/output/{dt}/{sequenceId}.jsonl";
            }
            string json = File.ReadLines(filePath).Skip(frameCount).FirstOrDefault();
            if (json == null)
            {
                return null;
            }
            HandPoseData handPoseData = JsonUtility.FromJson<HandPoseData>(json);
            return handPoseData;
        }

        public HandPoseData ReadHandPoseDataFromDB(string sequenceId, string dt, int frameCount)
        {
            // MongoDBからデータを取得
            //var filter = Builders<BsonDocument>.Filter.And(
            //    Builders<BsonDocument>.Filter.Eq("sequenceId", sequenceId),
            //    Builders<BsonDocument>.Filter.Eq("dateTime", dt),
            //    Builders<BsonDocument>.Filter.Eq("frameCount", frameCount)
            //);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", $"{dt}-{sequenceId}-{frameCount}");
            var document = collection.Find(filter).FirstOrDefault();
            if (document != null)
            {
                string json = document.ToJson();
                HandPoseData handPoseData = JsonUtility.FromJson<HandPoseData>(json);
                return handPoseData;
            }
            else
            {
                return null;
            }
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

        private void OnGUI()
        {
            if (isDisplay)
            {
                GUILayout.Label($"Sequence DateTime: {dateTime}\nSequence ID: {sequenceId}\nFrame: {frameCount}", guiStyle);
            }
        }
    }
}
