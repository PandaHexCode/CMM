using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KoopaTroopa : Entity{

    [Header("KoopaTroopa")]
    public bool isRed = false;

    [System.NonSerialized]public ShellState currentSheelState = ShellState.NoShell; 
    public enum ShellState { NoShell = 0, StandShell = 1, MovingShell = 2}

    private void OnEnable(){
        OnEnableTileAnimator();
        if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB3 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS3)
            this.GetComponent<EntityGravity>().onGroundAdd = 0.75f;
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.GetPowerup() == PlayerController.Powerup.Mini){
            p.Jump(-1, true);
            return;
        }

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            UnlockKey();
            Destroy(this.gameObject);
            return;
        }

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        if (p.input.RUN && this.currentSheelState == ShellState.StandShell && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1 && p.currentGrabedObject == null){
            p.GrabObject(this.gameObject);
            return;
        }

        if(this.currentSheelState != ShellState.StandShell)
            p.Jump(-1, true);
        else{
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = this.transform.position;
        }
        TriggerShell(p);
    }

    public void TriggerShell(PlayerController p){
        StopCoroutine("WaitForExitShellIE");
        this.transform.rotation = Quaternion.Euler(0, 0, 0);
        if (this.currentSheelState == ShellState.NoShell){
            StopCurrentAnimation();
            if(this.isRed)
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(8, TileManager.TilesetType.EnemyTileset);
            else
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(5, TileManager.TilesetType.EnemyTileset);
            this.cantFallAtEdges = false;
            this.GetComponent<BoxCollider2D>().offset = new Vector2(0.002857447f, -0.1144488f);
            this.GetComponent<BoxCollider2D>().size = new Vector2(0.8799853f, 0.7876992f);
            this.GetComponent<EntityGravity>().Y_onground = 0;
            this.GetComponent<EntityGravity>().X_onground = 0.1f;
            this.GetComponent<EntityGravity>().onGroundAdd = 0.5f;
            this.GetComponent<EntityGravity>().onGround = false;
            this.wandMask = GameManager.instance.shellWandMask;

            this.moveSpeed = 13;

            if(TileManager.instance.currentStyleID == TileManager.StyleID.SMW){
                GameObject clon = Instantiate(GameManager.instance.sceneManager.SMWNoShellPrefarb, this.transform.parent);
                p.StartIgnoreCollisionForSeconds(clon.GetComponents<BoxCollider2D>()[0], 0.3f);
                p.StartIgnoreCollisionForSeconds(clon.GetComponents<BoxCollider2D>()[1], 0.2f);
                clon.GetComponent<SpriteRenderer>().flipX = this.sp.flipX;
                clon.GetComponent<SMWNoShellKoopaTroopa>().isRed = this.isRed;
                clon.GetComponent<SMWNoShellKoopaTroopa>().direction = this.direction;
                clon.GetComponent<EntityGravity>().enabled = false;

                if (p.transform.position.x > this.transform.position.x + 0.1f)
                    clon.transform.position = this.transform.position + new Vector3(-1f, 0.3f, 0);
                else
                    clon.transform.position = this.transform.position + new Vector3(1f, 0.3f, 0);
            }
            this.currentSheelState = ShellState.StandShell;
        }
        else if (this.currentSheelState == ShellState.MovingShell)
            this.currentSheelState = ShellState.StandShell;
        else
        {
            this.currentSheelState = ShellState.MovingShell;
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[0], 0.3f);
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[1], 0.3f);
        }

        if (this.currentSheelState == ShellState.StandShell){
           if(TileManager.instance.currentStyleID != TileManager.StyleID.SMW)
                StartCoroutine(WaitForExitShellIE());
            StopCurrentAnimation();
            if (this.isRed)
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(8, TileManager.TilesetType.EnemyTileset);
            else
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(5, TileManager.TilesetType.EnemyTileset);
            this.canDespawn = true;
            this.gameObject.layer = 31;
            this.canMove = false;
        }else{
            if(p.currentGrabedObject != this.gameObject)
                StartAnimationClip(this.animationClips[1]);
            this.canDespawn = false;
            this.gameObject.layer = 18;
            this.canMove = true;
            if (p.GetOnGround()){
                if (!p.GetComponent<SpriteRenderer>().flipX)
                    this.direction = 0;
                else
                    this.direction = 1;
            }else{
                if (p.transform.position.x > this.transform.position.x + 0.1f)
                    this.direction = 1;
                else
                    this.direction = 0;
            }
        }
    }

    public override void CheckWand(){
        if (direction == 1){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, Vector2.left, 0.6f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 0)
                        direction = 0;
                }else if (raycastHit2D.collider.gameObject.layer == 28){
                    if (!raycastHit2D.collider.gameObject.tag.Equals("GrabNoWand"))
                        direction = 0;
                }else
                    direction = 0;

                if (this.currentSheelState == ShellState.MovingShell){
                    if (raycastHit2D.collider.gameObject.GetComponent<Entity>() != null && (raycastHit2D.collider.gameObject.GetComponent<Entity>().canDieFromShell | raycastHit2D.collider.gameObject.GetComponent<Entity>().canHitFromIceBall)){
                        raycastHit2D.collider.gameObject.GetComponent<Entity>().DieFromShell(this.gameObject);
                        direction = 1;
                    }else
                        SceneManager.CheckBlockToUseOrDestroy(raycastHit2D.collider.gameObject, false);
                }
            }
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, Vector2.right, 0.6f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 180)
                        direction = 1;
                }else if (raycastHit2D.collider.gameObject.layer == 28){
                    if (!raycastHit2D.collider.gameObject.tag.Equals("GrabNoWand"))
                        direction = 1;
                }else
                    direction = 1;

                if (this.currentSheelState == ShellState.MovingShell){
                    if (raycastHit2D.collider.gameObject.GetComponent<Entity>() != null && (raycastHit2D.collider.gameObject.GetComponent<Entity>().canDieFromShell | raycastHit2D.collider.gameObject.GetComponent<Entity>().canHitFromIceBall)){
                        raycastHit2D.collider.gameObject.GetComponent<Entity>().DieFromShell(this.gameObject);
                        direction = 0;
                    }else
                        SceneManager.CheckBlockToUseOrDestroy(raycastHit2D.collider.gameObject, false);
                }
            }
        }
    }

    public override void OnDamagePlayer(PlayerController p){
        if (this.currentSheelState == ShellState.NoShell | this.currentSheelState == ShellState.MovingShell)
            base.OnDamagePlayer(p);
        else if(this.canTrigger)
            TriggerFromPlayer(p);
    }

    public void TriggerFromPlayer(PlayerController p){
        GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
        eff.transform.position = this.transform.position;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        TriggerShell(p);
    }

    public IEnumerator WaitForExitShellIE(float seconds = 7){
        this.gameObject.layer = 14;
        yield return new WaitForSeconds(seconds);
        for (int i = 0; i < 5; i++){
            while(this.transform.rotation.z > -0.1449779 && this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall){
                this.transform.Rotate(0, 0, -155 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z < 0 && this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall){
                this.transform.Rotate(0, 0, 155 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z < 0.1449779 && this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall){
                this.transform.Rotate(0, 0, 155 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z > 0 && this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall){
                this.transform.Rotate(0, 0, -155 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            this.transform.rotation = Quaternion.Euler(0, 0, 0);

            if (this.currentSheelState != ShellState.StandShell | this.isFreezedFromIceBall)
                break;
        }

        if (this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall)
            yield return new WaitForSeconds(0.1f);
        if (this.currentSheelState == ShellState.StandShell&& !this.isFreezedFromIceBall){
            if (this.isRed)
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(66, TileManager.TilesetType.EnemyTileset);
            else
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(61, TileManager.TilesetType.EnemyTileset);
        }
        if (this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall)
            yield return new WaitForSeconds(0.1f);
        if (this.currentSheelState == ShellState.StandShell && !this.isFreezedFromIceBall)
            ExitShell();
    }

    public void ExitShell(){
        if(this.isRed)
            this.cantFallAtEdges = true;
        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            if (p.currentGrabedObject == this.gameObject){
                p.UngrabCurrentObject(true);
                p.Damage();
            }
        }
        this.GetComponent<BoxCollider2D>().offset = new Vector2(-0.04836696f, -0.3792081f);
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.9824342f, 0.7704248f);
        this.GetComponent<EntityGravity>().Y_onground = -0.25f;
        this.GetComponent<EntityGravity>().X_onground = 0.31f;
        if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB3)
            this.GetComponent<EntityGravity>().onGroundAdd = 0.75f;
        else
            this.GetComponent<EntityGravity>().onGroundAdd = 0.7f;
        this.wandMask = GameManager.instance.entityWandMask;
        this.StartAnimationClip(this.animationClips[0]);
        this.currentSheelState = ShellState.NoShell;
        this.canMove = true;
        this.canDespawn = true;
        this.gameObject.layer = 14;
        this.transform.Translate(0, 0.7f, 0); 

        this.moveSpeed = 1.7f;
    }

}
