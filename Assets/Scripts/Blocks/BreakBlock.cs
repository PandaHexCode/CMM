using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;

public class BreakBlock : ItemBlock{

    public GameObject destroyEffect;

    public override void UseItemBlock(GameObject player, bool isHitDown = false, bool noPowerupCheck = false){
        if (this.contentBlock != BlockID.ERASER)
            base.UseItemBlock(player, isHitDown);
        else
            DestroyBlock(player.GetComponent<PlayerController>(), isHitDown, noPowerupCheck);
    }

    public void DestroyBlock(PlayerController player, bool isHitDown, bool noPowerupCheck = false){
        if(TileManager.instance.currentStyle.id == TileManager.StyleID.SMW){
            GameManager.instance.CheckBlockHitTrigger(this.gameObject, isHitDown);
            StartCoroutine(SMWDestroyBlock(this.sp.sprite));
            StartAnimationClip(this.animationClips[0]);
            return;
        }

        if ((player.GetPowerup() == PlayerController.Powerup.Mini | player.GetPowerup() == PlayerController.Powerup.Small) && !noPowerupCheck){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
            this.hitBlockAnimationCor = GameManager.instance.PlayBlockHitAnimation(this.gameObject, isHitDown);
        }else{
            GameManager.instance.CheckBlockHitTrigger(this.gameObject, isHitDown);
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.breakBlock);
            GameObject effectClone = Instantiate(this.destroyEffect);
            effectClone.transform.position = this.transform.position + new Vector3(4, 2, 0);
            Destroy(this.gameObject);
        }
    }

    private IEnumerator SMWDestroyBlock(Sprite orgSprite) {
        GetComponentsInChildren<BoxCollider2D>()[0].enabled = false;
        GetComponentsInChildren<BoxCollider2D>()[1].enabled = false;
        yield return new WaitForSeconds(3);
        StopCurrentAnimation();
        this.sp.sprite = orgSprite;
        GetComponentsInChildren<BoxCollider2D>()[0].enabled = true;
        GetComponentsInChildren<BoxCollider2D>()[1].enabled = true;
    }

}
