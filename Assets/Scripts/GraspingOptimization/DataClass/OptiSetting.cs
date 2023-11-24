using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using Amazon.Runtime.SharedInterfaces;

namespace GraspingOptimization
{
    [System.Serializable]
    public class OptiSettingWrapper<T> where T : OptiSetting
    {
        public string _id; // MongoDBのためのユニークなID 

        public T optiSetting;

        public OptiSettingWrapper()
        {

        }

        public OptiSettingWrapper(T optiSetting)
        {
            this.optiSetting = optiSetting;
            string json = JsonUtility.ToJson(optiSetting);
            this._id = Helper.GetHash(json);
        }

        public void LoadOptiSetting(string hash)
        {
            // MongoDBから読み込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "opti-setting");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", hash);
            var result = collection.Find(filter).FirstOrDefault();
            string json = result.ToJson();
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public string ExportOptiSetting()
        {
            // MongoDBに書き込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "opti-setting");
            string json = JsonUtility.ToJson(this);
            BsonDocument document = BsonDocument.Parse(json);
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", this._id);
                collection.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            return this._id;
        }
    }

    [System.Serializable]
    public abstract class OptiSetting
    {
        public OptiSetting()
        {

        }


        public void LoadOptiSetting(string dataDir, string hash)
        {
            StreamReader sr = new StreamReader($"{dataDir}/opti-setting/{hash}.json");
            string json = sr.ReadToEnd();
            JsonUtility.FromJsonOverwrite(json, this);
            sr.Close();
        }

        public string ExportOptiSetting(string dataDir)
        {
            string json = JsonUtility.ToJson(this);
            string hash = Helper.GetHash(json);
            StreamWriter sw = new StreamWriter($"{dataDir}/opti-setting/{hash}.json", false);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
            return hash;
        }

    }

    /// <summary>
    /// それぞれの設定値がどの最適化手法に対応しているかを表す
    /// </summary>
    [System.Serializable]
    public class OptiTypeWrapper
    {
        public string _id; // OptiSettingのハッシュ値
        public OptiType optiType;

        public OptiTypeWrapper(OptiType optiType, string hash)
        {
            this._id = hash;
            this.optiType = optiType;
        }

        public OptiTypeWrapper()
        {

        }

        public void ExportOptiType()
        {
            // MongoDBに書き込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "opti-type");
            string json = JsonUtility.ToJson(this);
            BsonDocument document = BsonDocument.Parse(json);
            try
            {
                var filter = Builders<BsonDocument>.Filter.Eq("_id", this._id);
                collection.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        public void LoadOptiType(string hash)
        {
            // MongoDBから読み込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "opti-type");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", hash);
            var result = collection.Find(filter).FirstOrDefault();
            string json = result.ToJson();
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    [System.Serializable]
    public enum OptiType
    {
        LocalSearch
    }
}