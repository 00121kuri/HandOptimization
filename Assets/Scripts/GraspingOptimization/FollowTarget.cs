using UnityEngine;
using GraspingOptimization;

namespace GraspingOptimization {
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;  // 追従する目標のTransform
        //public float speed = 1f;  // 追従する速度

        public bool autoUpdate = false;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();  // Rigidbodyコンポーネントを取得
        }

        public void Follow()
        {
            if (target == null) return;  // 目標が設定されていなければ処理をスキップ

            this.transform.localScale = target.transform.localScale;

            /*
            this.transform.position = target.transform.position;
            this.transform.rotation = target.transform.rotation;
            */

            
            if (rb != null) {
                rb.MovePosition(target.position);  // Rigidbodyの位置を新しい位置に更新
                rb.MoveRotation(target.rotation);  // Rigidbodyの回転を新しい回転に更新
            }
            
            
            // うまく追従できない
            //Vector3 direction = (target.position - rb.position);
            //rb.AddForceAtPosition(direction * 2.0, target.position);  // Rigidbodyの位置を新しい位置に更新
        }

        private void FixedUpdate()
        {
            if (autoUpdate) {
                Follow();
            }
        }
    }
}