using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandartEffect : TileAnimator{

    public override void FinishedAnimationClip(AnimationClip clip){
        Destroy(this.gameObject);
    }

}
