using System.Collections;
using System.Collections.Generic;
using UMM.BlockField;
using UnityEngine;

public class LevelEditorPlatfromLift : MonoBehaviour{

    [System.NonSerialized]public LiftHelper.Direction direction = LiftHelper.Direction.DOWN;
    public int length = 0;
    public bool isBlue = false;

    private BlockField myBlockField = null;

    public void LoadLength(BlockField blockField = null){
         int middleSpriteID = 50;
         int endSpriteID = 51;
         if (this.isBlue){
            middleSpriteID = 53;
            endSpriteID = 54;
         }

        if (blockField != null)
            this.myBlockField = blockField;

        BlockField targetBlockField2 = this.myBlockField;
        for (int i = 0; i <= this.length + 1; i++){
            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1);
            LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PLATFORM);
        }

        GameObject clonReference = this.transform.GetChild(1).gameObject;
        Transform parent = this.transform.GetChild(0);
        clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(middleSpriteID, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, middleSpriteID, clonReference.GetComponent<SpriteRenderer>()));
        GameObject endRef = this.transform.GetChild(2).gameObject;
        endRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(endSpriteID, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, endSpriteID, endRef.GetComponent<SpriteRenderer>()));
        endRef.transform.localPosition = new Vector3(2, 0, 0);

        foreach (Transform child in parent){
            if(child != clonReference.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.length; i++){
            GameObject clon = Instantiate(clonReference, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(middleSpriteID, TileManager.TilesetType.ObjectsTileset);
            clon.transform.position = clonReference.transform.position + new Vector3(i, 0);
            endRef.transform.position = clon.transform.position + new Vector3(1, 0);

            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, middleSpriteID, clon.GetComponent<SpriteRenderer>()));
        }

        BlockField targetBlockField = this.myBlockField;
        for (int i = 0; i <= this.length; i++){
            targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField.blockFieldNumber + 1);
            LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField, this.gameObject, UMM.BlockData.BlockID.PLATFORM, this.myBlockField.blockFieldNumber);
        }

        if (!this.isBlue){
            this.transform.GetChild(3).localPosition = new Vector3(1f + (this.length * 0.5f), 0, 0);
            this.transform.GetChild(4).localPosition = new Vector3(2.3f + this.length, 0, 0);
        }else
            this.transform.GetChild(3).localPosition = new Vector3(2.3f + this.length, 0, 0);
    }

    public void ChangeDirectionButton(){
        switch (this.direction){
            case LiftHelper.Direction.DOWN:
                ChangeDirection(LiftHelper.Direction.RIGHT);
                break;
            case LiftHelper.Direction.RIGHT:
                ChangeDirection(LiftHelper.Direction.UP);
                break;
            case LiftHelper.Direction.UP:
                ChangeDirection(LiftHelper.Direction.LEFT);
                break;
            case LiftHelper.Direction.LEFT:
                ChangeDirection(LiftHelper.Direction.DOWN);
                break;
        }
    }

    public void ChangeDirection(LiftHelper.Direction direction){
        if (this.isBlue)
            return;

        this.direction = direction;
        switch (direction){
            case LiftHelper.Direction.UP:
                this.transform.GetChild(3).localEulerAngles = new Vector3(0, 0, -90);
                break;
            case LiftHelper.Direction.DOWN:
                this.transform.GetChild(3).localEulerAngles = new Vector3(0, 0, 90);
                break;
            case LiftHelper.Direction.RIGHT:
                this.transform.GetChild(3).localEulerAngles = new Vector3(0, 0, 180);
                break;
            case LiftHelper.Direction.LEFT:
                this.transform.GetChild(3).localEulerAngles = new Vector3(0, 0, 0);
                break;
        }
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
        orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x > lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.length++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x < lastY && this.length != 0){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.length--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }

        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }

}
