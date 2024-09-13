using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBlock : TileAnimator{

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.GetComponent<PlayerController>().Damage();
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.GetComponent<PlayerController>().Damage();
    }

}
