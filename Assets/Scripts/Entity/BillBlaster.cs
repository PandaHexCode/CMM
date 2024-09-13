using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBlaster : MonoBehaviour{

    public bool isRed = false;

    private EntityGravity gravity;
    private bool canSpawn = true;

    [System.NonSerialized]public int count = 0;

    private void OnEnable(){
        this.count = 0;
        this.gravity = this.GetComponent<EntityGravity>();
        StartCoroutine(SpawnLoop());
    }

    private void Update(){
        if (this.gravity.onGround)
            return;

        RaycastHit2D ray1 = Physics2D.Raycast(this.transform.position + new Vector3(0, this.gravity.Y_onground, 0), Vector2.down, 0.4f, GameManager.instance.stampMask);
        if(ray1 && ray1.collider.gameObject != null){
            if (ray1.collider.gameObject.layer == 9 && ray1.collider.gameObject.GetComponent<PlayerController>().GetOnGround())
                ray1.collider.gameObject.GetComponent<PlayerController>().Death();
            else if (ray1.collider.gameObject.GetComponent<Entity>())
                ray1.collider.gameObject.GetComponent<Entity>().StartCoroutine(ray1.collider.gameObject.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
        }
    }

    public int length = 0;

    public void LoadLength(){
        foreach (Transform child in this.transform.GetChild(0))
            Destroy(child.gameObject);

        for (int i = 0; i < this.length; i++){
            GameObject clon = new GameObject("Clon");
            clon.transform.SetParent(this.transform.GetChild(0));
            clon.transform.localPosition = new Vector3(0, -1 - i - 0.5f, 0);
            clon.AddComponent<SpriteRenderer>();
            clon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            clon.GetComponent<SpriteRenderer>().material = SettingsManager.instance.spriteMaterial;
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(33, TileManager.TilesetType.EnemyTileset);
        }

        this.GetComponent<EntityGravity>().Y_onground = this.GetComponent<EntityGravity>().Y_onground - this.length;
        this.GetComponent<EntityGravity>().onGroundAdd = 1f + this.length;
        this.GetComponent<BoxCollider2D>().size = new Vector2(0.87f, 2 + this.length);
        this.GetComponent<BoxCollider2D>().offset = new Vector2(0.002f, 0.003f - (0.5f * (this.length)));
    }

    private IEnumerator SpawnLoop(){
        int id = 52;
        if (this.isRed)
            id = 54;

        while (true){
            yield return new WaitForSeconds(2.5f);
            bool isABlockInWay = true;
            while (!this.canSpawn | isABlockInWay){
                RaycastHit2D ray1 = Physics2D.Raycast(this.transform.position + new Vector3(0, 0.5f, 0), Vector2.left, 1f, GameManager.instance.entityWandMask);
                if (GameManager.instance.sceneManager.players[0].transform.position.x > this.transform.position.x)
                    ray1 = Physics2D.Raycast(this.transform.position + new Vector3(0, 0.5f, 0), Vector2.right, 1f, GameManager.instance.entityWandMask);

                if (!ray1)
                    isABlockInWay = false;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);

            if(this.count > 10){
                while(this.count > 10){
                    yield return new WaitForSeconds(0.5f);
                }
            }

            GameObject clon = Instantiate(GameManager.instance.blockDataManager.blockDatas[id].prefarb);
            this.count++;
            clon.GetComponent<Entity>().canStack = false;
            clon.GetComponent<BillEnemy>().spawner = this;
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(GameManager.instance.blockDataManager.blockDatas[id].spriteId, TileManager.TilesetType.EnemyTileset);
            clon.transform.SetParent(this.transform.parent);
            if (GameManager.instance.sceneManager.players[0].transform.position.x > this.transform.position.x){
                clon.GetComponent<BillEnemy>().speed = -clon.GetComponent<BillEnemy>().speed;
                clon.GetComponent<SpriteRenderer>().flipX = true;
                clon.transform.position = this.transform.position + new Vector3(0.3f, 0.5f, 0);
            }else
                clon.transform.position = this.transform.position + new Vector3(-0.3f, 0.5f, 0);
            SceneManager.destroyAfterNewLoad.Add(clon);
            SoundManager.PlayAudioClipIfPlayerIsInNear(SoundManager.currentSoundEffects.cannon, this.transform.position);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.canSpawn = false;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.canSpawn = true;
    }

}
