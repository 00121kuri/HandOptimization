using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GraspingOptimization
{
    [System.Serializable]
    public class Contact {
        public GameObject contactPointObject;
        public Vector3 position;
        public Vector3 normal;
        public float force;
        public float separation;

        public Contact(GameObject contactPointObject, Vector3 position, Vector3 normal, float force, float separation) {
            this.contactPointObject = contactPointObject;
            this.position = position;
            this.normal = normal;
            this.force = force;
            this.separation = separation;
        }

        public Vector3 targetPosition() {
            return this.position - this.normal * this.separation;
        }

        public GameObject showContact(GameObject spherePrefab) {
            GameObject sphere = GameObject.Instantiate(spherePrefab);
            sphere.transform.position = this.position;
            return sphere;
        }

        public GameObject showTarget(GameObject spherePrefab) {
            GameObject sphere = GameObject.Instantiate(spherePrefab);
            sphere.transform.position = this.targetPosition();
            return sphere;
        }

        public GameObject showForce(GameObject linePrefab) {
            GameObject line = GameObject.Instantiate(linePrefab);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, this.position);
            lineRenderer.SetPosition(1, this.position + this.normal * this.force);
            return line;
        }
    }
}

