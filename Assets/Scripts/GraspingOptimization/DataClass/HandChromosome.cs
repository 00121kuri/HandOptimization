using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEditor;

namespace GraspingOptimization
{
    /// <summary>
    /// 最適化するパラメータ
    /// </summary>
    [System.Serializable]
    public class HandChromosome
    {
        public List<JointGene> jointGeneList;
        public float score;

        public Vector3 resultPosition;
        public Quaternion resultRotation;

        public HandChromosome()
        {
            this.jointGeneList = new List<JointGene>();
            this.score = float.MaxValue;
        }

        // public void SetCurrentJointRotation(Hand hand)
        // {
        //     int i = 0;
        //     foreach (Finger finger in hand.fingerList)
        //     {
        //         foreach (Joint joint in finger.jointList)
        //         {
        //             this.jointRotations[i] = joint.jointObject.transform.localRotation;
        //             i++;
        //         }
        //     }
        // }

        // public void GenerateRandomJointRotation(Hand hand)
        // {
        //     int i = 0;
        //     foreach (Finger finger in hand.fingerList)
        //     {
        //         foreach (Joint joint in finger.jointList)
        //         {
        //             this.jointRotations[i] = GenerateRandomRotation(
        //                     JointLimit.GetMinRotation(finger.fingerType, joint.jointType),
        //                     JointLimit.GetMaxRotation(finger.fingerType, joint.jointType)
        //                 );
        //             i++;
        //         }
        //     }
        // }

        Quaternion GenerateRandomRotation(Vector3 minRotation, Vector3 maxRotation)
        {
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(minRotation.x, maxRotation.x),
                Random.Range(minRotation.y, maxRotation.y),
                Random.Range(minRotation.z, maxRotation.z)
            );
            return randomRotation;
        }

        public float EvaluateChromosomeDiff(HandChromosome otherChromosome)
        {
            float diff = 0;
            // 内積で類似度を求める
            for (int i = 0; i < this.jointGeneList.Count; i++)
            {
                diff += Vector3.Angle(this.jointGeneList[i].localEulerAngles, otherChromosome.jointGeneList[i].localEulerAngles);
            }
            return diff / 180f / this.jointGeneList.Count;
        }

        public void EvaluationHand(
            Hands hands,
            GameObject tangibleObj,
            GameObject virtualObj,
            Vector3 initPosition,
            Quaternion initRotation,
            HandChromosome beforeChromosome,
            HandChromosome initChromosome,
            float weightDistance,
            float weightRotation,
            float weightChromosomeDiff)
        {
            // 初期位置
            int simulateTime = 20;
            int simulateStep = 2;
            hands.SetHandChromosome(beforeChromosome);
            virtualObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
            virtualObj.transform.SetPositionAndRotation(initPosition, initRotation);

            // hands.MoveHandChromosome(this);
            hands.SetHandChromosome(this);

            for (int i = 0; i < simulateTime; i++)
            {
                Physics.Simulate(Time.fixedDeltaTime * simulateStep);
            }
            // 結果
            this.resultPosition = virtualObj.transform.position;
            this.resultRotation = virtualObj.transform.rotation;

            //TODO: 評価関数を決める
            float distance = Vector3.Distance(tangibleObj.transform.position, this.resultPosition);
            float dot = Quaternion.Dot(tangibleObj.transform.rotation, this.resultRotation);
            float initChromosomeDiff = this.EvaluateChromosomeDiff(initChromosome);
            //float angle = Quaternion.Angle(tangibleObj.transform.rotation, this.resultRotation);
            this.score = weightDistance * distance + weightRotation * (1 - dot) + weightChromosomeDiff * initChromosomeDiff;
            //Debug.Log($"distance: {distance}, dot: {(1 - dot) / 2}, initChromosomeDiff: {initChromosomeDiff}, score: {this.score}");
        }

        public HandChromosome GenerateNeighborChromosome(float sigma, float mean, float mutationRate)
        {
            HandChromosome neighborChromosome = new HandChromosome();

            foreach (JointGene jointGene in this.jointGeneList)
            {
                Vector3 jointAngle = jointGene.localEulerAngles;
                // 関節を動かす
                if (Random.Range(0f, 1f) < mutationRate)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        float z = Helper.Gaussian(sigma, mean);
                        //Debug.Log($"jointAngle[{j}]: {jointAngle[j]}, z: {z}");
                        jointAngle[j] = Helper.ClampAngle(
                                jointAngle[j] + z,
                                JointLimit.GetMinRotation(jointGene.fingerType, jointGene.jointType)[j],
                                JointLimit.GetMaxRotation(jointGene.fingerType, jointGene.jointType)[j]
                            );
                    }
                }

                neighborChromosome.jointGeneList.Add(new JointGene(jointGene.jointType, jointGene.fingerType, jointGene.handType, jointAngle));
            }

            return neighborChromosome;
        }
    }

    [System.Serializable]
    public class JointGene
    {
        public HandType handType;
        public FingerType fingerType;
        public JointType jointType;
        public Vector3 localEulerAngles;


        public JointGene(JointType jointType, FingerType fingerType, HandType handType, Vector3 localEulerAngles)
        {
            this.jointType = jointType;
            this.fingerType = fingerType;
            this.handType = handType;
            this.localEulerAngles = localEulerAngles;
        }
    }
}

