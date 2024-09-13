using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : TileAnimator{

    public static int currentCheckpoint = -1;
    [System.NonSerialized]public int myNumber;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && currentCheckpoint != this.myNumber){
            if (collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Small)
                collision.gameObject.GetComponent<PlayerController>().SetPowerup(PlayerController.Powerup.Big);
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.checkpoint);
            CollectCheckPoint();
        }
    }

    public override void FinishedAnimationClip(AnimationClip clip){
        if (this.currentAnimation == 1)
            StartAnimationClip(this.animationClips[2]);
    }

    public void CollectCheckPoint(){
        this.startFirstClipAtStart = false;
        if (currentCheckpoint != -1){
            SceneManager.checkpoints[currentCheckpoint].StartAnimationClip(this.animationClips[0]);
        }

        StartAnimationClip(this.animationClips[1]);
        currentCheckpoint = this.myNumber;
    }

}
