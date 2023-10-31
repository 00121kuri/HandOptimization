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
        Hand hand;
        [SerializeField]
        GameObject handObject;

        public GameObject tangibleObj;
        public GameObject virtualObj;

        /// <summary>
        /// 変異確率
        /// 近傍探索の際にそれぞれの指が変化する確率
        /// 1にすると全ての指にノイズが加えられる
        /// </summary>
        [SerializeField]
        float mutationRate;

        /// <summary>
        /// 近傍探索で用いるノイズの正規分布のSigma
        /// </summary>
        [SerializeField]
        float sigma;

        /// <summary>
        /// アニーリング法の初期温度
        /// 0にすると単純な局所探索法になる
        /// </summary>
        [SerializeField]
        float temperature;

        /// <summary>
        /// アニーリング法の冷却係数
        /// </summary>
        [SerializeField]
        float cooling = 0.95f;

        /// <summary>
        /// 終了条件のスコア
        /// </summary>
        [SerializeField]
        float targetScore;

        /// <summary>
        /// 仮想オブジェクトの位置をリセットするときの閾値
        /// 0にすると毎回リセットされる
        /// </summary>
        [SerializeField]
        float worstScore;

        [SerializeField]
        bool isRunning = false;
        [SerializeField]
        bool autoStart = false;

        [SerializeField]
        bool outputLogImages = false;
        bool isEnd = false;

        /// <summary>
        /// 探索を打ち切る最大の世代数
        /// </summary>
        [SerializeField]
        int maxSteps;

        int step = 0;

        [SerializeField]
        FollowTarget wist;
        [SerializeField]
        FollowTarget elbow;

        HandChromosome initChromosome;
        [SerializeField]
        HandChromosome minScoreChromosome;

        [SerializeField]
        HandPoseReader handPoseReader;

        HandPoseData handPoseData;

        [SerializeField]
        int frameCount = 0;

        [SerializeField]
        string dataDir;



        GUIStyle guiStyle;
        void Start()
        {
            // GUIの文字の設定
            guiStyle = new GUIStyle();
            guiStyle.fontSize = 24;
            guiStyle.normal.textColor = Color.white;

            hand = handObject.GetComponent<HandManager>().hand;
            Physics.autoSimulation = false;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (isEnd) { return; }

            if (!isRunning && (Input.GetKey(KeyCode.Return) || autoStart))
            {
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
                frameCount++;
            }
        }

        IEnumerator StartOpti(string sequenceId, int frameCount)
        {
            float temperature_found = temperature; // 暫定解が見つかった時の温度
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
            minScoreChromosome.EvaluationHand(hand, initChromosome.jointRotations, tangibleObj, virtualObj, initPosition, initRotation, worstScore);

            FileLog.AppendLog(logfile, $"{step},{minScoreChromosome.score},{minScoreChromosome.score}\n");

            if (outputLogImages)
            {
                string capturefilefirst = $"{logDir}/{step}.png";
                ScreenCapture.CaptureScreenshot(capturefilefirst);
            }

            yield return null;



            while (minScoreChromosome.score > targetScore)
            {
                // 初期値
                //minScore = float.MaxValue;
                //minScoreIndex = 0;
                step++;

                if (step > maxSteps)
                {
                    Debug.Log("Cannot find Ans");
                    break;
                }

                if (notUpdatedCnt > maxSteps / 10)
                {
                    Debug.Log("Not Updated for a long time");
                    break;
                }

                // 生成
                HandChromosome child = HandPfGA.GenerateNeighbor(hand, minScoreChromosome, mutationRate, sigma);
                // 評価
                // 評価がworstScoreより悪かった場合は，仮想物体の位置をリセット
                if (minScoreChromosome.score > worstScore)
                {

                    child.EvaluationHand(
                            hand,
                            initChromosome.jointRotations,
                            tangibleObj,
                            virtualObj,
                            initPosition,
                            initRotation,
                            worstScore
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
                            worstScore
                            );
                }
                total_cnt++;
                // 選択
                // アニーリング法
                temperature *= cooling;
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
                    if (notUpdatedCnt > maxSteps / 100) { temperature = temperature_found; } // しばらく更新されていないときは，温度をあげる
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
            FileLog.AppendLog(filePath, $"Mutation Rate,{mutationRate}\n");
            FileLog.AppendLog(filePath, $"Sigma,{sigma}\n");
            FileLog.AppendLog(filePath, $"Target Score,{targetScore}\n");
            FileLog.AppendLog(filePath, $"Worst Score,{worstScore}\n");
            FileLog.AppendLog(filePath, $"Max Steps,{maxSteps}\n");
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
            GUILayout.Label($"Step: {step}", guiStyle);
        }

    }
}

