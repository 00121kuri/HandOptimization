using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class DebugMemory : MonoBehaviour
{
    float usedMemory;
    float totalMemory;

    // Update is called once per frame
    void Update()
    {
        // 5分ごとにメモリ使用量を表示
        if (Time.frameCount % 18000 == 0 || Time.frameCount == 1)
        {
            usedMemory = (Profiler.GetTotalAllocatedMemoryLong() >> 10) / 1024f;
            totalMemory = (Profiler.GetTotalReservedMemoryLong() >> 10) / 1024f;

            Debug.Log($"Used Memory: {usedMemory} MB");
            Debug.Log($"Total Memory: {totalMemory} MB");
        }
    }
}
