using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;

public class ItemBlock : TileAnimator{

    public BlockID contentBlock;
    public bool bigBlock = false;
    private bool wasUsed = false;
    [System.NonSerialized]public int extraNumber = 0 /*Used for mystery mushrom costume number save*/;
    [System.NonSerialized]public Coroutine hitBlockAnimationCor = null;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.wasUsed && collision.GetComponent<Rigidbody2D>().velocity.y > -5){
            float t = 0.8f;
            if (this.bigBlock)
                t = 1.6f;
            if (collision.gameObject.transform.position.x - t < this.transform.position.x && collision.gameObject.transform.position.x + t > this.transform.position.x){
                UseItemBlock(collision.gameObject);
                collision.gameObject.GetComponent<PlayerController>().CancelJump();
            }
        }
    }

    public virtual void UseItemBlock(GameObject player, bool isHitDown = false, bool noPowerupCheck = false){
        if (this.wasUsed)
            return;
        this.wasUsed = true;
        this.hitBlockAnimationCor = GameManager.instance.PlayBlockHitAnimation(this.gameObject, isHitDown);
        StartCoroutine(SpawnContent(isHitDown));
    }


    public IEnumerator SpawnContent(bool isHitDown){/*TODO: Find some good way with DeltaTime*/
        bool canSpawn = true;
        Vector3 offset = Vector3.up;
        if (isHitDown)
            offset = Vector3.down;
        RaycastHit2D ray1 = Physics2D.Raycast(this.transform.position + new Vector3(0, 0f, 0f), offset, 0.5f, GameManager.instance.entityWandMask);
        if (ray1 && ray1.collider.gameObject != null)
            canSpawn = false;
        if (this.contentBlock == BlockID.COIN)
            canSpawn = true;

        StopCurrentAnimation();
        if(!this.bigBlock)
            this.sp.sprite = TileManager.instance.GetSpriteFromTileset(5, TileManager.TilesetType.MainTileset);
        else
            this.sp.sprite = TileManager.instance.GetSpriteFromTileset(168, TileManager.TilesetType.ObjectsTileset);
        if (canSpawn){
            if (GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].onlyForSMB1 && TileManager.instance.currentStyle.id != TileManager.StyleID.SMB1)
                this.contentBlock = BlockID.COIN;

            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
            GameObject clon = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].prefarb, this.gameObject.transform.parent);
            clon.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].spriteId, GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].tilesetType);
            offset = new Vector3(0, 0.5f, 0);
            if (isHitDown)
                offset = new Vector3(0, -0.5f, 0);
            clon.transform.position = this.transform.position + offset;

            if (this.contentBlock == BlockID.MYSTERY_MUSHROM)
                clon.GetComponent<MysteryMushrom>().costumeNumber = this.extraNumber;
            else if(this.contentBlock == BlockID.VINE){
                clon.GetComponent<Vine>().SpawnFromQuestionBlock(this.gameObject, this.transform.GetChild(0).gameObject);
                offset = new Vector3(0, 0f, 0);
                clon.transform.position = this.transform.position + offset;
                StopAllCoroutines();
            }
      
            if (this.contentBlock == BlockID.COIN){
                if (!this.bigBlock){
                    SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.coin);
                    clon.GetComponent<BoxCollider2D>().enabled = false;
                    for (int i = 0; i < 15; i++)
                    {
                        clon.transform.Translate(0, 0.15f, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    GameManager.instance.sceneManager.AddCoins();
                    Destroy(clon);
                }else{
                    GameObject clon2 = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].prefarb, this.gameObject.transform.parent);
                    GameObject clon3 = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].prefarb, this.gameObject.transform.parent);
                    clon2.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].spriteId, GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].tilesetType);
                    clon3.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].spriteId, GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].tilesetType);
                    Vector3 offset2 = new Vector3(-1, 0.5f, 0);
                    Vector3 offset3 = new Vector3(1, 0.5f, 0);
                    if (isHitDown){
                        offset = new Vector3(0, -0.5f, 0);
                        offset2 = new Vector3(-1, -0.5f, 0);
                        offset3 = new Vector3(1, -0.5f, 0);
                    }

                    clon.transform.position = this.transform.position + offset;
                    clon2.transform.position = this.transform.position + offset2;
                    clon3.transform.position = this.transform.position + offset3;

                    SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.coin);
                    clon.GetComponent<BoxCollider2D>().enabled = false;
                    clon2.GetComponent<BoxCollider2D>().enabled = false;
                    clon3.GetComponent<BoxCollider2D>().enabled = false;
                    for (int i = 0; i < 15; i++){
                        clon.transform.Translate(0, 0.15f, 0);
                        clon2.transform.Translate(0, 0.15f, 0);
                        clon3.transform.Translate(0, 0.15f, 0);
                        yield return new WaitForSeconds(0.01f);
                    }
                    GameManager.instance.sceneManager.AddCoins(3);
                    Destroy(clon);
                    Destroy(clon2);
                    Destroy(clon3);
                }
            }else{
                SceneManager.destroyAfterNewLoad.Add(clon);
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.itemPop);
                SpriteRenderer clonSp = clon.GetComponentInChildren<SpriteRenderer>();
                int savedOrder = clonSp.sortingOrder;
                clonSp.sortingOrder = -1;
                bool wasEntity = false;
                if (clon.GetComponent<Entity>() != null){
                    clon.GetComponent<Entity>().Spawn();
                    if (clon.GetComponent<Entity>().canMove){
                        clon.GetComponent<Entity>().canMove = false;
                        wasEntity = true;
                    }
                }

                if (clon.GetComponentInChildren<CheepCheep>() != null)
                    clon.GetComponentInChildren<CheepCheep>().enabled = false;

                bool wasRB = false;
                if(clon.GetComponent<Rigidbody2D>() != null && !clon.GetComponent<Rigidbody2D>().isKinematic){
                    wasRB = true;
                    clon.GetComponent<Rigidbody2D>().isKinematic = true;
                    clon.GetComponent<Rigidbody2D>().simulated = false;
                }
                if (clon.GetComponent<StarItem>() != null)
                    clon.GetComponent<StarItem>().enabled = false;

                if (clon.GetComponent<EntityGravity>() != null)
                    clon.GetComponent<EntityGravity>().enabled = false;
                if(this.contentBlock == BlockID.PROBELLER | this.contentBlock == BlockID.FIRE_ENEMY){
                    clon.GetComponent<Animator>().StopPlayback();
                    clon.GetComponent<Animator>().enabled = false;
                }

                float speed = 0.015f;
                if (isHitDown)
                    speed = -speed;
                for (int i = 0; i < 35; i++){
                    clon.transform.Translate(0, speed, 0);
                    yield return new WaitForSeconds(0.01f);
                }
                clonSp.sortingOrder = savedOrder;
                if (wasEntity)
                    clon.GetComponent<Entity>().canMove = true;
                if (clon.GetComponent<EntityGravity>() != null)
                    clon.GetComponent<EntityGravity>().enabled = true;
                if (this.contentBlock == BlockID.PROBELLER | this.contentBlock == BlockID.FIRE_ENEMY){
                    clon.GetComponent<Animator>().enabled = true;
                    clon.GetComponent<Animator>().Play(0);
                }
                if (wasRB){
                    clon.GetComponent<Rigidbody2D>().isKinematic = false;
                    clon.GetComponent<Rigidbody2D>().simulated = true;
                }
                if (clon.GetComponent<StarItem>() != null)
                    clon.GetComponent<StarItem>().enabled = true;
                if (clon.GetComponentInChildren<CheepCheep>() != null)
                    clon.GetComponentInChildren<CheepCheep>().enabled = true;
            }
        }
        if (this != null)
            Destroy(this);
    }

}
