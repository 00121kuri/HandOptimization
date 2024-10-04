using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class ShowHnadForce : MonoBehaviour
    {
        private Hand hand;
        // private List<GameObject> lineList = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            hand = this.GetComponent<HandManager>().hand;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 forceTotal = Vector3.zero;
            int cnt = 0;
            foreach (Finger finger in hand.fingerList)
            {
                foreach (Joint joint in finger.jointList)
                {
                    if (joint.isCollision())
                    {
                        forceTotal += joint.jointManager.currentCollision.impulse;
                        joint.jointManager.showContact();
                        cnt++;
                    }

                }
            }
            Debug.Log("cnt: " + cnt + ", Force Total: " + forceTotal.ToString("F6"));
        }
    }
}