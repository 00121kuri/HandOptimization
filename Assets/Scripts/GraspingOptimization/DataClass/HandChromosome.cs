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

        public float distanceScore;
        public float rotationScore;
        public float initChromosomeDiffScore;
        public float inputChromosomeDiffScore;

        public HandChromosome()
        {
            this.jointGeneList = new List<JointGene>();
            this.score = float.MaxValue;
            this.distanceScore = float.MaxValue;
            this.rotationScore = float.MaxValue;
            this.initChromosomeDiffScore = float.MaxValue;
            this.inputChromosomeDiffScore = float.MaxValue;
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
                float dot = Quaternion.Dot(
                    Quaternion.Euler(Helper.ClampVector180(this.jointGeneList[i].localEulerAngles)),
                    Quaternion.Euler(Helper.ClampVector180(otherChromosome.jointGeneList[i].localEulerAngles))
                );
                diff += 1 - dot;
            }
            return diff / this.jointGeneList.Count;
        }

        /// <summary>
        /// 関節の角度の差のトータルを求める (deg)
        /// </summary>
        /// <param name="otherChromosome"></param>
        /// <returns></returns>
        public float EvaluateTotalChromosomeAngleDiff(HandChromosome otherChromosome)
        {
            float totalAngleDiff = 0;
            float angleDiff = 0;
            for (int i = 0; i < this.jointGeneList.Count; i++)
            {
                angleDiff = Quaternion.Angle(
                    Quaternion.Euler(this.jointGeneList[i].localEulerAngles),
                    Quaternion.Euler(otherChromosome.jointGeneList[i].localEulerAngles)
                );
                totalAngleDiff += angleDiff;
            }
            return totalAngleDiff;
        }

        public void EvaluationHand(
            Hands hands,
            GameObject tangibleObj,
            GameObject virtualObj,
            Vector3 initPosition,
            Quaternion initRotation,
            HandChromosome beforeChromosome,
            HandChromosome initChromosome,
            HandChromosome inputChromosome,
            float weightDistance,
            float weightRotation,
            float weightChromosomeDiff,
            float wieghtInputChromosomeDiff)
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
            this.distanceScore = Vector3.Distance(tangibleObj.transform.position, this.resultPosition);
            this.rotationScore = 1 - Quaternion.Dot(tangibleObj.transform.rotation, this.resultRotation);
            this.initChromosomeDiffScore = this.EvaluateChromosomeDiff(initChromosome);
            this.inputChromosomeDiffScore = this.EvaluateChromosomeDiff(inputChromosome);
            if (this.inputChromosomeDiffScore > 1f)
            {
                Debug.Log("inputChromosomeDiffScore > 1f");
                Debug.Log(JsonUtility.ToJson(this));
                Debug.Log(JsonUtility.ToJson(inputChromosome));
            }
            //float angle = Quaternion.Angle(tangibleObj.transform.rotation, this.resultRotation);
            this.score = this.GetTotalScore(weightDistance, weightRotation, weightChromosomeDiff, wieghtInputChromosomeDiff);
            //Debug.Log($"distance: {this.distanceScore}, rotation: {this.rotationScore}, initChromosomeDiff: {this.initChromosomeDiffScore}, inputChromosomeDiff: {this.inputChromosomeDiffScore}, score: {this.score}");
        }

        public float GetTotalScore(
            float weightDistance,
            float weightRotation,
            float weightChromosomeDiff,
            float wieghtInputChromosomeDiff
            )
        {
            float totalScore = 0;
            totalScore += weightDistance * this.distanceScore;
            totalScore += weightRotation * this.rotationScore;
            totalScore += weightChromosomeDiff * this.initChromosomeDiffScore;
            totalScore += wieghtInputChromosomeDiff * this.inputChromosomeDiffScore;
            return totalScore;
        }

        public float GetObjectScore(
            float weightDistance,
            float weightRotation
        )
        {
            return weightDistance * this.distanceScore + weightRotation * this.rotationScore;
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

