using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using GraspingOptimization;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using System.Linq;

namespace GraspingOptimization
{
    public class HandAgent : Agent
    {
        [SerializeField]
        private HandManager handManager;

        private HandPoseReader handPoseReader;

        [SerializeField]
        private OptiClientDisplay optiClientDisplay;

        private Hands hands = new Hands(new List<Hand>());

        [SerializeField]
        private GameObject virtualObject;

        private Rigidbody virtualObjectRigidbody;

        [SerializeField]
        private string sequenceId;
        [SerializeField]
        private List<string> dateTimeList;

        private string dateTime;

        private int frameCount = 0;

        private Vector3 initialObjectPosition;
        private Quaternion initialObjectRotation;

        private HandChromosome receivedHandChromosome = new HandChromosome();

        private List<Vector3> actionList = new List<Vector3>();

        private HandPoseData handPoseData;

        private int fixedPerUpdate;
        private int fixedCount = 0;

        private int episodeCount = 0;
        private int droppedEpisodeCount = 0;

        void Start()
        {
            hands.hands.Add(handManager.hand);
            handPoseReader = this.GetComponent<HandPoseReader>();
            virtualObjectRigidbody = virtualObject.GetComponent<Rigidbody>();
            if (optiClientDisplay == null)
            {
                Debug.LogError("OptiClientDisplay is not set.");
            }
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
            episodeCount++;

            Debug.Log("Drop rate: " + (float)droppedEpisodeCount / episodeCount + " (" + droppedEpisodeCount + "/" + episodeCount + ")");
            // 手のポーズを取得
            frameCount = 0;
            dateTime = Helper.GetRandom(dateTimeList);
            handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            handPoseReader.SetHandPose(handPoseData);
            virtualObject.transform.position = handPoseData.objectData.position;
            virtualObject.transform.rotation = handPoseData.objectData.rotation;
            initialObjectPosition = virtualObject.transform.position;
            initialObjectRotation = virtualObject.transform.rotation;
            virtualObjectRigidbody.velocity = Vector3.zero;
            virtualObjectRigidbody.angularVelocity = Vector3.zero;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Debug.Log("Observation collected");
            if (handPoseData == null)
            {
                // Debug.Log("No hand data");
                return;
            }
            optiClientDisplay.UpdateDisplay(sequenceId, dateTime, frameCount);
            foreach (HandData handData in handPoseData.handDataList)
            {
                // 手首の位置・姿勢
                sensor.AddObservation(handData.wristJoint.position);
                sensor.AddObservation(handData.wristJoint.rotation);
                foreach (FingerData fingerData in handData.fingerDataList)
                {
                    foreach (JointData jointData in fingerData.jointDataList)
                    {
                        // 手首からの相対位置・姿勢
                        sensor.AddObservation(Helper.CalculateRelativePosition(jointData.position, handData.wristJoint.position));
                        sensor.AddObservation(Helper.CalculateRelativeRotation(jointData.rotation, handData.wristJoint.rotation));
                    }
                }
            }

            // 実物体の手首からの相対位置・姿勢 
            sensor.AddObservation(Helper.CalculateRelativePosition(handPoseData.objectData.position, handPoseData.handDataList[0].wristJoint.position));
            sensor.AddObservation(Helper.CalculateRelativeRotation(handPoseData.objectData.rotation, handPoseData.handDataList[0].wristJoint.rotation));

            // 前フレームのAction
            foreach (JointGene jointGene in receivedHandChromosome.jointGeneList)
            {
                sensor.AddObservation(jointGene.localEulerAngles);
            }

            // 仮想物体の手首からの相対位置・姿勢
            sensor.AddObservation(Helper.CalculateRelativePosition(virtualObject.transform.position, handPoseData.handDataList[0].wristJoint.position));
            sensor.AddObservation(Helper.CalculateRelativeRotation(virtualObject.transform.rotation, handPoseData.handDataList[0].wristJoint.rotation));

            // 仮想物体の速度
            sensor.AddObservation(virtualObjectRigidbody.velocity);
            sensor.AddObservation(virtualObjectRigidbody.angularVelocity);
        }


        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Debug.Log("Action received");
            var actions = actionBuffers.ContinuousActions;
            actionList.Clear();

            receivedHandChromosome = hands.GetCurrentHandChromosome();

            for (int i = 0; i < receivedHandChromosome.jointGeneList.Count; i++)
            {
                receivedHandChromosome.jointGeneList[i].localEulerAngles += new Vector3(actions[i * 3], actions[i * 3 + 1], actions[i * 3 + 2]);
                actionList.Add(new Vector3(actions[i * 3], actions[i * 3 + 1], actions[i * 3 + 2]));
            }

            receivedHandChromosome = receivedHandChromosome.ClampChromosomeAngle();

            // 手首の位置は参照データのものを用いる
            handPoseReader.SetWristPose(handPoseData);
            hands.SetHandChromosome(receivedHandChromosome);

            // 物体の位置が初期位置・姿勢に近くなるように報酬を与える
            float distance = Vector3.Distance(virtualObject.transform.position, initialObjectPosition);
            float dot = Quaternion.Dot(virtualObject.transform.rotation, initialObjectRotation);
            // actionListの内積の和を計算
            float actionDot = 0.0f;
            foreach (Vector3 action in actionList)
            {
                actionDot += Vector3.Dot(action, action);
            }
            actionDot /= actionList.Count;


            // float stepReward = 1.0f;
            float worstDistance = 0.1f;
            float reward = 10.0f * (-distance) + 1.0f * dot + 0.1f * actionDot;

            if (distance > worstDistance)
            {
                // Debug.Log("Object dropped");
                droppedEpisodeCount++;
                EndEpisode();
                return;
            }
            else
            {
                AddReward(reward);
            }

            if (fixedCount < fixedPerUpdate)
            {
                fixedCount++;
            }
            else
            {
                fixedCount = 0;
                var success = UpdateHandPose();
                if (!success)
                {
                    EndEpisode();
                    return;
                }
            }
        }

        private bool UpdateHandPose()
        {
            handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);

            if (handPoseData == null)
            {
                frameCount = 0;
                return false;
            }
            else
            {
                frameCount++;
            }
            return true;
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {

        // }
    }

}