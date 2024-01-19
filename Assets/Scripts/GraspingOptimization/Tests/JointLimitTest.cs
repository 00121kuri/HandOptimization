using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GraspingOptimization;

namespace GraapshingOptimization.Tests
{
    public class JointLimitTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ClampMaxAngleTest(
            [Random(-20f, 380f, 10)] float angle,
            [Values] FingerType fingerType,
            [Values] JointType jointType,
            [Values(0, 1, 2)] int axis
            )
        {
            float clampedAngle = Helper.ClampAngle(
                angle,
                JointLimit.GetMinRotation(fingerType, jointType)[axis],
                JointLimit.GetMaxRotation(fingerType, jointType)[axis]
            );
            Assert.True(
                clampedAngle <= JointLimit.GetMaxRotation(fingerType, jointType)[axis]
            );
            if (clampedAngle <= JointLimit.GetMaxRotation(fingerType, jointType)[axis])
            {
                Debug.Log($"angle: {angle}, clampedAngle: {clampedAngle}, max: {JointLimit.GetMaxRotation(fingerType, jointType)[axis]}, min: {JointLimit.GetMinRotation(fingerType, jointType)[axis]}");
            }
        }

        [Test]
        public void ClampMinAngleTest(
            [Random(-20f, 380f, 10)] float angle,
            [Values] FingerType fingerType,
            [Values] JointType jointType,
            [Values(0, 1, 2)] int axis
            )
        {
            Assert.True(
                Helper.ClampAngle(
                    angle,
                    JointLimit.GetMinRotation(fingerType, jointType)[axis],
                    JointLimit.GetMaxRotation(fingerType, jointType)[axis]
                ) >= JointLimit.GetMinRotation(fingerType, jointType)[axis]
            );
        }
    }
}