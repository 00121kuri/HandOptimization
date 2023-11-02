using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using static GraspingOptimization.Helper;
using Unity.VisualScripting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.UIElements;
using Unity.VisualScripting.FullSerializer;
using System.Data.Common;
//using static GraspingOptimization.HandPfGA;

namespace GraspingOptimization
{
    public class HandOptiManager : MonoBehaviour
    {
        [SerializeField]
        OptiSetting optiSetting;

        [SerializeField]
        EnvSetting envSetting;

        Hand hand;
        [SerializeField]
        GameObject handObject;

        public GameObject tangibleObj;
        private GameObject virtualObj;


        [SerializeField]
        bool isRunning = false;
        [SerializeField]
        bool autoStart = false;

        [SerializeField]
        bool outputLogImages = false;
        bool isEnd = false;

        int step = 0;

        HandChromosome initChromosome;
        [SerializeField]
        HandChromosome minScoreChromosome;



        HandPoseData handPoseData;

        [SerializeField]
        int frameCount = -1;

        [SerializeField]
        string dataDir;

        [SerializeField]
        HandPoseReader handPoseReader;

        [SerializeField]
        HandPoseLogger handPoseLogger;

        [SerializeField]
        OptiSettingManager optiSettingManager;

        [SerializeField]
        EnvSettingManager envSettingManager;

        GUIStyle guiStyle;

        void Start()
        {
            // GUIの文字の設定
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 24;
            guiStyle.normal.textColor = Color.white;

            hand = handObject.GetComponent<HandManager>().hand;
            Physics.autoSimulation = false;

            optiSetting = optiSettingManager.optiSetting;
            envSetting = envSettingManager.envSetting;
            virtualObj = envSetting.virtualObject;

            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
        }

        // Update is called once per frame
        void Update()
        {
            if (isEnd)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
#endif
                return;
            }

            if (!isRunning && (Input.GetKey(KeyCode.Return) || autoStart))
            {
                frameCount++; // 表示上の都合で-1からスタート
                // 初期状態取得
                handPoseData = handPoseReader.ReadHandPoseData(frameCount);
                if (handPoseData == null)
                {
                    isEnd = true;
                    return;
                }
                handPoseReader.SetHandPose(handPoseData);
                // 最適化
                StartCoroutine(StartOpti(handPoseData.sequenceId, handPoseData.frameCount));
            }
        }

        IEnumerator StartOpti(string sequenceId, int frameCount)
        {
            float temperature = optiSetting.initTemperature;
            float temperature_found = optiSetting.initTemperature; // 暫定解が見つかった時の温度
            int notUpdatedCnt = 0;
            int total_cnt = 0; // 評価回数

            //string dt = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string optiId = Guid.NewGuid().ToString("N");
            string logDir = $"{dataDir}/logs/{sequenceId}/{frameCount}/{optiId}";
            string logfile = $"{logDir}/score.csv";
            System.IO.Directory.CreateDirectory(logDir); // スクリーンショット用のフォルダ
            LogParams($"{logDir}/params.csv");
            FileLog.AppendLog(logfile, "Step,minScore,childScore\n");
            // 初期化
            Debug.Log("init Opti");
            step = 0;
            // 位置を合わせる
            virtualObj.transform.SetPositionAndRotation(tangibleObj.transform.position, tangibleObj.transform.rotation);
            //Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);
            isRunning = true;


            Vector3 initPosition = virtualObj.transform.position;
            Quaternion initRotation = virtualObj.transform.rotation;

            // 手の移動開始位置
            initChromosome = HandPfGA.Init(hand);
            // 初期生成
            minScoreChromosome = initChromosome;
            minScoreChromosome.EvaluationHand(hand, initChromosome.jointRotations, tangibleObj, virtualObj, initPosition, initRotation, optiSetting.worstScore);

            FileLog.AppendLog(logfile, $"{step},{minScoreChromosome.score},{minScoreChromosome.score}\n");

            if (outputLogImages)
            {
                string capturefilefirst = $"{logDir}/{step}.png";
                ScreenCapture.CaptureScreenshot(capturefilefirst);
            }

            yield return null;



            while (minScoreChromosome.score > optiSetting.targetScore)
            {
                // 初期値
                //minScore = float.MaxValue;
                //minScoreIndex = 0;
                step++;

                if (step > optiSetting.maxSteps)
                {
                    Debug.Log("Cannot find Ans");
                    break;
                }

                if (notUpdatedCnt > optiSetting.maxSteps / 10)
                {
                    Debug.Log("Not Updated for a long time");
                    break;
                }

                // 生成
                HandChromosome child = HandPfGA.GenerateNeighbor(hand, minScoreChromosome, optiSetting.mutationRate, optiSetting.sigma);
                // 評価
                // 評価がworstScoreより悪かった場合は，仮想物体の位置をリセット
                if (minScoreChromosome.score > optiSetting.worstScore)
                {

                    child.EvaluationHand(
                            hand,
                            initChromosome.jointRotations,
                            tangibleObj,
                            virtualObj,
                            initPosition,
                            initRotation,
                            optiSetting.worstScore
                            );
                }
                else
                {
                    child.EvaluationHand(
                            hand,
                            initChromosome.jointRotations,
                            tangibleObj,
                            virtualObj,
                            minScoreChromosome.resultPosition,
                            minScoreChromosome.resultRotation,
                            optiSetting.worstScore
                            );
                }
                total_cnt++;
                // 選択
                // アニーリング法
                temperature *= optiSetting.cooling;
                if (child.score < minScoreChromosome.score)
                {
                    minScoreChromosome = child;
                    if (outputLogImages)
                    {
                        string capturefile = $"{logDir}/{step}.png";
                        ScreenCapture.CaptureScreenshot(capturefile);
                    }
                    temperature_found = temperature;
                    notUpdatedCnt = 0;
                }
                else
                {
                    float acceptRate = Mathf.Exp((-(child.score - minScoreChromosome.score)) / temperature);
                    //Debug.Log($"accept rate: {acceptRate}");
                    notUpdatedCnt++;
                    if (notUpdatedCnt > optiSetting.maxSteps / 100) { temperature = temperature_found; } // しばらく更新されていないときは，温度をあげる
                    if (UnityEngine.Random.Range(0f, 1f) < acceptRate)
                    {
                        minScoreChromosome = child;
                        if (outputLogImages)
                        {
                            string capturefile = $"{logDir}/{step}.png";
                            ScreenCapture.CaptureScreenshot(capturefile);
                        }
                        notUpdatedCnt = 0;
                    }
                }


                Debug.Log("Step: " + step
                        + ", minScore: " + minScoreChromosome.score);

                //Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);

                // ファイルに出力
                FileLog.AppendLog(logfile, $"{step},{minScoreChromosome.score},{child.score}\n");

                yield return null;
            }
            // 最も良かった結果を反映する
            hand.SetJointRotation(minScoreChromosome.jointRotations);
            virtualObj.transform.SetPositionAndRotation(minScoreChromosome.resultPosition, minScoreChromosome.resultRotation);

            // ログを出力
            HandPoseData outputHandPoseData = handPoseLogger.GetHandPoseData(sequenceId, frameCount);
            string settingHash = Helper.GetHash(JsonUtility.ToJson(optiSetting));
            handPoseLogger.ExportJson(JsonUtility.ToJson(outputHandPoseData), $"{dataDir}/output/{sequenceId}-{settingHash}.jsonl");

            Debug.Log("init position" + initPosition);
            //isRunning = false;
            //Time.timeScale = 0;
            Debug.Log($"total cnt: {total_cnt}");

            LogResults(
                    $"{logDir}/result.csv",
                    minScoreChromosome.resultPosition,
                    minScoreChromosome.resultRotation,
                    initPosition,
                    initRotation
                    );

            //Invoke(nameof(RunPhysics), 2f);
            isRunning = false;
            yield break;
        }

        void RunPhysics()
        {
            Time.timeScale = 1;
            Physics.autoSimulation = true;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
        }

        void LogParams(string filePath)
        {
            FileLog.AppendLog(filePath, $"Mutation Rate,{optiSetting.mutationRate}\n");
            FileLog.AppendLog(filePath, $"sigma,{optiSetting.sigma}\n");
            FileLog.AppendLog(filePath, $"Target Score,{optiSetting.targetScore}\n");
            FileLog.AppendLog(filePath, $"Worst Score,{optiSetting.worstScore}\n");
            FileLog.AppendLog(filePath, $"Max Steps,{optiSetting.maxSteps}\n");
        }

        void LogResults(string filePath, Vector3 resultPos, Quaternion resultRot, Vector3 initPos, Quaternion initRot)
        {
            FileLog.AppendLog(filePath, $"Result Pos,{resultPos.x},{resultPos.y},{resultPos.z}\n");
            FileLog.AppendLog(filePath, $"Result Rot,{resultRot.x},{resultRot.y},{resultRot.z},{resultRot.w}\n");
            FileLog.AppendLog(filePath, $"Init Pos,{initPos.x},{initPos.y},{initPos.z}\n");
            FileLog.AppendLog(filePath, $"Init Rot,{initRot.x},{initRot.y},{initRot.z},{initRot.w}\n");
            FileLog.AppendLog(filePath, $"Distance,{Vector3.Distance(initPos, resultPos)}\n");
            FileLog.AppendLog(filePath, $"AngleDiff,{Quaternion.Angle(initRot, resultRot)}\n");
        }


        private void OnGUI()
        {
            GUILayout.Label($"Frame: {frameCount}, Step: {step}", guiStyle);
        }

    }
}

