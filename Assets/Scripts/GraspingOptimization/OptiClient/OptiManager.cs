using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEngine.SocialPlatforms;

namespace GraspingOptimization
{
    public class OptiManager : MonoBehaviour
    {

        private HandPoseLogger handPoseLogger;
        private HandPoseReader handPoseReader;
        private SettingHashList settingHashList;

        public List<GameObject> handObjectList;

        private Hands hands;

        public bool isRunning = false;

        private SettingHash settingHash;

        private EnvSetting envSetting;

        private GameObject virtualObj;

        [SerializeField]
        private GameObject targetObj;

        private LocalSearch localSearch;

        OptiTypeWrapper optiTypeWrapper = new OptiTypeWrapper();

        [SerializeField]
        private bool isExportLog = false;

        void Start()
        {
            Physics.autoSimulation = false;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
            // インスタンスを取得
            if (handPoseLogger == null) handPoseLogger = this.GetComponent<HandPoseLogger>();
            if (handPoseReader == null) handPoseReader = this.GetComponent<HandPoseReader>();
            if (settingHashList == null) settingHashList = this.GetComponent<SettingHashList>();
            List<Hand> handList = new List<Hand>();
            foreach (GameObject handObject in handObjectList)
            {
                Hand hand = handObject.GetComponent<HandManager>().hand;
                handList.Add(hand);
            }
            hands = new Hands(handList);

        }

        void Update()
        {
            if (!isRunning)
            {
                settingHash = settingHashList.GetNextSettingHash();
                if (settingHash != null)
                {
                    InitEnv(settingHash.envSettingHash);
                    // BDから設定を取得
                    optiTypeWrapper.LoadOptiType(settingHash.optiSettingHash);
                    switch (optiTypeWrapper.optiType)
                    {
                        case OptiType.LocalSearch:
                            localSearch = new LocalSearch(targetObj, virtualObj, hands, handPoseLogger, handPoseReader, isExportLog);
                            localSearch.InitOpti(settingHash, settingHash.sequenceDt);
                            isRunning = true;
                            settingHashList.isWaiting = false;
                            StartCoroutine(localSearch.StartOptimization(onFinished: () => { isRunning = false; }));
                            break;
                        default:
                            Debug.Log("optiType is not defined");
                            break;
                    }
                }
                else
                {
                    localSearch = null;
                    settingHashList.isWaiting = true;
                    Application.targetFrameRate = 3;
                }
            }
        }

        private void InitEnv(string envSettingHash)
        {
            EnvSettingWrapper envSettingWrapper = new EnvSettingWrapper();
            envSettingWrapper.LoadEnvSetting(envSettingHash);
            envSetting = envSettingWrapper.envSetting;
            Destroy(virtualObj);
            virtualObj = envSetting.LoadObjectInstance();
            handPoseLogger.SetLogObject(virtualObj);
        }
    }
}
