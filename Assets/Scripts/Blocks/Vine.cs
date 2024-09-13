using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : MonoBehaviour{

    public int length = 0;

    private PlayerController p = null;

    private void Update(){
        if(this.p != null && this.p.input.UP && !this.p.input.JUMP){
            this.p.SetIsInClimb(true, this.gameObject, true);
        }
    }

    public void SpawnFromQuestionBlock(GameObject triggerBlock, GameObject triggerBlock2){
        StartCoroutine(SpawnFromQuestionBlockIE(triggerBlock, triggerBlock2));
    }

    private IEnumerator SpawnFromQuestionBlockIE(GameObject triggerBlock, GameObject triggerBlock2){
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.transform.GetChild(2).localPosition = new Vector3(0, 2, 0);
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(127, TileManager.TilesetType.MainTileset);
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(126, TileManager.TilesetType.MainTileset);
        this.length = 1;
        while(this.length != 22){
            Physics2D.queriesStartInColliders = true;

            RaycastHit2D ray = Physics2D.Raycast(this.transform.position + new Vector3(0, 1 + this.length, 0f), Vector2.up, 0.5f, GameManager.instance.entityGroundMask);
            if (ray && ray.collider.gameObject != triggerBlock && ray.collider.gameObject != triggerBlock2){
                this.length = 22;
                Physics2D.queriesStartInColliders = false;
                continue;
            }

            Physics2D.queriesStartInColliders = false;


            GameObject clon = Instantiate(this.transform.GetChild(1).gameObject, this.transform.GetChild(0));
            clon.transform.localPosition = new Vector3(0, 1 + this.length, 0);
            this.transform.GetChild(2).position = this.transform.GetChild(2).position + new Vector3(0, 1, 0);
            this.length++;
            this.GetComponent<BoxCollider2D>().size = new Vector2(0.87f, 2 + this.length);
            this.GetComponent<BoxCollider2D>().offset = new Vector2(0.002f, (0.5f) + (0.5f * this.length));
            yield return new WaitForSeconds(0.05f);
        }
    }

    public void LoadLength(int area){
        foreach (Transform child in this.transform.GetChild(0))
            Destroy(child.gameObject);

        this.transform.GetChild(2).localPosition = new Vector3(0, 2, 0);
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, 127, TileManager.TilesetType.MainTileset);
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, 126, TileManager.TilesetType.MainTileset);
        for (int i = 1; i < this.length; i++){
            GameObject clon = Instantiate(this.transform.GetChild(1).gameObject, this.transform.GetChild(0));
            clon.transform.localPosition = new Vector3(0, 1 + i, 0);
            this.transform.GetChild(2).position = this.transform.GetChild(2).position + new Vector3(0, 1, 0);
        }

        if (this.length == 0)
            this.length = 1;

        this.GetComponent<BoxCollider2D>().size = new Vector2(0.87f, 2 + this.length);
        this.GetComponent<BoxCollider2D>().offset = new Vector2(0.002f, (0.5f) + (0.5f * this.length));
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.p = collision.gameObject.GetComponent<PlayerController>();
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.p = null;
    }

}
