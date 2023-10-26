using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

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
    }
}