using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class HandManager : MonoBehaviour
    {
        public Hand hand;

        [SerializeField]
        List<GameObject> fingerObjectList;

        [SerializeField]
        HandType handType;


        // Start is called before the first frame update
        void Start()
        {
            hand = new Hand(fingerObjectList, handType);
        }
    }
}
