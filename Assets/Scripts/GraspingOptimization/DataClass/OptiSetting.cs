using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GraspingOptimization
{
    [System.Serializable]
    public class OptiSettingWrapper
    {
        public string _id; // MongoDBのためのユニークなID 
        public OptiSetting optiSetting;

        public OptiSettingWrapper()
        {

        }

        public OptiSettingWrapper(OptiSetting optiSetting)
        {
            this.optiSetting = optiSetting;
            // hashを計算
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
            collection.InsertOne(document);
            return this._id;
        }
    }

    [System.Serializable]
    public class OptiSetting
    {
        /// <summary>
        /// 変異確率
        /// 近傍探索の際にそれぞれの指が変化する確率
        /// 1にすると全ての指にノイズが加えられる
        /// </summary>
        public float mutationRate;

        /// <summary>
        /// 近傍探索で用いるノイズの正規分布のSigma
        /// </summary>
        public float sigma;

        /// <summary>
        /// アニーリング法の初期温度
        /// 0にすると単純な局所探索法になる
        /// </summary>
        public float initTemperature;

        /// <summary>
        /// アニーリング法の冷却係数
        /// </summary>
        public float cooling;

        /// <summary>
        /// 終了条件のスコア
        /// </summary>
        public float targetScore;

        /// <summary>
        /// 仮想オブジェクトの位置をリセットするときの閾値
        /// 0にすると毎回リセットされる
        /// </summary>
        public float worstScore;

        /// <summary>
        /// 最大ステップ数
        /// </summary>
        public int maxSteps;


        public OptiSetting()
        {

        }

        public OptiSetting(float mutationRate, float sigma, float initTemperature, float cooling, float targetScore, float worstScore, int maxSteps = 1000)
        {
            this.mutationRate = mutationRate;
            this.sigma = sigma;
            this.initTemperature = initTemperature;
            this.cooling = cooling;
            this.targetScore = targetScore;
            this.worstScore = worstScore;
            this.maxSteps = maxSteps;
            Debug.Log(JsonUtility.ToJson(this));
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

}