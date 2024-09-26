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

        private List<SequenceMetadata> sequenceMetadataList = new List<SequenceMetadata>();

        [SerializeField]
        private int framesPerEpisode = 1; // 1エピソードで学習するフレーム数
        private int currentFramesPerEpisode = 0; // 現在のエピソードのフレーム数

        [SerializeField]
        private int stepsPerOneFrame = 10; // 1フレームあたりのステップ数

        private int currentStepsPerOneFrame = 0; // 現在のフレームのステップ数

        private int frameCount = 0;

        private Vector3 initialObjectPosition;
        private Quaternion initialObjectRotation;

        private HandChromosome receivedHandChromosome = new HandChromosome();

        // private HandChromosome prevHandChromosome = new HandChromosome();

        private HandPoseData handPoseData;

        private int fixedPerUpdate; // 1UpdateあたりのFixedUpdateの回数
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

            foreach (string dateTime in dateTimeList)
            {
                SequenceMetadata sequenceMetadata = new SequenceMetadata();
                sequenceMetadata.Load(sequenceId, dateTime);
                sequenceMetadataList.Add(sequenceMetadata);
            }
            if (sequenceMetadataList.Count == 0)
            {
                Debug.LogError("No sequence metadata");
            }
        }
        public override void Initialize()
        {
            Physics.simulationMode = SimulationMode.FixedUpdate;
            return;
        }

        public override void OnEpisodeBegin()
        {
            episodeCount++;

            currentFramesPerEpisode = 0;
            currentStepsPerOneFrame = 0;
            fixedCount = 0;

            Debug.Log("Drop rate: " + (float)droppedEpisodeCount / episodeCount + " (" + droppedEpisodeCount + "/" + episodeCount + ")");
            // シーケンスと開始フレームをランダムに選択
            var sequenceMetadata = Helper.GetRandom(sequenceMetadataList);
            frameCount = UnityEngine.Random.Range(0, sequenceMetadata.totalFrameCount - framesPerEpisode);
            dateTime = sequenceMetadata.dateTime;
            sequenceId = sequenceMetadata.sequenceId;

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

            receivedHandChromosome = hands.GetCurrentHandChromosome();
            // prevHandChromosome = receivedHandChromosome.DeepCopy();

            for (int i = 0; i < receivedHandChromosome.jointGeneList.Count; i++)
            {
                receivedHandChromosome.jointGeneList[i].localEulerAngles += new Vector3(actions[i * 3], actions[i * 3 + 1], actions[i * 3 + 2]);
            }

            receivedHandChromosome = receivedHandChromosome.ClampChromosomeAngle();

            // 手首の位置は参照データのものを用いる
            handPoseReader.SetWristPose(handPoseData);
            hands.SetHandChromosome(receivedHandChromosome);

            // 物体の位置が初期位置・姿勢に近くなるように報酬を与える
            float distance = Vector3.Distance(virtualObject.transform.position, initialObjectPosition);
            float dot = Quaternion.Dot(virtualObject.transform.rotation, initialObjectRotation);
            // actionListの内積の和を計算
            // float actionDot = 0.0f;
            // for (int i = 0; i < receivedHandChromosome.jointGeneList.Count; i++)
            // {
            //     Quaternion q1 = Quaternion.Euler(receivedHandChromosome.jointGeneList[i].localEulerAngles);
            //     Quaternion q2 = Quaternion.Euler(prevHandChromosome.jointGeneList[i].localEulerAngles);
            //     actionDot += Mathf.Abs(Quaternion.Dot(q1, q2));
            // }
            // actionDot /= receivedHandChromosome.jointGeneList.Count;


            // float stepReward = 1.0f;
            float worstDistance = 0.1f;
            float reward = 10.0f * (-distance) + 0.5f * dot;

            //Debug.Log("Reward: " + reward + " Distance: " + distance + " Dot: " + dot + " ActionDot: " + actionDot);

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

            bool end = UpdateHandPose();
            if (end)
            {
                EndEpisode();
                return;
            }
        }


        /// <summary>
        /// 次の状態に進み，必要があれば手のポーズを更新する
        /// エピソードの終了条件を満たした場合はtrueを返す
        /// </summary>
        /// <returns></returns>
        private bool UpdateHandPose()
        {
            if (fixedCount < fixedPerUpdate)
            {
                fixedCount++;
            }
            else
            {
                fixedCount = 0;

                if (currentStepsPerOneFrame < stepsPerOneFrame)
                {
                    currentStepsPerOneFrame++;
                }
                else
                {
                    // 1フレームのステップ数に達した場合
                    currentStepsPerOneFrame = 0;

                    SequenceMetadata sequenceMetadata = sequenceMetadataList.Find(x => x._id == $"{sequenceId}-{dateTime}");

                    // 次のフレームに進む
                    if (currentFramesPerEpisode < framesPerEpisode - 1 && frameCount < sequenceMetadata.totalFrameCount)
                    {
                        frameCount++;
                        currentFramesPerEpisode++;

                        handPoseData = handPoseReader.ReadHandPoseDataFromDB(sequenceId, dateTime, frameCount);
                    }
                    else
                    {
                        // エピソードの最後まで到達した場合
                        return true;
                    }
                }
            }
            return false;
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {

        // }
    }

}