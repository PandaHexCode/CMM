using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : TileAnimator{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.GetComponent<PlayerController>().Death();
        else if (collision.gameObject.layer == 14 | collision.gameObject.layer == 17 | collision.gameObject.layer == 24){
            if (collision.gameObject.GetComponent<Entity>() != null)
                collision.gameObject.GetComponent<Entity>().UnlockKey();
            Destroy(collision.gameObject);
        }
    }

}
