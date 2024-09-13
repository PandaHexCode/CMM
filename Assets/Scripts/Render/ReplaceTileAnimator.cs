using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceTileAnimator : TileAnimator{

    public TileManager.StyleID replaceForStyle;

    private void OnEnable(){
        if (TileManager.instance.currentStyleID == this.replaceForStyle){
            TileAnimator realTileAnimator = this.gameObject.GetComponent<TileAnimator>();
            realTileAnimator.animationClips = this.animationClips;
            realTileAnimator.startFirstClipAtStart = this.startFirstClipAtStart;
        }

        Destroy(this);
    }

}
