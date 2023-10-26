using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GraspingOptimization;

namespace GraspingOptimization
{
    public static class Setting
    {
        /// <summary>
        /// 関節可動域の最大値を返す
        /// </summary>
        /// <param name="fingerType"></param>
        /// <param name="jointType"></param>
        /// <returns></returns>
        public static Vector3 GetMaxRotation(FingerType fingerType, JointType jointType)
        {
            Vector3 maxRotation = new Vector3(0, 0, 0);
            if (jointType == JointType.Meta)
            {
                // Metaの場合はデフォルトの値を返す
                switch (fingerType)
                {
                    case FingerType.Thumb: // なし
                        return new Vector3(0, 0, 0);
                    case FingerType.Index:
                        return new Vector3(0f, -9.666f, -8.531f);
                    case FingerType.Middle:
                        return new Vector3(-8.543f, -0.419f, -8.627f);
                    case FingerType.Ring:
                        return new Vector3(-11.442f, 8.786f, -8.705f);
                    case FingerType.Pinky:
                        return new Vector3(-19.707f, 17.302f, -6.456f);
                }
            }
            if (fingerType == FingerType.Thumb)
            {
                switch (jointType)
                {
                    // 親指の場合
                    case JointType.Proximal:
                        return new Vector3(60, 30, 10);
                    case JointType.Intermediate:
                        return new Vector3(0, 0, 10);
                    case JointType.Distal:
                        return new Vector3(0, 0, 10);
                }
            }
            else
            {
                switch (jointType)
                {
                    // 親指以外の場合
                    case JointType.Meta:
                        return new Vector3(0, 0, 0);
                    case JointType.Proximal:
                        return new Vector3(0, 10, 45);
                    case JointType.Intermediate:
                        return new Vector3(0, 0, 0);
                    case JointType.Distal:
                        return new Vector3(0, 0, 0);
                }
            }
            return maxRotation;
        }

        public static Vector3 GetMinRotation(FingerType fingerType, JointType jointType)
        {
            Vector3 minRotation = new Vector3(0, 0, 0);
            if (jointType == JointType.Meta)
            {
                switch (fingerType)
                {
                    // Metaの場合はデフォルトの値を返す
                    case FingerType.Thumb: // なし
                        return new Vector3(0, 0, 0);
                    case FingerType.Index:
                        return new Vector3(0f, -9.666f, -8.531f);
                    case FingerType.Middle:
                        return new Vector3(-8.543f, -0.419f, -8.627f);
                    case FingerType.Ring:
                        return new Vector3(-11.442f, 8.786f, -8.705f);
                    case FingerType.Pinky:
                        return new Vector3(-19.707f, 17.302f, -6.456f);
                }
            }
            if (fingerType == FingerType.Thumb)
            {
                switch (jointType)
                {
                    // 親指の場合
                    case JointType.Proximal:
                        return new Vector3(50, -90, -60);
                    case JointType.Intermediate:
                        return new Vector3(0, 0, -60);
                    case JointType.Distal:
                        return new Vector3(0, 0, -80);
                }
            }
            else
            {
                switch (jointType)
                {
                    // 親指以外の場合
                    case JointType.Proximal:
                        return new Vector3(0, -10, -90);
                    case JointType.Intermediate:
                        return new Vector3(0, 0, -100);
                    case JointType.Distal:
                        return new Vector3(0, 0, -80);
                }
            }
            return minRotation;
        }
    }
}
