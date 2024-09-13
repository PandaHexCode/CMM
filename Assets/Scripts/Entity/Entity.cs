using UnityEngine;
using System.Collections;
using UMM.BlockData;

public class Entity : TileAnimator{

    [Header("Entity")]
    public bool canMove = true;
    public float moveSpeed = 4f;
    public bool damageOnCollision = false;
    public bool canDieFromFireBall = true;
    public bool canDieFromShell = true;
    public bool canHitFromIceBall = true, bigIceBlock = false, biggestIceBlock = false;
    public bool cantFallAtEdges = false;
    public bool canStack = true;
    public bool canCapture = true;
    public BlockID captureID = BlockID.ERASER;
    public bool canDespawn = true;
    public bool killNotRespawn;
    public bool seeEnemiesAsWall = true;
    public Vector3 cappyCapturePos = new Vector3(0.1f, 0.02f, 0);
    public int startDirection = 1;
    public bool dontFlip = false;

    [System.NonSerialized]public int direction;
    [System.NonSerialized]public bool isSpawned = false;
    [System.NonSerialized]public bool isCaptured = false;
    [System.NonSerialized]public bool isFreezedFromIceBall = false;
    [System.NonSerialized]public bool hasKey = false;
    [System.NonSerialized]public bool isInStack = false;/*doesnt count for the main parent*/
    private bool isInSpawnZone = false;
    private SpriteRenderer sp = null;
    private EntityGravity entityGravity = null;
    private BoxCollider2D bx1 = null;
    private BoxCollider2D bx2 = null;
    private Transform _transform;
    private int orgLayer = 14;
    [System.NonSerialized]public LayerMask wandMask;
    [System.NonSerialized]public LayerMask groundMask;
    private LayerMask enemyWallMask;
    private Coroutine iceBlockCor = null;

    private int lastAnimationClipBeforeFreeze = 0;

    private void Awake(){
        this.sp = GetComponentInChildren<SpriteRenderer>();
        this.direction = this.startDirection;
        if (this.GetComponent<EntityGravity>() != null)
            this.entityGravity = this.GetComponent<EntityGravity>();
        this.bx1 = this.GetComponents<BoxCollider2D>()[0];
        if (this.GetComponents<BoxCollider2D>().Length > 1)
            this.bx2 = this.GetComponents<BoxCollider2D>()[1];
        this._transform = this.transform;
        this.wandMask = GameManager.instance.entityWandMask;
        this.groundMask = GameManager.instance.entityGroundMask;
        this.enemyWallMask = GameManager.instance.lazyEnemyMask;
        if(damageOnCollision)
            SceneManager.enemiesDamageColliders.Add(bx1);
        this.Despawn();
    }

    public void UnStack(){
        if (!this.canStack)
            return;

        foreach(Transform child in this._transform){
            if (child != null && child.GetComponent<Entity>() != null){
                child.SetParent(this._transform.parent);
                if (child.parent.GetComponent<Entity>() == null){
                    child.GetComponent<Entity>().canMove = true;
                    if (child.GetComponent<EntityGravity>() != null)
                        child.GetComponent<EntityGravity>().enabled = true;
                    child.GetComponent<Entity>().isInStack = false;
                }else{
                    child.GetComponent<Entity>().StartCoroutine(child.GetComponent<Entity>().StackFallIE());
                }
            }
        }
    }

    public IEnumerator StackFallIE(){
        float startPosY = this._transform.position.y;
        while (this._transform.position.y > startPosY - 1){
            this._transform.Translate(0, -5 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }
    }

    private void OnDestroy(){
        UnStack();
    }

    public void UnlockKey(){
        if (this.hasKey){
            if ((LevelEditorManager.instance != null && !LevelEditorManager.instance.isPlayMode) | !GameManager.instance.sceneManager.players[0].enabled)
                return;
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.doorUnlock);
            GameObject key = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)UMM.BlockData.BlockID.KEY].prefarb);
            key.transform.position = this.transform.position;
            GameManager.instance.sceneManager.players[0].AddKey(key.GetComponentInChildren<Key>());
            this.hasKey = false;
            SceneManager.GetRespawnableEntityFromEntity(this.gameObject).hasKey = false;
        }
    }

    private void Update(){
        if (this.isCaptured)
            return;

        if (this.canMove){
            if (this.seeEnemiesAsWall)
                CheckEnemiesWall();
            CheckWand();
            if (this.cantFallAtEdges && this.entityGravity.onGround)
                CheckEdge();

            float speed = -this.moveSpeed;/*Final walk Speed*/
            if (this.direction == 0){
                speed = -speed;
                if(!sp.flipX  && !this.dontFlip)
                    sp.flipX = true;
            }else if(sp.flipX && !this.dontFlip)
                sp.flipX = false;

            _transform.Translate(speed * Time.deltaTime, 0, 0);
        }else if (this.isInStack){
            CheckStackWall();
        }
    }

    public virtual void CheckStackWall(){
        if (direction == 1){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.left, 0.3f, this.wandMask);
            if (raycastHit2D){
                this._transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
                this.canMove = true;
                if (this.entityGravity != null)
                    this.entityGravity.enabled = true;
            }
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.right, 0.3f, this.wandMask);
            if (raycastHit2D){
                this._transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
                this.canMove = true;
                if (this.entityGravity != null)
                    this.entityGravity.enabled = true;
            }
        }
    }

    public virtual void CheckEnemiesWall(){
        if (direction == 1){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.left, 0.4f, this.enemyWallMask);
            if (raycastHit2D){
                direction = 0;
                raycastHit2D.collider.GetComponent<Entity>().direction = 1;
            }
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.right, 0.4f, this.enemyWallMask);
            if (raycastHit2D){
                direction = 1;
                raycastHit2D.collider.GetComponent<Entity>().direction = 0;
            }
        }
    }

    public virtual void CheckWand(){/*For seting direction*/
        if (direction == 1){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.left, 0.5f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 0)
                        direction = 0;
                }else if(raycastHit2D.collider.gameObject.layer == 28){
                    if (!raycastHit2D.collider.gameObject.tag.Equals("GrabNoWand"))
                        direction = 0;
                }else
                    direction = 0;
            }
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.right, 0.5f, this.wandMask);
            if (raycastHit2D){
                if (raycastHit2D.collider.gameObject.layer == 27){
                    if (raycastHit2D.collider.gameObject.transform.eulerAngles.z == 180)
                        direction = 1;
                }else if(raycastHit2D.collider.gameObject.layer == 28){
                    if (!raycastHit2D.collider.gameObject.tag.Equals("GrabNoWand"))
                        direction = 1;
                }else
                    direction = 1;
            }
        }
    }

    public virtual void CheckEdge(){
        float X = 0.3f;
        if (direction == 1)
            X = -X;

        if (!Physics2D.Raycast(_transform.position + new Vector3(X, this.entityGravity.Y_onground, 0f), -Vector2.up, 0.5f, this.groundMask)){
            if (direction == 1)
                direction = 0;
            else
                direction = 1;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        this.OnCollOther(collision);
        if (collision.gameObject.layer == 9 && this.damageOnCollision && !this.isFreezedFromIceBall){
            PlayerController p = collision.gameObject.GetComponent<PlayerController>();
            this.OnDamagePlayer(p);
        }else if((collision.gameObject.layer == 10 && ((this.canDieFromFireBall && !collision.gameObject.GetComponent<FireBall>().isIceBall) | (this.canHitFromIceBall && collision.gameObject.GetComponent<FireBall>().isIceBall))) | (collision.gameObject.layer == 18 && this.canDieFromShell) ){
            if(this.canHitFromIceBall && collision.gameObject.layer == 10 && collision.gameObject.GetComponent<FireBall>().isIceBall && !this.isFreezedFromIceBall){
                this.lastAnimationClipBeforeFreeze = this.currentAnimation;
                StopCurrentAnimation();
                UnStack();

                if(this.GetComponent<KoopaTroopa>() != null){
                    this.GetComponent<KoopaTroopa>().StopCoroutine("WaitForExitShellIE");
                    this.transform.rotation = Quaternion.Euler(0, 0, 0);
                }

                GameObject block = Instantiate(GameManager.instance.sceneManager.iceFlowerBlock);
                if (this.bigIceBlock && !this.biggestIceBlock){
                    block.transform.localScale = new Vector3(1.5f, 1.5f, 0);
                }else if (this.biggestIceBlock)
                    block.transform.localScale = new Vector3(4f, 4f, 0);
                block.transform.position = this.transform.position;
                block.transform.SetParent(this.transform);
                this.canMove = false;
                if (this.bx2 != null)
                    this.bx2.enabled = false;
                this.isFreezedFromIceBall = true;
                this.orgLayer = this.gameObject.layer;
                this.gameObject.layer = 28;
                collision.gameObject.GetComponent<FireBall>().Explode();
                if (this.GetComponent<HammerBro>() != null){
                    if (this.GetComponent<HammerBro>().currentHammer != null)
                        Destroy(this.GetComponent<HammerBro>().currentHammer);
                    this.GetComponent<HammerBro>().StopAllCoroutines();
                }
                if (this.iceBlockCor != null)
                    StopCoroutine(this.iceBlockCor);
                this.iceBlockCor = StartCoroutine(DestroyIceBlockAuto());
                return;
            }

            if (this.canHitFromIceBall && this.isFreezedFromIceBall && collision.gameObject.layer == 10 && !collision.gameObject.GetComponent<FireBall>().isIceBall){
                DestroyIceBlock();
                collision.gameObject.GetComponent<FireBall>().Explode();
                return;
            }

            if (!this.isFreezedFromIceBall && (this.canDieFromFireBall | this.canDieFromShell))
                HitFromFireBall(collision.gameObject);
        }else if(collision.gameObject.layer == 10){
            collision.gameObject.GetComponent<FireBall>().Explode();
        }
    }

    [System.NonSerialized]public bool canTrigger = true;/*To fix multiple triggers at once*/
    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && this.canTrigger){
            this.canTrigger = false;
            StartCoroutine(SetCanTriggerBack());
            if (this.canStack){
                UnStack();
                this._transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
                this.canMove = true;
                if (this.entityGravity != null)
                    this.entityGravity.enabled = true;
            }
            this.OnTriggerPlayer(collision.gameObject.GetComponent<PlayerController>());
        }else if(collision.gameObject.layer == 19)
            this.isInSpawnZone = true;
        else 
            this.OnTriggerOther(collision);
    }

    public IEnumerator SetCanTriggerBack(){
        yield return new WaitForSeconds(0.1f);
        this.canTrigger = true;
    }

    public void DestroyIceBlock(){
        if (!this.isFreezedFromIceBall)
            return;

        if (this.iceBlockCor != null)
            StopCoroutine(this.iceBlockCor);
        GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff.transform.position = this._transform.position;

        if (this.startFirstClipAtStart)
            StartAnimationClip(this.animationClips[this.lastAnimationClipBeforeFreeze]);
        StopCoroutine("DestroyIceBlockAuto");
        foreach(Transform trans in this.transform){
            if(trans.gameObject.name.StartsWith("IceFlowerBlock"))
                Destroy(trans.gameObject);
        }
        this.canMove = true;
        if (this.bx2 != null)
            this.bx2.enabled = true;
        this.bx1.enabled = true;
        this.isFreezedFromIceBall = false;
        if (this.GetComponent<HammerBro>() != null)
            this.GetComponent<HammerBro>().StartAction();
        this.gameObject.layer = this.orgLayer;
        this.GetComponent<EntityGravity>().enabled = true;
        foreach(PlayerController p in GameManager.instance.sceneManager.players){
            if(p.currentGrabedObject == this.gameObject){
                p.UngrabCurrentObject(true);
                p.Damage();
            }
        }
    }

    private IEnumerator DestroyIceBlockAuto(){
        yield return new WaitForSeconds(8);
        if (this.isFreezedFromIceBall){ 
            for (int i = 0; i < 5; i++){
                while(this.transform.rotation.z > -0.1449779){
                    this.transform.Rotate(0, 0, -155 * Time.deltaTime);
                    if (!this.isFreezedFromIceBall)
                        break;
                    yield return new WaitForSeconds(0f);
                }

                while (this.transform.rotation.z < 0){
                    this.transform.Rotate(0, 0, 155 * Time.deltaTime);
                    if (!this.isFreezedFromIceBall)
                        break;
                    yield return new WaitForSeconds(0f);
                }

                while (this.transform.rotation.z < 0.1449779){
                    this.transform.Rotate(0, 0, 155 * Time.deltaTime);
                    if (!this.isFreezedFromIceBall)
                        break;
                    yield return new WaitForSeconds(0f);
                }

                while (this.transform.rotation.z > 0){
                    this.transform.Rotate(0, 0, -155 * Time.deltaTime);
                    if (!this.isFreezedFromIceBall)
                        break;
                    yield return new WaitForSeconds(0f);
                }

                this.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            this.iceBlockCor = null;
            DestroyIceBlock();
        }
    }

    public IEnumerator ShootDieAnimation(GameObject shootObject){
        UnStack();
        UnlockKey();
        this.transform.rotation = Quaternion.Euler(Vector3.zero);
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);

        if (this.GetComponentInParent<Animator>() != null)
            this.GetComponentInParent<Animator>().enabled = false;

        FireBall fireBall = null;
        if (shootObject.GetComponent<FireBall>() != null)
            fireBall = shootObject.GetComponent<FireBall>();

        float xOffset = 4;
        if(fireBall != null){
            if (fireBall.GetSpawner().GetComponent<SpriteRenderer>().flipX)
                xOffset = -xOffset;
            fireBall.Explode();
        }else if(this.sp.flipX)
            xOffset = -xOffset;

        if (this.entityGravity != null)
            this.entityGravity.SetUseGravity(false);

        if(this.bx1 != null)
            this.bx1.enabled = false;
        if (this.bx2 != null)
            this.bx2.enabled = false;

        this.sp.flipY = true;
        this.canMove = false;

        float targetY = this.transform.position.y + 1.5f;

        while (this.transform.position.y < targetY){
            this.transform.Translate(xOffset * Time.deltaTime, 12 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0f);
        }

        yield return new WaitForSeconds(0f);

        while(this.transform.position.y > 2){  
            this.transform.Translate(xOffset * Time.deltaTime, -15 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }

        Destroy(this.gameObject);
    }

    public void Despawn(){
        if (!this.canMove){
            this.isSpawned = true;
            return;
        }

        this.sp.enabled = false;
        this.isSpawned = false;
        this.canMove = false;
        if (this.entityGravity != null)
            this.entityGravity.enabled = false;
    }

    public void Spawn(){
        if (this.isSpawned)
            return;
        this.canMove = true;
        this.sp.enabled = true;
        this.isSpawned = true;
        if (this.entityGravity != null)
            this.entityGravity.enabled = true;

        RaycastHit2D ray1 = Physics2D.Raycast(_transform.position + new Vector3(0, -0.1f, 0f), Vector2.down, 1.5f, GameManager.instance.entityMask);
        if (this.canStack && ray1 && ray1.collider.gameObject.GetComponent<Entity>().canStack)
            Stack(ray1.collider.transform.gameObject);

        OnSpawned();
    }

    public virtual void OnSpawned(){
    }

    public void Stack(GameObject parent){
        this._transform.SetParent(parent.transform);
        this.isInStack = true;
        this.canMove = false;
        if(this.entityGravity != null)
            this.entityGravity.enabled = false;
    }

    public virtual void DieFromShell(GameObject shell){
        StartCoroutine(ShootDieAnimation(this.gameObject));
    }

    public virtual void OnDamagePlayer(PlayerController p){
        p.Damage();
    }
    public virtual void OnTriggerPlayer(PlayerController p){}

    public virtual void OnTriggerOther(Collider2D col){}

    public virtual void OnCollOther(Collision2D col){}

    public virtual void HitFromFireBall(GameObject fireball){
        StartCoroutine(ShootDieAnimation(fireball));
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11 && this.canDespawn && !this.isInSpawnZone){
            if (this.killNotRespawn)
                Destroy(this.gameObject);
            else
                SceneManager.GetRespawnableEntityFromEntity(this.gameObject).Respawn(true);
        }else if (collision.gameObject.layer == 19)
            this.isInSpawnZone = false;
    }
}