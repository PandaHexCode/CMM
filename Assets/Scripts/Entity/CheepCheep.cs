using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheepCheep : Entity{

    public bool isRedCheepCheep = false;

    private bool was = false;
    private Transform parTrans;

    private void OnEnable(){
        this.parTrans = this.transform.parent;
        GetComponentInParent<Animator>().enabled = false;
        if (TileManager.instance.currentTileset.autoEnableIsWater){
            this.was = true;
            GetComponentInParent<Animator>().enabled = true;
            GetComponentInParent<Animator>().Play("CheepCheepWater");
            if(this.isRedCheepCheep)
                GetComponentInParent<Animator>().Play("RedCheepCheepWater");
        }
        OnEnableTileAnimator();
    }

    private void Update(){
        if (!this.canMove)
            return;

        if (!this.was){
            if (this.parTrans.position.y < 11.5f){
                this.was = true;
                GetComponentInParent<Animator>().enabled = true;
                if(this.isRedCheepCheep)
                    GetComponentInParent<Animator>().Play("RedCheepCheepOutside");
                this.isRedCheepCheep = false;
            }

            this.parTrans.Translate(0, -18 * Time.deltaTime, 0);
            return;
        }

        if(!this.isRedCheepCheep)
            this.parTrans.Translate(-2f * Time.deltaTime, 0, 0);
    }

    public override void OnTriggerPlayer(PlayerController p){
        if (((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround()) | TileManager.instance.currentTileset.autoEnableIsWater)
            return;

        p.Jump(-1, true);

        if (p.GetPowerup() == PlayerController.Powerup.Mini)
            return;

        UnlockKey();
 
        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            Destroy(this.gameObject);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(this.ShootDieAnimation(this.gameObject));
    }

}
