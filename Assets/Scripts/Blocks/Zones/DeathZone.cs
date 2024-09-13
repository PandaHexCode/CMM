using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.GetComponent<PlayerController>().Death();
        else if (collision.gameObject.layer == 14 | collision.gameObject.layer == 18 | collision.gameObject.layer == 21 | collision.gameObject.layer == 20 | collision.gameObject.layer == 24){
            if (collision.gameObject.GetComponent<Entity>() != null)
                collision.gameObject.GetComponent<Entity>().UnlockKey();
            Destroy(collision.gameObject);
        }else if(collision.gameObject.layer == 11){
            if (collision.gameObject.GetComponent<FireBall>() != null)
                collision.gameObject.GetComponent<FireBall>().Explode();
        }
    }

}
