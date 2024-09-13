using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTop : Entity{/*canceld, to difficult*/

    private int state = 0; /*0 = normal, 1 = on wall up, 2 = on left wall, 3 = on right wall*/

    private void OnEnable(){
        OnEnableTileAnimator();
        if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask))
            this.state = 1;
        else if (Physics2D.Raycast(this.transform.position, Vector2.left, 1f, this.groundMask))
            this.state = 2;
        else if (Physics2D.Raycast(this.transform.position, Vector2.right, 1f, this.groundMask))
            this.state = 3;
    }

    private void Update(){
        if (this.canMove){
            CheckEdge();
            CheckWand();

            float speed = -this.moveSpeed;/*Final walk Speed*/
            if (this.direction == 0){
                speed = -speed;
                if (!sp.flipX)
                    sp.flipX = true;
            }else if (sp.flipX)
                sp.flipX = false;

            this.transform.Translate(speed * Time.deltaTime, 0, 0);
        }
    }

    public override void CheckEdge(){
        float X = 0.3f;
        if (direction == 1)
            X = -X;

        Vector2 vt = Vector3.RotateTowards(Vector3.down, new Vector3(0, 0, this.transform.eulerAngles.z), 1000, 1000);
        if (!Physics2D.Raycast(this.transform.position + new Vector3(X, 0, 0f), vt, 0.5f, this.groundMask)){
            if (direction == 1)
                direction = 0;
            else
                direction = 1;
        }
    }

}
