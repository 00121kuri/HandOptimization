using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class OptiSettingManager : MonoBehaviour
    {
        [SerializeField]
        public OptiSetting optiSetting;

        [SerializeField]
        private OptiType optiType;
        [SerializeField]
        private string settingHash;

        [SerializeField]
        private float mutationRate;
        [SerializeField]
        private float sigma;
        [SerializeField]
        private float worstScore;
        [SerializeField]
        private int maxSteps;

        //private T optiSetting;


        public void Export()
        {
            switch (optiType)
            {
                case OptiType.LocalSearch:
                    optiSetting = new LocalSearchSetting(mutationRate, sigma, worstScore, maxSteps);
                    OptiSettingWrapper<LocalSearchSetting> localSearchSettingWrapper = new OptiSettingWrapper<LocalSearchSetting>((LocalSearchSetting)optiSetting);
                    settingHash = localSearchSettingWrapper.ExportOptiSetting();
                    OptiTypeWrapper optiTypeWrapper = new OptiTypeWrapper(OptiType.LocalSearch, settingHash);
                    optiTypeWrapper.ExportOptiType();
                    Debug.Log($"Exported OptiSetting: {settingHash}");
                    break;
                default:
                    Debug.Log("OptiType is not set");
                    break;
            }
        }


        public void Load()
        {
            OptiTypeWrapper optiTypeWrapper = new OptiTypeWrapper();
            optiTypeWrapper.LoadOptiType(settingHash);
            switch (optiTypeWrapper.optiType)
            {
                case OptiType.LocalSearch:
                    OptiSettingWrapper<LocalSearchSetting> localSearchSettingWrapper = new OptiSettingWrapper<LocalSearchSetting>();
                    localSearchSettingWrapper.LoadOptiSetting(settingHash);
                    optiSetting = localSearchSettingWrapper.optiSetting;
                    LoadLocalSearchSetting();
                    break;
                default:
                    Debug.Log("OptiType is not set");
                    break;
            }
        }

        public void LoadLocalSearchSetting()
        {
            optiType = OptiType.LocalSearch;
            mutationRate = ((LocalSearchSetting)optiSetting).mutationRate;
            sigma = ((LocalSearchSetting)optiSetting).sigma;
            worstScore = ((LocalSearchSetting)optiSetting).worstScore;
            maxSteps = ((LocalSearchSetting)optiSetting).maxSteps;
        }
    }
}