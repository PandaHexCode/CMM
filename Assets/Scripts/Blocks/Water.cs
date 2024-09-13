using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : TileAnimator{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            collision.GetComponent<PlayerController>().SetIsInWater(true);
        }
    }

}
