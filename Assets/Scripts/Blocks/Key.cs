using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : TileAnimator{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.GetComponent<PlayerController>().AddKey(this);
    }

}
