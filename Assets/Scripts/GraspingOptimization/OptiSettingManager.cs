using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class OptiSettingManager : MonoBehaviour
    {
        public OptiSetting optiSetting;

        [SerializeField]
        private string dataDir;
        [SerializeField]
        private string settingHash;

        [SerializeField]
        private float mutationRate;
        [SerializeField]
        private float sigma;
        [SerializeField]
        private float initTemperature;
        [SerializeField]
        private float cooling;
        [SerializeField]
        private float targetScore;
        [SerializeField]
        private float worstScore;
        [SerializeField]
        private int maxSteps;


        public void Export()
        {
            optiSetting = new OptiSetting(mutationRate, sigma, initTemperature, cooling, targetScore, worstScore, maxSteps);
            //string hash = optiSetting.ExportOptiSetting(dataDir);
            // DBへ保存する場合
            OptiSettingWrapper optiSettingWrapper = new OptiSettingWrapper(optiSetting);
            string hash = optiSettingWrapper.ExportOptiSetting();
            settingHash = hash;
            Debug.Log($"Exported OptiSetting: {hash}");
        }

        public void Load()
        {
            //OptiSetting optiSetting = new OptiSetting();
            //optiSetting.LoadOptiSetting(dataDir, settingHash);
            // DBから読みだす場合
            OptiSettingWrapper optiSettingWrapper = new OptiSettingWrapper();
            optiSettingWrapper.LoadOptiSetting(settingHash);
            optiSetting = optiSettingWrapper.optiSetting;

            mutationRate = optiSetting.mutationRate;
            sigma = optiSetting.sigma;
            initTemperature = optiSetting.initTemperature;
            cooling = optiSetting.cooling;
            targetScore = optiSetting.targetScore;
            worstScore = optiSetting.worstScore;
            maxSteps = optiSetting.maxSteps;
            Debug.Log($"Loaded OptiSetting: {settingHash}");
        }
    }
}