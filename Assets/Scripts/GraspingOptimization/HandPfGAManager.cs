using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;
using static GraspingOptimization.Helper;
using Unity.VisualScripting;
using System;
using System.Threading;
using System.Threading.Tasks;
//using static GraspingOptimization.HandPfGA;

namespace GraspingOptimization
{
    public class HandPfGAManager : MonoBehaviour
    {
        Hand hand;
        [SerializeField]
        GameObject handObject;

        public GameObject tangibleObj;
        public GameObject virtualObj;

        [SerializeField]
        float mutationRate;

        [SerializeField]
        float targetScore;

        [SerializeField]
        bool isRunning = false;
        [SerializeField]
        bool isCollision = false;
        [SerializeField]
        int maxGeneration;

        [SerializeField]
        FollowTarget wist;
        [SerializeField]
        FollowTarget elbow;

        HandChromosome initChromosome;

        [SerializeField]
        List<HandChromosome> handChromosomeList = new List<HandChromosome>();

        // Start is called before the first frame update
        void Start()
        {
            hand = handObject.GetComponent<HandManager>().hand;
            // 評価関数によって変える
            //targetScore = targetScore * targetScore;
            Physics.autoSimulation = false;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
        }

        // Update is called once per frame
        void Update()
        {
            if ((!isRunning) && isCollision) {
                // 遺伝的アルゴリズムによる最適化
                isCollision = false;
                //await Task.Run(() => StartGA());
                StartGA();
            }

            if (!isRunning) {
                // LeapMotionの情報に合わせる
                wist.Follow();
                elbow.Follow();
                foreach (Finger finger in hand.fingerList) {
                    foreach (Joint joint in finger.jointList) {
                        joint.followTarget.Follow();
                    }
                }
                Physics.Simulate(Time.fixedDeltaTime);
            }

            // Test
            /*
            float sigma = 10f;
            float ave = 45f;
            float z = Gaussian(sigma, ave);
            Debug.Log($"Gaussian: {z}");
            FileLog.AppendLog($"log/gaussian-s-{(int)sigma}-ave-{(int)ave}.csv", $"{z}\n");
            */
        }

        void StartGA() {
            int total_cnt = 0; // 評価回数
            string dt = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string logfile = $"log/ga-log-{dt}.csv";
            FileLog.AppendLog(logfile, "Generation,Score,AveScore\n");
            // 初期化
            Debug.Log("init PfGA");
            // 位置を合わせる
            virtualObj.transform.SetPositionAndRotation(tangibleObj.transform.position, tangibleObj.transform.rotation);
            Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);
            isRunning = true;
            

            Vector3 initPosition = virtualObj.transform.position;
            Quaternion initRotation = virtualObj.transform.rotation;

            Vector3 minScorePosition = Vector3.zero;
            Quaternion minScoreRotation = Quaternion.identity;


            initChromosome = HandPfGA.Init(hand);
            handChromosomeList.Clear();
            handChromosomeList.Add(initChromosome);
            float minScore = float.MaxValue;
            //int minScoreIndex = 0;

            List<HandChromosome> oneGeneratinon = new List<HandChromosome>();
            
            float generation = 0;

            while (minScore > targetScore) {
                // 初期値
                minScore = float.MaxValue;
                //minScoreIndex = 0;
                oneGeneratinon.Clear();
                
                generation++;
                
                // 常にhandChromosomeListは1個体のみ
                oneGeneratinon.Add(handChromosomeList[0]);
                // 生成
                HandChromosome child = HandPfGA.GenerateNeighbor(hand, handChromosomeList[0], mutationRate, 5f);
                oneGeneratinon.Add(child);
                // 評価
                total_cnt += HandPfGA.Evaluation(hand, ref oneGeneratinon, initChromosome, tangibleObj, virtualObj, initPosition, initRotation, 0.05f);
                // 選択
                float totalScore = oneGeneratinon[0].score + oneGeneratinon[1].score;
                if (oneGeneratinon[0].score < oneGeneratinon[1].score) {
                    handChromosomeList[0] = oneGeneratinon[0];
                    minScore = oneGeneratinon[0].score;
                } else {
                    handChromosomeList[0] = oneGeneratinon[1];
                    minScore = oneGeneratinon[1].score;
                }


                // 2個体以下のとき
                /*
                while (handChromosomeList.Count < 2) {
                    // 1個体ランダムに追加する
                    handChromosomeList.Add(HandPfGA.GenerateRandom(hand));
                }

                
                // 2個体を選ぶ
                HandChromosome parent1 = HandPfGA.RemoveRandomOne(ref handChromosomeList);
                HandChromosome parent2 = HandPfGA.RemoveRandomOne(ref handChromosomeList);

                oneGeneratinon.Add(parent1);
                oneGeneratinon.Add(parent2);
                
                // 交叉
                var children = HandPfGA.Crossover(parent1, parent2);
                HandChromosome child1 = children.child1;
                oneGeneratinon.Add(child1);
                HandChromosome child2 = children.child2;
                oneGeneratinon.Add(child2);

                // 突然変異
                if (RandomBool()) {
                    child1 = HandPfGA.Mutation(hand, mutationRate, child1);
                }

                // 評価
                total_cnt += HandPfGA.Evaluation(hand, ref oneGeneratinon, initChromosome, tangibleObj, virtualObj, initPosition, initRotation);
                
                float totalScore = 0;
                foreach(HandChromosome item in oneGeneratinon) {
                    totalScore += item.score;
                }

                // 選択
                oneGeneratinon = HandPfGA.Selection(parent1, parent2, child1, child2);
                
                
                // oneGenerationを集団に結合
                foreach(HandChromosome item in oneGeneratinon) {
                    handChromosomeList.Add(item);
                    totalScore += item.score;
                }
                */

                
                // 最も良かった結果を求める
                /*
                for (int i = 0; i < handChromosomeList.Count; i++) {
                    if (minScore > handChromosomeList[i].score) {
                        minScore = handChromosomeList[minScoreIndex].score;
                        minScoreIndex = i;
                    }
                }
                */

                minScorePosition = handChromosomeList[0].resultPosition;
                minScoreRotation = handChromosomeList[0].resultRotation;

                Debug.Log("Gen: " + generation
                        + ", Population: " + handChromosomeList.Count
                        + ", minScore: " + minScore);
                Debug.Log("virtualObj Transform Position" + virtualObj.transform.position);

                // ファイルに出力
                FileLog.AppendLog(logfile, $"{generation},{minScore},{totalScore/oneGeneratinon.Count}\n");

                if (generation > maxGeneration) {
                    Debug.Log("Cannot find Ans");
                    break;
                }
            }
            // 最も良かった結果を反映する
            hand.SetJointRotation(handChromosomeList[0].jointRotations);
            virtualObj.transform.SetPositionAndRotation(minScorePosition, minScoreRotation);
            Debug.Log("init position" + initPosition);
            //isRunning = false;
            //Time.timeScale = 0;
            Debug.Log($"total cnt: {total_cnt}");

            Invoke(nameof(RunPhysics), 2f);
        }

        void RunPhysics() {
            Time.timeScale = 1;
            Physics.autoSimulation = true;
            Debug.Log("PhysicsManager: Physics.autoSimulation = " + Physics.autoSimulation);
        }
    }
}

