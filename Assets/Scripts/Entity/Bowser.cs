using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowser : Entity{

    public float extraSeconds = 0;
    
    private float lives = 20;
    private float speed = 1f;
    private float startX;

    private TileManager.StyleID styleID;
    private Transform _transform;
    private Transform playerTrans;

    private void OnEnable(){
        OnEnableTileAnimator();
        this._transform = this.transform;
        this.playerTrans = GameManager.instance.sceneManager.players[0].transform;
        this.styleID = TileManager.instance.currentStyleID;
        StartAction();
    }

    public void StartAction(){
        this.startX = this._transform.position.x;
        StartCoroutine(ActionIE());
    }

    private void Update(){
        
    }

    private IEnumerator ActionIE(){
        while (!this.isSpawned){
            yield return new WaitForSeconds(0);
        }

        if (this.styleID == TileManager.StyleID.SMW)
            StartAnimationClip(this.animationClips[4]);
        else
            StartAnimationClip(this.animationClips[0]);

        StartCoroutine(MoveIE());
        if(this.styleID == TileManager.StyleID.SMB1 | this.styleID == TileManager.StyleID.SMAS1){
            while (true){
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(-1));
                yield return new WaitForSeconds(0.7f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.7f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(-1));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(0.7f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(-1));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.7f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.7f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(-1));
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
            }
        }else if(this.styleID == TileManager.StyleID.SMB3 | this.styleID == TileManager.StyleID.SMAS3){
            int t = 0;
            while (true){ 
                for (int i = 0; i < 3; i++){
                    yield return new WaitForSeconds(1.1f + this.extraSeconds);
                    StartCoroutine(JumpIE());
                    yield return new WaitForSeconds(0.3f + this.extraSeconds);
                    float z = 0;
                    if (i == 1)
                        z = 0.4f;
                    if (t == 1 && i == 0)
                        z = -1;
                    else if (t == 2 && i == 1)
                        t = 1;
                    StartCoroutine(StartShootFireIE(z));
                    yield return new WaitForSeconds(0.6f + this.extraSeconds);
                }
                GameManager.instance.StartCoroutine(TryToJumpToPlayer(this.playerTrans.position.x));
                while (!this.GetComponent<EntityGravity>().onGround | this.currentAnimation != 3){
                    yield return new WaitForSeconds(0);
                }

                yield return new WaitForSeconds(1.5f);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(2.3f + this.extraSeconds);
                if(!this.sp.flipX)
                    GameManager.instance.StartCoroutine(TryToJumpToPlayer(this._transform.position.x - 5));
                else
                    GameManager.instance.StartCoroutine(TryToJumpToPlayer(this._transform.position.x + 5));
                while (!this.GetComponent<EntityGravity>().onGround){
                    yield return new WaitForSeconds(0);
                }
                yield return new WaitForSeconds(0.6f + this.extraSeconds);
                if (t == 0)
                    t = 1;
                else if (t == 1)
                    t = 2;
                else
                    t = 0;
            }
        }else if(this.styleID == TileManager.StyleID.SMW){
            int t = 0;
            while (true){
                yield return new WaitForSeconds(this.extraSeconds);
                if(t == 0 | t == 2)
                    StartCoroutine(StartShootFireIE(1));
                else
                    StartCoroutine(StartShootFireIE(-1));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(1));
                yield return new WaitForSeconds(0.9f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0.5f));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0.5f));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(StartFireRainIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                while (this.currentAnimation == 6 | this.currentAnimation == 7)
                    yield return new WaitForSeconds(0);
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0.5f));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(JumpIE());
                yield return new WaitForSeconds(0.3f + this.extraSeconds);
                StartCoroutine(StartShootFireIE(0.5f));
                yield return new WaitForSeconds(1.1f + this.extraSeconds);
                StartCoroutine(TryToJumpToPlayer(this.playerTrans.position.x + 4));
                if (t == 0)
                    t = 1;
                else if (t == 1)
                    t = 2;
                else
                    t = 0;
            }
        }
    }

    private bool wasPlayerXBigger = false; 
    private IEnumerator MoveIE(){
        float targetX = startX - 5;
        while (this._transform.position.x > targetX && !CheckWand(Vector2.left)){
            this._transform.Translate(-this.speed * Time.deltaTime, 0, 0);
            if (this.playerTrans.position.x > this._transform.position.x && !this.wasPlayerXBigger){
                this.sp.flipX = true;
                this.wasPlayerXBigger = true;
                break;
            }else if (this.wasPlayerXBigger && this.playerTrans.position.x < this._transform.position.x){
                this.wasPlayerXBigger = false;
                this.sp.flipX = false;
            }
            yield return new WaitForSeconds(0);
        }

        targetX = startX + 5;
     
        while (this._transform.position.x < targetX  && !CheckWand(Vector2.right)){
            this._transform.Translate(this.speed * Time.deltaTime, 0, 0);
            if (this.playerTrans.position.x < this._transform.position.x && this.sp.flipX && this.wasPlayerXBigger){
                this.sp.flipX = false;
                this.wasPlayerXBigger = false;
                break;
            }else if (!this.wasPlayerXBigger && this.playerTrans.position.x > this._transform.position.x){
                this.wasPlayerXBigger = true;
                this.sp.flipX = true;
            }
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(0);
        StartCoroutine(MoveIE());
    }

    private bool CheckWand(Vector2 vt){
        RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position + new Vector3(0.2f, 0.3f, 0), vt, 1f, this.wandMask);
        RaycastHit2D raycastHit2D2 = Physics2D.Raycast(_transform.position + new Vector3(0.2f, -0.3f, 0), vt, 1f, this.wandMask);
        if (raycastHit2D |raycastHit2D2)
            return true;

        return false;
    }

    public bool canShoot = true;
    public IEnumerator StartShootFireIE(float offsetY, bool friendly = false){
        if (friendly)
            this.canShoot = false;

        this.speed = 0;
        if (this.styleID == TileManager.StyleID.SMW)
            StartAnimationClip(this.animationClips[5]);
        else
            StartAnimationClip(this.animationClips[1]);
        yield return new WaitForSeconds(0.3f);
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.explode);
        GameObject fire = null;
        if (!friendly)
            fire = Instantiate(GameManager.instance.sceneManager.bowserFire);
        else
            fire = Instantiate(GameManager.instance.sceneManager.friendlyBowserFire);

        if (!this.sp.flipX){
            fire.transform.position = this._transform.position + new Vector3(-1.3f, 0.4f, 0);
            fire.GetComponent<Bone>().speed = -fire.GetComponent<Bone>().speed;
        }else{
            fire.transform.position = this._transform.position + new Vector3(1.3f, 0.4f, 0);
            fire.GetComponent<SpriteRenderer>().flipX = true;
        }
        StartCoroutine(FireOffset(fire, offsetY));
        fire.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(110, TileManager.TilesetType.EnemyTileset);
        SceneManager.destroyAfterNewLoad.Add(fire);
        yield return new WaitForSeconds(0.3f);
        this.speed = 1;

        if(this.styleID == TileManager.StyleID.SMW)
            StartAnimationClip(this.animationClips[4]);
        else
            StartAnimationClip(this.animationClips[0]);

        if (friendly){
            yield return new WaitForSeconds(0.5f);
            this.canShoot = true;
        }
    }

    private IEnumerator FireOffset(GameObject fire, float offsetY){
        float targetY = fire.transform.position.y + offsetY;
        if (targetY > fire.transform.position.y){
            while (fire.transform.position.y < targetY) {
                fire.transform.Translate(0, 5 * Time.deltaTime, 0);
                yield return new WaitForSeconds(0);
            }
        }else{
            while (fire.transform.position.y > targetY) {
                fire.transform.Translate(0, -5 * Time.deltaTime, 0);
                yield return new WaitForSeconds(0);
            }
        }
    }

    public IEnumerator JumpIE(){
        this.GetComponent<EntityGravity>().enabled = false;
        float targetY = this._transform.position.y + 3;
        while (this._transform.position.y < targetY){
            if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask))
                break;
            this.transform.Translate(0, 10 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }
        this.GetComponent<EntityGravity>().enabled = true;
        this.GetComponent<EntityGravity>().onGround = false;
    }

    public IEnumerator TryToJumpToPlayer(float playerX){/*For SMB3 & SMAS3 & sometimes for SMW*/
        this.speed = 0;
        float targetY = this._transform.position.y + 8;

        StartCoroutine(TryToJumpToPlayer_Y());
        if(playerX < this._transform.position.x){
            while(playerX < this._transform.position.x && this._transform.position.y < targetY){
                if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask))
                    break;
                if (!CheckWand(Vector2.left))
                    this._transform.Translate(-18 * Time.deltaTime, 0, 0);
                yield return new WaitForSeconds(0);
            }
        }else{
            while(playerX > this._transform.position.x && this._transform.position.y < targetY){
                if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask))
                    break;
                if (!CheckWand(Vector2.right))
                    this._transform.Translate(18 * Time.deltaTime, 0, 0);
                yield return new WaitForSeconds(0);
            }
        }

        while (this.speed != 0.1f)
            yield return new WaitForSeconds(0);

        this.speed = 0;

        yield return new WaitForSeconds(0.05f);

        this.GetComponent<EntityGravity>().enabled = true;
        if (this.styleID != TileManager.StyleID.SMW) {
            StopCurrentAnimation();
            StartAnimationClip(this.animationClips[2]);
            this.GetComponent<EntityGravity>().gravity = 18;
            this.GetComponent<EntityGravity>().onGround = false;

            while (!this.GetComponent<EntityGravity>().onGround){
                if (this.styleID != TileManager.StyleID.SMW)
                    StartAnimationClip(this.animationClips[2]);
                yield return new WaitForSeconds(0);
            }

            StartAnimationClip(this.animationClips[3]);
            RaycastHit2D ray1 = Physics2D.Raycast(_transform.position + new Vector3(0.4f, -0.71f, 0f), Vector2.down, 0.5f, GameManager.instance.entityGroundMask);
            RaycastHit2D ray2 = Physics2D.Raycast(_transform.position + new Vector3(-0.4f, -0.71f, 0f), Vector2.down, 0.5f, GameManager.instance.entityGroundMask);
            if (ray1 | ray2){
                if(ray1)
                    SceneManager.CheckBlockToUseOrDestroy(ray1.collider.gameObject, false);
                if(ray2)
                    SceneManager.CheckBlockToUseOrDestroy(ray2.collider.gameObject, false);
            }

            foreach (PlayerController p in GameManager.instance.sceneManager.players)
                p.StartFreezingFromStamp();

            yield return new WaitForSeconds(1f);
            this.GetComponent<EntityGravity>().gravity = 8;
            StartAnimationClip(this.animationClips[0]);
        }

        this.speed = 1;
    }

    private IEnumerator TryToJumpToPlayer_Y(){
        this.GetComponent<EntityGravity>().enabled = false;
        float targetY = this._transform.position.y + 8;
        while (this._transform.position.y < targetY){
            if (Physics2D.Raycast(this.transform.position, Vector2.up, 1f, this.groundMask))
                break;
            this.transform.Translate(0, 13 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }
        this.speed = 0.1f;
    }

    private IEnumerator StartFireRainIE(){/*for SMW*/
        this.speed = 0;
        StartAnimationClip(this.animationClips[7]);
        while (this.currentAnimation == 7)
            yield return new WaitForSeconds(0);
        StartAnimationClip(this.animationClips[6]);

        float playerPos = this.playerTrans.transform.position.x - 2;

        for (int i = 0; i < 5; i++){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.explode);
            GameObject fire = Instantiate(GameManager.instance.sceneManager.bowserFire);
            SceneManager.destroyAfterNewLoad.Add(fire);
            if (!this.sp.flipX){
                fire.transform.position = this._transform.position + new Vector3(-1f, 1f, 0);
                fire.transform.rotation = Quaternion.Euler(0, 0, -60 - i);
            }else{ 
                fire.transform.position = this._transform.position + new Vector3(1f, 1f, 0);
                fire.transform.rotation = Quaternion.Euler(0, 0, -120 - i);
            }
            fire.GetComponent<Bone>().speed = -fire.GetComponent<Bone>().speed;
            StartCoroutine(BowserFallFireControllIE(fire, playerPos + (i * 2)));
            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(0);
        StartAnimationClip(this.animationClips[4]);
        this.speed = 1;
    }

    private IEnumerator BowserFallFireControllIE(GameObject fire, float posX){
        while (fire != null)
            yield return new WaitForSeconds(0);

        yield return new WaitForSeconds(0.1f);

        GameObject fallFire = Instantiate(GameManager.instance.sceneManager.SMWBowserFallFire);
        SceneManager.destroyAfterNewLoad.Add(fallFire);
        fallFire.transform.position = new Vector3(posX, this.playerTrans.transform.position.y + 11);

        while (!fallFire.GetComponent<EntityGravity>().onGround)
            yield return new WaitForSeconds(0);

        yield return new WaitForSeconds(2);
        GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff.transform.position = fallFire.transform.position;
        Destroy(fallFire);
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9 && this.damageOnCollision && !this.isFreezedFromIceBall){
            PlayerController p = collision.gameObject.GetComponent<PlayerController>();
            this.OnDamagePlayer(p);
        }else if(collision.gameObject.layer == 10){
            collision.gameObject.GetComponent<FireBall>().Explode();
            DamageBowser(1, collision.gameObject);
        }
    }

    public override void DieFromShell(GameObject shell){
        shell.GetComponent<Entity>().StartCoroutine(shell.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
        DamageBowser(1, shell);
    }

    private IEnumerator HitColorAnimationIE(){
        this.sp.color = new Color(1, 0.5424528f, 0.5424528f);
        yield return new WaitForSeconds(0.2f);
        this.sp.color = Color.white;
    }

    public void DamageBowser(float damage, GameObject gameObject = null){
        if (gameObject == null)
            gameObject = this.gameObject;

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        this.lives = this.lives - damage;
        StartCoroutine(HitColorAnimationIE());
        if(this.lives < 1){
            StopCoroutine("ActionIE");
            StopCoroutine("MoveIE");
            StartCoroutine(ShootDieAnimation(gameObject));
            UnlockKey();
        }
    }

}
