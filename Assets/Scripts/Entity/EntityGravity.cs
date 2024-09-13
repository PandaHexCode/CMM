using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityGravity : MonoBehaviour{

    [Header("EntityGravity")]
    public bool useGravity = true;
    public float gravity = 9f;
    public float X_onground = 0.31f;
    public float Y_onground = 0;
    public float onGroundAdd = 0.7f;
    public float length = 0.5f;

    private float vel = 0;
    private float velOff = 0.07f;
    public bool onGround = false;

    private Transform _transform;
    private LayerMask layerMask;

    private void Awake(){
        this._transform = this.transform;
        this.layerMask = GameManager.instance.entityGroundMask;
        if(TileManager.instance.currentTileset.autoEnableIsWater){
            this.gravity = this.gravity / 5;
            this.velOff = 0.01f;
        }
    }

    private void Update(){
        if (GameManager.fps < 10)
            return;

        OnGroundchecker();
        if (!onGround && useGravity && Time.timeScale != 0){
            _transform.Translate(0, -gravity * Time.deltaTime - this.vel, 0);
            this.vel = this.vel + this.velOff * Time.deltaTime;
            if (this.vel > 0.5f)
                this.vel = 0.5f;
        }
    }

    public void OnGroundchecker(){
        float length = 0.5f;
        if (GameManager.fps < 60)
            length = 0.95f;
        RaycastHit2D ray1 = Physics2D.Raycast(_transform.position + new Vector3(X_onground, Y_onground, 0f), Vector2.down, length, this.layerMask);
        RaycastHit2D ray2 = Physics2D.Raycast(_transform.position + new Vector3(-X_onground, Y_onground, 0f), Vector2.down, length, this.layerMask);
        if (ray1)
            CheckRay(ray1);
        else if (ray2)
            CheckRay(ray2);
        else
            this.onGround = false;
    }

    private GameObject lastLift = null;
    private void CheckRay(RaycastHit2D ray){
        if (ray && ray.collider.gameObject.layer == 27){
            if (ray && ray.collider.transform.eulerAngles.z == 90)
                onGround = true;
            else
                onGround = false;
        }else{
            if (this.lastLift == null && ray.collider.gameObject.layer == 17 && ray.collider.gameObject.CompareTag("Lift")){
                this.lastLift = ray.collider.gameObject;
                this.lastLift.GetComponent<LiftHelper>().entityFollowers.Add(this._transform);
            }else if(this.lastLift != null && ray.collider.gameObject != lastLift){
                this.lastLift.GetComponent<LiftHelper>().entityFollowers.Remove(this._transform);
                this.lastLift = null;
                this.onGround = false;
            }

            if (!this.onGround){
                this.vel = 0;
                this.onGround = true;
                this._transform.position = new Vector3(this._transform.position.x, ray.collider.transform.position.y + ray.collider.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y + this.onGroundAdd + ray.collider.gameObject.GetComponent<BoxCollider2D>().offset.y, this._transform.position.z);
            }
        }
    }

    public void SetUseGravity(bool state){
        this.useGravity = state;
    }

}
