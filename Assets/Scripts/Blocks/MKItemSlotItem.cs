using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MKItemSlotItem : MonoBehaviour{

    public PlayerController.MKItemSlotContent content;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            collision.gameObject.GetComponent<PlayerController>().SetMkItemSlot(content);
            Destroy(this.gameObject);
        }
    }

}
