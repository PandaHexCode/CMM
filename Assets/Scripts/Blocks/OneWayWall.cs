using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayWall : MonoBehaviour{/*This is only for collide animation*/

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 14 | collision.gameObject.layer == 18 | collision.gameObject.layer == 21 | collision.gameObject.layer == 20 | collision.gameObject.layer == 24 | collision.gameObject.layer == 9){
            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().Play(0);
        }
    }

}
