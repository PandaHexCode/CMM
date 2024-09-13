using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PSwitch : TileAnimator{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.isTrigger)
            return;

        if(collision.gameObject.layer == 8 | collision.gameObject.layer == 23 | collision.gameObject.layer == 24 | collision.gameObject.layer == 9){
            if ((collision.gameObject.layer == 23 | collision.gameObject.layer == 8) && collision.gameObject.GetComponent<EntityGravity>() == null)
                return;

            if (collision.gameObject.layer == 9){
                if ((int)collision.gameObject.transform.position.y == (int)this.transform.position.y && collision.gameObject.GetComponent<PlayerController>().GetOnGround())
                    return;

                if(collision.gameObject.GetComponent<PlayerController>().input.JUMP)
                    collision.gameObject.GetComponent<PlayerController>().Jump(-1, true);
            }
            Activate();
        }
    }

    public void Activate(){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
        GetComponent<EntityGravity>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponents<BoxCollider2D>()[1].enabled = false;
        SceneManager.ActivatePSwitch();
        StartAnimationClip(this.animationClips[1]);
    }

    public override void FinishedAnimationClip(AnimationClip clip){
        if (clip.id == 1)
            Destroy(this.gameObject);
    }

}
