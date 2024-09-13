using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoRender : MonoBehaviour{

    public bool dontDestroy = false;
    public int[] tileNumbers;
    public TileManager.TilesetType tilesetType = TileManager.TilesetType.MainTileset;

    private void OnEnable(){
        if (LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode){
            if (this.transform.parent == null)
                return;
            int area = GameManager.StringToInt(this.transform.parent.name);
            for (int i = 0; i < this.tileNumbers.Length; i++){
                this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.tileNumbers[i], this.tilesetType);
            }
        }else{
            if(this.GetComponent<SpriteRenderer>() != null)
                this.GetComponent<SpriteRenderer>().sortingOrder = 0;
            for (int i = 0; i < this.tileNumbers.Length; i++){
                this.transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 0;
                this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.tileNumbers[i], this.tilesetType);
                new TileManager.Tile(this.tilesetType, this.tileNumbers[i], this.transform.GetChild(i).GetComponent<SpriteRenderer>());
            }
        }
        if(!this.dontDestroy)
            Destroy(this);
    }

}
