using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisbleQuestionBlock : ItemBlock{
   
    public void Awake(){
        this.GetComponentInChildren<SpriteRenderer>().enabled = false;
        this.GetComponentsInChildren<BoxCollider2D>()[1].enabled = false;
    }

    public override void UseItemBlock(GameObject player, bool isHitDown = false, bool noPowerupCheck = false){
        if (player.GetComponent<Rigidbody2D>().velocity.y > 0){
            this.GetComponentInChildren<SpriteRenderer>().enabled = true;
            this.GetComponentsInChildren<BoxCollider2D>()[1].enabled = true;
            base.UseItemBlock(player, isHitDown);
        }
    }
}
