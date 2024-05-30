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

        private int frameCount = -1;

        private Vector3 initialObjectPosition;
        private Quaternion initialObjectRotation;

        private HandChromosome receivedHandChromosome = new HandChromosome();

        void Start()
        {
            hands.hands.Add(handManager.hand);
            handPoseReader = this.GetComponent<HandPoseReader>();
        }
        public override void Initialize()
        {
            return;
        }

        public override void OnEpisodeBegin()
        {
            // 手のポーズを取得
            frameCount++;
            HandPoseData handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);
            if (handPoseData == null)
            {
                frameCount = -1;
                return;
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
            // HandChromosome handChromosome = hands.GetCurrentHandChromosome();
            sensor.AddObservation(virtualObject.transform.position);
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

            hands.SetHandChromosome(receivedHandChromosome);

            if (virtualObject.transform.position.y < 0)
            {
                Debug.Log("Object fell");
                AddReward(-1.0f);
                EndEpisode();
            }

            // 物体の位置が初期位置・姿勢に近くなるように報酬を与える
            float distance = Vector3.Distance(virtualObject.transform.position, initialObjectPosition);
            // float dot = 1 - Quaternion.Dot(virtualObject.transform.rotation, initialObjectRotation);

            if (distance > 0.05f)
            {
                EndEpisode();
            }
            else
            {
                AddReward(0.05f - distance);
            }
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {

        // }
    }
}