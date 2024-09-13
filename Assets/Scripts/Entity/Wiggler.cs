using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggler : Entity{

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        p.Jump();
    }

    private void Update(){
        if (this.canMove){
            CheckWand();
            if (this.cantFallAtEdges && this.GetComponent<EntityGravity>().onGround)
                CheckEdge();

            float speed = -this.moveSpeed;/*Final walk Speed*/
            if (this.direction == 0){
                speed = -speed;
                this.transform.localScale = new Vector3(-1, 1, 1);
            }else
                this.transform.localScale = new Vector3(1, 1, 1);

            this.transform.Translate(speed * Time.deltaTime, 0, 0);
        }
    }

}
