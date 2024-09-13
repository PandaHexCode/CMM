using System.Collections;
using UnityEngine;

public class GroundBoo : Entity{

    private void OnEnable(){
        OnEnableTileAnimator();
        StopAllCoroutines();
        StartCoroutine(GroundBooIE1());
        this.GetComponent<EntityGravity>().OnGroundchecker();
        if (!this.GetComponent<EntityGravity>().onGround){
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            Destroy(this.gameObject);
        }
    }

    public IEnumerator GroundBooIE1(){
        yield return new WaitForSeconds(4);
        StartAnimationClip(this.animationClips[0]);
        this.canMove = false;
    }

    private IEnumerator GroundBooIE2(){
        this.sp.enabled = false;
        if (this.transform.childCount > 0)
            this.transform.GetChild(0).gameObject.SetActive(false);
        GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSeconds(2);
        this.sp.enabled = true;
        if (this.transform.childCount > 0)
            this.transform.GetChild(0).gameObject.SetActive(true);
        StartAnimationClip(this.animationClips[1]);
    }


    public override void FinishedAnimationClip(AnimationClip clip){
        if (clip.id == 1 && !this.isCaptured){
            this.canMove = true;
            GetComponent<BoxCollider2D>().enabled = true;
            StartCoroutine(GroundBooIE1());
        }else if (clip.id == 0 && !this.isCaptured)
            StartCoroutine(GroundBooIE2());
    }

}
