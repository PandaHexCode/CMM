using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour{/*https://www.youtube.com/watch?v=zit45k6CUMk&ab_channel=Dani*/

    public float parallaxEffect;
    public bool scrooling = false;
    public float scroolSpeed;
    public float of = 19;
    public float yOffset = 0;
    public bool useY = true;public float notUseYOff = 0;
    public bool isLave = false;

    public float length, startpos;
    private float length2, startpos2;
    private float orgY;
    private Transform cam;
    private PlayerCamera playerCamera;
    private Transform _transform;

    private void Awake(){
        this.cam = Camera.main.transform;
        this.playerCamera = this.cam.GetComponent<PlayerCamera>();
        this._transform = this.transform;
        if (this.isLave)
            this._transform.position = new Vector3(this._transform.position.x, 12.36f, this._transform.position.z);
        this.orgY = this._transform.position.y;
        this.startpos = transform.position.x;
        this.startpos2 = transform.position.y + -9;
        if (!this.useY)
            this.startpos2 = transform.position.y + notUseYOff;
        if(GetComponent<SpriteRenderer>() != null && GetComponent<SpriteRenderer>().sprite != null)
            this.length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update(){
        if (this.scrooling){
            this.startpos = this.startpos + scroolSpeed * Time.deltaTime;
        }

        float temp = (cam.position.x * (1 - parallaxEffect));
        float dist = (cam.position.x * parallaxEffect);
        float dist2 = (cam.position.y * parallaxEffect);
        if (!this.useY)
            dist2 = 0;

        _transform.position = new Vector3(startpos + dist, startpos2 + dist2 + this.yOffset, _transform.position.z);

        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }

}
