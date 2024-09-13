using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerBro : Entity{

    public bool isBig = false;
    public float startDirection = 1;
    private float speed = 1.5f;
    private float startX = 0;

    [System.NonSerialized]public GameObject currentHammer;

    private Transform _transform;
    private EntityGravity gravity;
    private Transform playerTrans;

    private void OnEnable(){
        OnEnableTileAnimator();
        this._transform = this.transform;
        this.gravity = GetComponent<EntityGravity>();
        this.playerTrans = GameManager.instance.sceneManager.players[0].transform;
        StartAction();
    }

    public void StartAction(){
        this.startX = this._transform.position.x;
        StartCoroutine(ActionIE());
    }

    private IEnumerator ActionIE(){
        while (!this.isSpawned){
            yield return new WaitForSeconds(0);
        }
        if (this._transform.parent.GetComponent<HammerBro>() != null)
            this.startDirection = this._transform.parent.GetComponent<HammerBro>().startDirection;

        StartCoroutine(MoveIE(this.startDirection));

        int i = 0;

        while (true){
            yield return new WaitForSeconds(1);
            StartCoroutine(ThrowHammer());
            yield return new WaitForSeconds(1.4f);
            StartCoroutine(ThrowHammer());
            yield return new WaitForSeconds(1.4f);
            StartCoroutine(ThrowHammer());
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ThrowHammer());
            yield return new WaitForSeconds(0.5f);
            if ((i == 0 | i == 1 | (this.isBig && i == 0)) && this._transform.parent.GetComponent<Entity>() == null){
                if (this.startDirection == 1)
                    StartCoroutine(JumpDownIE());
                else
                    StartCoroutine(JumpIE());
            }else if (i == 2 && this._transform.parent.GetComponent<Entity>() == null){
                if (this.startDirection == 1)
                    StartCoroutine(JumpIE());
                else
                    StartCoroutine(JumpDownIE());
            }

            float seconds = 1;
            if (i == 1)
                seconds = 1.1f;
            if (this.startDirection == 1)
                seconds = seconds + 0.5f;
            yield return new WaitForSeconds(seconds);

            if (i == 0)
                i = 1;
            else if (i == 1)
                i = 2;
            else
                i = 0;
        }
    }

    private IEnumerator MoveIE(float off = 1){
        float targetX = startX + off;
        while (this._transform.position.x < targetX && CheckEdgeGround() && !CheckWand(Vector2.right)){
            if(this.gravity.onGround)
                this._transform.Translate(this.speed * Time.deltaTime, 0, 0);
            if (this._transform.position.x > this.playerTrans.position.x)
                this.sp.flipX = false;
            else
                this.sp.flipX = true;
            yield return new WaitForSeconds(0);
        }

        targetX = startX - 1;
        while (this._transform.position.x > targetX && CheckEdgeGround(-0.3f) && !CheckWand(Vector2.left)){
            if (this.gravity.onGround)
                this._transform.Translate(-this.speed * Time.deltaTime, 0, 0);
            if (this._transform.position.x > this.playerTrans.position.x)
                this.sp.flipX = false;
            else
                this.sp.flipX = true;
            yield return new WaitForSeconds(0);
        }

        yield return new WaitForSeconds(0);
        StartCoroutine(MoveIE(1));
    }

    private bool CheckWand(Vector2 vt){
        RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, vt, 0.3f, this.wandMask);
        if (raycastHit2D)
            return true;

        return false;
    }

    private bool CheckEdgeGround(float x = 0.3f){
        if (Physics2D.Raycast(_transform.position + new Vector3(x, gravity.Y_onground, 0f), -Vector2.up, 0.5f, this.groundMask))
            return true;

        return false;
    }

    public IEnumerator ThrowHammer(bool friendlyHammer = false){
        this.speed = 0;
        GameObject hammer = null;
        if (friendlyHammer)
            hammer = Instantiate(GameManager.instance.sceneManager.friendlyHammer);
        else
            hammer = Instantiate(GameManager.instance.sceneManager.hammer);
        this.currentHammer = hammer;
        hammer.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(75, TileManager.TilesetType.EnemyTileset);
        SceneManager.destroyAfterNewLoad.Add(hammer);
        if (!this.isBig)
            hammer.transform.position = this.transform.position + new Vector3(0, 1, 0);
        else{
            if(!this.sp.flipX)
                hammer.transform.position = this.transform.position + new Vector3(0.5f, 1, 0);
            else
                hammer.transform.position = this.transform.position + new Vector3(-0.5f, 1, 0);
        }
        StartAnimationClip(this.animationClips[1]);
        yield return new WaitForSeconds(0.1f);
        StartAnimationClip(this.animationClips[2]);
        yield return new WaitForSeconds(0.3f);
        if(!friendlyHammer)
            this.currentHammer = null;
        if(this.sp.flipX)
            hammer.AddComponent<Rigidbody2D>().velocity = new Vector2(6, 13);
        else
            hammer.AddComponent<Rigidbody2D>().velocity = new Vector2(-6, 13);
        StartCoroutine(DespawnHammerIE(hammer));
        hammer.GetComponent<RotateBlock>().enabled = true;
        StartAnimationClip(this.animationClips[0]);
        this.speed = 1.5f;
    }

    private IEnumerator DespawnHammerIE(GameObject hammer){
        Rigidbody2D rb = hammer.GetComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        while (rb.velocity.y > 0)
            yield return new WaitForSeconds(0);

        rb.gravityScale = 4;
        while (hammer.transform.position.y > 11)
            yield return new WaitForSeconds(0);

        Destroy(hammer);
    }

    private IEnumerator JumpIE(){
        this.gravity.enabled = false;
        float targetY = this._transform.position.y + 4;
        while(this._transform.position.y < targetY){
            this.transform.Translate(0, 10 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }
        this.gravity.enabled = true;
        this.gravity.onGround = false;
        if(this.isBig)
            GameManager.instance.StartCoroutine(BigStampIE());
    }

    private IEnumerator JumpDownIE(){
        RaycastHit2D ray = Physics2D.Raycast(this.transform.position + new Vector3(0, -1, 0), Vector2.down, 5f, GameManager.instance.entityGroundMask);
        bool can = false;
        
        if (ray){
            RaycastHit2D ray2 = Physics2D.Raycast(ray.collider.gameObject.transform.position, Vector2.up, 1f, GameManager.instance.entityGroundMask);
            if (!ray2)
                can = true;
        }
        
        this.gravity.enabled = false;
        if (!can)
            StartCoroutine(JumpIE());
        else{
            float targetY = this._transform.position.y + 1;
            while (this._transform.position.y < targetY){
                this.transform.Translate(0, 10 * Time.deltaTime, 0);
                yield return new WaitForSeconds(0);
            }

            targetY = ray.collider.gameObject.transform.position.y + ray.collider.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y + this.gravity.onGroundAdd + ray.collider.gameObject.GetComponent<BoxCollider2D>().offset.y;
            while (this._transform.position.y > targetY){
                if(!this.isBig)
                    this.transform.Translate(0, -10 * Time.deltaTime, 0);
                else
                    this.transform.Translate(0, -13 * Time.deltaTime, 0);
                yield return new WaitForSeconds(0);
            }

            if(this.isBig)
                GameManager.instance.StartCoroutine(BigStampIE());
            this.gravity.enabled = true;
        }
    }

    private IEnumerator BigStampIE(){
        while (!this.gravity.onGround){
            yield return new WaitForSeconds(0);
        }

        foreach (PlayerController p in GameManager.instance.sceneManager.players)
            p.StartFreezingFromStamp();
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        p.Jump(-1, true);

        if (p.GetPowerup() == PlayerController.Powerup.Mini)
            return;

        UnlockKey();
        if (this.currentHammer != null)
            Destroy(this.currentHammer);

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            Destroy(this.gameObject);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(this.ShootDieAnimation(this.gameObject));
    }

    private void OnDestroy(){
        UnStack();
        if (this.currentHammer != null)
            Destroy(this.currentHammer);
    }

}
