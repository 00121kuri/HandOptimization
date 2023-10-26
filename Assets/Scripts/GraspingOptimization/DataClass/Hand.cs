using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    /// <summary>
    /// Hand class
    /// 手の情報を格納するクラス
    /// - fingerList: 指のリスト
    /// - jointRotation: 関節の回転角度の配列
    /// - score: 評価値
    /// </summary>
    [System.Serializable]
    public class Hand
    {
        public List<Finger> fingerList;
        public HandType handType;

        public GameObject elbowObject;
        public GameObject wristObject;

        public Hand(List<GameObject> fingerObjectList, HandType handType, GameObject elbowObject, GameObject wristObject)
        {
            this.fingerList = new List<Finger>();
            foreach (GameObject fingerObject in fingerObjectList)
            {
                if (fingerObject.GetComponent<FingerManager>() == null)
                {
                    continue;
                }
                this.fingerList.Add(fingerObject.GetComponent<FingerManager>().finger);
            }
            this.handType = handType;

            this.elbowObject = elbowObject;
            this.wristObject = wristObject;
        }

        public void SetJointRotation(Quaternion[] jointRotations)
        {
            int i = 0;
            foreach (Finger finger in this.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    joint.jointObject.transform.localRotation = jointRotations[i];
                    i++;
                }
            }
        }

        public void MoveStepJointRotation(Quaternion[] initRotation, Quaternion[] jointRotations, float interpolant)
        {
            int i = 0;
            foreach (Finger finger in this.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    Quaternion stepRotation = Quaternion.Slerp(initRotation[i], jointRotations[i], interpolant);
                    //joint.rb.MoveRotation(stepRotation);
                    joint.jointObject.transform.localRotation = stepRotation;
                    i++;
                }
            }
        }

        public void MoveJointRotation(Quaternion[] jointRotations)
        {
            int i = 0;
            foreach (Finger finger in this.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    // うまく動かない
                    //joint.rb.velocity = Vector3.zero;
                    //joint.rb.MoveRotation(jointRotations[i]);
                    joint.jointObject.transform.localRotation = jointRotations[i];
                    i++;
                }
            }
        }
    }
}
