using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexCollider : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    List<GameObject> collList = new List<GameObject>();
    

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log(vertices[i]);
            // コライダーを追加
            collList.Add(new GameObject("collider_" + i.ToString()));
            collList[i].AddComponent<SphereCollider>();
            
            // コライダーの半径を調節
            collList[i].GetComponent<SphereCollider>().radius = 0.1f;
            // コライダーの位置を頂点位置に移動
            collList[i].transform.position = transform.TransformPoint(vertices[i]);

            collList[i].transform.parent = this.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
