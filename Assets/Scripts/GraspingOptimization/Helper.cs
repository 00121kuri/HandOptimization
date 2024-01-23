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
    public static class Helper
    {
        public static bool RandomBool()
        {
            return UnityEngine.Random.Range(0, 2) == 0;
        }

        public static float Gaussian(float sigma, float ave)
        {
            double z;


            float x = UnityEngine.Random.Range(0f, 1f);
            float y = UnityEngine.Random.Range(0f, 1f);

            z = sigma * Math.Sqrt(-2f * Math.Log(x)) * Math.Cos(2f * Math.PI * y) + ave;

            return (float)z;
        }

        public static List<int> GetRandomIndexN(int listCount, int n)
        {
            var indexList = new List<int>(listCount);
            List<int> valueList = new List<int>();
            for (int p = 0; p < listCount; p++) indexList.Add(p);

            for (int i = 0; i < n; i++)
            {
                int index = UnityEngine.Random.Range(0, indexList.Count);
                int value = indexList[index];
                indexList.RemoveAt(index);
                valueList.Add(value);
            }
            return valueList;
        }

        public static IEnumerable<T> GetRandomN<T>(this IList<T> collection, int n)
        {
            var indexList = new List<int>(collection.Count);
            for (int p = 0; p < collection.Count; p++) indexList.Add(p);

            for (int i = 0; i < n; i++)
            {
                int index = UnityEngine.Random.Range(0, indexList.Count);
                int value = indexList[index];
                indexList.RemoveAt(index);
                yield return collection[value];
            }
        }

        public static float ClampAngle(float angle, float from, float to)
        {
            angle = Mathf.Repeat(angle + 180, 360) - 180;

            angle = Mathf.Clamp(angle, from, to);

            return angle;
        }

        public static float ClampAngle180(float angle)
        {
            return Mathf.Repeat(angle + 180, 360) - 180;
        }

        public static Vector3 ClampVector180(Vector3 vector)
        {
            vector.x = ClampAngle180(vector.x);
            vector.y = ClampAngle180(vector.y);
            vector.z = ClampAngle180(vector.z);
            return vector;
        }

        // Jsonに変換し，ハッシュを返す
        public static string GetHash(string json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] hashBytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 対象のディープコピーを行う
        /// シリアライズ(Serializable 属性)されていないクラスではエラー
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(this T src)
        {
            // Jsonをシリアライズしてディープコピーを行う
            string json = JsonUtility.ToJson(src);
            return JsonUtility.FromJson<T>(json);
        }
    }
}