using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using UnityEditor;

namespace GraspingOptimization
{
    /// <summary>
    /// 遺伝的アルゴリズムの染色体
    /// </summary>
    /// 
    [System.Serializable]
    public class HandChromosome
    {
        public Quaternion[] jointRotations;
        public float score;

        public Vector3 resultPosition;
        public Quaternion resultRotation;

        public HandChromosome()
        {
            this.jointRotations = new Quaternion[19];
            this.score = float.MaxValue;
        }

        public void SetCurrentJointRotation(Hand hand)
        {
            int i = 0;
            foreach (Finger finger in hand.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    this.jointRotations[i] = joint.jointObject.transform.localRotation;
                    i++;
                }
            }
        }

        public void GenerateRandomJointRotation(Hand hand)
        {
            int i = 0;
            foreach (Finger finger in hand.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    this.jointRotations[i] = GenerateRandomRotation(
                            Setting.GetMinRotation(finger.fingerType, joint.jointType),
                            Setting.GetMaxRotation(finger.fingerType, joint.jointType)
                        );
                    i++;
                }
            }
        }

        Quaternion GenerateRandomRotation(Vector3 minRotation, Vector3 maxRotation)
        {
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(minRotation.x, maxRotation.x),
                Random.Range(minRotation.y, maxRotation.y),
                Random.Range(minRotation.z, maxRotation.z)
            );
            return randomRotation;
        }

        public void EvaluationHand(Hand hand, Quaternion[] initJointRotations, GameObject tangibleObj, GameObject virtualObj, Vector3 initPosition, Quaternion initRotation, float worstScore)
        {
            // 初期位置
            int simulateTime = 20;
            int simulateStep = 2;
            hand.SetJointRotation(initJointRotations);
            virtualObj.GetComponent<Rigidbody>().velocity = Vector3.zero;
            virtualObj.transform.SetPositionAndRotation(initPosition, initRotation);

            hand.MoveJointRotation(this.jointRotations);

            for (int i = 0; i < simulateTime; i++)
            {
                // 指を動かす
                //hand.MoveStepJointRotation(initJointRotations, this.jointRotations, i/simulateTime);
                Physics.Simulate(Time.fixedDeltaTime * simulateStep);
            }
            // 結果
            this.resultPosition = virtualObj.transform.position;
            this.resultRotation = virtualObj.transform.rotation;

            //TODO: 評価関数を決める
            float distance = Vector3.Distance(tangibleObj.transform.position, this.resultPosition);
            float dot = Quaternion.Dot(tangibleObj.transform.rotation, this.resultRotation);
            float angle = Quaternion.Angle(tangibleObj.transform.rotation, this.resultRotation);
            this.score = distance + 0.2f * ((1 - dot) / 2);
            Debug.Log($"distance: {distance}, dot: {(1 - dot) / 2}, angle: {angle}, score: {this.score}");
        }
    }
}

