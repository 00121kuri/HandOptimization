using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPosition : MonoBehaviour
{
    private bool isCollision = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionStay(Collision other) {
        // タグがHandCollならば
        if (other.gameObject.CompareTag("HandColl")) {isCollision = true;}
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.CompareTag("HandColl")) {isCollision = false;}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 位置をログに出力する
        if (isCollision) {
            Debug.Log(transform.position.ToString("F5"));
        }
    }

    void Update()
    {
        if (isCollision) {
            Debug.Log("Update: " + transform.position.ToString("F5"));
        }   
    }
}
