using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using LeapInternal;

namespace GraspingOptimization
{
    /// <summary>
    /// jsonで保存するためのデータクラス
    /// </summary>
    [System.Serializable]
    public class HandPoseData
    {
        public int frameCount;
        public List<HandData> handDataList;


        public HandPoseData()
        {
            handDataList = new List<HandData>();
        }
    }

    [System.Serializable]
    public class HandData
    {
        public HandType handType;
        public List<FingerData> fingerDataList;


        public HandData(HandType handType)
        {
            fingerDataList = new List<FingerData>();
            this.handType = handType;
        }
    }

    [System.Serializable]
    public class FingerData
    {
        public FingerType fingerType;
        public List<JointData> jointDataList;


        public FingerData(FingerType fingerType)
        {
            jointDataList = new List<JointData>();
            this.fingerType = fingerType;
        }
    }

    [System.Serializable]
    public class JointData
    {
        public JointType jointType;
        public Vector3 localPosition;
        public Quaternion localRotation;


        public JointData(Vector3 localPosition, Quaternion localRotation, JointType jointType)
        {
            this.jointType = jointType;
            this.localPosition = localPosition;
            this.localRotation = localRotation;
        }
    }
}