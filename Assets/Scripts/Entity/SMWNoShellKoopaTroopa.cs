using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMWNoShellKoopaTroopa : Entity{

    public bool isRed = false;
    private bool canEnterShell = false;

    private void OnEnable(){
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);

        OnEnableTileAnimator();
       
        StartCoroutine(WaitForOnGroundIE());
    }

    private IEnumerator WaitForOnGroundIE(){
        this.canMove = false;
        this.moveSpeed = 0;
        yield return new WaitForSeconds(0.01f);
        if (this.direction == 0)
            this.GetComponent<EntityGravity>().StartCoroutine(SceneManager.DropGameObject(this.gameObject, 6));
        else
            this.GetComponent<EntityGravity>().StartCoroutine(SceneManager.DropGameObject(this.gameObject, -6));
        if (this.isRed)
            StartAnimationClip(this.animationClips[3]);
        else
            StartAnimationClip(this.animationClips[2]);

        while (!this.GetComponent<EntityGravity>().enabled){
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(1f);

        this.canMove = true;
        this.moveSpeed = 1.7f;
        if (this.isRed)
            this.cantFallAtEdges = true;

        if (this.isRed)
            StartAnimationClip(this.animationClips[1]);
        else
            StartAnimationClip(this.animationClips[0]);

        this.canEnterShell = true;
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            Destroy(this.gameObject);
            return;
        }

        UnStack();
        this.moveSpeed = 0;
        this.canMove = false;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        p.Jump(-1, true);

        StopCurrentAnimation();
        if (this.isRed)
            this.sp.sprite = TileManager.instance.GetSpriteFromTileset(20, TileManager.TilesetType.ExtraTileset);
        else
            this.sp.sprite = TileManager.instance.GetSpriteFromTileset(14, TileManager.TilesetType.ExtraTileset);
        this.GetComponents<BoxCollider2D>()[0].enabled = false;
        this.GetComponents<BoxCollider2D>()[1].enabled = false;
        StartCoroutine(DestroyIE());
    }

    public IEnumerator DestroyIE(){
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }

     public override void CheckWand(){
        if (direction == 1){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, Vector2.left, 0.3f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 0)
                        direction = 0;
                }
                else
                    direction = 0;

                if (raycastHit2D.collider.gameObject.layer == 31 && this.canEnterShell){
                    raycastHit2D.collider.gameObject.GetComponent<KoopaTroopa>().StartCoroutine(raycastHit2D.collider.gameObject.GetComponent<KoopaTroopa>().WaitForExitShellIE(0));
                    Destroy(this.gameObject);
                }
            }
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, Vector2.right, 0.3f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 180)
                        direction = 1;
                }else
                    direction = 1;

                if (raycastHit2D.collider.gameObject.layer == 31 && this.canEnterShell){
                    raycastHit2D.collider.gameObject.GetComponent<KoopaTroopa>().StartCoroutine(raycastHit2D.collider.gameObject.GetComponent<KoopaTroopa>().WaitForExitShellIE(0));
                    Destroy(this.gameObject);
                }
            }
        }
    }

}
