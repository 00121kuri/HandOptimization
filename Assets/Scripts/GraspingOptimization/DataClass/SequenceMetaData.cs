using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using MongoDB.Driver;
using MongoDB.Bson;
using System;

namespace GraspingOptimization
{
    [Serializable]
    public class SequenceMetadata
    {
        public string _id;
        public string sequenceId;
        public string dateTime;
        public int totalFrameCount;


        public void Load(string sequenceId, string dateTime)
        {
            // MongoDBから読み込み
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "input-metadata");
            string id = $"{sequenceId}-{dateTime}";
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            try
            {
                var result = collection.Find(filter).FirstOrDefault();
                string json = result.ToJson();
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}
