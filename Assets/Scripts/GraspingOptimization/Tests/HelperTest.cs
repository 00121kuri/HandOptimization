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
        }

    }
}
