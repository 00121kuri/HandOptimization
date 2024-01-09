using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization
{
    public class ShowHandContact : MonoBehaviour
    {
        public Hand hand;
        [SerializeField] private GameObject sphere;
        [SerializeField] private GameObject sphereTarget;
        // private List<GameObject> sphereList = null;
        // private List<GameObject> targetList = null;

        // Start is called before the first frame update
        void Start()
        {
            hand = this.GetComponent<HandManager>().hand;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

        }
    }
}
