using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzleBeetle : Entity{

    [System.NonSerialized]public ShellState currentSheelState = ShellState.NoShell; 
    public enum ShellState { NoShell = 0, StandShell = 1, MovingShell = 2}

    public bool isSpinny = false;

    private bool isUp = false;
    private bool wasUp = false;
    private EntityGravity gravity;

    private void OnEnable(){
        OnEnableTileAnimator();
        this.gravity = this.GetComponent<EntityGravity>();
    }

    public override void OnSpawned(){
        if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask)){
            this.isUp = true;
            this.sp.flipY = true;
            this.gravity.SetUseGravity(false);
        }
    }

    private void Update(){
        if (this.isCaptured)
            return;

        if (this.canMove){
            CheckWand();
            CheckEdge();
            if (this.wasUp && !this.gravity.onGround)
                return;
            else if (this.wasUp && this.gravity.onGround)
                this.wasUp = false;

            float speed = -this.moveSpeed;/*Final walk Speed*/
            if (this.direction == 0){
                speed = -speed;
                if (!sp.flipX)
                    sp.flipX = true;
            }else if (sp.flipX)
                sp.flipX = false;

            this.transform.Translate(speed * Time.deltaTime, 0, 0);
        }

        this.canCapture = true;

        if (!this.isUp)
            return;

        this.canCapture = false;

        if (!Physics2D.Raycast(this.transform.position, Vector2.up, 0.5f, this.groundMask))
            Fall();

        foreach (PlayerController player in GameManager.instance.sceneManager.players){
            if((int)player.transform.position.x > (int)this.transform.position.x - 3 && (int)player.transform.position.x < (int)this.transform.position.x + 3 && player.transform.position.y < this.transform.position.y){
                    if (player.transform.position.x > this.transform.position.x)
                        this.direction = 0;
                    else
                        this.direction = 1;
                    Fall();
            }
        }
    }

    public void Fall(){
        this.GetComponent<EntityGravity>().X_onground = 0.2f;
        this.isUp = false;
        this.gravity.SetUseGravity(true);
        StopCurrentAnimation();
        this.wandMask = GameManager.instance.shellWandMask;
        this.moveSpeed = 10;
        StartAnimationClip(this.animationClips[1]);
        if (!this.isSpinny)
            this.gameObject.layer = 18;
        this.canMove = true;
        this.currentSheelState = ShellState.MovingShell;
        this.wasUp = true;
    }

    public override void CheckEdge(){
        if (!this.isUp)
            return;

        float X = 0.3f;
        if (direction == 1)
            X = -X;

        if (!Physics2D.Raycast(this.transform.position + new Vector3(X, 0, 0f), Vector2.up, 0.5f, this.groundMask)){
            if (direction == 1)
                direction = 0;
            else
                direction = 1;
        }
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

         if (p.isInSpin){
            p.Jump(-1, true);
            if (!this.isSpinny | this.sp.flipY){
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
                GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
                eff.transform.position = this.transform.position;
                UnlockKey();
                Destroy(this.gameObject);
            }else{
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
                GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
                eff.transform.position = this.transform.position;
            }
            return;
        }

        if (this.isSpinny && !this.sp.flipY){
            p.Damage();
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

        if(this.currentSheelState == ShellState.NoShell){
            StopCurrentAnimation();
            if(this.isSpinny)
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(101, TileManager.TilesetType.EnemyTileset);
            else
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(93, TileManager.TilesetType.EnemyTileset);
            this.wandMask = GameManager.instance.shellWandMask;
            this.moveSpeed = 13;
        }

        TriggerShell(p);
    }

    public void TriggerShell(PlayerController p){
        StopCoroutine("WaitForExitShellIE");
        this.GetComponent<EntityGravity>().X_onground = 0.2f;
        this.transform.rotation = Quaternion.Euler(0, 0, 0);
        if (this.currentSheelState == ShellState.NoShell){
            this.gravity.X_onground = 0.1f;
            this.currentSheelState = ShellState.StandShell;
        }else if (this.currentSheelState == ShellState.MovingShell)
            this.currentSheelState = ShellState.StandShell;
        else
        {
            this.wandMask = GameManager.instance.shellWandMask;
            this.currentSheelState = ShellState.MovingShell;
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[0], 0.3f);
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[1], 0.3f);
        }

        if (this.currentSheelState == ShellState.StandShell){
            StopCurrentAnimation();
            if (this.isSpinny)
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(101, TileManager.TilesetType.EnemyTileset);
            else
                this.sp.sprite = TileManager.instance.GetSpriteFromTileset(93, TileManager.TilesetType.EnemyTileset);
            this.canDespawn = true;
            if(!this.isSpinny | this.sp.flipY)
                this.gameObject.layer = 31;
            this.canMove = false;
        }else{
            if(p.currentGrabedObject != this.gameObject)
                StartAnimationClip(this.animationClips[1]);
            if (!this.isSpinny | this.sp.flipY)
                this.gameObject.layer = 18;
            this.canMove = true;
            if (p.transform.position.x > this.transform.position.x + 0.1f)
                this.direction = 1;
            else
                this.direction = 0;
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
                    if (raycastHit2D.collider.gameObject.GetComponent<Entity>() != null && (raycastHit2D.collider.gameObject.GetComponent<Entity>().canDieFromShell| raycastHit2D.collider.gameObject.GetComponent<Entity>().canHitFromIceBall)){
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
        if (this.isSpinny)
            return;

        GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
        eff.transform.position = this.transform.position;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        TriggerShell(p);
    }

    public void ExitShell(){
        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            if (p.currentGrabedObject == this.gameObject){
                p.UngrabCurrentObject(true);
                p.Damage();
            }
        }
        this.wandMask = GameManager.instance.entityWandMask;
        this.StartAnimationClip(this.animationClips[0]);
        this.currentSheelState = ShellState.NoShell;
        this.canMove = true;
        this.canDespawn = true;
        this.gameObject.layer = 14;

        this.moveSpeed = 1.7f;
    }

}
