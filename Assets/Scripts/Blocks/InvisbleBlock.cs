using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisbleBlock : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.GetComponent<SpriteRenderer>().enabled = false;
    }

}
