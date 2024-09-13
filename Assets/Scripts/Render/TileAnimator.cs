using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimator : MonoBehaviour{

    [Header("TileAnimator")]
    public AnimationClip[] animationClips;
    public bool startFirstClipAtStart;
    [System.NonSerialized]public int currentAnimation = -1;

    private bool isInAnimation = false;
    private int animationBeforeDisabled = -1;
    private Coroutine currentAnimationCor = null;

    [System.NonSerialized]public SpriteRenderer sp;

    [System.Serializable]
    public class AnimationClip{
        public int id = 0; 
        public int[] tileNumbers;
        public TileManager.TilesetType tilesetType;
        public float delay = 0.1f;
        public bool waitBeforeRepeat = false;
        public bool repeat = false;
        public bool backwards = false;
    }

    private void OnEnable(){
        OnEnableTileAnimator();
    }

    public void OnEnableTileAnimator(){
        if (GetComponent<SpriteRenderer>() == null)
            this.sp = GetComponentInChildren<SpriteRenderer>();
        else
            this.sp = GetComponent<SpriteRenderer>();

        if (this.animationBeforeDisabled != -1){
            StartAnimationClip(animationClips[this.animationBeforeDisabled]);
            this.animationBeforeDisabled = -1;
        }
    }

    private void Start(){
        if (this.startFirstClipAtStart)
            StartAnimationClip(animationClips[0]);
    }

    private void OnDisable(){
        if (this.isInAnimation){
            this.animationBeforeDisabled = this.currentAnimation;
            StopCurrentAnimation();
        }else
            this.animationBeforeDisabled = -1;
    }

    public void StartAnimationClip(AnimationClip animationClip){
        this.StopCurrentAnimation();
        this.isInAnimation = true;
        this.currentAnimation = animationClip.id;

        this.currentAnimationCor = StartCoroutine(PlayAnimationClip(animationClip));
        animationClip = null;
        return;
    }

    private IEnumerator PlayAnimationClip(AnimationClip animationClip){
        TileManager tileManager = TileManager.instance;
        if (!animationClip.backwards){
            foreach (int tile in animationClip.tileNumbers){
                this.sp.sprite = tileManager.GetSpriteFromTileset(tile, animationClip.tilesetType);
                yield return new WaitForSeconds(animationClip.delay);
            }
        }else{
            for (int i = animationClip.tileNumbers.Length-1; i >= 0; i--){
                this.sp.sprite = tileManager.GetSpriteFromTileset(animationClip.tileNumbers[i], animationClip.tilesetType);
                yield return new WaitForSeconds(animationClip.delay);
            }
        }

        if (animationClip.repeat){
            if (animationClip.waitBeforeRepeat)
                yield return new WaitForSeconds(0.5f);
            this.currentAnimationCor = StartCoroutine(PlayAnimationClip(animationClip));
        }else{
            this.isInAnimation = false;
            this.currentAnimation = -1;
        }

        this.FinishedAnimationClip(animationClip);
    }

    public void StopCurrentAnimation(){
        this.isInAnimation = false;
        this.currentAnimation = -1;
        if (this.currentAnimationCor != null)
            StopCoroutine(this.currentAnimationCor);
    }

    public virtual void FinishedAnimationClip(AnimationClip clip){
        return;
    }

}
