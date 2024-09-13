using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorRotate : MonoBehaviour{

    public bool arrowRotate = false;
    public bool flipX = false;
    public bool extraTrans = false;

    public void Rotate(){
        if (this.arrowRotate){
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, this.transform.eulerAngles.z - 30);
            return;
        }
        if (this.flipX)
            GetComponent<SpriteRenderer>().flipY = false;

        switch (this.transform.eulerAngles.z){
            case 0:
                if (this.extraTrans)
                    this.transform.Translate(0, -0.5f, 0);
                this.transform.eulerAngles = new Vector3(0, 0, -90);
               
                // this.transform.position = this.transform.position + new Vector3(1, 0, 0);
                break;
            case -90:
            case 270:
                if (this.flipX)
                    GetComponent<SpriteRenderer>().flipY = true;
                this.transform.eulerAngles = new Vector3(0, 0, 180);
                if (this.extraTrans)
                    this.transform.Translate(0, 0.5f, 0);
                //this.transform.position = this.transform.position + new Vector3(0, -1, 0);
                break;
            case 180:
                this.transform.eulerAngles = new Vector3(0, 0, 0);
                if (this.extraTrans)
                    this.transform.Translate(0, 0.5f, 0);
                this.transform.eulerAngles = new Vector3(0, 0, 90);
                // this.transform.position = this.transform.position + new Vector3(-1, 0, 0);
                break;
            case 90:
                this.transform.eulerAngles = new Vector3(0, 0, 0);
                if (this.extraTrans)
                    this.transform.Translate(0, 0.5f, 0);
                // this.transform.position = this.transform.position + new Vector3(0, 1, 0);
                break;
        }

    }

}
