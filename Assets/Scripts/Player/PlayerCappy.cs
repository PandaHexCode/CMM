using System;
using System.Collections;
using System.Collections.Generic;
using UMM.BlockData;
using UnityEngine;

public class PlayerCappy : MonoBehaviour{/*SMO Powerup Cappy*/

    private float speed = 14f;

    private float startX = 0;
    private bool isMove = true;
    private bool isMovesBack = false;
    private bool isInCapturePower = false;
    private float yTarget = 0;

    private SpriteRenderer sp;
    private Transform _transform;
    private Transform player;
    private Entity capturedEnemy = null;

    [System.NonSerialized]public Vector3 cappyCapturePos = new Vector3(0.1f, 0.02f, 0);
    [System.NonSerialized]public bool canUncapture = true;

    private void OnEnable(){
        if (this.transform.parent == null)
            return;

        this._transform = this.transform;
        this.player = this._transform.parent;
        this._transform.localPosition = new Vector3(0.1f, 0.94f, 0);
        this._transform.SetParent(null, true);
        this.startX = this._transform.position.x;
        this.sp = GetComponent<SpriteRenderer>();
        this.player.GetComponent<PlayerController>().StopAnimation();
        this.player.GetComponent<PlayerController>().currentAnimation = PlayerController.AnimationID.Stand;
        if (this.player.GetComponent<PlayerController>().GetOnGround() && ((!sp.flipX && this.player.GetComponent<PlayerController>().GetCanMoveRight()) | (sp.flipX && this.player.GetComponent<PlayerController>().GetCanMoveLeft())))
            this.yTarget = this._transform.position.y - 1f;
        else
            this.yTarget = this._transform.position.y;

        if ((this.sp.flipX && this.speed > 0) | (!this.sp.flipX && this.speed < 0))
            this.speed = -this.speed;

        this.isMove = true;
    }

    private void OnDisable(){
        this.player.GetComponent<PlayerController>().StopAnimation();
        this.player.GetComponent<PlayerController>().currentAnimation = PlayerController.AnimationID.Stand;
        try{
            if (this.transform.parent == null)
                this.transform.SetParent(this.player);
        }catch (Exception e){
        }
        UncaptureEnemy();
        this.GetComponent<SpriteRenderer>().enabled = true;
        this.isMove = false;
        this.isMovesBack = false;
    }

    private void Update(){
        if(this.player.GetComponent<PlayerController>().GetPowerup() != PlayerController.Powerup.Smo)
            this.gameObject.SetActive(false);

        if (this.capturedEnemy != null){
            this.sp.flipX = this.player.GetComponent<SpriteRenderer>().flipX;
            if (this.sp.flipX)
                this.transform.localPosition = new Vector3(-this.cappyCapturePos.x, this.cappyCapturePos.y, 0);
            else
                this.transform.localPosition = new Vector3(this.cappyCapturePos.x, this.cappyCapturePos.y, 0);

            this.capturedEnemy.GetComponentInChildren<SpriteRenderer>().flipX = !this.player.GetComponent<SpriteRenderer>().flipX;
        }

        if (!this.isMove && !this.isMovesBack)
            return;
        else if (this.isMovesBack){
            if((int)this._transform.position.x == (int)this.player.transform.position.x && (int)this._transform.position.y == (int)this.player.transform.position.y){
                StopCappy();
                if(!this.player.GetComponent<PlayerController>().isInCapture)
                    this.gameObject.SetActive(false);
                return;
            }

            if ((int)this._transform.position.x != (int)this.player.transform.position.x){
                if (this._transform.position.x > this.player.transform.position.x)
                    this._transform.Translate(-55 * Time.deltaTime, 0, 0);
                else
                    this._transform.Translate(55 * Time.deltaTime, 0, 0);
            }

            if ((int)this._transform.position.y != (int)this.player.transform.position.y){
                if (this._transform.position.y > this.player.transform.position.y)
                    this._transform.Translate(0, -55 * Time.deltaTime, 0);
                else
                    this._transform.Translate(0, 55 * Time.deltaTime, 0);
            }
        }

        Vector2 vt = Vector2.right;
        if (this.sp.flipX)
            vt = Vector2.left;
        
        RaycastHit2D ray = Physics2D.Raycast(_transform.position, vt, 0.5f, GameManager.instance.entityWandMask);
        if (vt == Vector2.left && ray && ray.collider.gameObject.layer == 27 && ray.collider.gameObject.transform.eulerAngles.z != 0)
            ray = new RaycastHit2D();
        else if (vt == Vector2.right && ray && ray.collider.gameObject.layer == 27 && ray.collider.gameObject.transform.eulerAngles.z != 180)
            ray = new RaycastHit2D();

        if (ray && ray.collider.gameObject.GetComponent<Entity>() != null)
            CaptureEnemy(ray.collider.gameObject.GetComponent<Entity>());

        if (ray){
            bool t = SceneManager.CheckBlockToUseOrDestroy(ray.collider.gameObject, false);
            if (t)
                this.isMovesBack = true;
        }

        if ((!this.sp.flipX && this._transform.position.x > (this.startX + 7)) | (this.sp.flipX && this._transform.position.x < (this.startX - 7)) | ray)
            StartCoroutine(StartGoBackIE());

        this._transform.Translate(this.speed * Time.deltaTime, 0, 0);
        if(this.yTarget < this._transform.position.y)
            this._transform.Translate(0, -10 * Time.deltaTime, 0);
    }

    private IEnumerator StartGoBackIE(){
        this.isMove = false;
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 15; i++){
            this.sp.flipX = !this.sp.flipX;
            yield return new WaitForSeconds(0.15f);
        }
        this.isMovesBack = true;
    }

    public void StopCappy(){
        if (this.player.GetComponent<PlayerController>().GetPowerup() != PlayerController.Powerup.Smo)
            return;

        this.isMove = false;
        this.isMovesBack = false;
        if (this.player.GetComponent<PlayerController>().playerType == PlayerController.PlayerType.MARIO)
            this.player.GetComponent<PlayerController>().currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSprites.smoMario;
        else if (this.player.GetComponent<PlayerController>().playerType == PlayerController.PlayerType.LUIGI)
            this.player.GetComponent<PlayerController>().currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSpritesLuigi.smoMario;
        else
            this.player.GetComponent<PlayerController>().currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSpritesToad.smoMario;
        this.player.GetComponent<SpriteRenderer>().sprite = this.player.GetComponent<PlayerController>().currentPlayerSprites.stand[0];
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.transform == this.player){
            if (this.player.GetComponent<PlayerController>().GetOnGround() && !this.isMove && !this.isMovesBack){
                StopCappy();
                this.gameObject.SetActive(false);
            }else if (!this.player.GetComponent<PlayerController>().GetOnGround() && !this.isMovesBack)
                this.player.GetComponent<PlayerController>().Jump();
        }else if(collision.gameObject.GetComponent<Entity>() != null && !collision.isTrigger)
            CaptureEnemy(collision.gameObject.GetComponent<Entity>());
        else if(collision.gameObject.GetComponent<Checkpoint>() != null && Checkpoint.currentCheckpoint != collision.gameObject.GetComponent<Checkpoint>().myNumber)
            collision.gameObject.GetComponent<Checkpoint>().CollectCheckPoint();
        else if(collision.gameObject.GetComponent<Coin>())
            collision.gameObject.GetComponent<Coin>().Collect();
    }

    public void CaptureEnemy(Entity enemy){
        if (this.capturedEnemy != null | !enemy.canCapture | enemy.isFreezedFromIceBall)
            return;

        StopCappy();
        enemy.isCaptured = true;
        enemy.GetComponent<BoxCollider2D>().enabled = false;
        if (enemy.GetComponents<BoxCollider2D>().Length > 1)
            enemy.GetComponents<BoxCollider2D>()[1].enabled = false;
        if (enemy.GetComponent<EntityGravity>() != null)
            enemy.GetComponent<EntityGravity>().enabled = false;

        if (!enemy.bigIceBlock && !enemy.biggestIceBlock)
            this.player.GetComponent<PlayerController>().SetCollision(20);

        this.GetComponent<BoxCollider2D>().enabled = false;
        this._transform.SetParent(this.player);
        this.cappyCapturePos = enemy.GetComponent<Entity>().cappyCapturePos;
        enemy.GetComponent<Entity>().UnStack();
        this.player.position = enemy.transform.position + new Vector3(0, 0.5f, 0);
        this.player.GetComponent<PlayerController>().isInCapture = true;
        this.player.GetComponent<PlayerController>().canThrowCappy = false;
        this.player.GetComponent<SpriteRenderer>().enabled = false;
        this.player.GetComponent<PlayerController>().SetCurrentCaptureID(enemy.gameObject);
        enemy.transform.SetParent(this.player);
        if(!enemy.bigIceBlock)
            enemy.transform.localPosition = new Vector3(0, -0.5f, 0);
        else
            enemy.transform.localPosition = new Vector3(0, -0.3f, 0);
        DisableExtraEnemyFeatures(enemy);

        this.capturedEnemy = enemy;
    }

    private void DisableExtraEnemyFeatures(Entity enemy){
        if (enemy.captureID == BlockID.THWOMP){
            enemy.GetComponent<Thwomp>().StopAllActions();
            return;
        }
        if(enemy.captureID == BlockID.FIRE_PIRANHA){
            enemy.GetComponent<FirePiranha>().StopAllCoroutines();
            enemy.GetComponent<FirePiranha>().StartAnimationClip(enemy.GetComponent<FirePiranha>().animationClips[0]);
            return;
        }
        if(enemy.captureID == BlockID.BOO){
            enemy.GetComponent<Boo>().sp.sprite = TileManager.instance.currentStyle.enemyTileset[26];
            return;
        }
        if (enemy.captureID == BlockID.GROUNDBOO){
            enemy.GetComponent<GroundBoo>().StopAllCoroutines();
            enemy.GetComponent<GroundBoo>().StopCurrentAnimation();
            enemy.GetComponent<SpriteRenderer>().sprite = TileManager.instance.currentStyle.enemyTileset[46];
            return;
        }
        if(enemy.captureID == BlockID.HAMMERBRO){
            enemy.GetComponent<HammerBro>().StopAllCoroutines();
            enemy.GetComponent<HammerBro>().StartAnimationClip(enemy.GetComponent<HammerBro>().animationClips[0]);
            if (enemy.GetComponent<HammerBro>().currentHammer != null)
                Destroy(enemy.GetComponent<HammerBro>().currentHammer);
            if (enemy.GetComponent<HammerBro>().isBig)
                enemy.transform.localPosition = Vector3.zero;
            return;
        }
        if(enemy.captureID == BlockID.BUZZYBEETLE){
            if (enemy.GetComponent<BuzzleBeetle>().currentSheelState == BuzzleBeetle.ShellState.MovingShell | enemy.GetComponent<BuzzleBeetle>().currentSheelState == BuzzleBeetle.ShellState.StandShell)
                enemy.GetComponent<BuzzleBeetle>().ExitShell();
            return;
        }
        if (enemy.captureID == BlockID.BOWSER){
            enemy.GetComponent<Bowser>().StopAllCoroutines();
            enemy.GetComponent<Bowser>().StartAnimationClip(enemy.GetComponent<Bowser>().animationClips[0]);
            enemy.GetComponent<Bowser>().canShoot = true;
            enemy.transform.localPosition = Vector3.zero;
            return;
        }
    }

    private void RestoreExtraEnemyFeatures(Entity enemy){
        if (enemy.captureID == BlockID.FIRE_PIRANHA){
            enemy.GetComponent<FirePiranha>().StartCoroutine(enemy.GetComponent<FirePiranha>().FireBallIE());
            return;
        }
        if (enemy.captureID == BlockID.GROUNDBOO){
            enemy.GetComponent<GroundBoo>().StartCoroutine(enemy.GetComponent<GroundBoo>().GroundBooIE1());
            return;
        }
        if(enemy.captureID == BlockID.BILLENEMY){
            if (this.player.GetComponent<SpriteRenderer>().flipX && enemy.GetComponent<BillEnemy>().speed > 0){
                enemy.GetComponent<BillEnemy>().speed = -enemy.GetComponent<BillEnemy>().speed;
                enemy.GetComponent<SpriteRenderer>().flipX = false;
            }else if (!this.player.GetComponent<SpriteRenderer>().flipX && enemy.GetComponent<BillEnemy>().speed < 0){
                enemy.GetComponent<BillEnemy>().speed = -enemy.GetComponent<BillEnemy>().speed;
                enemy.GetComponent<SpriteRenderer>().flipX = true;
            }
            return;
        }
        if (enemy.captureID == BlockID.HAMMERBRO){
            enemy.GetComponent<HammerBro>().StartAction();
            return;
        }
        if (enemy.captureID == BlockID.BOWSER){
            enemy.GetComponent<Bowser>().StartAction();
            return;
        }
    }

    public void UncaptureEnemy(bool killEnemy = false){
        if (this.player == null)
            return;

        if (LevelEditorManager.instance != null && !LevelEditorManager.instance.isPlayMode)
            this.canUncapture = true;

        if (!this.canUncapture)
            return;

        this.player.GetComponent<PlayerController>().SetCurrentCaptureID(null);
        this.player.GetComponent<PlayerController>().normalGravityScale = 3.15f;
        this.player.GetComponent<PlayerController>().fallGravityScale = 6;
        if (this.player.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Smo)
            this.player.GetComponent<PlayerController>().SetCollision(1);

        if (killEnemy | (LevelEditorManager.instance != null && !LevelEditorManager.instance.isPlayMode)){
            this.GetComponent<BoxCollider2D>().enabled = true;
            this.player.GetComponent<PlayerController>().canThrowCappy = true;
            this.player.GetComponent<PlayerController>().isInCapture = false;
            if(this.capturedEnemy != null)
                Destroy(this.capturedEnemy.gameObject);
            return;
        }

        if (this.capturedEnemy != null){
            if (LevelEditorManager.instance != null && !LevelEditorManager.instance.isPlayMode)
                Destroy(this.capturedEnemy.gameObject);
            else
                this.capturedEnemy.transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
        }

        if(this.gameObject.active)
            StartCoroutine(UnCaptureEnemyIE(this.capturedEnemy));
        else{
            this.GetComponent<BoxCollider2D>().enabled = true;
            this.player.GetComponent<PlayerController>().canThrowCappy = true;
            this.player.GetComponent<PlayerController>().isInCapture = false;
        }

        this.capturedEnemy = null;
    }

    public IEnumerator UnCaptureEnemyIE(Entity enemy){
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.player.GetComponent<SpriteRenderer>().enabled = true;
        this.player.GetComponent<PlayerController>().canGetDamage = false;
        this.player.GetComponent<PlayerController>().IgnoreAllEnemiesDamageCollisions();
        this.player.GetComponent<PlayerController>().StartCoroutine(this.player.GetComponent<PlayerController>().DamageAnimation());
        this.player.GetComponent<PlayerController>().Jump();
        yield return new WaitForSeconds(0.3f);

        if (enemy != null){
            enemy.isCaptured = false;
            enemy.GetComponent<BoxCollider2D>().enabled = true;
            if (enemy.GetComponents<BoxCollider2D>().Length > 1)
                enemy.GetComponents<BoxCollider2D>()[1].enabled = true;
            if (enemy.GetComponent<EntityGravity>() != null)
                enemy.GetComponent<EntityGravity>().enabled = true;
            if (this.player.GetComponent<SpriteRenderer>().flipX)
                enemy.direction = 1;
            else
                enemy.direction = 0;
            RestoreExtraEnemyFeatures(enemy);
        }

        if (this.capturedEnemy == null){
            this.GetComponent<BoxCollider2D>().enabled = true;
            this.player.GetComponent<PlayerController>().canThrowCappy = true;
            this.player.GetComponent<PlayerController>().isInCapture = false;

            this.gameObject.SetActive(false);
        }
    }

}
