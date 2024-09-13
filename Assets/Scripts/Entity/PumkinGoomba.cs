using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumkinGoomba : Entity{

      public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.GetPowerup() == PlayerController.Powerup.Mini){
            p.Jump(-1, true);
            return;
        }

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            UnlockKey();
            Destroy(this.gameObject);
            return;
        }

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        p.Jump(-1, true);

        if (this.currentAnimation == 2){
            UnStack();
            this.moveSpeed = 0;
            this.canMove = false;
            StartAnimationClip(animationClips[3]);
            this.GetComponents<BoxCollider2D>()[0].enabled = false;
            this.GetComponents<BoxCollider2D>()[1].enabled = false;
            UnlockKey();
            StartCoroutine(DestroyIE());
        }else
            HitPumkin();
    }

    public IEnumerator DestroyIE(){
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }

    public override void HitFromFireBall(GameObject fireball){
        if (this.currentAnimation == 2)
            base.HitFromFireBall(fireball);
        else{
            HitPumkin();
            fireball.GetComponent<FireBall>().Explode();
        }
    }

    private void HitPumkin(){
        GameObject eff = Instantiate(GameManager.instance.sceneManager.pumkinEffect);
        eff.transform.position = this.transform.position + new Vector3(4, 2.5f, 0);
        SceneManager.destroyAfterNewLoad.Add(eff);

        if (this.currentAnimation == 0)
            StartAnimationClip(animationClips[1]);
        else if (this.currentAnimation == 1)
            StartAnimationClip(animationClips[2]);
    }

}
