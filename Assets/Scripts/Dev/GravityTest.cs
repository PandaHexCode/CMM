using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GravityTest : MonoBehaviour{
    /*and other tests*/

    public MeshFilter mesh;
    public GameObject test;

    private void OnEnable(){
        int i = 0;

        foreach (Vector3 vert in mesh.mesh.vertices){
            if (i == 4)
                i = 0;
            if (i == 0){
                Vector3 pos = GetVertexWorldPosition(vert, this.mesh.transform);
                Debug.Log(pos.x + "," + pos.y + "," + pos.z);
                Instantiate(test);
                test.transform.position = pos;
            }
            i++;
        }
    }

    private void Update(){
       

    }

    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner){
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }

}
