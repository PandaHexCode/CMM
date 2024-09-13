using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thwomp : Entity{

    private bool isInArea = false;
    private bool isInAttack = false;
    private bool isInWait = false;
    private bool isInReturn = false;
    private bool wasAttack = false;

    private float oldY = 0;
    private Transform player = null;

    public void StopAllActions(){
      //  this.isInArea = false;
        this.isInAttack = false;
        this.isInWait = false;
        this.isInReturn = false;
        this.wasAttack = false;
        StopAllCoroutines();
    }

    private void Update(){
        if (this.isInAttack && !this.wasAttack){
            RaycastHit2D ray1 = Physics2D.Raycast(transform.position + new Vector3(0.31f, -0.51f, 0f), Vector2.down, 0.5f, (GameManager.instance.entityGroundMask | GameManager.instance.entityMask));
            RaycastHit2D ray2 = Physics2D.Raycast(transform.position + new Vector3(-0.31f, -0.51f, 0f), Vector2.down, 0.5f, (GameManager.instance.entityGroundMask | GameManager.instance.entityMask));
            if (ray1 | ray2){
                bool cancel = false;
                if (ray1){
                    if (ray1.collider.GetComponent<Entity>() != null && !ray1.collider.gameObject.tag.Equals("CU") && ray1.collider.GetComponent<PowerupItem>() == null){
                        ray1.collider.GetComponent<Entity>().StartCoroutine(ray1.collider.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
                        cancel = true;
                    }
                    if (ray1.collider.GetComponent<Entity>() != null && ray1.collider.gameObject.tag.Equals("CU"))
                        cancel = true;
                    if (ray1.collider.GetComponent<PSwitch>())
                        ray1.collider.GetComponent<PSwitch>().Activate();
                    SceneManager.CheckBlockToUseOrDestroy(ray1.collider.gameObject, true);
                }if (ray2){
                    if (ray2.collider.GetComponent<Entity>() != null && !ray2.collider.gameObject.tag.Equals("CU") && ray2.collider.GetComponent<PowerupItem>() == null){
                        ray2.collider.GetComponent<Entity>().StartCoroutine(ray2.collider.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
                        cancel = true;
                    }
                    if (ray2.collider.GetComponent<Entity>() != null && ray2.collider.gameObject.tag.Equals("CU"))
                        cancel = true;
                    if ((ray1 && ray1.collider.GetComponentInParent<OnOffSwitcher>() != null) && ray2.collider.GetComponentInParent<OnOffSwitcher>() != null){
                    }else{
                        if (ray2.collider.GetComponent<PSwitch>())
                            ray2.collider.GetComponent<PSwitch>().Activate();
                        SceneManager.CheckBlockToUseOrDestroy(ray2.collider.gameObject, true);
                    }
                }

                if (!cancel){
                    SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.cannon);
                    SceneManager.ShakeCamera();
                    this.isInAttack = false;
                    this.wasAttack = true;
                    this.isInWait = true;
                    StartCoroutine(WaitIE(true));
                    return;
                }
            }
            if(this.transform.position.y < 4){
                StopAllCoroutines();
                Destroy(this.gameObject);
                return;
            }
            this.transform.Translate(0, -10 * Time.deltaTime, 0);
            return;
        }else if (this.isInReturn){
            RaycastHit2D ray1 = Physics2D.Raycast(transform.position + new Vector3(0.31f, 0.7f, 0f), Vector2.up, 0.5f, GameManager.instance.entityGroundMask);
            RaycastHit2D ray2 = Physics2D.Raycast(transform.position + new Vector3(-0.31f, 0.7f, 0f), Vector2.up, 0.5f, GameManager.instance.entityGroundMask);
            if(ray1 | ray2){
                this.isInReturn = false;
                this.isInWait = true;
                StartCoroutine(WaitIE(false));
            }

            if (this.transform.position.y >= this.oldY){
                this.wasAttack = false;
                this.isInReturn = false;
                this.transform.position = new Vector3(this.transform.position.x, this.oldY, this.transform.position.z);
                this.isInWait = true;
                StartCoroutine(WaitIE(false));
                return;
            }
            this.transform.Translate(0, 5 * Time.deltaTime, 0);
            return;
        }

        if (!this.isInArea | this.isInWait)
            return;

        if ((this.player.position.x < this.transform.position.x && this.player.position.x < this.transform.position.x - 2.3f) | (this.player.position.x > this.transform.position.x && this.player.position.x > this.transform.position.x + 2.3f))
            this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(40, TileManager.TilesetType.EnemyTileset);
        else{
            this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(41, TileManager.TilesetType.EnemyTileset);
            if(this.oldY == 0)
                this.oldY = this.transform.position.y;
            this.isInAttack = true;
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            this.OnTriggerPlayer(collision.gameObject.GetComponent<PlayerController>());
            this.isInArea = true;
            this.player = collision.gameObject.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            this.isInArea = false;
            if(!this.isInAttack)
                this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(39, TileManager.TilesetType.EnemyTileset);
        }
    }

    private IEnumerator WaitIE(bool setReturn){
        if(setReturn)
            yield return new WaitForSeconds(0.2f);
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(39, TileManager.TilesetType.EnemyTileset);
        yield return new WaitForSeconds(0.3f);
        if (setReturn){
            this.isInReturn = true;
            yield return new WaitForSeconds(0.5f);
        }else if (this.isInArea){
            this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(40, TileManager.TilesetType.EnemyTileset);
            yield return new WaitForSeconds(0.2f);
        }

        this.isInWait = false;
    }

    public override void OnTriggerPlayer(PlayerController p){
        if (!p.isInSpin && ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround()))
            return;

        if(((int)this.transform.position.x + 1.1f > (int)p.transform.position.x) && (int)this.transform.position.x - 1.1f < (int)p.transform.position.x){

        }else
            return;

        if (p.isInSpin){
            StartCoroutine(ReactivateDamage());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = p.transform.position;
            return;
        }
    }

    private IEnumerator ReactivateDamage(){
        this.damageOnCollision = false;
        yield return new WaitForSeconds(0.05f);
        this.damageOnCollision = true;
    }

}
