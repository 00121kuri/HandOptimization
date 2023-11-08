using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using System;

namespace GraspingOptimization
{
    public class OptiSettingList : MonoBehaviour
    {
        [SerializeField]
        List<string> optiSettingHasheList;
        [SerializeField]
        List<string> envSettingHasheList;
        [SerializeField]
        List<string> dtList;

        public int settingCount;


        public (string optiSettingHash, string envSettingHash, string dt) GetSettings(int index)
        {
            int cnt = 0;
            foreach (string optiSettingHash in optiSettingHasheList)
            {
                foreach (string envSettingHash in envSettingHasheList)
                {
                    foreach (string dt in dtList)
                    {
                        if (cnt == index)
                        {
                            return (optiSettingHash, envSettingHash, dt);
                        }
                        cnt++;
                    }
                }
            }
            return (null, null, null);
        }

        public int GetTotalSequenceCount()
        {
            settingCount = optiSettingHasheList.Count * envSettingHasheList.Count * dtList.Count;
            return settingCount;
        }
    }
}
