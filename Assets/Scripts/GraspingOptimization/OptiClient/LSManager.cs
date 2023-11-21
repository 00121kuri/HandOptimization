using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEngine.InputSystem.Interactions;
using MongoDB.Driver;
using System;

namespace GraspingOptimization
{
    public class LSManager : BaseOpti
    {
        float sigma = 7f;
        float mean = 0f;

        float mutationRate = 1f;



        public override IEnumerator StartOptimization()
        {
            // 暫定
            int maxFrameCount = 100;
            int maxStepCount = 20000;

            // フレームごとのループ
            for (int frameCount = 0; frameCount < maxFrameCount; frameCount++)
            {
                // 手のポーズを取得
                HandPoseData handPoseData = handPoseReader.ReadHandPoseDataFromDB("inputdata", sequenceDt, frameCount);
                if (handPoseData == null)
                {
                    // データを最後まで読み終わったら終了
                    Debug.Log("handPoseData is null");
                    isRunning = false;
                    yield break;
                }

                // 1フレームにおける初期の手のポーズと物体の位置を設定
                handPoseReader.SetHandPose(handPoseData);

                // 初期の物体の位置
                Vector3 initPosition;
                Quaternion initRotation;

                HandChromosome initChromosome = hands.GetCurrentHandChromosome();
                HandChromosome minScoreChromosome = initChromosome;
                minScoreChromosome.EvaluationHand(hands, targetObj, virtualObj, handPoseData.objectData.position, handPoseData.objectData.rotation, minScoreChromosome);

                // 1ステップのループ
                for (int stepCount = 0; stepCount < maxStepCount; stepCount++)
                {
                    if (minScoreChromosome.score < optiSetting.worstScore)
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
                    HandChromosome neighborChromosome = minScoreChromosome.GenerateNeighborChromosome(sigma, mean, mutationRate);

                    // 評価
                    neighborChromosome.EvaluationHand(hands, targetObj, virtualObj, initPosition, initRotation, minScoreChromosome);

                    if (neighborChromosome.score < minScoreChromosome.score)
                    {
                        // 近傍の方が良かったら更新
                        minScoreChromosome = neighborChromosome;
                    }

                    Debug.Log($"frameCount: {frameCount}, stepCount: {stepCount}, score: {minScoreChromosome.score}");

                    yield return null;
                }
            }
        }
    }
}
