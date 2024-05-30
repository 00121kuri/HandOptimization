using UnityEngine;
using UnityEngine.TestTools;
using GraspingOptimization;
using NUnit.Framework;
using System;

namespace GraapshingOptimization.Tests
{
    public class ChromosomeTest
    {

        [Test]
        // 微妙に値が異なるので，テストが通らない
        public void OneChromosomeDiffTest1()
        {
            HandChromosome handChromosome1 = new HandChromosome();
            handChromosome1.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(-10, 0, 0)));
            HandChromosome handChromosome2 = new HandChromosome();
            handChromosome2.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(10, 0, 0)));

            Debug.Log($"joint count: {handChromosome1.jointGeneList.Count}");
            Assert.AreEqual(
                handChromosome1.EvaluateChromosomeDiff(handChromosome2),
                1 - Quaternion.Dot(Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 20, 0)));
        }

        [Test]
        public void OneChromosomeDiffTest2()
        {
            HandChromosome handChromosome2 = new HandChromosome();
            handChromosome2.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(10, 0, 0)));
            HandChromosome handChromosome3 = new HandChromosome();
            handChromosome3.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(30, 0, 0)));

            Debug.Log($"joint count: {handChromosome3.jointGeneList.Count}");
            Assert.AreEqual(
                handChromosome3.EvaluateChromosomeDiff(handChromosome2),
                1 - Quaternion.Dot(Quaternion.Euler(0, 0, 0), Quaternion.Euler(20, 0, 0)));
        }

        [Test]
        public void OneChromosomeDiffSameTest()
        {
            HandChromosome handChromosome1 = new HandChromosome();
            handChromosome1.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(-10, 0, 0)));

            Assert.AreEqual(
                handChromosome1.EvaluateChromosomeDiff(handChromosome1),
                1 - Math.Cos(0f * Mathf.Deg2Rad));

            HandChromosome handChromosome2 = new HandChromosome();
            handChromosome2.jointGeneList.Add(new JointGene(JointType.Proximal, FingerType.Thumb, HandType.Right, new Vector3(350, 0, 0)));

            Assert.AreEqual(
                handChromosome1.EvaluateChromosomeDiff(handChromosome2),
                1 - Math.Cos(0f * Mathf.Deg2Rad));
        }
    }
}