using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;

public class LevelEditorVine : MonoBehaviour{

    public int length = 0;

    private BlockField myBlockField = null;

    public void LoadLength(BlockField myBlockField = null){
        if(myBlockField != null)
            this.myBlockField = myBlockField;
        BlockField targetBlockField2 = this.myBlockField;
        for (int i = 0; i <= this.length; i++){
            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField2);
            LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.VINE);
        }

        foreach (Transform child in this.transform.GetChild(0))
            Destroy(child.gameObject);

        this.transform.GetChild(2).localPosition = new Vector3(0, 2, 0);
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(127, TileManager.TilesetType.MainTileset);
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(126, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, 127, this.transform.GetChild(2).GetComponent<SpriteRenderer>()));
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, 126, this.transform.GetChild(1).GetComponent<SpriteRenderer>()));
        for (int i = 1; i < this.length; i++){
            GameObject clon = Instantiate(this.transform.GetChild(1).gameObject, this.transform.GetChild(0));
            clon.transform.localPosition = new Vector3(0, 1 + i , 0);
            this.transform.GetChild(2).position = this.transform.GetChild(2).position + new Vector3(0, 1, 0);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, 126, clon.GetComponent<SpriteRenderer>()));
        }
        if(this.length == 0)
            this.transform.GetChild(3).localPosition = new Vector3(0, 2 + this.length, 0);
        else
            this.transform.GetChild(3).localPosition = new Vector3(0, 1 + this.length, 0);

        BlockField targetBlockField = this.myBlockField;
        for (int i = 0; i <= this.length; i++){
            if (i == this.length)
                targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(this.myBlockField);
            else 
                targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField);

            LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField, this.gameObject, UMM.BlockData.BlockID.VINE, this.myBlockField.blockFieldNumber);
        }
    }

    public void AddLength(){
        this.length++;
        LoadLength();
    }

    public void RemoveLength(){
        if (this.length == 0)
            return;

        this.length--;
        LoadLength();
    }

    public void StartMovingLength(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        StartCoroutine(MovingLengthIE(button));
    }

    private IEnumerator MovingLengthIE(GameObject button){
        int orgY = 0;
        if(this.transform.eulerAngles.z == 0)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
        else if (this.transform.eulerAngles.z == -90 | this.transform.eulerAngles.z == 270 | this.transform.eulerAngles.z == 90)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.length++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY && this.length != 1){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.length--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }
        
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }

}
