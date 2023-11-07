using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraspingOptimization
{
    [System.Serializable]
    public class EnvSettingWrapper
    {
        public string _id; // MongoDBのためのユニークなID 
        public EnvSetting envSetting;

        public EnvSettingWrapper()
        {

        }

        public EnvSettingWrapper(EnvSetting envSetting)
        {
            this.envSetting = envSetting;
            // hashを計算
            string json = JsonUtility.ToJson(envSetting);
            this._id = Helper.GetHash(json);
        }

        public void LoadEnvSetting(string hash)
        {
            // MongoDBから読み込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "env-setting");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", hash);
            var result = collection.Find(filter).FirstOrDefault();
            string json = result.ToJson();
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public string ExportEnvSetting()
        {
            // MongoDBに書き込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "env-setting");
            string json = JsonUtility.ToJson(this);
            BsonDocument document = BsonDocument.Parse(json);
            collection.InsertOne(document);
            return this._id;
        }
    }

    [System.Serializable]
    public class EnvSetting
    {
        //public GameObject virtualObject;
        public string objectPath;
        public Vector3 objectPosOffset;
        public Quaternion objectRotOffset;
        public Vector3 objectScale;

        public EnvSetting()
        {

        }

        public EnvSetting(GameObject virtualObject)
        {
            this.objectPath = virtualObject.name;

            objectPosOffset = virtualObject.transform.localPosition;
            objectRotOffset = virtualObject.transform.localRotation;
            objectScale = virtualObject.transform.localScale;
        }

        public void LoadEnvSetting(string dataDir, string hash)
        {
            StreamReader sr = new StreamReader($"{dataDir}/env-setting/{hash}.json");
            string json = sr.ReadToEnd();
            JsonUtility.FromJsonOverwrite(json, this);
            sr.Close();
        }

        public string ExportOptiSetting(string dataDir)
        {
            string json = JsonUtility.ToJson(this);
            string hash = Helper.GetHash(json);
            StreamWriter sw = new StreamWriter($"{dataDir}/env-setting/{hash}.json", false);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            return hash;
        }

        public GameObject LoadObjectInstance()
        {
            // ResourcesからGameObjectをロードします。
            GameObject loadedObject = Resources.Load<GameObject>(this.objectPath);

            GameObject instance = GameObject.Instantiate(loadedObject);
            instance.transform.localPosition = objectPosOffset;
            instance.transform.localRotation = objectRotOffset;
            instance.transform.localScale = objectScale;

            return instance;
        }

        public GameObject LoadObjectPrefab()
        {
            // ResourcesからGameObjectをロードします。
            GameObject loadedObject = Resources.Load<GameObject>(this.objectPath);
            return loadedObject;
        }
    }
}

