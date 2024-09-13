using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MysteryMushrom : EntityGravity{

    public int costumeNumber = 0;

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.layer == 9){
            collision.gameObject.GetComponent<PlayerController>().ApplyMysteryCostume(GameManager.instance.mysteryCostumesManager.mysteryCostumes[this.costumeNumber]);
            SceneManager.respawnableEntities.Remove(SceneManager.GetRespawnableEntityFromEntity(this.gameObject));
            Destroy(this.gameObject);
        }
    }

}
