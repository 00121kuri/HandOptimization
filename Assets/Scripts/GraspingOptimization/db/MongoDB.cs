using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace GraspingOptimization
{
    public static class MongoDB
    {
        public static IMongoCollection<BsonDocument> GetCollection(string databaseName, string collectionName)
        {
            string connectionString = "mongodb://192.168.10.5:27017";
            MongoClient client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase(databaseName);
            IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(collectionName);
            return collection;
        }
    }
}
