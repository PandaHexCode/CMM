using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffBlock : OnOffObject{

    [Header("OnOffBlock")]
    public bool isBlueBlock = false;

    public override void OnSwitch(bool state){
        if (!this.isBlueBlock){
            if (state){
                this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(160, TileManager.TilesetType.MainTileset);
                this.GetComponent<BoxCollider2D>().enabled = true;
            }
            else{
                this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(176, TileManager.TilesetType.MainTileset);
                this.GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        else{
            if (state){
                this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(177, TileManager.TilesetType.MainTileset);
                this.GetComponent<BoxCollider2D>().enabled = false;
            }
            else{
                this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(161, TileManager.TilesetType.MainTileset);
                this.GetComponent<BoxCollider2D>().enabled = true;
            }
        }

        if (this.GetComponent<BoxCollider2D>().enabled)
            SceneManager.CheckToKillEnemy(this.gameObject);
    }

}
