using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if ((collision.gameObject.layer == 14 | collision.gameObject.layer == 21) && collision.gameObject.GetComponent<Entity>() != null){
            collision.gameObject.GetComponent<Entity>().Spawn();
        }
    }

}
