using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEngine.InputSystem.Interactions;
using MongoDB.Driver;
using System;

namespace GraspingOptimization
{
    [System.Serializable]
    public class LocalSearchSetting : OptiSetting
    {
        /// <summary>
        /// 変異確率
        /// 近傍探索の際にそれぞれの指が変化する確率
        /// 1にすると全ての指にノイズが加えられる
        /// </summary>
        public float mutationRate;

        /// <summary>
        /// 近傍探索で用いるノイズの正規分布のSigma
        /// </summary>
        public float sigma;
        public float mean = 0;

        /// <summary>
        /// 仮想オブジェクトの位置をリセットするときの閾値
        /// 0にすると毎回リセットされる
        /// </summary>
        public float worstScore;

        /// <summary>
        /// 最大ステップ数
        /// </summary>
        public int maxSteps;

        public bool isUsePreviousResult = false;

        public float weightDistance = 1f;

        public float weightRotation = 0.1f;

        public float weightChromosomeDiff = 0f;

        public LocalSearchSetting(float mutationRate, float sigma, float worstScore, int maxSteps = 1000, float mean = 0f, bool isUsePreviousResult = false, float weightDistance = 1f, float weightRotation = 0.1f, float weightChromosomeDiff = 0f) : base()
        {
            this.mutationRate = mutationRate;
            this.sigma = sigma;
            this.worstScore = worstScore;
            this.maxSteps = maxSteps;
            this.mean = mean;
            this.isUsePreviousResult = isUsePreviousResult;
            this.weightDistance = weightDistance;
            this.weightRotation = weightRotation;
            this.weightChromosomeDiff = weightChromosomeDiff;
            Debug.Log(JsonUtility.ToJson(this));
        }
    }

    public class LocalSearch : BaseOpti
    {
        LocalSearchSetting localSearchSetting;

        OptiClientDisplay optiClientDisplay;

        public LocalSearch(GameObject targetObj, GameObject virtualObj, Hands hands, HandPoseLogger handPoseLogger, HandPoseReader handPoseReader) : base(targetObj, virtualObj, hands, handPoseLogger, handPoseReader)
        {
            optiClientDisplay = GameObject.Find("OptiClientDisplay").GetComponent<OptiClientDisplay>();
            Debug.Log("LocalSearch");
        }

        public override IEnumerator StartOptimization(Action onFinished)
        {
            HandChromosome previousResultChromosome = null;
            // フレームごとのループ
            for (int frameCount = 0; ; frameCount++)
            {
                // 手のポーズを取得
                HandPoseData handPoseData = handPoseReader.ReadHandPoseDataFromDB("inputdata", sequenceDt, frameCount);
                if (handPoseData == null)
                {
                    // データを最後まで読み終わったら終了
                    Debug.Log("handPoseData is null");
                    if (onFinished != null) onFinished();
                    optiClientDisplay.WaitingDisplay();
                    yield break;
                }

                // 1フレームにおける初期の手のポーズと物体の位置を設定
                handPoseReader.SetHandPose(handPoseData);

                HandChromosome initChromosome = new HandChromosome();
                if (localSearchSetting.isUsePreviousResult && previousResultChromosome != null)
                {
                    initChromosome.jointGeneList = previousResultChromosome.jointGeneList;
                }
                else
                {
                    initChromosome = hands.GetCurrentHandChromosome();
                }
                HandChromosome minScoreChromosome = initChromosome;
                minScoreChromosome.EvaluationHand(
                    hands,
                    targetObj,
                    virtualObj,
                    handPoseData.objectData.position,
                    handPoseData.objectData.rotation,
                    minScoreChromosome,
                    initChromosome,
                    localSearchSetting.weightDistance,
                    localSearchSetting.weightRotation,
                    localSearchSetting.weightChromosomeDiff
                    );

                // 1ステップのループ
                for (int stepCount = 0; stepCount < localSearchSetting.maxSteps; stepCount++)
                {
                    // 初期の物体の位置
                    Vector3 initPosition = Vector3.zero;
                    Quaternion initRotation = Quaternion.identity;
                    if (minScoreChromosome.score < localSearchSetting.worstScore)
                    {
                        // 前のステップの結果を使う
                        initPosition = minScoreChromosome.resultPosition;
                        initRotation = minScoreChromosome.resultRotation;
                    }
                    else
                    {
                        // 初期の物体の位置を使う
                        initPosition = handPoseData.objectData.position;
                        initRotation = handPoseData.objectData.rotation;
                    }

                    // 近傍のHandChromosomeを生成
                    HandChromosome neighborChromosome = minScoreChromosome.GenerateNeighborChromosome(localSearchSetting.sigma, localSearchSetting.mean, localSearchSetting.mutationRate);

                    // 評価
                    neighborChromosome.EvaluationHand(
                        hands,
                        targetObj,
                        virtualObj,
                        initPosition,
                        initRotation,
                        minScoreChromosome,
                        initChromosome,
                        localSearchSetting.weightDistance,
                        localSearchSetting.weightRotation,
                        localSearchSetting.weightChromosomeDiff);

                    if (neighborChromosome.score < minScoreChromosome.score)
                    {
                        // 近傍の方が良かったら更新
                        minScoreChromosome = neighborChromosome;
                    }

                    // 画面を更新
                    try
                    {
                        optiClientDisplay.UpdateDisplay(sequenceDt, sequenceId, frameCount, stepCount);
                    }
                    catch (NullReferenceException e)
                    {
                        Debug.Log(e);
                    }
                    //Debug.Log($"frameCount: {frameCount}, stepCount: {stepCount}, score: {minScoreChromosome.score}");

                    yield return null;
                }

                // minScoreChromosomeを反映
                hands.SetHandChromosome(minScoreChromosome);
                virtualObj.transform.SetPositionAndRotation(minScoreChromosome.resultPosition, minScoreChromosome.resultRotation);

                // 結果を出力
                ExportResult(sequenceId, sequenceDt, frameCount, settingHash.optiSettingHash, settingHash.envSettingHash, minScoreChromosome, handPoseData.objectData.position, handPoseData.objectData.rotation);
                ExportCurrentHandPoseData(sequenceId, sequenceDt, frameCount);
                previousResultChromosome = minScoreChromosome;
            }
        }

        public override void InitOpti(SettingHash settingHash, string dt)
        {
            sequenceDt = dt;
            sequenceId = Guid.NewGuid().ToString("N");

            this.settingHash = settingHash;

            // ハッシュ値から設定を読み込む
            OptiSettingWrapper<LocalSearchSetting> optiSettingWrapper = new OptiSettingWrapper<LocalSearchSetting>();
            optiSettingWrapper.LoadOptiSetting(settingHash.optiSettingHash);
            localSearchSetting = optiSettingWrapper.optiSetting;

            // 初期化
            Application.targetFrameRate = -1;
            isRunning = true;

        }
    }
}
