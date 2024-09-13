using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePlatform : ChildrenTileAnimator{

    public float flySpeed = 4f;

    private bool isMoving = false;

    private LiftHelper liftHelper;

    private void Awake(){
        this.liftHelper = GetComponent<LiftHelper>();
    }

    private void Update(){
        if (this.isMoving)
            liftHelper.MoveLift(this.flySpeed * Time.deltaTime, 0, 0, LiftHelper.Direction.RIGHT);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            if (collision.gameObject.transform.position.y < (this.transform.position.y + 0.5f))
                return;
            this.isMoving = true;
            collision.gameObject.transform.SetParent(this.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(null);
    }

}
