using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirBubbles : MonoBehaviour{

    public void Load(){
        switch (this.transform.eulerAngles.z){
            case 0:
                this.transform.eulerAngles = new Vector3(0, 0, 180);
                break;
            case 180:
                this.transform.eulerAngles = new Vector3(0, 0, 0);
                break;
            case -90:
            case 270:
                this.transform.eulerAngles = new Vector3(0, 0, 90);
                //this.transform.position = this.transform.position + new Vector3(0, -1, 0);
                break;
            case 90:
                this.transform.eulerAngles = new Vector3(0, 0, -90);
                break;
        }
    }

    private void OnTriggerStay2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            float v = collision.gameObject.GetComponent<Rigidbody2D>().velocity.y;
            if (v < 0)
                v = -v;

            switch (this.transform.eulerAngles.z){
                case 0:
                    RaycastHit2D ray1 = Physics2D.Raycast(collision.gameObject.transform.position, Vector2.down, 0.6f, GameManager.instance.entityGroundMask);
                    if (ray1)
                        return;
                    collision.gameObject.transform.Translate(0, - ((5 + v) * Time.deltaTime), 0);
                    break;
                case 180:
                    RaycastHit2D ray2 = Physics2D.Raycast(collision.gameObject.transform.position, Vector2.up, 0.6f, GameManager.instance.entityGroundMask);
                    if (ray2)
                        return;
                    collision.gameObject.transform.Translate(0, (5 + v) * Time.deltaTime, 0);
                    break;
                case -90:
                case 270:
                    v = collision.gameObject.GetComponent<PlayerController>().GetLastSpeed();
                    if (v < 0)
                        v = -v;
                    RaycastHit2D ray3 = Physics2D.Raycast(collision.gameObject.transform.position, Vector2.left, 0.6f, GameManager.instance.entityGroundMask);
                    if (ray3)
                        return;
                    collision.gameObject.transform.Translate(-((3 + v) * Time.deltaTime), 0, 0);
                    break;
                case 90:
                    v = collision.gameObject.GetComponent<PlayerController>().GetLastSpeed();
                    if (v < 0)
                        v = -v;
                    RaycastHit2D ray4 = Physics2D.Raycast(collision.gameObject.transform.position, Vector2.right, 0.6f, GameManager.instance.entityGroundMask);
                    if (ray4)
                        return;
                    collision.gameObject.transform.Translate(((3 + v) * Time.deltaTime), 0, 0);
                    break;
            }
        }
    }

}
