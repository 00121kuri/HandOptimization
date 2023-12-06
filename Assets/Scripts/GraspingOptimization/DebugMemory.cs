using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using UnityEngine;
using UnityEngine.Profiling;

public class DebugMemory : MonoBehaviour
{
    float usedMemory;
    float totalMemory;

    string path;

    void Start()
    {
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        path = $"opti-data/logs/memory/{date}.csv";
        FileLog.AppendLog(path, "Used Memory (MB), Total Memory(MB)\n");
    }

    // Update is called once per frame
    void Update()
    {
        // 20分ごとにメモリ使用量を表示
        if (Time.frameCount % 72000 == 0 || Time.frameCount == 1)
        {
            usedMemory = (Profiler.GetTotalAllocatedMemoryLong() >> 10) / 1024f;
            totalMemory = (Profiler.GetTotalReservedMemoryLong() >> 10) / 1024f;

            //Debug.Log($"Used Memory: {usedMemory} MB, Total Memory: {totalMemory} MB");
            FileLog.AppendLog(path, $"{usedMemory}, {totalMemory}\n");
        }
    }
}
