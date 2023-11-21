using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using Unity.VisualScripting;
using MongoDB.Driver;
using System;

namespace GraspingOptimization
{
    public abstract class BaseOpti : MonoBehaviour
    {
        public OptiSetting optiSetting;
        public EnvSetting envSetting;
        public string sequenceDt;

        public string sequenceId;

        public GameObject targetObj;
        public GameObject virtualObj;

        public List<GameObject> handObjectList;

        public Hands hands;

        public HandPoseLogger handPoseLogger;
        public HandPoseReader handPoseReader;
        public SettingHashList settingHashList;

        public bool isRunning = false;

        public SettingHash settingHash;

        void Start()
        {
            Physics.autoSimulation = false;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
            // インスタンスを取得
            if (handPoseLogger == null) handPoseLogger = this.GetComponent<HandPoseLogger>();
            if (handPoseReader == null) handPoseReader = this.GetComponent<HandPoseReader>();
            if (settingHashList == null) settingHashList = this.GetComponent<SettingHashList>();
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                hands.hands.Add(hand);
            }

        }

        void Update()
        {
            if (!isRunning)
            {
                settingHash = settingHashList.GetNextSettingHash();
                if (settingHash != null)
                {
                    InitOpti(settingHash);
                    StartCoroutine(StartOptimization());
                }
                else
                {
                    Application.targetFrameRate = 3;
                }
            }
        }

        /// <summary>
        /// 最適化を開始する
        /// 一連のシーケンスをまとめて最適化する
        /// </summary>
        public abstract IEnumerator StartOptimization();

        public void InitOpti(SettingHash settingHash)
        {
            string optiSettingHash = settingHash.optiSettingHash;
            string envSettingHash = settingHash.envSettingHash;
            sequenceDt = settingHash.sequenceDt;
            sequenceId = Guid.NewGuid().ToString("N");

            // BDから設定を取得
            OptiSettingWrapper optiSettingWrapper = new OptiSettingWrapper();
            optiSettingWrapper.LoadOptiSetting(optiSettingHash);
            optiSetting = optiSettingWrapper.optiSetting;
            EnvSettingWrapper envSettingWrapper = new EnvSettingWrapper();
            envSettingWrapper.LoadEnvSetting(envSettingHash);
            envSetting = envSettingWrapper.envSetting;
            Destroy(virtualObj);
            virtualObj = envSetting.LoadObjectInstance();
            handPoseLogger.SetLogObject(virtualObj);

            // 初期化
            Application.targetFrameRate = -1;
            isRunning = true;
        }
    }
}
