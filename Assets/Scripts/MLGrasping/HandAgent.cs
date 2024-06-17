using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using GraspingOptimization;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

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

        void Start()
        {
            hands.hands.Add(handManager.hand);
            handPoseReader = this.GetComponent<HandPoseReader>();
        }
        public override void Initialize()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
            return;
        }

        public override void OnEpisodeBegin()
        {
            // 手のポーズを取得
            handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            if (handPoseData == null)
            {
                frameCount = 0;
                return;
            }
            else
            {
                frameCount++;
            }

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
            // handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            if (handPoseData == null)
            {
                return;
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
            sensor.AddObservation(handPoseData.objectData.position);
            sensor.AddObservation(handPoseData.objectData.rotation);

            // 現在の手と物体の情報を与える
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
            // handPoseReader.SetWristPose(handPoseData);
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

            if (episodeStep > maxEpisodeStep)
            {
                episodeStep = 0;
                Debug.Log("Episode step exceeded");
                EndEpisode();
            }

            // 物体の位置が初期位置・姿勢に近くなるように報酬を与える
            float distance = Vector3.Distance(virtualObject.transform.position, initialObjectPosition);
            // float dot = 1 - Quaternion.Dot(virtualObject.transform.rotation, initialObjectRotation);

            if (distance > 0.05f)
            {
                Debug.Log("Object dropped");
                EndEpisode();
            }
            else
            {
                AddReward(0.05f - distance);
            }
            episodeStep++;
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {

        // }
    }
}