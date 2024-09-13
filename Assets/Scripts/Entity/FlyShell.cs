using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyShell : Entity{

    public bool isGreen = false;

    private void Update(){
        if (this.canMove && !this.isCaptured){
            this.transform.Translate(-this.moveSpeed * Time.deltaTime, 0, 0);
        }
    }

    public override void OnTriggerPlayer(PlayerController p){
        this.gameObject.layer = 23;
        p.Damage();
    }

    public override void OnDamagePlayer(PlayerController p){
        this.gameObject.layer = 23;

        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.isInSpin){
            p.Jump(-1, true);
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            UnlockKey();
            Destroy(this.gameObject);
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(this.transform);
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(null);
    }

    private void OnCollisionStay2D(Collision2D collision){
        if (collision.gameObject.layer == 9){
            collision.gameObject.transform.SetParent(this.transform);
            OnDamagePlayer(collision.gameObject.GetComponent<PlayerController>());
            if (!this.isGreen){
                if (collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini){
                    this.transform.Translate(0, -0.4f * Time.deltaTime, 0);
                    
                }else{
                    this.transform.Translate(0, -2 * Time.deltaTime, 0);
                   
                }
            }else{
                if (collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini){
                    this.transform.Translate(0, 0.4f * Time.deltaTime, 0);
                 
                }else{
                    this.transform.Translate(0, 2 * Time.deltaTime, 0);
                  
                }
            }
        }
    }
}
