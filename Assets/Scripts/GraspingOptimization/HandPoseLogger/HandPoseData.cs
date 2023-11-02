using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using LeapInternal;

namespace GraspingOptimization
{
    [System.Serializable]
    enum DataType
    {
        Input,
        Output
    }

    /// <summary>
    /// jsonで保存するためのデータクラス
    /// </summary>
    [System.Serializable]
    public class HandPoseData
    {
        public string sequenceId;
        public int frameCount;
        public List<HandData> handDataList;
        public ObjectData objectData;


        public HandPoseData(string sequenceId, int frameCount)
        {
            this.sequenceId = sequenceId;
            this.frameCount = frameCount;
            handDataList = new List<HandData>();
        }
    }

    [System.Serializable]
    public class ObjectData
    {
        public Vector3 position;
        public Quaternion rotation;

        public ObjectData(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class HandData
    {
        public HandType handType;
        public List<FingerData> fingerDataList;

        public JointData wristJoint;
        public JointData elbowJoint;


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
        public Vector3 position;
        public Quaternion rotation;

        public Vector3 localScale;


        public JointData(Vector3 position, Quaternion rotation, Vector3 localScale, JointType jointType)
        {
            this.jointType = jointType;
            this.position = position;
            this.rotation = rotation;
            this.localScale = localScale;
        }
    }
}