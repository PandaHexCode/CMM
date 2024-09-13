using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlock : MonoBehaviour{

    public int rotateSpeed = 20;
    private Transform _transform;

    private void Awake(){
        this._transform = this.transform;
    }

    private void Update(){
        this._transform.Rotate(0, 0, this.rotateSpeed * Time.deltaTime);
    }

}
