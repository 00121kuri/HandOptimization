using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    [System.Serializable]
    public class SettingHash
    {
        public string optiSettingHash;
        public string envSettingHash;
        public string sequenceDt;

        public SettingHash(string optiSettingHash, string envSettingHash, string sequenceDt)
        {
            this.optiSettingHash = optiSettingHash;
            this.envSettingHash = envSettingHash;
            this.sequenceDt = sequenceDt;
        }
    }
}