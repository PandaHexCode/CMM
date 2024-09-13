using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorPlayer : MonoBehaviour{

    public int flySpeed = 15;
    public int fastFlySpeed = 25;
    private int currentSpeed;

    private Transform _transform;
    private InputManager input;

    private void OnEnable(){
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.simulated = false;
        rb.velocity = Vector3.zero;
        this.GetComponent<SpriteRenderer>().sprite = this.GetComponent<PlayerController>().currentPlayerSprites.stand[0];
    }

    private void Awake(){
        this._transform = this.transform;
        this.input = GetComponentInChildren<InputManager>();
    }

    private void FixedUpdate(){
         if (this._transform.position.y < 9)
             this._transform.position = new Vector3(this._transform.position.x, 12, this._transform.position.z);
         else if (this._transform.position.y > 35)
             this._transform.position = new Vector3(this._transform.position.x, 33, this._transform.position.z);
         else if (this._transform.position.x < 2)
             this._transform.position = new Vector3(3, this._transform.position.y, this._transform.position.z);
        
        if (this.input.RUN)
            this.currentSpeed = this.fastFlySpeed;
        else
            this.currentSpeed = this.flySpeed;

        if (this.input.UP)
            this._transform.Translate(0, this.currentSpeed * Time.deltaTime, 0);
        else if(this.input.DOWN)
            this._transform.Translate(0, -this.currentSpeed * Time.deltaTime, 0);

        if (this.input.RIGHT)
            this._transform.Translate(this.currentSpeed * Time.deltaTime, 0, 0);
        else if (this.input.LEFT)
            this._transform.Translate(-this.currentSpeed * Time.deltaTime, 0, 0);
    }
}
