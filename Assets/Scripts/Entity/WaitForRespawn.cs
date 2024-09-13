using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForRespawn : MonoBehaviour{

    public SceneManager.RespawnableEntity respawnableEntity;

    private void OnEnable(){
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11){
            this.respawnableEntity.Respawn();
            Destroy(this.gameObject);
        }
    }

}
