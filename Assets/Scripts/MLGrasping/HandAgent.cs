using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using GraspingOptimization;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

namespace GraspingOptimization
{
    public class HandAgent : Agent
    {
        [SerializeField]
        private HandManager handManager;

        private HandPoseReader handPoseReader;

        private Hands hands = new Hands(new List<Hand>());

        [SerializeField]
        private GameObject virtualObject;

        [SerializeField]
        private string sequenceId;
        [SerializeField]
        private string dateTime;

        private int frameCount = 0;

        private Vector3 initialObjectPosition;
        private Quaternion initialObjectRotation;

        private HandChromosome receivedHandChromosome = new HandChromosome();

        private HandPoseData handPoseData;

        private int episodeStep = 0;

        private int maxEpisodeStep = 200;

        private int fixedPerUpdate;
        private int fixedCount = 0;

        void Start()
        {
            hands.hands.Add(handManager.hand);
            handPoseReader = this.GetComponent<HandPoseReader>();
            fixedPerUpdate = (int)Math.Round(1.0 / Time.fixedDeltaTime / Application.targetFrameRate);
            Debug.Log("Fixed per update: " + fixedPerUpdate);
        }
        public override void Initialize()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
            return;
        }

        public override void OnEpisodeBegin()
        {
            // 手のポーズを取得
            frameCount = 0;
            handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            handPoseReader.SetHandPose(handPoseData);
            virtualObject.transform.position = handPoseData.objectData.position;
            virtualObject.transform.rotation = handPoseData.objectData.rotation;
            initialObjectPosition = virtualObject.transform.position;
            initialObjectRotation = virtualObject.transform.rotation;
            virtualObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            virtualObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Debug.Log("Observation collected");
            if (fixedCount < fixedPerUpdate)
            {
                fixedCount++;

            }
            else
            {
                fixedCount = 0;
                UpdateHandPose();
            }

            // トラッキングされた手と物体の情報をそのまま与える
            foreach (HandData handData in handPoseData.handDataList)
            {
                foreach (FingerData fingerData in handData.fingerDataList)
                {
                    foreach (JointData jointData in fingerData.jointDataList)
                    {
                        sensor.AddObservation(jointData.position);
                        sensor.AddObservation(jointData.rotation);
                    }
                }
            }

            // 実物体の位置・姿勢
            sensor.AddObservation(handPoseData.objectData.position);
            sensor.AddObservation(handPoseData.objectData.rotation);

            // 前フレームのAction
            foreach (JointGene jointGene in receivedHandChromosome.jointGeneList)
            {
                sensor.AddObservation(jointGene.localEulerAngles);
            }

            // 仮想物体の位置・姿勢
            sensor.AddObservation(virtualObject.transform.position);
            sensor.AddObservation(virtualObject.transform.rotation);
        }


        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Debug.Log("Action received");
            var actions = actionBuffers.ContinuousActions;

            receivedHandChromosome = hands.GetCurrentHandChromosome();

            for (int i = 0; i < receivedHandChromosome.jointGeneList.Count; i++)
            {
                receivedHandChromosome.jointGeneList[i].localEulerAngles += new Vector3(actions[i * 3], actions[i * 3 + 1], actions[i * 3 + 2]);
            }

            receivedHandChromosome = receivedHandChromosome.ClampChromosomeAngle();

            // 手首の位置は参照データのものを用いる
            handPoseReader.SetWristPose(handPoseData);
            hands.SetHandChromosome(receivedHandChromosome);

            // 十分なステップ数実行する
            // int simulateTime = 20;
            // int simulateStep = 1;
            // for (int i = 0; i < simulateTime; i++)
            // {
            //     Physics.Simulate(Time.fixedDeltaTime * simulateStep);
            // }

            // キャプチャしたデータの位置による
            // if (virtualObject.transform.position.y < 0)
            // {
            //     Debug.Log("Object fell");
            //     EndEpisode();
            // }

            // if (episodeStep > maxEpisodeStep)
            // {
            //     episodeStep = 0;
            //     Debug.Log("Episode step exceeded");
            //     EndEpisode();
            // }

            // 物体の位置が初期位置・姿勢に近くなるように報酬を与える
            float distance = Vector3.Distance(virtualObject.transform.position, initialObjectPosition);
            float dot = Quaternion.Dot(virtualObject.transform.rotation, initialObjectRotation);

            float worstDistance = 0.1f;
            float rewardDistance = worstDistance - distance;
            float reward = 10.0f * rewardDistance + 0.5f * dot;

            if (rewardDistance < 0)
            {
                Debug.Log("Object dropped");
                EndEpisode();
            }
            else
            {
                AddReward(reward);
            }
            // episodeStep++;
        }

        private void UpdateHandPose()
        {
            handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            if (handPoseData == null)
            {
                EndEpisode();
                return;
            }
            else
            {
                frameCount++;
            }
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {

        // }
    }

}