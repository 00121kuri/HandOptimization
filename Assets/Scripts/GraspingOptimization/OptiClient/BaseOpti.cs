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

        public SettingHash settingHash;

        public bool isExportLog = false;

        public BaseOpti(GameObject targetObj, GameObject virtualObj, Hands hands, HandPoseLogger handPoseLogger, HandPoseReader handPoseReader, bool isExportLog = false)
        {
            this.targetObj = targetObj;
            this.virtualObj = virtualObj;
            this.hands = hands;
            this.handPoseLogger = handPoseLogger;
            this.handPoseReader = handPoseReader;
            this.isExportLog = isExportLog;
        }

        /// <summary>
        /// 最適化を開始する
        /// 一連のシーケンスをまとめて最適化する
        /// </summary>
        public abstract IEnumerator StartOptimization(Action onFinished = null);

        public abstract void InitOpti(SettingHash settingHash, string sequenceDt);

        public void ExportCurrentHandPoseData(string sequenceId, string sequenceDt, int frameCount)
        {
            HandPoseData outHandPoseData = handPoseLogger.GetHandPoseData(sequenceId, sequenceDt, frameCount);
            handPoseLogger.ExportDB(JsonUtility.ToJson(outHandPoseData));
        }

        public void ExportResult(
            string sequenceId,
            string sequenceDt,
            int frameCount,
            string optiSettingHash,
            string envSettingHash,
            HandChromosome minScoreChromosome,
            HandChromosome initChromosome,
            HandChromosome inputChromosome,
            Vector3 initPosition,
            Quaternion initRotation
        )
        {
            OptiResult optiResult = new OptiResult(
                    sequenceId,
                    sequenceDt,
                    frameCount,
                    optiSettingHash,
                    envSettingHash,
                    minScoreChromosome,
                    initChromosome,
                    inputChromosome,
                    initPosition,
                    initRotation
                );
            optiResult.ExportDB();
        }
    }
}
