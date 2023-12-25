using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using Unity.VisualScripting;
using System.Runtime.Serialization;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;


namespace GraspingOptimization
{
    public class OptiResult
    {
        public string _id; // MongoDBのためのユニークなID
        public string sequenceId;
        public string dateTime;
        public int frameCount;

        public string optiSettingHash;
        public string envSettingHash;

        public float score;
        public float distance;
        public float angleDiff;
        public float chromosomeAngleDiff;
        public float inputChromosomeAngleDiff;

        public Vector3 resultPos;
        public Quaternion resultRot;
        public Vector3 initPos;
        public Quaternion initRot;

        public OptiResult(string sequenceId, string dateTime, int frameCount, string optiSettingHash, string envSettingHash, HandChromosome minScoreChromosome, HandChromosome initChromosome, HandChromosome inputChromosome, Vector3 initPos, Quaternion initRot)
        {
            this._id = $"{dateTime}-{sequenceId}-{frameCount}";
            this.sequenceId = sequenceId;
            this.dateTime = dateTime;
            this.frameCount = frameCount;
            this.optiSettingHash = optiSettingHash;
            this.envSettingHash = envSettingHash;
            this.score = minScoreChromosome.score;
            this.resultPos = minScoreChromosome.resultPosition;
            this.resultRot = minScoreChromosome.resultRotation;
            this.initPos = initPos;
            this.initRot = initRot;
            this.distance = Vector3.Distance(resultPos, initPos);
            this.angleDiff = Quaternion.Angle(resultRot, initRot);
            this.chromosomeAngleDiff = minScoreChromosome.EvaluateTotalChromosomeAngleDiff(initChromosome);
            this.inputChromosomeAngleDiff = minScoreChromosome.EvaluateTotalChromosomeAngleDiff(inputChromosome);
        }

        public void Export(string filePath)
        {
            string json = JsonUtility.ToJson(this);
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(json);
            sw.Flush();
            sw.Close();
        }

        public void ExportDB()
        {
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "result");
            string json = JsonUtility.ToJson(this);
            BsonDocument document = BsonDocument.Parse(json);
            try
            {
                collection.InsertOne(document);
            }
            catch (MongoWriteException e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
