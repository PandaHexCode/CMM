using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : TileAnimator{

    public float speed = 7f;
    public bool friendly = false;
    private Transform _transform;

    private void Awake(){
        this._transform = this.transform;
    }
    private void Update(){
        this._transform.Translate(this.speed * Time.deltaTime, 0, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9 && !this.friendly)
            collision.gameObject.GetComponent<PlayerController>().Damage();
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.friendly)
            collision.gameObject.GetComponent<PlayerController>().Damage();
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 19)
            Destroy(this.gameObject);
    }

}
