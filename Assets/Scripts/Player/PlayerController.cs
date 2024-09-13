using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using UMM.BlockData;
using UMM.Mystery;
using UnityEngine;
using UnityEngine.Profiling;

public class PlayerController : MonoBehaviour {

    public LayerMask groundMask;
    public LayerMask wallMask;
    public LayerMask duckMask;
    public GameObject cappy;
    public GameObject probellerDownEffect;private GameObject currentProbellerDownEffect = null;
    public Color[] starColors;private Color lastColor;
    public PlayerType playerType = PlayerType.MARIO;
    public Material toadMaterial;
    public Material normalMaterial;

    public enum PlayerType { MARIO = 0, LUIGI = 1, BLUETOAD = 2, REDTOAD = 3, GREENTOAD = 4};

    /*Speed Values etc*/
    public float playerWalkSpeed = 6f;
    public float playerRunSpeed = 12.5f;
    public float jumpVelocity = 16.5f;
    public float walkJumpVelocity = 18.5f;
    public float lowJumpMultiplier = 12f;
    public float fallmultiplier = 1f;
    public float normalGravityScale = 3.15f;
    public float fallGravityScale = 6;
    public float fireBallSpeed = 14f;
    public float jumpSpeedMultipler;

    /*Ray Values*/
    private float onGroundYRay = 0.01f;
    private Vector3 onGroundVector = Vector3.down;
    private float distanceXRay = 0.1f;
    private float distanceXRay2 = 0.1f;
    private float distanceYRay = 0f;
    private float distanceYRayU = 0f;

    private Vector2 increasedBoxColVt = new Vector2(0.05f, 0);

    public enum Powerup { Small, Big, Fire, Probeller, Acorn, Ice, Smash, Smo, Mini};
    private Powerup powerup;

    public enum MKItemSlotContent {NULL = 0, FEATHER = 1, ONOFFSWITCH = 2, };
    private MKItemSlotContent currentMKItemSlotContent = MKItemSlotContent.NULL;

    /*Check Values*/
    private bool isMoving = false;
    private bool isDuck = false;private bool isSmallDuck = false;
    private bool isIceGround = false;
    private bool isStar = false;
    private bool isMystery = false;
    private bool isInGrab = false;[System.NonSerialized]public GameObject currentGrabedObject = null;private int savedGrabedObjectSortingID;
    private bool isSpecialMystery = false;private int mysterySpecialID = 0;
    private bool isInBubble = false;
    private bool isInClimb = false;private GameObject climbedGameObject;
    private bool isIncreasedCol = false;
    private bool isInWater = false;
    [System.NonSerialized]public bool isInGoal = false;
    [System.NonSerialized]public bool isInSpin = false;
    [System.NonSerialized]public bool isFreeze = false;
    [System.NonSerialized]public bool isInCapture = false;
    [System.NonSerialized]public bool dontManipulateJump = false;
    private bool wasMoving = false;
    private bool hasUsedProbeller = false;
    private bool hasUsedAcorn = false;

    private bool onGround = true;

    [System.NonSerialized] public bool dontAllowStopMovingCor = false;

    /*Can Values*/
    private bool canMove = true;
    private bool canLeftMove = true;
    private bool canRightMove = true;
    private bool canAnimate = true;
    [System.NonSerialized]public bool canGetDamage = true;
    [System.NonSerialized]public bool canThrowCappy = true;

    /*Last - Values*/
    private float lastSpeed = 0;

    /*Counters*/
    private float walkFrames = 0;
    private float directionPressFrames = 0;/*For clim*/
    private List<GameObject> fireBalls = new List<GameObject>();
    private List<Transform> keys = new List<Transform>();

    /*Components*/
    private Transform _transform;
    [System.NonSerialized] public InputManager input;
    private BoxCollider2D bx;
    private SpriteRenderer sp;
    private SpriteMask fakeWallMask;
    private Rigidbody2D rb;
    private SceneManager sceneManager;
    private TileManager tileManager;
    private AudioSource audioJumpSource = null;private AudioSource audioStarSource;
    private GameObject bubble;
    [System.NonSerialized] public PlayerSpriteManager.PlayerPowerupSprites currentPlayerSprites = null;

    /*Coroutines*/
    private Coroutine stopMovingCor = null;
    private Coroutine returnCor = null;
    private Coroutine animationCor = null;
    private Coroutine starCor = null;
    private Coroutine powerupCor = null;
    private Coroutine gunCooldownCor = null;

    /*Saved Values*/
    public float savedFallMultipler;

    public enum AnimationID {Stand = 0, Walk = 1, Run = 2, Jump = 3, Fall = 4, Return = 5, Duck = 6, Death = 7, ThrowFireBall = 8, ProbellerFly = 9, AcornFly = 10, SmashFlowerSmash = 11, Kick = 12, Climb = 13, RunJump = 14, Spin = 15, Swim = 16,};
    public AnimationID currentAnimation = 0;

    private UMM.BlockData.BlockID currentCaptureID;
    private GameObject currentCapture;
    private int captureSavedState = 0;

    public static int DEATHS = 0;

    private void Awake() {
        /*Register Components*/
        this._transform = transform;
        this.input = GetComponentInChildren<InputManager>();
        this.bx = GetComponent<BoxCollider2D>();
        this.sp = GetComponent<SpriteRenderer>();
        this.fakeWallMask = this.GetComponentInChildren<SpriteMask>();
        this.rb = GetComponent<Rigidbody2D>();
        this.sceneManager = GameManager.instance.sceneManager;
        this.tileManager = TileManager.instance;
        /**/

        /*Save Values*/
        this.savedFallMultipler = this.fallmultiplier;
        /**/
        this.SetCollision(0);
        this.playerType = (PlayerController.PlayerType)SettingsManager.instance.GetOption("PlayerType");
        SetPlayerSpritesFromPowerup(Powerup.Small);
        this.sp.sprite = this.currentPlayerSprites.stand[0];

        DEATHS = 0;
    }

    private void OnEnable(){
        this.isInGoal = false;
        this.normalGravityScale = 3.15f;
        this.fallGravityScale = 6f;
        this.rb.gravityScale = this.fallGravityScale;
        GameManager.instance.GetComponent<MenuManager>().canRestart = true;
        this.sp.sortingOrder = 0;
        this.sp.sortingLayerID = 155026845;
        this.isFreeze = false;
        CancelIgnoreEnemiesDamageCollision();
        this.fallmultiplier = this.savedFallMultipler;
        this.canAnimate = true;
        this.hasUsedProbeller = false;
        this.isInSpin = false;
        this.rb.isKinematic = false;
        this.rb.simulated = true;
        this.rb.velocity = Vector3.zero;
        this.bx.enabled = true;
        this.canThrowCappy = true;
        this.checkWaterRay = true;
        this.cappy.SetActive(false);
        this.lastSpeed = 1;
        this.fakeWallMask.enabled = false;
        this._transform.localScale = new Vector3(1f, 1f, 1f);
        this.gunCooldownCor = null;
        if (this.tileManager.currentTileset.autoEnableIsWater)
            StartCoroutine(AutoEnableWaterIE());
        if (LevelEditorManager.isLevelEditor){
            RaycastHit2D ray1 = Physics2D.Raycast(transform.position + new Vector3(0.37f, this.onGroundYRay, 0f), -Vector2.up, 12, this.groundMask);
            RaycastHit2D ray2 = Physics2D.Raycast(transform.position + new Vector3(-0.37f, this.onGroundYRay, 0f), -Vector2.up, 12, this.groundMask);
            if (!ray1 | !ray2){
                GameObject bridge = Instantiate(this.sceneManager.levelEditorSaveBlock);
                bridge.transform.position = this.transform.position + new Vector3(0, -1, 0);
                bridge.transform.SetParent(this.sceneManager.GetAreaParent(this.sceneManager.currentArea));
            }
        }
    }

    public IEnumerator AutoEnableWaterIE(){
        yield return new WaitForSeconds(0.05f);
        SetIsInWater(true);
    }

    private void OnDisable(){
        if (this.normalGravityScale < 0)
            ChangeGravity();

        this.normalGravityScale = 3.15f;
        this.fallGravityScale = 6f;
        UngrabCurrentObject();
        this.isInGrab = false;
        this._transform.localScale = new Vector3(1f, 1f, 1f);
        CancelIgnoreEnemiesDamageCollision();
        DestroyAllKeys();
        this.sp.sortingOrder = 5;
        this.sp.sortingLayerID = 0;
        if (this.currentGrabedObject != null)
            Destroy(this.currentGrabedObject);
        this.cappy.SetActive(false);
        StopStar();
        SetIsInClimb(false);
        ResetMkItemSlot();
        StopAllCoroutines();
        this.canAnimate = true;
        PlayAnimation(this.currentPlayerSprites.stand, AnimationID.Stand);
        SetPowerup(Powerup.Small, true);
        this.canMove = true;
        this.rb.isKinematic = true;
        this.rb.simulated = false;
        this.rb.velocity = Vector3.zero;
        this.bx.enabled = false;
        this.sp.enabled = true;
        this.canGetDamage = true;
        this.isInBubble = false;
        this.isIceGround = false;
        this.isMystery = false;
        this.isSpecialMystery = false;
        this.gameObject.layer = 9;
        if (this.currentProbellerDownEffect != null)
            Destroy(this.currentProbellerDownEffect);
        if (this.isIncreasedCol){
            this.isIncreasedCol = false;
            this.bx.size = this.bx.size - this.increasedBoxColVt;
        }
        foreach (GameObject gm in this.fireBalls)
            Destroy(gm);
        this.fireBalls.Clear();
        if (this.bubble != null)
            Destroy(this.bubble);
    }

    private void Update(){
        if (this.isInGrab && !sp.flipX)
            this.currentGrabedObject.transform.localPosition = new Vector3(0.7f, 0, 0);
        else if (this.isInGrab)
            this.currentGrabedObject.transform.localPosition = new Vector3(-0.7f, 0, 0);

        if(this.keys.Count > 0){
            for(int i = 0; i < this.keys.Count; i++){
                Transform trans = this.keys[i];
                float speed = lastSpeed;
                int z = i + 1;

                if ((int)trans.position.x > (int)this.transform.position.x){
                    if (speed > 0)
                        speed = -speed;
                    speed = speed + (i * 0.3f);
                    if (speed > 0)
                        speed = -1;
                    trans.Translate(speed * Time.deltaTime, 0, 0);
                }else if((int)trans.position.x < (int)this.transform.position.x){
                    if (speed < 0)
                        speed = -speed;
                    speed = speed - (i * 0.3f);
                    if (speed < 0)
                        speed = 10;
                    trans.Translate(speed * Time.deltaTime, 0, 0);
                }

                if((int)trans.position.y > (int)this.transform.position.y){
                    float speed2 = -12 + i;
                    if (speed2 > -1)
                        speed2 = -1;
                    trans.Translate(0, speed2 * Time.deltaTime, 0);
                }else if ((int)trans.position.y < (int)this.transform.position.y){
                    float speed2 = 12 - i;
                    if (speed2 < 1)
                        speed2 = 1;
                    trans.Translate(0, speed2 * Time.deltaTime, 0);
                }
            }
        }

        if (this.isFreeze)
            return;

        if (this.rb.velocity.y < -19)
            this.rb.velocity = new Vector2(this.rb.velocity.x, -19);

        if (this.isInBubble)
            BubbleMovement();

        if(this.normalGravityScale < 0){
            bool left = input.LEFT;
            this.input.LEFT = this.input.RIGHT;
            this.input.RIGHT = left;
        }

        this.isMoving = input.LEFT | input.RIGHT;
        if (!this.isMoving | this.isDuck) {
            if (this.wasMoving && !this.isInClimb) {
                this.wasMoving = false;
                if (this.stopMovingCor != null)
                    StopCoroutine(this.stopMovingCor);
                if(this.returnCor == null && this.canMove && !this.dontAllowStopMovingCor)
                    this.stopMovingCor = StartCoroutine(StopMoving());
            }else if (!this.isDuck && this.onGround && !this.isInClimb && this.currentAnimation != AnimationID.Walk && !this.isInSpin) {
                PlayAnimation(currentPlayerSprites.stand, AnimationID.Stand, false, 0.1f, true);
            }
        }else
            this.wasMoving = true;

        if (this.canMove)
            MovementUpdate();
        else
            this.walkFrames = 0;

        if (rb.velocity.y < 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + Vector2.up.y * Physics2D.gravity.y * (fallmultiplier - 1) * Time.deltaTime);
        else if ((rb.velocity.y > 0 && !input.JUMP && !input.SPIN && this.currentAnimation != AnimationID.AcornFly) | this.dontManipulateJump | this.isInWater |  this.hasUsedProbeller)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + Vector2.up.y * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime);

        if (rb.velocity.y > 0)
            rb.gravityScale = normalGravityScale;
        else if (rb.velocity.y < 0)
            rb.gravityScale = fallGravityScale;

        if (!this.canMove && input.JUMP_DOWN && !this.isInClimb){
            //BreakReturn();
            OnGroundCheck();
            if(this.onGround)
                Jump();
        }

        /*Capture Powers*/
        if (this.isInCapture)
            CaptureMovement();
    }

    private void CaptureMovement(){
        switch (this.currentCaptureID){
            case BlockID.KOOOPATROOPA:
                KoopaTroopa koopa = this.currentCapture.GetComponent<KoopaTroopa>();
                if (this.input.DOWN && this.captureSavedState == 0){
                    SetCollision(0);
                    koopa.TriggerShell(this);
                    if (koopa.currentSheelState == KoopaTroopa.ShellState.StandShell)
                        koopa.TriggerShell(this);
                    koopa.canMove = true;
                    koopa.moveSpeed = 0;
                    this.captureSavedState = 1;
                    koopa.transform.localPosition = new Vector3(0, -0.15f, 0);
                    this._transform.Translate(0, 0.2f, 0);
                    this.cappy.GetComponent<PlayerCappy>().cappyCapturePos = new Vector3(0.064f, this.cappy.GetComponent<PlayerCappy>().cappyCapturePos.y, this.cappy.GetComponent<PlayerCappy>().cappyCapturePos.z);
                }else if (!this.input.DOWN && this.captureSavedState == 1){
                    SetCollision(1);
                    koopa.ExitShell();
                    koopa.canMove = true;
                    this.captureSavedState = 0;
                    koopa.transform.localPosition = new Vector3(0, -0.3f, 0);
                    this._transform.Translate(0, 0.2f, 0);
                    this.cappy.GetComponent<PlayerCappy>().cappyCapturePos = koopa.cappyCapturePos;
                }

                if(this.captureSavedState == 1){
                    koopa.CheckWand();
                    if (this.sp.flipX)
                        koopa.direction = 1;
                    else
                        koopa.direction = 0;
                }
                break;
            case BlockID.FIRE_PIRANHA:
                if (this.input.UP_DOWN)
                    this.currentCapture.GetComponent<FirePiranha>().StartAnimationClip(this.currentCapture.GetComponent<FirePiranha>().animationClips[0]);
                else if (this.input.DOWN_DOWN)
                    this.currentCapture.GetComponent<FirePiranha>().StartAnimationClip(this.currentCapture.GetComponent<FirePiranha>().animationClips[1]);

                if (this.input.USEPOWERUP)
                    this.currentCapture.GetComponent<FirePiranha>().SpawnFireBall(true);
                break;
            case BlockID.BOO:
                SetCanMove(false);
                BreakReturn();
                this.rb.velocity = new Vector2(0, 0);

                this.rb.gravityScale = 0;

                if (this.input.UP)
                    this._transform.Translate(0, this.playerWalkSpeed * Time.deltaTime, 0);
                else if (this.input.DOWN)
                    this._transform.Translate(0, -this.playerWalkSpeed * Time.deltaTime, 0);
                if (this.input.RIGHT){
                    this.sp.flipX = false;
                    this._transform.Translate(this.playerWalkSpeed * Time.deltaTime, 0, 0);
                }else if (this.input.LEFT){
                    this.sp.flipX = true;
                    this._transform.Translate(-this.playerWalkSpeed * Time.deltaTime, 0, 0);
                }

                if (this.input.USEPOWERUP_ZR)
                    this.cappy.GetComponent<PlayerCappy>().UncaptureEnemy();
                break;
            case BlockID.GROUNDBOO:
                if(this.onGround)
                    this.rb.velocity = new Vector2(0, 0);

                if (this.onGround && this.input.DOWN_DOWN){
                    SetCanMove(false);
                    BreakReturn();
                    IgnoreAllEnemiesDamageCollisions();
                    this.cappy.GetComponent<SpriteRenderer>().enabled = false;
                    this.cappy.GetComponent<PlayerCappy>().canUncapture = false;
                    this.currentCapture.GetComponent<GroundBoo>().StartAnimationClip(this.currentCapture.GetComponent<GroundBoo>().animationClips[0]);
                }else if(this.input.UP_DOWN && !this.cappy.GetComponent<PlayerCappy>().canUncapture){
                    SetCanMove(true);
                    this.cappy.GetComponent<SpriteRenderer>().enabled = true;
                    this.cappy.GetComponent<PlayerCappy>().canUncapture = true;
                    CancelIgnoreEnemiesDamageCollision();
                    this.currentCapture.GetComponent<GroundBoo>().StartAnimationClip(this.currentCapture.GetComponent<GroundBoo>().animationClips[1]);
                }
                break;
            case BlockID.DRYBONE:
                if (this.onGround && this.input.DOWN_DOWN){
                    SetCanMove(false);
                    BreakReturn();
                    IgnoreAllEnemiesDamageCollisions();
                    this.cappy.GetComponent<SpriteRenderer>().enabled = false;
                    this.cappy.GetComponent<PlayerCappy>().canUncapture = false;
                    this.currentCapture.GetComponent<DryBone>().StartAnimationClip(this.currentCapture.GetComponent<DryBone>().animationClips[1]);
                }else if (this.input.UP_DOWN && !this.cappy.GetComponent<PlayerCappy>().canUncapture){
                    SetCanMove(true);
                    this.cappy.GetComponent<SpriteRenderer>().enabled = true;
                    this.cappy.GetComponent<PlayerCappy>().canUncapture = true;
                    CancelIgnoreEnemiesDamageCollision();
                    this.currentCapture.GetComponent<DryBone>().StartAnimationClip(this.currentCapture.GetComponent<DryBone>().animationClips[2]);
                }
                break;
            case BlockID.BILLENEMY:
                if (!this.onGround){
                    if (this.sp.flipX && this.canLeftMove)
                        this._transform.Translate(-20 * Time.deltaTime, 0, 0);
                    else if (!this.sp.flipX && this.canRightMove)
                        this._transform.Translate(20 * Time.deltaTime, 0, 0);
                }
                break;
            case BlockID.HAMMERBRO:
                if (this.input.USEPOWERUP && this.currentCapture.GetComponent<HammerBro>().currentHammer == null){
                    this.currentCapture.GetComponent<HammerBro>().StartCoroutine(this.currentCapture.GetComponent<HammerBro>().ThrowHammer(true));
                    SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.throwFireBall);
                }
                break;
            case BlockID.BUZZYBEETLE:
                BuzzleBeetle buzzle = this.currentCapture.GetComponent<BuzzleBeetle>();
                if (this.input.DOWN && this.captureSavedState == 0){
                    buzzle.TriggerShell(this);
                    if (buzzle.currentSheelState == BuzzleBeetle.ShellState.StandShell)
                        buzzle.TriggerShell(this);
                    buzzle.canMove = true;
                    buzzle.currentSheelState = BuzzleBeetle.ShellState.MovingShell;
                    this.captureSavedState = 1;
                }else if (!this.input.DOWN && this.captureSavedState == 1){
                    buzzle.ExitShell();
                    buzzle.canMove = true;
                    this.captureSavedState = 0;
                }

                if (this.captureSavedState == 1){
                    buzzle.CheckWand();
                    if (this.sp.flipX)
                        buzzle.direction = 1;
                    else
                        buzzle.direction = 0;
                }
                break;
            case BlockID.BOWSER:
                if (this.input.USEPOWERUP && this.currentCapture.GetComponent<Bowser>().canShoot){
                    this.currentCapture.GetComponent<Bowser>().StartCoroutine(this.currentCapture.GetComponent<Bowser>().StartShootFireIE(0, true));
                }
                break;
        }
    }

    private IEnumerator StopMoving() {
        float lastWalkFrames = this.walkFrames;
        float speed = this.lastSpeed;
        float anSpeed;
        float t = this.lastSpeed;
        if (t < 0)
            t = -t;
        if (t > 10)
            t = 10;
        anSpeed = 0.15f - (t * 0.01f);
        this.walkFrames = 0;
        if (this.isIncreasedCol){
            this.isIncreasedCol = false;
            this.bx.size = this.bx.size - this.increasedBoxColVt;
        }

        float substractor = 5f;
        if (speed > this.playerWalkSpeed | speed < -this.playerWalkSpeed | this.input.RUN){
            substractor = 3f;
        } 
        if (this.onGround && !this.isIceGround)
            speed = speed / 1.5f;

        if (this.isIceGround){
            substractor = 0.5f;
        }

        while (speed > 0.9f | speed < 0.0f){
            this._transform.Translate(speed * Time.deltaTime, 0, 0);
            if (speed > 0){
                DistanceRayRight();
                if (!this.canRightMove){
                    speed = 0;
                    continue;
                }

                if (this.onGround | this.isInWater | this.hasUsedProbeller | this.hasUsedAcorn)
                    speed = speed - substractor * Time.deltaTime;
            }else{
                DistanceRayLeft();

                if (!this.canLeftMove){
                    speed = 0;
                    continue;
                }
                if (this.onGround | this.isInWater | this.hasUsedProbeller | this.hasUsedAcorn)
                    speed = speed + substractor * Time.deltaTime;
            }
            if(this.onGround && !this.isDuck)
                PlayAnimation(currentPlayerSprites.walk, AnimationID.Walk, false, anSpeed);
            anSpeed = anSpeed + 0.1f * Time.deltaTime;
            if (substractor < 5 && (this.onGround | this.hasUsedProbeller | this.hasUsedAcorn | this.isInWater))
                substractor = substractor + 5 * Time.deltaTime;
            this.lastSpeed = speed;
            yield return new WaitForSeconds(0);
        }

        if (this.onGround && !this.isDuck)
            PlayAnimation(currentPlayerSprites.stand, AnimationID.Stand);
        this.stopMovingCor = null;
    }

    [System.NonSerialized]public bool checkWaterRay = true;

    private void MovementUpdate(){
        /*Moving*/
        if (!this.sp.flipX | this.input.RIGHT | !this.canRightMove){
            this.DistanceRayRight();
        }

        if (this.sp.flipX | this.input.LEFT | !this.canLeftMove)
            this.DistanceRayLeft();

        if (this.isInClimb){
            if (this._transform.parent != null)
                this._transform.SetParent(null);

            OnGroundCheck(0f, 0f);
            this._transform.rotation = Quaternion.Euler(0, 0, 0);
            if (this.input.LEFT){
                this.directionPressFrames = this.directionPressFrames + 3 * Time.deltaTime;

                if (this.sp.flipX){
                    this.directionPressFrames = 0;
                    this.sp.flipX = false;
                }

                if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB1 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS1)
                    this._transform.position = new Vector3(this.climbedGameObject.transform.position.x, this._transform.position.y, this._transform.position.z) + new Vector3(-0.4f, 0, 0);
                else{
                    GameObject t = VineCheckHorz(-0.1f, Vector2.left);
                    if (t != null){
                        this._transform.Translate(-4 * Time.deltaTime, 0, 0);
                        SetIsInClimb(true, t);
                    }

                    if (this.currentAnimation != AnimationID.Climb)
                        PlayAnimation(this.currentPlayerSprites.climb, AnimationID.Climb);
                }
            }else if (this.input.RIGHT){
                this.directionPressFrames = this.directionPressFrames + 3 * Time.deltaTime;

                if (!this.sp.flipX){
                    this.directionPressFrames = 0;
                    this.sp.flipX = true;
                }
                if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB1 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS1)
                    this._transform.position = new Vector3(this.climbedGameObject.transform.position.x, this._transform.position.y, this._transform.position.z) + new Vector3(0.4f, 0, 0);
                else{
                    GameObject t = VineCheckHorz(0.5f, Vector2.left);
                    if (t != null){
                        this._transform.Translate(4 * Time.deltaTime, 0, 0);
                        SetIsInClimb(true, t);
                    }

                    if (this.currentAnimation != AnimationID.Climb)
                        PlayAnimation(this.currentPlayerSprites.climb, AnimationID.Climb);
                }
            }
            else
                this.directionPressFrames = 0;

            if(this.directionPressFrames > 1.5){
                this.directionPressFrames = 0;

                if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB1 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS1){
                    if (this.input.LEFT) {
                        GameObject t = VineCheckHorz(-0.5f, Vector2.left);
                        if (t != null) {
                            this.sp.flipX = !this.sp.flipX;
                            SetIsInClimb(true, t);
                            return;
                        }
                    } else if (this.input.RIGHT) {
                        GameObject t = VineCheckHorz(0.5f, Vector2.right);
                        if (t != null) {
                            this.sp.flipX = !this.sp.flipX;
                            SetIsInClimb(true, t);
                            return;
                        }
                    }
                }

                this.sp.flipX = !this.sp.flipX;
                SetIsInClimb(false);
                return;
            }

            if (this.input.UP && !CheckIfOverPlayerIsABlock(0f)){
                /*Check for Vine*/
                Physics2D.queriesStartInColliders = true;
                Physics2D.queriesHitTriggers = true;
                RaycastHit2D ray = Physics2D.Raycast(_transform.position, Vector2.up, 0.5f, groundMask);
                
                if (ray && ray.collider.gameObject == this.climbedGameObject){
                    this._transform.Translate(0, 4 * Time.deltaTime, 0);
                }

                Physics2D.queriesHitTriggers = false;
                Physics2D.queriesStartInColliders = false;
            }else if (this.input.DOWN){
                if (this.onGround){
                    SetIsInClimb(false);
                    return;
                }

                /*Check for Vine*/
                Physics2D.queriesStartInColliders = true;
                Physics2D.queriesHitTriggers = true;
                RaycastHit2D ray = Physics2D.Raycast(_transform.position, Vector2.down, 0.5f, groundMask);

                if (ray && ray.collider.gameObject == this.climbedGameObject){
                    this._transform.Translate(0, -4 * Time.deltaTime, 0);
                }

                Physics2D.queriesHitTriggers = false;
                Physics2D.queriesStartInColliders = false;
            }

            if (this.input.JUMP_DOWN){
                this.sp.flipX = !this.sp.flipX;
                SetIsInClimb(false);
                Jump();
            }

            if (this.currentAnimation != AnimationID.Climb && (this.input.UP |this.input.DOWN))
                PlayAnimation(this.currentPlayerSprites.climb, AnimationID.Climb);
            return;
        }else if (this.isInWater){
            Physics2D.queriesHitTriggers = true;
            RaycastHit2D ray1 = Physics2D.Raycast(_transform.position, Vector2.right, 1.5f, GameManager.instance.waterMask);
            RaycastHit2D ray2 = Physics2D.Raycast(_transform.position, Vector2.left, 1.5f, GameManager.instance.waterMask);
            if (this.checkWaterRay && !ray1 && !ray2 && !this.tileManager.currentTileset.autoEnableIsWater){
                SetIsInWater(false);
                return;
            }
            Physics2D.queriesHitTriggers = false;

            if (this.onGround){
                this.canAnimate = true;
                this.lastSpeed = 2;
            }else{
                this.fallmultiplier = -4.5f;
                this.canAnimate = false;

                if (this.lastSpeed > 0){
                    this.lastSpeed = this.lastSpeed - 5 * Time.deltaTime;
                    if(this.lastSpeed > 6)
                        this.lastSpeed = 6;
                }else if (this.lastSpeed < 0){
                    this.lastSpeed = this.lastSpeed + 5 * Time.deltaTime;
                    if (this.lastSpeed < -6)
                        this.lastSpeed = -6;
                }

                if(this.currentAnimation != AnimationID.Swim)
                    this.sp.sprite = this.currentPlayerSprites.swim[1];
                if (this.input.JUMP_DOWN){
                    GameObject eff = Instantiate(this.sceneManager.waterEffect);
                    SceneManager.destroyAfterNewLoad.Add(eff);
                    eff.transform.position = this._transform.position;
                    this.canAnimate = true;
                    PlayAnimation(this.currentPlayerSprites.swim, AnimationID.Swim);
                    this.canAnimate = false;
                    Jump(16);
                    this.input.JUMP_DOWN = false;
                    this.input.JUMP = false;
                }
            }
        }

        if (input.USEPOWERUP && !this.isInGrab)
            PowerupSpecialFunction();
        if (input.USEPOWERUP_ZR && !this.isInGrab)
            PowerupExtraSpecialFunction();
        if (input.USEMKITEMSLOT && this.currentMKItemSlotContent != MKItemSlotContent.NULL)
            UseMkItemSlot();

        if (input.RIGHT && this.canRightMove && (!this.isDuck | !this.onGround))
            Move(false);
        else if (input.LEFT && this.canLeftMove && (!this.isDuck | !this.onGround))
            Move(true);

        if (this.isDuck){
            if (input.LEFT)
                FlipX(true);
            else if (input.RIGHT)
                FlipX(false);
        }

        if ((!this.canLeftMove && this.sp.flipX) | (!this.canRightMove && !this.sp.flipX))
            this.lastSpeed = 1;

        /*Duck*/
        if (input.DOWN){
            if (this.hasUsedProbeller && !this.isMystery){
                if (this.currentProbellerDownEffect == null)
                    this.currentProbellerDownEffect = Instantiate(this.probellerDownEffect, this.transform);
                this.fallmultiplier = 15;
                this.rb.velocity = new Vector2(0, -25);
            }else
                this.TryEnableDuck(false);
        }else if (this.isDuck && this.onGround) {
            if (!this.CheckIfOverPlayerIsABlock() | this.powerup == Powerup.Mini){
                DisableDuck();
            }
        }

        if (this.hasUsedProbeller && !input.DOWN && this.rb.velocity.y < -0.5f){
            if (this.currentProbellerDownEffect != null)
                Destroy(this.currentProbellerDownEffect);
            this.fallmultiplier = this.savedFallMultipler;
            this.rb.velocity = new Vector2(0, -5);
        }

        /*Jump & JumpVelocity Checks*/
        OnGroundCheck();

        if (rb.velocity.y > 0 && !this.isInSpin  &&!this.isDuck && !this.onGround && !this.hasUsedProbeller && this.currentAnimation != AnimationID.AcornFly && this.currentAnimation != AnimationID.RunJump)
            PlayAnimation(this.currentPlayerSprites.jump, AnimationID.Jump);
        else if (rb.velocity.y < 0 && !this.isDuck && !this.isInSpin && !this.onGround && !this.hasUsedProbeller && this.currentAnimation != AnimationID.AcornFly && this.currentAnimation != AnimationID.RunJump){
            if (this.powerup == Powerup.Acorn && this.input.JUMP){
                rb.velocity = Vector3.zero;
                this.fallmultiplier = -8;
                PlayAnimation(this.currentPlayerSprites.acornFly, AnimationID.AcornFly);
            }else
                PlayAnimation(this.currentPlayerSprites.fall, AnimationID.Fall);
        }

        if (input.JUMP_DOWN && this.onGround)
            Jump();

        if (this.isInGrab && !input.RUN)
            UngrabCurrentObject();

        if (!this.isDuck && this.input.SPIN_DOWN && this.onGround && this.currentAnimation != AnimationID.Spin && this.tileManager.currentStyleID == TileManager.StyleID.SMW && !this.isMystery && this.currentGrabedObject == null){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
            StartCoroutine(SetIsInSpin()); 
            Jump(15, true);
        }

        if (this.isInSpin && this.currentAnimation != AnimationID.Spin)
            PlayAnimation(this.currentPlayerSprites.spin, AnimationID.Spin);

        /*MysteryCostume*/
        if (!this.isMystery)
            return;

        if (input.UP && this.onGround && !this.isMoving)
            this.sp.sprite = this.currentPlayerSprites.mysteryExtra;
        else if (!input.UP && this.sp.sprite == this.currentPlayerSprites.mysteryExtra)
            this.sp.sprite = this.currentPlayerSprites.stand[0];

        /*SpecialMysterCostumes*/
        if(this.isSpecialMystery && this.mysterySpecialID == 1 /*Kirby*/ && input.JUMP_DOWN && !this.onGround){
            this.rb.velocity = Vector2.zero;
            if (this.hasUsedProbeller)
                Jump(22, true);
            else
                Jump();
            PlayAnimation(this.currentPlayerSprites.probellerFly, AnimationID.ProbellerFly, false, 0.3f);
            this.hasUsedProbeller = true;
        }else if(this.hasUsedProbeller)
            PlayAnimation(this.currentPlayerSprites.probellerFly, AnimationID.ProbellerFly, false, 0.3f);

        if(this.isSpecialMystery && this.input.USEPOWERUP_ZR && this.mysterySpecialID == 2 /*GunMario*/ && !this.isInClimb && this.gunCooldownCor == null){
            GameObject bullet = Instantiate(this.sceneManager.gunBullet);
            if (this.sp.flipX){
                if (this.isDuck){
                    bullet.transform.position = this.transform.position;
                    bullet.transform.rotation = Quaternion.Euler(0, 0, -40);
                }else
                    bullet.transform.position = this.transform.position + new Vector3(-0.5f, 0, 0);
                bullet.GetComponent<GunBullet>().speed = -bullet.GetComponent<GunBullet>().speed;
            }else{
                if (this.isDuck){
                    bullet.transform.position = this.transform.position;
                    bullet.transform.rotation = Quaternion.Euler(0, 0, 40);
                }else
                    bullet.transform.position = this.transform.position + new Vector3(0.5f, 0, 0);
            }
            if (this.sp.sprite == currentPlayerSprites.mysteryExtra){
                bullet.transform.rotation = Quaternion.Euler(0, 0, 90);
                bullet.transform.position = this.transform.position;
                if(this.sp.flipX)
                    bullet.GetComponent<GunBullet>().speed = -bullet.GetComponent<GunBullet>().speed;
            }
            SceneManager.destroyAfterNewLoad.Add(bullet);
            this.gunCooldownCor = StartCoroutine(GunMarioCooldown());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.throwFireBall);
        }
    }

    private void PowerupSpecialFunction(){
        if ((this.powerup == Powerup.Fire | this.powerup == Powerup.Ice) && this.fireBalls.Count < 2 && !this.isDuck){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.throwFireBall);
            PlayAnimation(this.currentPlayerSprites.throwFireBall, AnimationID.ThrowFireBall);
            this.canAnimate = false;
            SetCanAnimateAfterSeconds(0.08f);

            GameObject fireBall;
            float speed = this.fireBallSpeed;
            if (sp.flipX)
                speed = -speed;

            if(this.powerup == Powerup.Fire)
                fireBall = this.sceneManager.SpawnFireBall(_transform.position, speed);
            else
                fireBall = this.sceneManager.SpawnIceBall(_transform.position, speed);

            fireBall.GetComponent<FireBall>().SetSpawner(this.gameObject);

            this.fireBalls.Add(fireBall);
        }else if (this.powerup == Powerup.Smash && this.onGround)
            StartCoroutine(SmashFlowerSmashIE());
        
    }

    private IEnumerator SmashFlowerSmashIE(){
        this.StopAnimation();
        this.PlayAnimation(this.currentPlayerSprites.smashFlowerSmash, AnimationID.SmashFlowerSmash);
        this.canAnimate = false;
        this.SetCanMove(false);

        yield return new WaitForSeconds(0.5f);
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.explode);
        GameObject clon = Instantiate(GameManager.instance.sceneManager.smashFirePrefarb);
        clon.GetComponent<SpriteRenderer>().flipX = this.sp.flipX;
        if (this.sp.flipX){
            clon.transform.position = this.transform.position + new Vector3(-1, 0, 0);
            clon.GetComponent<SmashFire>().speed = -clon.GetComponent<SmashFire>().speed;
        }else
            clon.transform.position = this.transform.position + new Vector3(1, 0, 0);

        yield return new WaitForSeconds(0.3f);

        this.canAnimate = true;
        this.SetCanMove(true);
        this.StopAnimation();
        this.sp.sprite = this.currentPlayerSprites.stand[0];
        this.PlayAnimation(this.currentPlayerSprites.stand, AnimationID.Stand);
    }

    private void PowerupExtraSpecialFunction(){
        if (this.powerup == Powerup.Probeller && !this.hasUsedProbeller && !this.isDuck && !this.isInWater) {
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.probellerFly);
            rb.velocity = new Vector2(0, 0);
            rb.angularVelocity = 0;
            Vector2 velocity = new Vector2(0, 40);
            if (this.onGround)
                velocity = new Vector2(0, 32);

            this.fallmultiplier = 0;
            rb.velocity = velocity;
            StartCoroutine(SetHasUsedProbeller());
        } else if (this.powerup == Powerup.Acorn && this.currentAnimation == AnimationID.AcornFly && !this.hasUsedAcorn && !this.isInWater) {
            rb.velocity = new Vector2(0, 15);
            this.hasUsedAcorn = true;
        } else if (this.powerup == Powerup.Smo){
            if (this.canThrowCappy){
                this.canThrowCappy = false;
                this.cappy.GetComponent<SpriteRenderer>().flipX = this.sp.flipX;
                this.cappy.SetActive(true);
                if (this.playerType == PlayerType.MARIO){
                    this.cappy.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(30, TileManager.TilesetType.ItemTileset);
                    this.currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSprites.smoCaplessMario;
                }else if (this.playerType == PlayerType.LUIGI){
                    this.cappy.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(36, TileManager.TilesetType.ItemTileset);
                    this.currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSpritesLuigi.smoCaplessMario;
                }else{
                    this.cappy.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(36, TileManager.TilesetType.ItemTileset);
                    this.currentPlayerSprites = PlayerSpriteManager.instance.currentPlayerSpritesToad.smoCaplessMario;
                }
                this.sp.sprite = this.currentPlayerSprites.stand[0];
            }else if (this.isInCapture){
                this.cappy.GetComponent<PlayerCappy>().UncaptureEnemy();
            }
        } 
    }

    private IEnumerator SetHasUsedProbeller(){
        yield return new WaitForSeconds(0.1f);
        this.hasUsedProbeller = true;
        PlayAnimation(this.currentPlayerSprites.probellerFly, AnimationID.ProbellerFly, true);
        this.canAnimate = false;
    }

    private IEnumerator SetIsInSpin(){
        yield return new WaitForSeconds(0.05f);
        this.canAnimate = true;
        PlayAnimation(this.currentPlayerSprites.spin, AnimationID.Spin, true);
        this.isInSpin = true;
        this.canAnimate = false;
    }

    public void RemoveFireBall(GameObject fireBallObject){
        this.fireBalls.Remove(fireBallObject);
    }

    public void TryEnableDuck(bool onlyIfNotSmall = true){
        if (onlyIfNotSmall){
            if (this.powerup == Powerup.Small | this.powerup == Powerup.Mini)
                return;
        }

        if (!this.isDuck && this.onGround && !this.isInCapture){
            PlayAnimation(currentPlayerSprites.duck, AnimationID.Duck);
            this.isSmallDuck = true;

            if (powerup > 0 && !this.isMystery && (this.lastSpeed > 4f | this.lastSpeed < -4f))
                EnableDuckCol();
            else
                this.isSmallDuck = false;

            this.isDuck = true;
        }
    }

    public void DisableDuck(){
        if (!this.isMystery)
            SetCollision((int)powerup);
        this.isDuck = false;
        this.lastSpeed = 1;
    }

    public void EnableDuckCol(){
        if ((this.powerup == Powerup.Small | this.powerup == Powerup.Mini) | this.isMystery)
            return;

        if (this.isIncreasedCol){
            this.isIncreasedCol = false;
            this.bx.size = this.bx.size - this.increasedBoxColVt;
        }
        this.distanceYRay = -0.5f;
        this.distanceYRayU = -0.5f;
        this.bx.offset = new Vector2(-0.02997065f, -0.6222746f);
        this.bx.size = new Vector2(0.5f, 0.7363094f);
    }

    public void Jump(float customVel = -1, bool noSound = false) {
        if (this.isInSpin && input.SPIN)
            customVel = 15;

        float lSpeed = this.lastSpeed;
        if (lSpeed < 0)
            lSpeed = -lSpeed;
        float vel = this.jumpVelocity + (this.jumpSpeedMultipler * lSpeed);
        /* if (this.isMoving && (this.lastSpeed > this.playerWalkSpeed + 1 | this.lastSpeed < this.playerWalkSpeed - 1) && this.walkFrames > 0.3f){
             vel = this.walkJumpVelocity;
             this.walkFrames = 5;
         }*/

       if (this.currentAnimation == AnimationID.Run && this.currentPlayerSprites.runJump != null && !this.isInGrab && !this.isInSpin){
            StopAnimation();
            this.sp.sprite = this.currentPlayerSprites.runJump;
            this.currentAnimation = AnimationID.RunJump;
            this.canAnimate = false;
            SetCanAnimateAfterSeconds(0.2f);
        }

        if (customVel != -1)
            vel = customVel;
        this.onGround = false;
        rb.velocity = new Vector2(rb.velocity.x, Vector2.up.y * vel);
        if (!noSound && this.audioJumpSource == null){
            if(this.powerup == Powerup.Small)
                this.audioJumpSource = SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.jumpSmall);
            else
                this.audioJumpSource = SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.jumpBig);
        }
    }

    public void CancelJump(){    
        this.rb.velocity = new Vector2(0, -0.5f);
    }


    private bool test = false;
    private void Move(bool flip) {
        if (flip != sp.flipX && !this.isInWater){/*Return*/
            this.returnCor = StartCoroutine(ReturnIE(flip));
            return;
        }else if (flip != sp.flipX && (this.isInWater | this.currentAnimation == AnimationID.AcornFly | (this.isSpecialMystery && this.currentAnimation == AnimationID.ProbellerFly)))
            FlipX(flip);

        if (this.stopMovingCor != null)
            StopCoroutine(this.stopMovingCor);

        float speed = playerWalkSpeed;
        if ((flip && lastSpeed > 0) | (!flip && lastSpeed < 0))
            lastSpeed = -lastSpeed;

        if ((input.RUN | this.test) && this.currentAnimation != AnimationID.AcornFly) {
            float mult = 6f;

            if ((lastSpeed <= playerWalkSpeed && lastSpeed > 0) | (lastSpeed >= -playerWalkSpeed && lastSpeed < 0))
                mult = 11f;

            if (lastSpeed != 0) {
                if(test)
                    mult = 5.5f;
                speed = lastSpeed;
                if (flip) {
                    speed = speed - mult * Time.deltaTime;
                    if (speed < -playerRunSpeed)
                        speed = -playerRunSpeed;
                }
                else {
                    speed = speed + mult * Time.deltaTime;
                    if (speed > playerRunSpeed)
                        speed = playerRunSpeed;
                }
            }

            if (!this.isIncreasedCol && this.onGround){
                this.isIncreasedCol = true;
                this.bx.size = this.bx.size + this.increasedBoxColVt;
            }else if(this.isIncreasedCol && !this.onGround){
                this.isIncreasedCol = false;
                this.bx.size = this.bx.size - this.increasedBoxColVt;
            }
        }else{
            float mult = 10f;
            if (test)
                mult = 5.5f;
            speed = lastSpeed;
            if (flip){
                if (speed < -playerWalkSpeed)
                    speed = speed + mult * Time.deltaTime;
                else
                    speed = speed - mult * Time.deltaTime; 
            }else{
                if (speed > playerWalkSpeed)
                    speed = speed - mult * Time.deltaTime;
                else
                    speed = speed + mult * Time.deltaTime;
            }

            if (this.isIncreasedCol){
                this.isIncreasedCol = false;
                this.bx.size = this.bx.size - this.increasedBoxColVt;
            }
        }

        if (!this.onGround && flip != sp.flipX){
            speed = 1;
            if (!this.isMystery | (this.isMystery && this.mysterySpecialID == 2))
                FlipX(flip);
        }

        if (this.isSpecialMystery && this.currentAnimation == AnimationID.ProbellerFly && (speed > 5 | speed < -5))
            speed = 5;

        if (flip && speed > 0)
            speed = -speed;

        _transform.Translate(speed * Time.deltaTime, 0, 0);

        this.lastSpeed = speed;

        if (test && !this.onGround && (this.lastSpeed > 5 | this.lastSpeed < -5))
            this.lastSpeed = 5;

        if (walkFrames < 100)
            this.walkFrames = this.walkFrames + 3 * Time.deltaTime;

        if (!this.onGround)
            return;

        float anSpeed;
        float t = this.lastSpeed;
        if (t < 0)
            t = -t;
        if (t > 10)
            t = 10;
        anSpeed = 0.15f - (t * 0.01f);

        if (speed == playerRunSpeed | speed == -playerRunSpeed)
            PlayAnimation(this.currentPlayerSprites.run, AnimationID.Run, false, anSpeed);
        else
            PlayAnimation(this.currentPlayerSprites.walk, AnimationID.Walk, false, anSpeed);
    }

    private IEnumerator ReturnIE(bool newFlip){
        if (this.stopMovingCor != null)
            StopCoroutine(this.stopMovingCor);

        bool wasOnGround = this.onGround;
        this.canMove = false;
        float lastSpeed = this.lastSpeed;

        float speed = lastSpeed;
        PlayAnimation(this.currentPlayerSprites.runReturn, AnimationID.Return);

        float substractor = 25f;
        if ((lastSpeed > this.playerWalkSpeed + 0.5f && lastSpeed > 0) | (lastSpeed < -this.playerWalkSpeed - 0.5f  && lastSpeed < 0)){
            while (speed > 0.3f | speed < 0.0f){
                this._transform.Translate(speed * Time.deltaTime, 0, 0);
                if (speed > 0){
                    DistanceRayRight();

                    if (!this.canRightMove){
                        speed = 0;
                        continue;
                    }
                    speed = speed - substractor * Time.deltaTime;
                }else{
                    DistanceRayLeft();

                    if (!this.canLeftMove){
                        speed = 0;
                        continue;
                    }
                    speed = speed + substractor * Time.deltaTime;
                }

                if (substractor < 30)
                    substractor = substractor + 25 * Time.deltaTime;

                this.lastSpeed = speed;
                OnGroundCheck();

                if (this.input.LEFT && !this.onGround)
                    this.sp.flipX = true;
                else if (this.input.RIGHT && !this.onGround)
                    this.sp.flipX = false;

                if (this.isIceGround)
                    substractor = 10;

                if (!this.onGround){
                    substractor = 15;
                    this.dontAllowStopMovingCor = true;
                    PlayAnimation(this.currentPlayerSprites.jump, AnimationID.Jump);
                }else if(this.currentAnimation != AnimationID.Return)
                    PlayAnimation(this.currentPlayerSprites.walk, AnimationID.Walk);
                else
                    PlayAnimation(this.currentPlayerSprites.walk, AnimationID.Return);
                yield return new WaitForSeconds(0);
            }
        }

        this.canAnimate = true;
        this.canMove = true;
        this.walkFrames = 0;
        if (this.stopMovingCor != null)
            StopCoroutine(this.stopMovingCor);

        if (this.onGround){
            this.lastSpeed = 0.5f;
        }else{
            this.lastSpeed = lastSpeed / 4;
        }

        FlipX(newFlip);
        this.returnCor = null;
        PlayAnimation(currentPlayerSprites.stand, AnimationID.Stand);
    }

    public void BreakReturn() {
        if (this.returnCor == null)
            return;

        if(this.returnCor != null)
            StopCoroutine(this.returnCor);

        FlipX(!sp.flipX);
        this.canAnimate = true;
        this.canMove = true;
        this.walkFrames = 0;
        this.lastSpeed = 1;
        this.returnCor = null;
    }

    public void PlayAnimation(Sprite[] sprites, AnimationID id, bool repeat = false, float speed = 0.1f, bool noCheck = false){
        if (this.isInGrab){
            if(id == AnimationID.Stand)
                sprites = this.currentPlayerSprites.grabStand;
            else
                sprites = this.currentPlayerSprites.grabWalk;
        }

        if((this.currentAnimation != id | noCheck) && this.canAnimate){
            this.StopAnimation();
            this.currentAnimation = id;
            this.animationCor = StartCoroutine(PlayAnimationIE(sprites, repeat, speed));
        }
    }

    public void PlayAnimation(Sprite sprite, AnimationID id){
        if (!this.canAnimate)
            return;

        if (this.isInGrab){
            if (id == AnimationID.Duck)
                sprite = this.currentPlayerSprites.grabDuck;
            else
                sprite = this.currentPlayerSprites.grabWalk[0];
        }

        this.StopAnimation();
        this.sp.sprite = sprite;
        this.currentAnimation = id;
    }

    private IEnumerator PlayAnimationIE(Sprite[] sprites, bool repeat, float speed = 0.1f){
        foreach (Sprite sprite in sprites){
            sp.sprite = sprite;
            yield return new WaitForSeconds(speed);
        }

        if (repeat)/*Repeat can used for bypass canAnimate false*/
            this.animationCor = StartCoroutine(PlayAnimationIE(sprites, repeat));
        else{
            this.currentAnimation = AnimationID.Stand;
            this.animationCor = null;
        }
    }

    public void SetCanAnimateAfterSeconds(float seconds){
        StartCoroutine(SetCanAnimateAfterSecondsIE(seconds));
    }

    private IEnumerator SetCanAnimateAfterSecondsIE(float seconds){
        yield return new WaitForSeconds(seconds);
        this.canAnimate = true;
    }

    public void StopAnimation(){
        if (this.animationCor != null)
            StopCoroutine(this.animationCor);
    }

    public void FlipX(bool state){
        sp.flipX = state;
    }

    public void SetCollision(int number){
        if (bx == null)
            return;

        if (this.isIncreasedCol){
            this.isIncreasedCol = false;
            this.bx.size = this.bx.size - this.increasedBoxColVt;
        }

        switch (number){
            case (int) Powerup.Small:
         
                bx.offset = new Vector2(-0.004460216f, -0.1105735f);
                bx.size = new Vector2(0.5f, 0.719836f);
                this.onGroundYRay = -0.01f;
                

                this.distanceXRay = 0.05f;
                this.distanceXRay2 = 0.02f;
                this.distanceYRay = -0.4f;
                this.distanceYRay = 0f;
                this.distanceYRayU = 0.0f;

                this.increasedBoxColVt = new Vector2(0.3f, 0);

                if (this.powerup == Powerup.Big)
                    transform.Translate(0, -0.5f, 0);
                break;

            case (int) Powerup.Mini:
                bx.offset = new Vector2(-0.004460216f, -0.2089155f);
                bx.size = new Vector2(0.5f, 0.4907227f);
                this.onGroundYRay = 0f;
                this.distanceYRay = 0f;
                this.distanceXRay = 0.05f;
                this.distanceXRay2 = 0.02f;
                this.distanceYRay = -0.4f;
                this.distanceYRay = 0f;
                this.distanceYRayU = 0.0f;

                this.increasedBoxColVt = new Vector2(0.3f, 0);

                if (this.powerup == Powerup.Big)
                    transform.Translate(0, -0.5f, 0);
                break;
            case 20://CapturedEnemy
                SetCollision(0);
                this.onGroundYRay = -0.6f;
                bx.offset = new Vector2(-0.004460216f, -0.6731716f);
                bx.size = new Vector2(0.5f, 0.5892384f);
                break;
            default:
                bx.offset = new Vector2(-0.004460216f, -0.2142913f);
                bx.size = new Vector2(0.5f, 1.506999f);
                this.onGroundYRay = -0.6f;
                this.distanceXRay = 0.05f;
                this.distanceXRay2 = 0.02f;
                this.distanceYRay = -0.982f;
                this.distanceYRay = -0.5f;
                this.distanceYRayU = 0.3f;

                this.increasedBoxColVt = new Vector2(0.3f, 0);

                if (this.powerup == Powerup.Small)
                    transform.Translate(0, 0.5f, 0);
                break;


        }
    }

    public void SetCurrentCaptureID(GameObject capturedObject){
        this.captureSavedState = 0;
        UngrabCurrentObject();

        if (capturedObject == null){
            if (this.currentCapture != null){
                if (this.currentCapture.GetComponent<KoopaTroopa>() != null && this.currentCapture.GetComponent<KoopaTroopa>().currentSheelState == KoopaTroopa.ShellState.MovingShell){
                    this.currentCapture.GetComponent<KoopaTroopa>().ExitShell();
                    this.currentCapture.GetComponent<KoopaTroopa>().TriggerFromPlayer(this);
                    if (this.currentCapture.GetComponent<KoopaTroopa>().currentSheelState != KoopaTroopa.ShellState.MovingShell)
                        this.currentCapture.GetComponent<KoopaTroopa>().TriggerFromPlayer(this);
                    this.currentCapture.GetComponent<EntityGravity>().enabled = true;
                }
                SetCanMove(true);
            }

            this.currentCaptureID = BlockID.ERASER;
            SetCollision((int)this.powerup);
            this.currentCapture = null;
            return;
        }

        this.currentCapture = capturedObject;

        this.currentCaptureID = capturedObject.GetComponent<Entity>().captureID;

        if(capturedObject.GetComponent<KoopaTroopa>() != null){
            if (capturedObject.GetComponent<KoopaTroopa>().currentSheelState != KoopaTroopa.ShellState.NoShell){
                capturedObject.GetComponent<KoopaTroopa>().ExitShell();
                SetCollision(0);
                this.currentCapture.GetComponent<KoopaTroopa>().TriggerShell(this);
                if (this.currentCapture.GetComponent<KoopaTroopa>().currentSheelState == KoopaTroopa.ShellState.StandShell)
                    this.currentCapture.GetComponent<KoopaTroopa>().TriggerShell(this);
                this.currentCapture.GetComponent<KoopaTroopa>().canMove = true;
                this.currentCapture.GetComponent<KoopaTroopa>().moveSpeed = 0;
                this.captureSavedState = 1;
                this.currentCapture.GetComponent<KoopaTroopa>().transform.localPosition = new Vector3(0, -0.15f, 0);
                this._transform.Translate(0, 0.2f, 0);
            }
            return;
        }
    }

    public void SetPowerup(Powerup newPowerup, bool noSound = false, bool noAnimation = false){
        if (this.isInCapture | this.isInBubble)
            return;

        if (this.powerupCor != null){
            StopCoroutine(this.powerupCor);
            GameManager.ResumeTimeScale();
            if(newPowerup == Powerup.Mini && this.powerup == Powerup.Mini)
                this._transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        this.isMystery = false;
        SetCollision((int)newPowerup);

        if (this.powerup != newPowerup){
            if(!noAnimation)
                this.powerupCor = StartCoroutine(PowerupAnimation(this.powerup, newPowerup));
            if(this.powerup == Powerup.Acorn){
                this.currentAnimation = 0;
                this.fallmultiplier = this.savedFallMultipler;
                this.hasUsedAcorn = false;
            }
        }

        if (this.currentProbellerDownEffect != null)
            Destroy(this.currentProbellerDownEffect);

        this.powerup = newPowerup;
        SetPlayerSpritesFromPowerup(newPowerup);
        if (!noSound)
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerup);
    }

    public Powerup GetPowerup(){
        return this.powerup;
    }

    private IEnumerator PowerupAnimation(Powerup oldPowerup, Powerup newPowerup){
        if (newPowerup == Powerup.Mini){
            UngrabCurrentObject();
            GameManager.StopTimeScale();
            SetPlayerSpritesFromPowerup(Powerup.Mini);
            this.normalGravityScale = 2.2f;
            this.fallGravityScale = 4f;
            while (this._transform.localScale.x > 0.7f){
                this._transform.localScale = new Vector3(this._transform.localScale.x - 1f * Time.unscaledDeltaTime, this._transform.localScale.y - 1f * Time.unscaledDeltaTime, 1);
                yield return new WaitForSecondsRealtime(0);
            }
            this._transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            SetCollision((int)Powerup.Mini);
            GameManager.ResumeTimeScale();
        }else{
            this.normalGravityScale = 3.15f;
            this.fallGravityScale = 6f;
            if (oldPowerup == Powerup.Mini){
                UngrabCurrentObject();
                GameManager.StopTimeScale();
                while (this._transform.localScale.x < 1f){
                    this._transform.localScale = new Vector3(this._transform.localScale.x + 1f * Time.unscaledDeltaTime, this._transform.localScale.y + 1f * Time.unscaledDeltaTime, 1);
                    this._transform.Translate(0, 5f * Time.unscaledDeltaTime, 0);
                    yield return new WaitForSecondsRealtime(0);
                }
                this._transform.localScale = new Vector3(1f, 1f, 1f);
            }

            GameManager.StopTimeScale();
            yield return new WaitForSecondsRealtime(0.05f);
            if (oldPowerup == Powerup.Small && newPowerup > 0){
                if (CheckIfOverPlayerIsABlock())
                    this.TryEnableDuck(false);
            }
            yield return new WaitForSecondsRealtime(0);

            int lenght = 6;

            if (newPowerup == 0){
                SetCollision((int)oldPowerup);
                lenght = 3;
            }

            bool z = false;
            for (int i = 0; i < lenght; i++){
                if (z)
                    SetPlayerSpritesFromPowerup(oldPowerup);
                else
                    SetPlayerSpritesFromPowerup(newPowerup);
                if (this.isDuck)
                    this.sp.sprite = this.currentPlayerSprites.duck;
                else
                    this.sp.sprite = this.currentPlayerSprites.stand[0];
                z = !z;

                yield return new WaitForSecondsRealtime(0.07f);
            }

            if (newPowerup == 0)
                SetCollision(0);
            SetPlayerSpritesFromPowerup(newPowerup);
            if (this.isDuck)
                this.sp.sprite = this.currentPlayerSprites.duck;
            else
                this.sp.sprite = this.currentPlayerSprites.stand[0];
            yield return new WaitForSecondsRealtime(0.1f);
            GameManager.ResumeTimeScale();
        }
    }

    private void SetPlayerSpritesFromPowerup(Powerup powerup, bool useStandartPlayerType = false) {
        PlayerSpriteManager.PlayerSprites temp = PlayerSpriteManager.instance.currentPlayerSprites;
        if (this.playerType == PlayerType.LUIGI && !useStandartPlayerType)
            temp = PlayerSpriteManager.instance.currentPlayerSpritesLuigi;
        else if ((this.playerType == PlayerType.BLUETOAD | this.playerType == PlayerType.REDTOAD | this.playerType == PlayerType.GREENTOAD) && !useStandartPlayerType)
            temp = PlayerSpriteManager.instance.currentPlayerSpritesToad;

        if (this.sp == null)
            this.sp = this.GetComponent<SpriteRenderer>();

        if (this.playerType == PlayerType.BLUETOAD | this.playerType == PlayerType.REDTOAD | this.playerType == PlayerType.GREENTOAD)
            this.sp.material = this.toadMaterial;
        else
            this.sp.material = this.normalMaterial;

        if (this.playerType == PlayerType.BLUETOAD)
            ChangeToadColor(tileManager.currentStyle.toadColors[0]);
        else if(this.playerType == PlayerType.REDTOAD)
            ChangeToadColor(tileManager.currentStyle.toadColors[1]);
        else if (this.playerType == PlayerType.GREENTOAD)
            ChangeToadColor(tileManager.currentStyle.toadColors[2]);

        switch (powerup){
            case Powerup.Mini:
            case Powerup.Small:
                this.currentPlayerSprites = temp.smallMario;
                break;

            case Powerup.Big:
                this.currentPlayerSprites = temp.bigMario;
                break;

            case Powerup.Fire:
                this.currentPlayerSprites = temp.fireMario;
                break;

            case Powerup.Probeller:
                this.currentPlayerSprites = temp.probellerMario;
                break;

            case Powerup.Acorn:
                this.currentPlayerSprites = temp.acornMario;
                break;

            case Powerup.Ice:
                this.currentPlayerSprites = temp.iceMario;
                break;

            case Powerup.Smash:
                this.currentPlayerSprites = temp.smashMario;
                break;

            case Powerup.Smo:
                if (this.currentPlayerSprites == temp.smoCaplessMario)
                    return;

                this.currentPlayerSprites = temp.smoMario;
                break;
        }

        if (this.currentPlayerSprites.jump == null)
            SetPlayerSpritesFromPowerup(powerup, true);
    }

    public void Damage(){
        if (!this.canGetDamage)
            return;

        if (this.isInCapture){
            this.canGetDamage = false;
            Jump();
            this.cappy.GetComponent<PlayerCappy>().UncaptureEnemy();
            StartCoroutine(DamageAnimation());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerdown);
            return;
        }

        if (this.powerup == Powerup.Small | this.powerup ==  Powerup.Mini)
            Death();
        else{
            this.canGetDamage = false;

            IgnoreAllEnemiesDamageCollisions();
            if (this.powerup != Powerup.Big)
                SetPowerup(Powerup.Big, true);
            else
                SetPowerup(Powerup.Small, true);

            StartCoroutine(DamageAnimation());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerdown);
        }
    }

    public IEnumerator DamageAnimation(){
        for (int i = 0; i < 30; i++){
            if(!this.isInCapture && !this.canGetDamage)
                sp.enabled = !sp.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (!this.isInCapture)
            sp.enabled = true;
        CancelIgnoreEnemiesDamageCollision();
        if(!this.isStar)
            this.canGetDamage = true;
    }

    public void IgnoreAllEnemiesDamageCollisions(){
        foreach (BoxCollider2D ebx in SceneManager.enemiesDamageColliders){
            if (ebx != null)
                Physics2D.IgnoreCollision(this.bx, ebx, true);
        }
    }

    private void CancelIgnoreEnemiesDamageCollision(){
        foreach (BoxCollider2D ebx in SceneManager.enemiesDamageColliders){
            if(ebx != null)
                Physics2D.IgnoreCollision(this.bx, ebx, false);
        }
    }

    public void Death(){
        if (this.isInBubble | this.isInGoal)
            return;

        if (this.powerup != Powerup.Small && this.powerup != Powerup.Mini)
            SetPowerup(Powerup.Small, true, true);

        DEATHS++;
        BreakReturn();
        DestroyAllKeys();
        GameManager.instance.GetComponent<MenuManager>().canRestart = false;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.die);
        this.canAnimate = true;
        if (this.currentGrabedObject != null)
            UngrabCurrentObject(true);
        this.isInGrab = false;
        PlayAnimation(this.currentPlayerSprites.death, AnimationID.Death, true);
        this.canAnimate = false;
        this.canGetDamage = false;
        this.SetCanMove(false);
        this.isFreeze = true;
        this.bx.enabled = false;
        this.rb.velocity = Vector3.zero;
        this.rb.isKinematic = true;
        this.rb.simulated = false;
        if (LevelEditorManager.isLevelEditor){/*Fast death*/
            RespawnClient();
            return;
        }else if (GameManager.instance.sceneManager.players.Count == 1)
            StartCoroutine(DeathFreezeIE());

        BackgroundMusicManager.instance.StopCurrentBackgroundMusic();
        StartCoroutine(DeathIE());

        StartCoroutine(ClientDeathIE());
    }

    private IEnumerator DeathFreezeIE(){
        GameManager.StopTimeScale();
        yield return new WaitForSecondsRealtime(0.15f);
        GameManager.ResumeTimeScale();
    }

    public void ResetPlayer(){
        this.enabled = true;
        this.lastSpeed = 1;
        this.checkWaterRay = true;
        this.normalGravityScale = 3.15f;
        this.fallGravityScale = 6f;
        this.rb.gravityScale = this.fallGravityScale;
        this._transform.localScale = new Vector3(1f, 1f, 1f);
        this.fakeWallMask.enabled = false;
        this.isFreeze = false;
        GameManager.instance.GetComponent<MenuManager>().canRestart = true;
        SetMkItemSlot(MKItemSlotContent.NULL);
        this.cappy.GetComponent<PlayerCappy>().UncaptureEnemy(true);
        this.isInCapture = false;
        this.isInBubble = false;
        this.isIceGround = false;
        this.canThrowCappy = true;
        this.isFreeze = false;
        DestroyAllKeys();
        SetIsInClimb(false);
        UngrabCurrentObject();
        CancelIgnoreEnemiesDamageCollision();
        if (this.powerup != Powerup.Small)
            SetPowerup(Powerup.Small, true, true);
        MenuManager.canOpenMenu = true;
        this.rb.isKinematic = false;
        this.rb.simulated = true;
        this.rb.velocity = Vector3.zero;
        this.sp.sortingOrder = 0;
        this.sp.enabled = true;
        this.sp.color = Color.white;
        this.canGetDamage = true;
        this.gameObject.layer = 9;
        this.canAnimate = true;
        this.SetCanMove(true);
        this._transform.SetParent(null);
        this.bx.enabled = true;
        this.bubbleYTarget = -1;
        this.isMystery = false;
        this.isSpecialMystery = false;
        this.sp.sortingOrder = 0;
        this.sp.sortingLayerID = 155026845;
        if (GameManager.instance.isInMainMenu){
            PlayAnimation(this.currentPlayerSprites.stand[0], AnimationID.Stand);
            GameManager.instance.sceneManager.RespawnEntities();
        }
        if (this.isIncreasedCol){
            this.isIncreasedCol = false;
            this.bx.size = this.bx.size - this.increasedBoxColVt;
        }
        if (this.bubble != null)
            Destroy(this.bubble);
        this.gunCooldownCor = null;
    }

    private IEnumerator ClientDeathIE(){
        for (int i = 0; i < 30; i++){
            _transform.Translate(0, 8 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator DeathIE(){
        this.sp.sortingOrder = 5;
        this.sp.sortingLayerID = 0;
        yield return new WaitForSeconds(1);
        this.rb.isKinematic = false;
        this.rb.simulated = true;
        yield return new WaitForSeconds(3);
        RespawnClient();
    }

    private void RespawnClient(){
        if (GameManager.instance.sceneManager.players.Count > 1){
            if (!SpawnInBubble()){
                foreach (PlayerController p in this.sceneManager.players)
                    p.ResetPlayer();
                if (LevelEditorManager.isLevelEditor)
                    LevelEditorManager.instance.SwitchMode();
                else
                    GameManager.instance.RestartCurrentLevel();
            }

        }else{
            foreach (PlayerController p in this.sceneManager.players)
                p.ResetPlayer();
            if (LevelEditorManager.isLevelEditor)
                LevelEditorManager.instance.SwitchMode();
            else
                GameManager.instance.RestartCurrentLevel();
        }
    }

    private bool SpawnInBubble(){
        bool t = false;
        foreach(PlayerController p in this.sceneManager.players){
            if (!p.isInBubble && p != this)
                t = true;
        }
        if (!t)
            return false;

        this.rb.velocity = Vector3.zero;
        this.rb.isKinematic = true;
        this.rb.simulated = true;
        this.sp.sortingOrder = 0;
        this.isInBubble = true;
        this.bx.enabled = true;
        this.gameObject.layer = 22;

        this.bubble = new GameObject("Bubble");
        this.bubble.AddComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(42, TileManager.TilesetType.ObjectsTileset);
        this.bubble.GetComponent<SpriteRenderer>().sortingOrder = 4;
        this.bubble.transform.SetParent(this._transform);
        this.bubble.transform.localPosition = Vector3.zero;
        this._transform.position = new Vector3(this._transform.position.x, 5, this._transform.position.z);

        foreach (PlayerController p in this.sceneManager.players)
            if (!p.isInBubble)
                this.sceneManager.playerCamera.playerTransform = p.transform;
        return true;
    }

    private bool bubbleRight = false;
    private float bubbleYTarget = -1;
    private void BubbleMovement(){
        Transform otherPlayer = null;
        foreach (PlayerController p in this.sceneManager.players)
            if (!p.isInBubble)
                otherPlayer = p.transform;

        if (this.input.JUMP_DOWN){
            this.bubbleYTarget = otherPlayer.transform.position.y;
            if (otherPlayer.transform.position.x > this._transform.position.x)
                this.bubbleRight = false;
            else
                this.bubbleRight = true;
        }

        if (this.bubbleYTarget == -1)
            this.bubbleYTarget = UnityEngine.Random.Range(21, 30);

        if (this.bubbleYTarget != 5 && this._transform.position.y < this.bubbleYTarget)
            this._transform.Translate(0, 5 * Time.deltaTime, 0);
        else if(this.bubbleYTarget != 5)
            this.bubbleYTarget = 5;

        if (this.bubbleYTarget == 5 && this._transform.position.y > this.bubbleYTarget)
            this._transform.Translate(0, -5 * Time.deltaTime, 0);
        else if (this.bubbleYTarget == 5)
            this.bubbleYTarget = -1;

        if (!this.bubbleRight && this._transform.position.x < (otherPlayer.position.x + 25))
            this._transform.Translate(5 * Time.deltaTime, 0, 0);
        else if (!this.bubbleRight)
            this.bubbleRight = true;

        if (this.bubbleRight && this._transform.position.x > (otherPlayer.position.x - 25))
            this._transform.Translate(-5 * Time.deltaTime, 0, 0);
        else if (this.bubbleRight)
            this.bubbleRight = false;
    }

    private void OnGroundCheck(float xOffset = 0.5f, float xOffset2 = -0.43f){
        if (!this.onGround && this.powerup != Powerup.Small && this.powerup != Powerup.Mini){
            xOffset = 0.3f;
            xOffset2 = -0.23f;
        }

        RaycastHit2D ray1;
        RaycastHit2D ray2;

        if (this.canLeftMove)
            ray1 = Physics2D.Raycast(_transform.position + new Vector3(xOffset2, this.onGroundYRay, 0f), this.onGroundVector, 0.6f, groundMask);
        else{
            if (!this.isDuck)
                this._transform.Translate(4 * Time.deltaTime, 0, 0);
            ray1 = Physics2D.Raycast(_transform.position + new Vector3(0, this.onGroundYRay, 0f), this.onGroundVector, 0.6f, groundMask);
        }

        if (this.canRightMove)
            ray2 = Physics2D.Raycast(_transform.position + new Vector3(xOffset, this.onGroundYRay, 0f), this.onGroundVector, 0.6f, groundMask);
        else if (this.canLeftMove){
            if(!this.isDuck)
                this._transform.Translate(-4 * Time.deltaTime, 0, 0);
            ray2 = Physics2D.Raycast(_transform.position + new Vector3(0, this.onGroundYRay, 0f), this.onGroundVector, 0.6f, groundMask);
        }else
            ray2 = new RaycastHit2D();

        if (ray1 | ray2){
            if ((ray1 && ray1.collider.gameObject.layer == 27) | (ray2 && ray2.collider.gameObject.layer == 27)){
                if ((ray1 && ray1.collider.gameObject.transform.eulerAngles.z == 90) | (ray2 && ray2.collider.gameObject.transform.eulerAngles.z == 90))
                    onGround = true;
                else
                    return;
            }
            this.onGround = true;
            this.dontAllowStopMovingCor = false;
            this.dontManipulateJump = false;

            if (this.test)
                this.lastSpeed = 1;
            if (this.test){
                this.test = false;
                if (this.lastSpeed > 0)
                    this.lastSpeed = this.lastSpeed + 1;
                else
                    this.lastSpeed = this.lastSpeed - 1;
            }

            this.hasUsedAcorn = false;
            GameObject col = null;
            if (ray1)
                col = ray1.collider.gameObject;
            else
                col = ray2.collider.gameObject;
            if (col != null && col.gameObject.layer == 29)
                this.isIceGround = true;
            else
                this.isIceGround = false;

            if (this.hasUsedProbeller | this.isInSpin){
                if (this.currentProbellerDownEffect != null)
                    Destroy(this.currentProbellerDownEffect);

                if (((this.fallmultiplier != this.savedFallMultipler && input.DOWN) | (this.isInSpin && this.powerup != Powerup.Small && this.powerup != Powerup.Mini)) && !this.isMystery){
                    bool hasBlock = SceneManager.CheckBlockToUseOrDestroy(col, true);
                    if (hasBlock){
                        if(this.isInSpin)
                            Jump();
                        this.onGround = false;
                        return;
                    }
                }

                if (col != null | this.isMystery | (this.isInSpin && this.powerup == Powerup.Small | this.powerup == Powerup.Mini)){
                    if(this.isInSpin && ((ray1 && ray1.collider.gameObject.layer == 24) | (ray2 && ray2.collider.gameObject.layer == 24))){
                        this.onGround = false;
                        return;
                    }
                    this.canAnimate = true;
                    this.fallmultiplier = this.savedFallMultipler;
                    this.hasUsedProbeller = false;
                    this.isInSpin = false;
                    PlayAnimation(this.currentPlayerSprites.stand, AnimationID.Stand);
                }else
                    this.onGround = false;
            }
            this.fallmultiplier = this.savedFallMultipler;
            if (this.isDuck && this._transform.parent != null)
                EnableDuckCol();
            else if (this.isDuck && !this.isSmallDuck && !this.CheckIfOverPlayerIsABlock() && !this.isMystery)
                SetCollision((int)powerup);
        }else{ 
            this.onGround = false;
            if (this.isDuck && !this.isSmallDuck)
                EnableDuckCol();
        }
    }

    private void DistanceRayRight(){
        RaycastHit2D ray1 = Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay, this.distanceYRay, 0f), Vector2.right, 0.5f, wallMask);
       // Debug.DrawRay(_transform.position + new Vector3(this.distanceXRay, this.distanceYRay, 0f), Vector2.right, Color.blue);

        if ((ray1 && ray1.collider.gameObject.layer != 27) | (ray1 && ray1.collider.gameObject.layer == 27 && ray1.collider.gameObject.transform.eulerAngles.z != 0)){
            if (ray1.collider.gameObject.layer == 23)
                Damage();
            if(ray1.collider.gameObject.layer == 28){
                if (input.RUN && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1)
                    GrabObject(ray1.collider.gameObject);
                else if (ray1.collider.gameObject.tag.Equals("GrabableBomb"))
                    ray1.collider.GetComponent<BombEnemy>().HitBomb(this);
                else if (ray1.collider.gameObject.tag.Equals("GrabNoWand")){
                    this.canRightMove = true;
                    return;
                }
            }
            if (ray1.collider.gameObject.layer == 31 && input.RUN && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1)
                GrabObject(ray1.collider.gameObject);
            else if (ray1.collider.gameObject.layer == 31){
                if (ray1.collider.gameObject.GetComponent<KoopaTroopa>() != null)
                    ray1.collider.gameObject.GetComponent<KoopaTroopa>().TriggerFromPlayer(this);
                else
                    ray1.collider.gameObject.GetComponent<BuzzleBeetle>().TriggerFromPlayer(this);
            }
            this.canRightMove = false;   
        }else
            this.canRightMove = true;
        
        RaycastHit2D rayU = Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay, this.distanceYRayU, 0f), Vector2.right, 0.5f, wallMask);
        //Debug.DrawRay(_transform.position + new Vector3(this.distanceXRay, this.distanceYRayU, 0f), Vector2.right, Color.blue);

        if ((rayU && rayU.collider.gameObject.layer != 27) | (rayU && rayU.collider.gameObject.layer == 27 && rayU.collider.gameObject.transform.eulerAngles.z != 0)){
            if (rayU.collider.gameObject.layer == 23)
                Damage();
            this.canRightMove = false;   
        }
    }

    private void DistanceRayLeft(){
        RaycastHit2D ray2 = Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay2, this.distanceYRay, 0f), Vector2.left, 0.5f, wallMask);
        //Debug.DrawRay(_transform.position + new Vector3(this.distanceXRay2, this.distanceYRay, 0f), Vector2.left, Color.blue);
        if ((ray2 && ray2.collider.gameObject.layer != 27) | (ray2 && ray2.collider.gameObject.layer == 27 && ray2.collider.gameObject.transform.eulerAngles.z == 0)){
            if (ray2.collider.gameObject.layer == 23)
                Damage();
            if (ray2.collider.gameObject.layer == 28){
                if (input.RUN && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1)
                    GrabObject(ray2.collider.gameObject);
                else if(ray2.collider.gameObject.tag.Equals("GrabableBomb"))
                     ray2.collider.GetComponent<BombEnemy>().HitBomb(this);
                else if (ray2.collider.gameObject.tag.Equals("GrabNoWand")){
                    this.canLeftMove = true;
                    return;
                }
            }
            if (ray2.collider.gameObject.layer == 31 && input.RUN && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1)
                GrabObject(ray2.collider.gameObject);
            else if (ray2.collider.gameObject.layer == 31){
                if (ray2.collider.gameObject.GetComponent<KoopaTroopa>() != null)
                    ray2.collider.gameObject.GetComponent<KoopaTroopa>().TriggerFromPlayer(this);
                else
                    ray2.collider.gameObject.GetComponent<BuzzleBeetle>().TriggerFromPlayer(this);
            }
            canLeftMove = false;
        }else
            canLeftMove = true;

        RaycastHit2D rayU = Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay2, this.distanceYRayU, 0f), Vector2.left, 0.5f, wallMask);
        //Debug.DrawRay(_transform.position + new Vector3(this.distanceXRay2, this.distanceYRayU, 0f), Vector2.left, Color.blue);
        if ((rayU && rayU.collider.gameObject.layer != 27) | (rayU && rayU.collider.gameObject.layer == 27 && rayU.collider.gameObject.transform.eulerAngles.z == 0)){
            if (rayU.collider.gameObject.layer == 23)
                Damage();
            canLeftMove = false;
        }
    }

    public void GrabObject(GameObject gameObject){
        if (this.isInCapture | this.currentPlayerSprites.grabDuck == null | this.isInGrab | (gameObject.GetComponent<KoopaTroopa>() != null && gameObject.GetComponent<KoopaTroopa>().currentSheelState != KoopaTroopa.ShellState.StandShell))
            return;

        if (gameObject.tag.StartsWith("GrabParent"))
            gameObject = gameObject.transform.parent.gameObject;

        this.lastSpeed = 1;
        this.isInGrab = true;
        this.currentGrabedObject = gameObject;
        this.savedGrabedObjectSortingID = this.currentGrabedObject.GetComponentInChildren<SpriteRenderer>().sortingLayerID;
        this.currentGrabedObject.GetComponentInChildren<SpriteRenderer>().sortingLayerID = 2104620637;
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        if (gameObject.GetComponent<EntityGravity>() != null)
            gameObject.GetComponent<EntityGravity>().enabled = false;
        gameObject.transform.SetParent(this._transform);
        if (gameObject.GetComponentsInChildren<BoxCollider2D>()[1] != null)
            gameObject.GetComponentsInChildren<BoxCollider2D>()[1].enabled = false;

        foreach(Transform child in gameObject.transform){
            if (child.GetComponent<BoxCollider2D>() != null)
                child.GetComponent<BoxCollider2D>().enabled = false;
            foreach(Transform child2 in child){
                if (child2.GetComponent<BoxCollider2D>() != null)
                    child2.GetComponent<BoxCollider2D>().enabled = false;
            }
        }

        if (gameObject.GetComponent<KoopaTroopa>() != null){
            gameObject.GetComponent<KoopaTroopa>().currentSheelState = KoopaTroopa.ShellState.StandShell;
            if (gameObject.GetComponent<KoopaTroopa>().currentSheelState != KoopaTroopa.ShellState.StandShell && TileManager.instance.currentStyleID != TileManager.StyleID.SMW)
                gameObject.GetComponent<KoopaTroopa>().StartCoroutine(gameObject.GetComponent<KoopaTroopa>().WaitForExitShellIE());
            gameObject.GetComponent<TileAnimator>().StopCurrentAnimation();
        }

        if (this.currentAnimation == AnimationID.Duck)
            PlayAnimation(this.currentPlayerSprites.grabDuck, AnimationID.Duck);
        if (gameObject.GetComponent<Entity>() != null)
            GameManager.instance.StartCoroutine(SceneManager.KillLaserIE(gameObject, this, false));
    }

    public void UngrabCurrentObject(bool withoutCor = false){
        if (LevelEditorManager.instance && !LevelEditorManager.instance.isPlayMode)
            return;
        
        bool direction = this.sp.flipX;

        if ((Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay, this.distanceYRay, 0f), Vector2.right, 1.8f, wallMask) && !this.sp.flipX) | ((Physics2D.Raycast(_transform.position + new Vector3(this.distanceXRay, this.distanceYRay, 0f), Vector2.left, 1.8f, wallMask) && this.sp.flipX)))
            direction = !direction;

        this.isInGrab = false;
        if (this.currentGrabedObject == null)
            return;

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
        eff.transform.position = this.currentGrabedObject.transform.position;

        this.currentGrabedObject.GetComponentInChildren<SpriteRenderer>().sortingLayerID = this.savedGrabedObjectSortingID;

        if (this.currentAnimation == AnimationID.Duck)
            PlayAnimation(this.currentPlayerSprites.duck, AnimationID.Duck);
        else
            PlayAnimation(this.currentPlayerSprites.walk[0], AnimationID.Kick);
        this.canAnimate = false;
        SetCanAnimateAfterSeconds(0.2f);
        this.currentGrabedObject.transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
        if (!this.isDuck){
            if(!withoutCor)
                this.currentGrabedObject.GetComponent<EntityGravity>().StartCoroutine(DropGrabedObject(this.currentGrabedObject, direction));
        }else{
            if (this.currentGrabedObject.GetComponent<EntityGravity>() != null)
                this.currentGrabedObject.GetComponent<EntityGravity>().enabled = true;
            if (!direction)
                this.currentGrabedObject.transform.position = this.currentGrabedObject.transform.position + new Vector3(0.3f, 0, 0);
            else
                this.currentGrabedObject.transform.position = this.currentGrabedObject.transform.position + new Vector3(-0.3f, 0, 0);
        }
        if (this.currentGrabedObject.GetComponent<KoopaTroopa>() != null){
            if (direction)
                this.currentGrabedObject.GetComponent<KoopaTroopa>().direction = 1;
            else
                this.currentGrabedObject.GetComponent<KoopaTroopa>().direction = 0;

            //this.currentGrabedObject.GetComponent<KoopaTroopa>().StartAnimationClip(this.currentGrabedObject.GetComponent<KoopaTroopa>().animationClips[1]);
        }

        this.currentGrabedObject.GetComponent<BoxCollider2D>().enabled = true;
        StartIgnoreCollisionForSeconds(this.currentGrabedObject.GetComponent<BoxCollider2D>(), 0.4f);
        if (this.currentGrabedObject.GetComponentsInChildren<BoxCollider2D>().Length > 1 && (this.currentGrabedObject.GetComponent<Entity>() == null | (this.currentGrabedObject.GetComponent<KoopaTroopa>() != null | this.currentGrabedObject.GetComponent<BuzzleBeetle>() != null))){
            this.currentGrabedObject.GetComponentsInChildren<BoxCollider2D>()[1].enabled = true;
            StartIgnoreCollisionForSeconds(this.currentGrabedObject.GetComponentsInChildren<BoxCollider2D>()[1], 0.4f);
        }
        foreach (Transform child in this.currentGrabedObject.transform){
            if (child.GetComponent<BoxCollider2D>() != null)
                child.GetComponent<BoxCollider2D>().enabled = true;
            foreach (Transform child2 in child){
                if (child2.GetComponent<BoxCollider2D>() != null)
                    child2.GetComponent<BoxCollider2D>().enabled = true;
            }
        }

        GameManager.instance.StartCoroutine(SceneManager.KillLaserIE(this.currentGrabedObject, this, true));

        GameObject t = this.currentGrabedObject;
        this.currentGrabedObject = null;
        if (t.layer == 31){
            if (!this.isDuck){
                if (t.GetComponent<KoopaTroopa>() != null)
                    t.GetComponent<KoopaTroopa>().TriggerShell(this);
                else
                    t.GetComponent<BuzzleBeetle>().TriggerShell(this);
            }else{
                if (t.GetComponent<KoopaTroopa>() != null){
                    if(t.GetComponent<KoopaTroopa>().currentSheelState == KoopaTroopa.ShellState.MovingShell)
                         t.GetComponent<KoopaTroopa>().TriggerShell(this);
                }else{
                    if (t.GetComponent<BuzzleBeetle>().currentSheelState == BuzzleBeetle.ShellState.MovingShell)
                        t.GetComponent<BuzzleBeetle>().TriggerShell(this);
                }
            }
        }
    }

    public void StartIgnoreCollisionForSeconds(Collider2D col, float sec){
        StartCoroutine(IgnoreCollisionTimer(col, sec));
    }

    private IEnumerator IgnoreCollisionTimer(Collider2D col, float time){
        Physics2D.IgnoreCollision(this.bx, col, true);
        yield return new WaitForSeconds(time);
        Physics2D.IgnoreCollision(this.bx, col, false);
    }

    public IEnumerator DropGrabedObject(GameObject grabedObject, bool direction){
        float yTarget = grabedObject.transform.position.y + 0.8f;
        float xSpeed = 8;
        if (direction)
            xSpeed = -xSpeed;

        grabedObject.GetComponent<EntityGravity>().onGround = false;

        if (grabedObject.GetComponent<KoopaTroopa>() | grabedObject.GetComponent<BuzzleBeetle>()){
            if(!this.onGround)
                grabedObject.transform.Translate(0, -0.4f, 0);
            grabedObject.GetComponent<EntityGravity>().enabled = false;
            yield return new WaitForSeconds(0.13f);
            grabedObject.GetComponent<EntityGravity>().enabled = true;
            grabedObject.GetComponent<EntityGravity>().onGround = false;
        }else{

            while (grabedObject.transform.position.y < yTarget){
                RaycastHit2D dRay = Physics2D.Raycast(grabedObject.transform.position + new Vector3(0.1f, 0, 0f), Vector2.right, 0.5f, GameManager.instance.entityWandMask);
                if (xSpeed < 0)
                    dRay = Physics2D.Raycast(grabedObject.transform.position + new Vector3(0.1f, 0, 0f), Vector2.left, 0.5f, GameManager.instance.entityWandMask);
                if (dRay){
                    if (grabedObject.GetComponent<Entity>() != null && grabedObject.GetComponent<Entity>().isFreezedFromIceBall){
                        grabedObject.GetComponent<Entity>().DestroyIceBlock();
                        break;
                    }
                    xSpeed = -(xSpeed / 2);
                }

                grabedObject.transform.Translate(xSpeed * Time.deltaTime, 0, 0);
                RaycastHit2D rayG = Physics2D.Raycast(grabedObject.transform.position, Vector3.up, 0.5f, GameManager.instance.entityGroundMask);
                if (!rayG)
                    grabedObject.transform.Translate(0, 6 * Time.deltaTime, 0);
                else
                    yTarget = grabedObject.transform.position.y - 10;

                if (grabedObject.transform.parent.gameObject.layer == 9)
                    break;
                yield return new WaitForSeconds(0);
            }

            grabedObject.GetComponent<EntityGravity>().StartCoroutine(SceneManager.DropGameObject(grabedObject, xSpeed));
        }
        // yield return new WaitForSeconds(0.1f);
 
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9 && this.isInBubble)
            this.ResetPlayer();

        if (collision.gameObject.layer == 9 && !this.onGround)
            Jump();

        if (this.isStar && collision.gameObject.GetComponent<Entity>() != null)
            collision.gameObject.GetComponent<Entity>().StartCoroutine(collision.gameObject.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
    }

    public void ResetMkItemSlot(){
        this.currentMKItemSlotContent = MKItemSlotContent.NULL;
        GameManager.instance.sceneManager.mkItemSlotContentRenderer.transform.parent.gameObject.SetActive(false);
    }

    public void SetMkItemSlot(MKItemSlotContent content){
        if(content == MKItemSlotContent.NULL){
            ResetMkItemSlot();
            return;
        }

        GameManager.instance.sceneManager.mkItemSlotContentRenderer.transform.parent.gameObject.SetActive(true);
        this.currentMKItemSlotContent = content;
        if (content == MKItemSlotContent.FEATHER)
            this.sceneManager.mkItemSlotContentRenderer.sprite = TileManager.instance.GetSpriteFromTileset(32, TileManager.TilesetType.ItemTileset);
        else if (content == MKItemSlotContent.ONOFFSWITCH)
            this.sceneManager.mkItemSlotContentRenderer.sprite = TileManager.instance.GetSpriteFromTileset(34, TileManager.TilesetType.ItemTileset);
    }

    public void UseMkItemSlot(){
        if (this.currentMKItemSlotContent == MKItemSlotContent.FEATHER){
            if (!this.onGround)
                Jump(23, true);
            else
                Jump(43, true);
            GameObject effect = Instantiate(this.sceneManager.destroyEffect);
            effect.transform.position = this.transform.position;
        }else if (this.currentMKItemSlotContent == MKItemSlotContent.ONOFFSWITCH) { 
            SceneManager.SwitchOnOffState();
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.breakBlock);

            /*Effect*/
            InitItemSlotOnOffEffect(new Vector3(0, 0, 0));
            InitItemSlotOnOffEffect(new Vector3(1, 1, 0));
            InitItemSlotOnOffEffect(new Vector3(-1, 1, 0));
            InitItemSlotOnOffEffect(new Vector3(1, -1, 0));
            InitItemSlotOnOffEffect(new Vector3(-1, -1, 0));
        }

        ResetMkItemSlot();
    }

    private void InitItemSlotOnOffEffect(Vector3 vt){
        Color color = Color.red;
        if (!SceneManager.onOffState)
            color = Color.blue;

        GameObject eff = Instantiate(this.sceneManager.coinEffect);
        eff.GetComponent<SpriteRenderer>().color = color;
        eff.transform.position = this.transform.position + vt;
    }

    public void GetStar(){
        if(this.starCor != null | this.isStar){
            StopStar();
            GetStar();
            return;
        }

        BackgroundMusicManager.instance.StopCurrentBackgroundMusic();
        this.audioStarSource = SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.star);
        this.isStar = true;
        this.canGetDamage = false;
        this.starCor = StartCoroutine(StarCor());
    }

    public void StopStar(){
        if (this.starCor != null)
            StopCoroutine(this.starCor);
        this.starCor = null;
        this.isStar = false;
        this.sp.color = Color.white;
        this.canGetDamage = true;
        CancelIgnoreEnemiesDamageCollision();
        BackgroundMusicManager.instance.StartPlayingBackgroundMusic();
        if (this.audioStarSource != null)
            Destroy(this.audioStarSource);
    }

    private IEnumerator StarCor(){
        GameObject coinEffect = this.sceneManager.coinEffect;

        for (int f = 0; f < 140; f++){
            GameObject eff = Instantiate(coinEffect);
            eff.transform.position = this.transform.position + new Vector3(UnityEngine.Random.Range(-1, 3), UnityEngine.Random.Range(-1, 3), UnityEngine.Random.Range(1, 3));

            Color col = starColors[UnityEngine.Random.Range(0, starColors.Length)];
            if (col == lastColor)
                col = starColors[UnityEngine.Random.Range(0, starColors.Length)];
            this.lastColor = col;
            this.sp.color = col;

            if (f == 125){
                if (this.audioStarSource != null)
                    Destroy(this.audioStarSource);

                BackgroundMusicManager.instance.StartPlayingBackgroundMusic();
            }
            yield return new WaitForSeconds(0.1f);
        }
        StopStar();
    }

    public void ApplyMysteryCostume(MysteryCostume mysteryCostume){
        UngrabCurrentObject();
        SetPowerup(Powerup.Big, true, true);
        SetCollision(0);
        this.currentPlayerSprites = mysteryCostume.sprites;
        this.sp.sprite = this.currentPlayerSprites.stand[0];
        this.isMystery = true;
        this.isSpecialMystery = mysteryCostume.isSpecial;
        this.mysterySpecialID = mysteryCostume.specialID;
        if (this.powerupCor != null)
            StopCoroutine(this.powerupCor);
        this._transform.localScale = new Vector3(1, 1, 1);
    }

    private IEnumerator GunMarioCooldown(){
        yield return new WaitForSeconds(0.3f);
        this.gunCooldownCor = null;
    }

    public void AddKey(Key key){
        key.GetComponent<SpriteRenderer>().sortingLayerID = 2104620637;
        key.GetComponent<SpriteRenderer>().sortingOrder = -3;
        key.GetComponent<SpriteRenderer>().sprite = this.tileManager.GetSpriteFromTileset(177, TileManager.TilesetType.ObjectsTileset);
        this.keys.Add(key.transform.parent);
        Destroy(key);
    }

    public void RemoveKey(){
        GameObject eff = GameManager.instance.sceneManager.destroyEffect;
        eff.transform.position = this.keys[this.keys.Count - 1].position;
        Destroy(this.keys[this.keys.Count - 1].gameObject);
        this.keys.RemoveAt(this.keys.Count - 1);
    }

    public bool HasKey(){
        if (this.keys.Count < 1)
            return false;

        return true;
    }

    public void DestroyAllKeys(){
        foreach(Transform trans in this.keys){
            if(trans != null){
                GameObject eff = GameManager.instance.sceneManager.destroyEffect;
                eff.transform.position = trans.position;
                Destroy(trans.gameObject);
            }
        }

        this.keys.Clear();
    }

    public void ChangeGravity(){
        this.normalGravityScale = -this.normalGravityScale;
        this.fallGravityScale = this.normalGravityScale + 1;
        this.fallmultiplier = -this.savedFallMultipler;
        this.savedFallMultipler = -this.savedFallMultipler;
        this.lowJumpMultiplier = -this.lowJumpMultiplier;
        this.jumpVelocity = -this.jumpVelocity;

        if(this.normalGravityScale < 0){
            this._transform.eulerAngles = new Vector3(0, 0, 180);
            this.onGroundVector = Vector3.up;
        }else{
            this._transform.eulerAngles = Vector3.zero;
            this.onGroundVector = Vector3.down;
        }

        if (this.onGround)
            this.rb.gravityScale = this.normalGravityScale;
    }

    public void SetOnGround(bool var){
        this.onGround = var;
    }

    public bool GetOnGround(){
        return this.onGround;
    }

    public bool GetIsStar(){
        return this.isStar;
    }

    public bool GetIsInBubble(){
        return this.isInBubble;
    }

    public bool GetCanMoveRight(){
        return this.canRightMove;
    }

    public bool GetCanMoveLeft(){
        return this.canLeftMove;
    }

    public bool GetCanMove(){
        return this.canMove;
    }

    public void ResetLastSpeed(){
        this.lastSpeed = 1;
    }

    public float GetLastSpeed(){
        return this.lastSpeed;
    }

    public void SetCanMove(bool state){
        this.canMove = state;
        if (!this.canMove){
            BreakReturn();
            this.lastSpeed = 1;
        }
        if (!this.canMove && this.stopMovingCor != null)
             StopCoroutine(this.stopMovingCor);
    }

    public IEnumerator FreezeMovementForSeconds(float sec){
        this.canMove = false;
        yield return new WaitForSeconds(sec);
        this.canMove = true;
    }

    public void StartFreezingFromStamp(float sec = 1f, bool noEffect = false){
        if (this.isFreeze | !this.onGround)
            return;

        StartCoroutine(FreezeStampCor(sec, noEffect));
    }

    private IEnumerator FreezeStampCor(float sec, bool noEffect){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitGround);
        SceneManager.ShakeCamera();
        GameObject eff = null;
        if (!noEffect){
            eff = Instantiate(GameManager.instance.sceneManager.elcHitEffect);
            SceneManager.destroyAfterNewLoad.Add(eff);
            eff.transform.position = this._transform.position + new Vector3(1, 0, 0);
            eff.transform.SetParent(this._transform);
        }
        SetCanMove(false);
        BreakReturn();
        this.isFreeze = true;
        SetCanMove(true);

        yield return new WaitForSeconds(sec);
        this.isFreeze = false;

        if(!noEffect)
            Destroy(eff);
    }

    public void SetIsInWater(bool state){
        if (!state && this.tileManager != null && this.tileManager.currentTileset.autoEnableIsWater)
            state = true;
        if (this.isInWater && !state)
            StartCoroutine(ResetIsWaterCorForWaitIdkIE());
        this.isInWater = state;
    }

    private IEnumerator ResetIsWaterCorForWaitIdkIE(){
        yield return new WaitForSeconds(0.2f);
        if (!this.isInWater){
            this.canAnimate = true;
            PlayAnimation(this.currentPlayerSprites.jump, AnimationID.Jump);
            this.fallmultiplier = this.savedFallMultipler;
        }
    }

    public void SetIsInClimb(bool state, GameObject climbedGameObject = null, bool checkAlready = false){
        if (this.isInClimb == state && this.climbedGameObject == climbedGameObject | (checkAlready && this.isInClimb == state))
            return;

        this.directionPressFrames = 0;
        this.isInClimb = state;
        BreakReturn();
        if(this.stopMovingCor != null)
            StopCoroutine(this.stopMovingCor);

        this._transform.rotation = Quaternion.Euler(0, 0, 0);
        bool was = false;
        if (this.climbedGameObject != null)
            was = true;
        this.climbedGameObject = climbedGameObject;
        if (this.isInClimb){
            UngrabCurrentObject(false);
            Physics2D.IgnoreLayerCollision(9, 8, true);
            Physics2D.IgnoreLayerCollision(9, 23, true);
            this.sp.sortingOrder = 5;
            this.sp.sortingLayerID = 0;
            if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB1 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS1 | !was){
                StartCoroutine(FreezeMovementForSeconds(0.2f));
                this.canAnimate = false;
                SetCanAnimateAfterSeconds(0.2f);
            }
            ResetLastSpeed();
            this.rb.constraints = RigidbodyConstraints2D.FreezePositionY;
            this.sp.sprite = this.currentPlayerSprites.climb[0];

            if (TileManager.instance.currentStyleID == TileManager.StyleID.SMB1 | TileManager.instance.currentStyleID == TileManager.StyleID.SMAS1){
                if (!this.sp.flipX)
                    this._transform.position = new Vector3(this.climbedGameObject.transform.position.x, this._transform.position.y, this._transform.position.z) + new Vector3(-0.4f, 0, 0);
                else
                    this._transform.position = new Vector3(this.climbedGameObject.transform.position.x, this._transform.position.y, this._transform.position.z) + new Vector3(0.4f, 0, 0);
            }
        }
        else{
            Physics2D.IgnoreLayerCollision(9, 8, false);
            Physics2D.IgnoreLayerCollision(9, 23, false);
            this.sp.sortingOrder = 0;
            this.sp.sortingLayerID = 155026845;
            this.sp.sprite = this.currentPlayerSprites.stand[0];
            this.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public GameObject VineCheckHorz(float of, Vector2 direction){
        Physics2D.queriesStartInColliders = true;
        Physics2D.queriesHitTriggers = true;
        RaycastHit2D ray1 = Physics2D.Raycast(_transform.position + new Vector3(of, 0, 0), direction, 0.5f, wallMask);
        Physics2D.queriesHitTriggers = false;
        Physics2D.queriesStartInColliders = false;

        if (ray1 && ray1.collider.gameObject.GetComponent<Vine>() != null)
            return ray1.collider.gameObject;
        else
            return null;
    }

    public bool CheckIfOverPlayerIsABlock(float customXOffset = 0.37f){
        Physics2D.queriesStartInColliders = true;
        if (!Physics2D.Raycast(_transform.position + new Vector3(-customXOffset, 0f, 0f), -this.onGroundVector, 0.5f, this.duckMask) && !Physics2D.Raycast(_transform.position + new Vector3(customXOffset, 0f, 0f), Vector2.up, 0.5f, this.duckMask)){
            Physics2D.queriesStartInColliders = false;
            return false;
        }else{
            Physics2D.queriesStartInColliders = false;
            return true;
        }
    }

    public void ChangePlayerTypeFromButton(UnityEngine.UI.Text buttonText){
        if(this.playerType == PlayerType.MARIO){
            this.playerType = PlayerType.LUIGI;
            buttonText.text = "L";
            buttonText.color = Color.green;
        }else if(this.playerType == PlayerType.GREENTOAD){
            this.playerType = PlayerType.MARIO;
            buttonText.text = "M";
            buttonText.color = Color.red;
        }else if(this.playerType == PlayerType.LUIGI){
            this.playerType = PlayerType.BLUETOAD;
            buttonText.text = "T";
            buttonText.color = Color.blue;
            ChangeToadColor(this.tileManager.currentStyle.toadColors[0]);
        }else if(this.playerType == PlayerType.BLUETOAD){
            this.playerType = PlayerType.REDTOAD;
            buttonText.text = "T";
            buttonText.color = Color.red;
            ChangeToadColor(this.tileManager.currentStyle.toadColors[1]);
        }else{
            this.playerType = PlayerType.GREENTOAD;
            buttonText.text = "T";
            buttonText.color = Color.green;
            ChangeToadColor(this.tileManager.currentStyle.toadColors[2]);
        }

        if(!this.isMystery)
            this.SetPlayerSpritesFromPowerup(this.powerup, false);
        this.sp.sprite = this.currentPlayerSprites.stand[0];
        SettingsManager.instance.SetOption("PlayerType", (int)this.playerType);
    }

    public void ChangeToadColor(PlayerSpriteManager.ToadColors toadColors){
        this.toadMaterial.SetColor("_NewCol_Standart1", toadColors.standartCol1);
        this.toadMaterial.SetColor("_NewCol_Standart2", toadColors.standartCol2);
        this.toadMaterial.SetColor("_NewCol_Standart3", toadColors.standartCol3);
        this.toadMaterial.SetColor("_NewCol_Standart4", toadColors.standartCol4);
    }

    private void OnDestroy(){
        Debug.LogWarning("Player object got destroyed.");
    }

    private void OnTriggerStay2D(Collider2D collision){
        if (collision.gameObject.layer == 30)
            this.fakeWallMask.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 30)
            this.fakeWallMask.enabled = false;
    }

    private void OnGUI(){
        if (!DevGameManager.SHOWDEBUGINFORMATIONS)
            return;

        string text = "\nVersion: " + GameManager.instance.buildData.VERSION_STRING + ", FPS: " + GameManager.fps + "\nPowerup: " + this.powerup + "\nPlayerPos: " + _transform.position + "\nGravityScale: " + rb.gravityScale + " , Velocity: " + rb.velocity + "\nOnGround: " + this.onGround + ", FallMultipler: " + this.fallmultiplier + "\nIsIceGround: " + this.isIceGround + ", WalkFrames: " + this.walkFrames + "\nLastSpeed: " + this.lastSpeed + ", IsInWater: " + this.isInWater +
            "\nCurrentAnimation: " + this.currentAnimation + ", test: " + this.test + "\nIsMoving: " + this.isMoving + ", CanMove: " + this.canMove +
            "\nCanLeftMove: " + this.canLeftMove + ", CanRightMove: " + this.canRightMove + "\nDontAllowStopMovingCor:" + dontAllowStopMovingCor;

        GUI.Box(new Rect(0, 0, 300, 200),"Debug");
        GUI.Label(new Rect(0, 0, 300, 200), text);
    }
}

