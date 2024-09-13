using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffObject : TileAnimator{/*All OnOff Blocks use this as reference, OnSwitch get called from GameManager/SceneManager, TileAnimator id0 = OnAnimation, id1 = OffAnimation*/

    private void Awake(){
        SceneManager.onOffObjects.Add(this.gameObject);
    }

    public virtual void OnSwitch(bool state){
        if (!this.gameObject.active)
            return;

        if(state)
            this.StartAnimationClip(this.animationClips[0]);
        else
            this.StartAnimationClip(this.animationClips[1]);
    }

}
