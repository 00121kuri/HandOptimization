using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using GraspingOptimization;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GraspingOptimization
{
    public static class OptiDB
    {
        public static SettingHash FetchSettingHash(string sequenceId)
        {
            IMongoCollection<BsonDocument> collection = MongoDB.GetCollection("opti-data", "result");
            var filter = Builders<BsonDocument>.Filter.Eq("sequenceId", sequenceId);
            try
            {
                var document = collection.Find(filter).FirstOrDefault();
                if (document != null)
                {
                    string json = document.ToJson();
                    OptiResult optiResult = JsonUtility.FromJson<OptiResult>(json);
                    SettingHash settingHash = new SettingHash(
                        optiResult.optiSettingHash,
                        optiResult.envSettingHash,
                        optiResult.dateTime
                    );
                    return settingHash;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
            return null;
        }

        public static EnvSetting FetchEnvSetting(string envSettingHash)
        {
            EnvSettingWrapper envSettingWrapper = new EnvSettingWrapper();
            envSettingWrapper.LoadEnvSetting(envSettingHash);
            return envSettingWrapper.envSetting;
        }
    }
}