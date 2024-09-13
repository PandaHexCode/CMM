using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piranha : Entity{

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.isInSpin){
            StartCoroutine(ReactivateDamage());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = p.transform.position;
            return;
        }
    }

    private IEnumerator ReactivateDamage(){
        this.damageOnCollision = false;
        yield return new WaitForSeconds(0.05f);
        this.damageOnCollision = true;
    }

}
