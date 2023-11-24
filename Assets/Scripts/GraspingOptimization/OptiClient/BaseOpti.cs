using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using Unity.VisualScripting;
using MongoDB.Driver;
using System;

namespace GraspingOptimization
{
    public abstract class BaseOpti
    {
        public string sequenceDt;

        public string sequenceId;

        public GameObject targetObj;
        public GameObject virtualObj;

        public Hands hands;

        public HandPoseLogger handPoseLogger;
        public HandPoseReader handPoseReader;

        public bool isRunning = false;

        public BaseOpti(GameObject targetObj, GameObject virtualObj, Hands hands, HandPoseLogger handPoseLogger, HandPoseReader handPoseReader)
        {
            this.targetObj = targetObj;
            this.virtualObj = virtualObj;
            this.hands = hands;
            this.handPoseLogger = handPoseLogger;
            this.handPoseReader = handPoseReader;
        }

        /// <summary>
        /// 最適化を開始する
        /// 一連のシーケンスをまとめて最適化する
        /// </summary>
        public abstract IEnumerator StartOptimization(Action onFinished = null);

        public abstract void InitOpti(string optiSettingHash, string sequenceDt);
    }
}
