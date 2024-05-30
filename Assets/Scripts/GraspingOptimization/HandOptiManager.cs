// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using GraspingOptimization;
// using static GraspingOptimization.Helper;
// using Unity.VisualScripting;
// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using System.IO;
// using UnityEngine.UIElements;
// using Unity.VisualScripting.FullSerializer;
// using System.Data.Common;
// using MongoDB.Driver;
// //using Palmmedia.ReportGenerator.Core;

// namespace GraspingOptimization
// {
//     public class HandOptiManager : MonoBehaviour
//     {
//         [SerializeField]
//         OptiSetting optiSetting;

//         [SerializeField]
//         EnvSetting envSetting;

//         Hand hand;
//         [SerializeField]
//         GameObject handObject;

//         public GameObject tangibleObj;
//         private GameObject virtualObj;


//         [SerializeField]
//         bool isRunning = false;
//         [SerializeField]
//         bool autoStart = false;

//         [SerializeField]
//         bool outputLogImages = false;
//         bool isWaiting = true;

//         int step = 0;

//         HandChromosome initChromosome;
//         [SerializeField]
//         HandChromosome minScoreChromosome;



//         HandPoseData handPoseData;

//         [SerializeField]
//         int frameCount = -1;

//         [SerializeField]
//         string dataDir = "opti-data";

//         [SerializeField]
//         HandPoseReader handPoseReader;

//         [SerializeField]
//         HandPoseLogger handPoseLogger;

//         [SerializeField]
//         OptiSettingManager optiSettingManager;

//         [SerializeField]
//         EnvSettingManager envSettingManager;

//         [SerializeField]
//         SettingHashList optiSettingList;

//         GUIStyle guiStyle;

//         string sequenceId;

//         string optiSettingHash;
//         string envSettingHash;
//         string sequenceDt;

//         void Start()
//         {
//             // GUIの文字の設定
//             guiStyle = new GUIStyle();
//             guiStyle.fontSize = 24;
//             guiStyle.normal.textColor = Color.white;

//             hand = handObject.GetComponent<HandManager>().hand;
//             Physics.autoSimulation = false;

//             Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
//         }

//         // Update is called once per frame
//         void Update()
//         {
//             if (isWaiting)
//             {
//                 SettingHash settingHash = optiSettingList.GetNextSettingHash();
//                 if (settingHash == null)
//                 {
//                     optiSettingList.isWaiting = true;
//                     // FPSを下げる
//                     Application.targetFrameRate = 5;
//                     return;
//                 }

//                 LoadSettings(settingHash);
//                 sequenceId = Guid.NewGuid().ToString("N");
//                 isWaiting = false;
//                 optiSettingList.isWaiting = false;
//                 frameCount = -1;
//                 // FPSを戻す
//                 Application.targetFrameRate = -1;

//             }

//             if (!isRunning && (Input.GetKey(KeyCode.Return) || autoStart))
//             {
//                 frameCount++; // 表示上の都合で-1からスタート

//                 Debug.Log("Sequence datetime: " + sequenceDt);
//                 //handPoseData = handPoseReader.ReadHandPoseData("inputdata", sequenceDt, frameCount);
//                 handPoseData = handPoseReader.ReadHandPoseDataFromDB("inputdata", sequenceDt, frameCount);



//                 if (handPoseData == null)
//                 {
//                     isWaiting = true;
//                     return;
//                 }
//                 handPoseReader.SetHandPose(handPoseData);

//                 // 最適化
//                 StartCoroutine(StartOpti(sequenceId, handPoseData.dateTime, handPoseData.frameCount));
//             }
//         }

//         IEnumerator StartOpti(string sequenceId, string dt, int frameCount)
//         {
//             float temperature = optiSetting.initTemperature;
//             float temperature_found = optiSetting.initTemperature; // 暫定解が見つかった時の温度
//             int notUpdatedCnt = 0;
//             int total_cnt = 0; // 評価回数

//             string logDir = $"{dataDir}/logs/{dt}/{sequenceId}";
//             System.IO.Directory.CreateDirectory(logDir);
//             if (outputLogImages)
//             {
//                 System.IO.Directory.CreateDirectory($"{logDir}/{frameCount}"); // スクリーンショット用のフォルダ
//             }

//             string logfile = $"{logDir}/{frameCount}-score.csv";
//             FileLog.AppendLog(logfile, "Step,minScore,childScore\n");
//             // 初期化
//             Debug.Log("init Opti");
//             step = 0;
//             // 位置を合わせる
//             virtualObj.transform.SetPositionAndRotation(tangibleObj.transform.position, tangibleObj.transform.rotation);
//             //Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);
//             isRunning = true;


//             Vector3 initPosition = virtualObj.transform.position;
//             Quaternion initRotation = virtualObj.transform.rotation;

//             // 手の移動開始位置
//             initChromosome = HandPfGA.Init(hand);
//             // 初期生成
//             minScoreChromosome = initChromosome;
//             minScoreChromosome.EvaluationHand(hand, initChromosome.jointRotations, tangibleObj, virtualObj, initPosition, initRotation, optiSetting.worstScore);

//             FileLog.AppendLog(logfile, $"{step},{minScoreChromosome.score},{minScoreChromosome.score}\n");

//             if (outputLogImages)
//             {
//                 string capturefilefirst = $"{logDir}/{frameCount}/{step}.png";
//                 ScreenCapture.CaptureScreenshot(capturefilefirst);
//             }

//             yield return null;



//             while (minScoreChromosome.score > optiSetting.targetScore)
//             {
//                 // 初期値
//                 //minScore = float.MaxValue;
//                 //minScoreIndex = 0;
//                 step++;

//                 if (step > optiSetting.maxSteps)
//                 {
//                     Debug.Log("Cannot find Ans");
//                     break;
//                 }

//                 if (notUpdatedCnt > optiSetting.maxSteps / 10)
//                 {
//                     Debug.Log("Not Updated for a long time");
//                     break;
//                 }

//                 // 生成
//                 HandChromosome child = HandPfGA.GenerateNeighbor(hand, minScoreChromosome, optiSetting.mutationRate, optiSetting.sigma);
//                 // 評価
//                 // 評価がworstScoreより悪かった場合は，仮想物体の位置をリセット
//                 if (minScoreChromosome.score > optiSetting.worstScore)
//                 {

//                     child.EvaluationHand(
//                             hand,
//                             initChromosome.jointRotations,
//                             tangibleObj,
//                             virtualObj,
//                             initPosition,
//                             initRotation,
//                             optiSetting.worstScore
//                             );
//                 }
//                 else
//                 {
//                     child.EvaluationHand(
//                             hand,
//                             initChromosome.jointRotations,
//                             tangibleObj,
//                             virtualObj,
//                             minScoreChromosome.resultPosition,
//                             minScoreChromosome.resultRotation,
//                             optiSetting.worstScore
//                             );
//                 }
//                 total_cnt++;
//                 // 選択
//                 // アニーリング法
//                 temperature *= optiSetting.cooling;
//                 if (child.score < minScoreChromosome.score)
//                 {
//                     minScoreChromosome = child;
//                     if (outputLogImages)
//                     {
//                         string capturefile = $"{logDir}/{frameCount}/{step}.png";
//                         ScreenCapture.CaptureScreenshot(capturefile);
//                     }
//                     temperature_found = temperature;
//                     notUpdatedCnt = 0;
//                 }
//                 else
//                 {
//                     float acceptRate = Mathf.Exp((-(child.score - minScoreChromosome.score)) / temperature);
//                     //Debug.Log($"accept rate: {acceptRate}");
//                     notUpdatedCnt++;
//                     if (notUpdatedCnt > optiSetting.maxSteps / 100) { temperature = temperature_found; } // しばらく更新されていないときは，温度をあげる
//                     if (UnityEngine.Random.Range(0f, 1f) < acceptRate)
//                     {
//                         minScoreChromosome = child;
//                         if (outputLogImages)
//                         {
//                             string capturefile = $"{logDir}/{frameCount}/{step}.png";
//                             ScreenCapture.CaptureScreenshot(capturefile);
//                         }
//                         notUpdatedCnt = 0;
//                     }
//                 }


//                 Debug.Log("Step: " + step
//                         + ", minScore: " + minScoreChromosome.score);

//                 //Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);

//                 // ファイルに出力
//                 FileLog.AppendLog(logfile, $"{step},{minScoreChromosome.score},{child.score}\n");

//                 yield return null;
//             }
//             // 最も良かった結果を反映する
//             hand.SetJointRotation(minScoreChromosome.jointRotations);
//             virtualObj.transform.SetPositionAndRotation(minScoreChromosome.resultPosition, minScoreChromosome.resultRotation);

//             // ログを出力
//             HandPoseData outputHandPoseData = handPoseLogger.GetHandPoseData(sequenceId, dt, frameCount);

//             // 出力フォルダ作成
//             //System.IO.Directory.CreateDirectory($"{dataDir}/output/{dt}");
//             //handPoseLogger.ExportJson(JsonUtility.ToJson(outputHandPoseData), $"{dataDir}/output/{dt}/{sequenceId}.jsonl");
//             handPoseLogger.ExportDB(JsonUtility.ToJson(outputHandPoseData));

//             Debug.Log("init position" + initPosition);
//             //isRunning = false;
//             //Time.timeScale = 0;
//             Debug.Log($"total cnt: {total_cnt}");

//             string optiSettingHash = Helper.GetHash(JsonUtility.ToJson(optiSetting));
//             string envSettingHash = Helper.GetHash(JsonUtility.ToJson(envSetting));
//             OptiResult optiResult = new OptiResult(
//                     sequenceId,
//                     dt,
//                     frameCount,
//                     optiSettingHash,
//                     envSettingHash,
//                     minScoreChromosome.score,
//                     minScoreChromosome.resultPosition,
//                     minScoreChromosome.resultRotation,
//                     initPosition,
//                     initRotation
//                 );
//             //System.IO.Directory.CreateDirectory($"{dataDir}/result/{dt}");
//             //optiResult.Export($"{dataDir}/result/{dt}/{sequenceId}.jsonl");
//             optiResult.ExportDB();


//             //Invoke(nameof(RunPhysics), 2f);
//             isRunning = false;
//             yield break;
//         }

//         private void LoadSettings(SettingHash settingHash)
//         {
//             optiSettingHash = settingHash.optiSettingHash;
//             envSettingHash = settingHash.envSettingHash;
//             sequenceDt = settingHash.sequenceDt;

//             // ファイルから読み込み
//             //optiSetting.LoadOptiSetting(dataDir, optiSettingHash);
//             //envSetting.LoadEnvSetting(dataDir, envSettingHash);

//             // DBから読み込み
//             OptiSettingWrapper optiSettingWrapper = new OptiSettingWrapper();
//             optiSettingWrapper.LoadOptiSetting(optiSettingHash);
//             optiSetting = optiSettingWrapper.optiSetting;
//             EnvSettingWrapper envSettingWrapper = new EnvSettingWrapper();
//             envSettingWrapper.LoadEnvSetting(envSettingHash);
//             envSetting = envSettingWrapper.envSetting;
//             Destroy(virtualObj);
//             virtualObj = envSetting.LoadObjectInstance();
//             handPoseLogger.SetLogObject(virtualObj);
//         }

//         void RunPhysics()
//         {
//             Time.timeScale = 1;
//             Physics.autoSimulation = true;
//             Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
//         }

//         private void OnGUI()
//         {
//             if (isWaiting)
//             {
//                 GUILayout.Label($"Waiting for next sequence", guiStyle);
//                 return;
//             }
//             else
//             {
//                 GUILayout.Label($"Sequence: {sequenceDt}\nSequenceID: {sequenceId}\nFrame: {frameCount}, Step: {step}", guiStyle);
//             }
//         }
//     }
// }

