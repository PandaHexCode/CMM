using UnityEngine;

public class WaterStay : MonoBehaviour{

    private void OnTriggerStay2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            collision.gameObject.GetComponent<PlayerController>().SetIsInWater(true);
            collision.gameObject.GetComponent<PlayerController>().checkWaterRay = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            collision.gameObject.GetComponent<PlayerController>().SetIsInWater(false);
            collision.gameObject.GetComponent<PlayerController>().checkWaterRay = true;
        }
    }

}
