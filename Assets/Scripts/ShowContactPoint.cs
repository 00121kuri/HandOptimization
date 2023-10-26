using System.Collections.Generic;
using UnityEngine;

public class ShowContactPoint : MonoBehaviour
{
    public GameObject spherePrefab;
    public GameObject godObjectPrefab;
    public GameObject linePrefab;  // LineRenderer用のPrefab
    public float sphereSize = 0.1f;
    public float normalLength = 0.1f;  // 法線の長さ
    public float normalWidth = 0.005f;  // 法線の太さ
    //public bool isEnabled = true;  // 追加されたブール型の変数
    private List<GameObject> currentSpheres = new List<GameObject>();
    private List<GameObject> currentLines = new List<GameObject>();  // 法線を描く線を保持するリスト
    private Rigidbody rb;

    void Start()
    {

    }

    void OnCollisionStay(Collision collision)
    {
        //if (!isEnabled) return;  // 追加された行：isEnabledがfalseの場合、関数から抜け出す

        // 全ての現在の球体と線を削除
        foreach (GameObject sphere in currentSpheres)
        {
            Destroy(sphere);
        }
        foreach (GameObject line in currentLines)
        {
            Destroy(line);
        }
        currentSpheres.Clear();
        currentLines.Clear();

        // 衝突力を接触点の数で割ります。
        float forcePerContact = collision.impulse.magnitude / collision.contactCount;

        // 各接触点に新しい球体を作成し、法線を描く
        foreach (ContactPoint contact in collision.contacts)
        {
            GameObject sphere = Instantiate(spherePrefab);
            sphere.transform.position = contact.point;
            sphere.transform.localScale = Vector3.one * sphereSize;
            currentSpheres.Add(sphere);

            GameObject line = Instantiate(linePrefab);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.widthMultiplier = normalWidth;
            lineRenderer.SetPosition(0, contact.point);
            lineRenderer.SetPosition(1, contact.point + contact.normal * forcePerContact * normalLength); // 法線の長さを力の大きさに反映
            currentLines.Add(line);

            // コライダーの距離を表示
            //Debug.Log("Distance: " + contact.separation);
            GameObject godObject = Instantiate(godObjectPrefab);
            godObject.transform.position = contact.point  - contact.normal * contact.separation;
            godObject.transform.localScale = Vector3.one * sphereSize;
            currentSpheres.Add(godObject);
        }
    }

    void LateUpdate() {

    }

    void OnCollisionExit(Collision collision)
    {
        //if (!isEnabled) return;  // 追加された行：isEnabledがfalseの場合、関数から抜け出す

        // 全ての現在の球体と線を削除
        foreach (GameObject sphere in currentSpheres)
        {
            Destroy(sphere);
        }
        foreach (GameObject line in currentLines)
        {
            Destroy(line);
        }
        currentSpheres.Clear();
        currentLines.Clear();
    }
}
