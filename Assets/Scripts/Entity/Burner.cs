using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burner : DamageBlock{

    public bool firstDisable = false;

    private void OnEnable() {
        GetComponentsInParent<SpriteRenderer>()[1].sprite = TileManager.instance.GetSpriteFromTileset(20, TileManager.TilesetType.ObjectsTileset);
        OnEnableTileAnimator();
        StopAllCoroutines();

        if (this.firstDisable)
            StartCoroutine(BurnerIE2());
        else
            StartCoroutine(BurnerIE1());
    }

    private IEnumerator BurnerIE1(){
        this.sp.enabled = true;
        StartAnimationClip(this.animationClips[1]);
        SoundManager.PlayAudioClipIfPlayerIsInNear(SoundManager.currentSoundEffects.burner, this.transform.position);
        this.transform.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForSeconds(0.4f);
        GetComponent<BoxCollider2D>().enabled = true;

        //   this.getco
        yield return new WaitForSeconds(2.5f);
        //SoundManager.instance.currentPlayedSound.Remove(SoundManager.currentSoundEffects.burner);
        StartAnimationClip(this.animationClips[2]);
    }

    private IEnumerator BurnerIE2(bool extraWait = false){
        this.sp.enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        this.transform.GetChild(0).gameObject.SetActive(false);
        if (extraWait)
            yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(3.7f);
        StartCoroutine(BurnerIE1());
    }

    public override void FinishedAnimationClip(AnimationClip clip){
        if (clip.id == 1)
            StartAnimationClip(this.animationClips[0]);
        else if (clip.id == 2)
            StartCoroutine(BurnerIE2(true));
    }

}
