using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GraspingOptimization;

namespace GraapshingOptimization.Tests
{
    public class HelperTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void ClampAngleTest()
        {
            Assert.AreEqual(Helper.ClampAngle(0f, -10f, 10f), 0f);
            Assert.AreEqual(Helper.ClampAngle(20f, -10f, 10f), 10f);
            Assert.AreEqual(Helper.ClampAngle(-20f, -10f, 10f), -10f);
        }

        [Test]
        public void ClampAngle0to360()
        {
            Assert.AreEqual(Helper.ClampAngle(170f, -10f, 10f), 10f);
            //Assert.AreEqual(Helper.ClampAngle(180f, -10f, 10f), 10f);
            Assert.AreEqual(Helper.ClampAngle(190f, -10f, 10f), -10f);
        }

        [Test]
        public void ClampAngleOver360()
        {
            Assert.AreEqual(Helper.ClampAngle(360f, -10f, 10f), 0f);
            Assert.AreEqual(Helper.ClampAngle(370f, -10f, 10f), 10f);
            Assert.AreEqual(Helper.ClampAngle(380f, -10f, 10f), 10f);
        }

    }
}
