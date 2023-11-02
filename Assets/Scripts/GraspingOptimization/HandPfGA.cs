using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using static GraspingOptimization.Helper;
using System.IO.Compression;

namespace GraspingOptimization
{
    public static class HandPfGA
    {
        public static HandChromosome Init(Hand hand)
        {
            HandChromosome initChromosome = new HandChromosome();
            initChromosome.SetCurrentJointRotation(hand);
            return initChromosome;
        }

        public static HandChromosome GenerateRandom(Hand hand)
        {
            HandChromosome newHandChromosome = new HandChromosome();
            newHandChromosome.GenerateRandomJointRotation(hand);
            return newHandChromosome;
        }

        public static HandChromosome RemoveRandomOne(ref List<HandChromosome> chromosomeList)
        {
            int index = Random.Range(0, chromosomeList.Count);
            HandChromosome removedChromosome = chromosomeList[index];
            chromosomeList.RemoveAt(index);
            return removedChromosome;
        }

        /// <summary>
        /// 評価
        /// 局所集団のそれぞれの個体を全て評価する
        /// 物理演算が呼ばれる
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="chromosomeList"></param>
        /// <returns></returns>
        public static int Evaluation(Hand hand, ref List<HandChromosome> chromosomeList, HandChromosome initChromosome, GameObject tangibleObject, GameObject virtualObject, Vector3 initPosition, Quaternion initRotation, float worstScore)
        {
            int cnt = 0;
            foreach (HandChromosome handChromosome in chromosomeList)
            {
                if (handChromosome.score == float.MaxValue)
                {
                    cnt++;
                    handChromosome.EvaluationHand(hand, initChromosome.jointRotations, tangibleObject, virtualObject, initPosition, initRotation, worstScore);
                }
                else
                {
                    Debug.Log("evaluated");
                }
            }
            return cnt;
        }

        /// <summary>
        /// 選択
        /// 親2個体と子2個体から1~3個体を選択する
        /// スコアが小さいほど良い
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <param name="child1"></param>
        /// <param name="child2"></param>
        /// <returns></returns>
        public static List<HandChromosome> Selection(HandChromosome parent1, HandChromosome parent2, HandChromosome child1, HandChromosome child2)
        {
            List<HandChromosome> chromosomeList = new List<HandChromosome>();
            if ((child1.score < parent1.score && child1.score < parent2.score) && (child2.score < parent1.score && child2.score < parent2.score))
            {
                // Case1: $C_1$, $C_2$ともに$P_1$, $P_2$よりも良かった場合
                // $C_1$, $C_2$と適応度の高い親1個体がS'となる
                chromosomeList.Add(child1);
                chromosomeList.Add(child2);
                if (parent1.score < parent2.score)
                {
                    chromosomeList.Add(parent1);
                }
                else
                {
                    chromosomeList.Add(parent2);
                }
            }
            else if ((child1.score > parent1.score && child1.score > parent2.score) && (child2.score > parent1.score && child2.score > parent2.score))
            {
                // Case2:  $C_1$, $C_2$ともに$P_1$, $P_2$よりも悪かった場合
                // 適応度の高い親1個体がS'となる
                if (parent1.score < parent2.score)
                {
                    chromosomeList.Add(parent1);
                }
                else
                {
                    chromosomeList.Add(parent2);
                }
            }
            else if ((child1.score < parent1.score && child1.score < parent2.score) || (child2.score < parent1.score && child2.score < parent2.score))
            {
                // Case3: $C_1$, $C_2$のどちらかが$P_1$, $P_2$よりも良かった場合
                // 適応度の高い子1個体がS'となり，さらにSから1個体を無作為に取り出し追加する
                if (child1.score < child2.score)
                {
                    chromosomeList.Add(child1);
                }
                else
                {
                    chromosomeList.Add(child2);
                }

            }
            else
            {
                // Case4: そのほかの場合
                // 適応度の高い親1個体と適応度の高い子1個体がS'となる
                if (parent1.score < parent2.score)
                {
                    chromosomeList.Add(parent1);
                }
                else
                {
                    chromosomeList.Add(parent2);
                }

                if (child1.score < child2.score)
                {
                    chromosomeList.Add(child1);
                }
                else
                {
                    chromosomeList.Add(child2);
                }
            }
            return chromosomeList;
        }

        /// <summary>
        /// 交叉
        /// </summary>
        /// <param name="parent1"></param>
        /// <param name="parent2"></param>
        /// <returns></returns>
        public static (HandChromosome child1, HandChromosome child2) Crossover(HandChromosome parent1, HandChromosome parent2)
        {
            HandChromosome child1 = new HandChromosome();
            HandChromosome child2 = new HandChromosome();

            child1.jointRotations = CrossoverRotations(parent1, parent2);
            child2.jointRotations = CrossoverRotations(parent1, parent2);

            return (child1, child2);
        }

        static Quaternion[] CrossoverRotations(HandChromosome parent1, HandChromosome parent2)
        {
            Quaternion[] jointRotations = new Quaternion[19];
            for (int i = 0; i < jointRotations.Length; i++)
            {
                if (RandomBool())
                {
                    jointRotations[i] = parent1.jointRotations[i];
                }
                else
                {
                    jointRotations[i] = parent2.jointRotations[i];
                }
            }
            return jointRotations;
        }

        /// <summary>
        /// 突然変異
        /// mutationRateの確率で手の関節の回転を変更する
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="mutationRate"></param>
        /// <param name="handChromosome"></param>
        public static HandChromosome Mutation(Hand hand, float mutationRate, HandChromosome handChromosome)
        {
            HandChromosome randChromosome = new HandChromosome();
            randChromosome.GenerateRandomJointRotation(hand);

            for (int i = 0; i < handChromosome.jointRotations.Length; i++)
            {
                if (Random.Range(0f, 1f) < mutationRate)
                {
                    handChromosome.jointRotations[i] = randChromosome.jointRotations[i];
                }
            }
            return handChromosome;
        }


        public static HandChromosome GenerateNeighbor(Hand hand, HandChromosome handChromosome, float mutationRate, float sd)
        {
            HandChromosome neighborChromosome = new HandChromosome();
            int i = 0;
            foreach (Finger finger in hand.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    Vector3 jointAngle = handChromosome.jointRotations[i].eulerAngles;
                    // 関節を動かす
                    if (Random.Range(0f, 1f) < mutationRate)
                    {
                        for (int j = 0; j < 3; j++)
                        {

                            float z = Gaussian(sd, 0f);
                            //Debug.Log($"jointAngle[{j}]: {jointAngle[j]}, z: {z}");
                            jointAngle[j] = ClampAngle(
                                    jointAngle[j] + z,
                                    JointLimit.GetMinRotation(finger.fingerType, joint.jointType)[j],
                                    JointLimit.GetMaxRotation(finger.fingerType, joint.jointType)[j]
                                );
                        }
                    }

                    neighborChromosome.jointRotations[i] = Quaternion.Euler(jointAngle);
                    i++;
                }
            }

            return neighborChromosome;
        }
    }

}