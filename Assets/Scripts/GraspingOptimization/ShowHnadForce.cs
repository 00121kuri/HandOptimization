using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization {
    public class ShowHnadForce : MonoBehaviour
    {
        [SerializeField]
        private GameObject linePrefab;
        private Hand hand;
        private List<GameObject> lineList = new List<GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            hand = this.GetComponent<HandManager>().hand;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 forceTotal = Vector3.zero;
            foreach (GameObject line in lineList) {
                GameObject.Destroy(line);
            }
            lineList.Clear();
            int cnt = 0;
            foreach (Finger finger in hand.fingerList) {
                foreach (Joint joint in finger.jointList) {
                    foreach (Contact contact in joint.contactList) {
                        lineList.Add(contact.showForce(linePrefab));
                        Debug.Log((contact.force * contact.normal).ToString("F6"));
                        forceTotal += contact.force * contact.normal;
                        cnt++;
                    }
                }
            }
            Debug.Log("cnt: " + cnt + ", Force Total: " + forceTotal.ToString("F6"));
        }
    }
}