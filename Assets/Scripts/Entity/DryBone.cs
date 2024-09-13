using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DryBone : Entity{

    [System.NonSerialized] public bool dontSetBack = false;

    private void OnEnable(){
        OnEnableTileAnimator();
        if (TileManager.instance.currentStyleID == TileManager.StyleID.SMW | TileManager.instance.currentStyleID == TileManager.StyleID.SMB3 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS3){
            this.GetComponent<EntityGravity>().onGroundAdd = 0.75f;
            if(TileManager.instance.currentStyleID == TileManager.StyleID.SMW)
                StartCoroutine(SMWThrowBone());
        }
    }

    private IEnumerator SMWThrowBone(){
        while (true){
            yield return new WaitForSeconds(3.8f);
            if(this.currentAnimation == 0 && this.isSpawned){
                StartAnimationClip(this.animationClips[3]);
                this.canMove = false;
                GameObject bone = Instantiate(GameManager.instance.sceneManager.bonePrefarb, this.transform.parent);
                bone.transform.position = this.transform.position;
                if (!this.sp.flipX)
                    bone.GetComponent<Bone>().speed = -bone.GetComponent<Bone>().speed;
            }
        }
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        p.Jump(-1, true);

        if (p.GetPowerup() == PlayerController.Powerup.Mini)
            return;

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.breakBlock);

        StartAnimationClip(this.animationClips[1]);
        this.GetComponents<BoxCollider2D>()[0].enabled = false;
        this.GetComponents<BoxCollider2D>()[1].enabled = false;
        this.canMove = false;
        StartCoroutine(DryBoneRespawnIE());
    }

    private IEnumerator DryBoneRespawnIE(){
        yield return new WaitForSeconds(5);
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play(0);
        yield return new WaitForSeconds(2);
        this.GetComponent<Animator>().enabled = false;
        this.transform.GetChild(0).transform.localPosition = Vector3.zero;
        StartAnimationClip(this.animationClips[2]);
    }

    public override void FinishedAnimationClip(AnimationClip clip){
        if(clip.id == 2 | clip.id == 3){
            if (!this.dontSetBack){
                this.GetComponents<BoxCollider2D>()[0].enabled = true;
                this.GetComponents<BoxCollider2D>()[1].enabled = true;
                if(this.transform.parent.GetComponent<Entity>() == null)
                    this.canMove = true;
            }
            StartAnimationClip(this.animationClips[0]);
            this.dontSetBack = false;
        }

    }

}
