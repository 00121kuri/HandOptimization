using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GraspingOptimization;

namespace GraapshingOptimization.Tests
{
    public class MathTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void DivideTest()
        {
            Assert.AreEqual(0.1 / 10, 0.01);
        }

    }
}