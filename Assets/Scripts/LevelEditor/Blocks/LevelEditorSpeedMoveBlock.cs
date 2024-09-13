using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;

public class LevelEditorSpeedMoveBlock : MonoBehaviour{

    public int length = 0;
    public SpeedMoveBlock.OnOffState onOffState = SpeedMoveBlock.OnOffState.NONE;
    public LiftHelper.Direction direction = LiftHelper.Direction.LEFT;
    public int fast = 0;/*1 == true*/

    private int middleSpriteID = 80;
    private int endSpriteID = 81;

    private BlockField myBlockField = null;

    public void LoadLength(BlockField blockField = null){
        if (blockField != null)
            this.myBlockField = blockField;

        BlockField targetBlockField2 = this.myBlockField;
        for (int i = 0; i <= this.length + 1; i++){
            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1);
            LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.SPEEDMOVEBLOCK);
        }

        GameObject clonReference = this.transform.GetChild(0).gameObject;
        Transform parent = this.transform.GetChild(2);
        clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteID, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, this.middleSpriteID, clonReference.GetComponent<SpriteRenderer>()));
        GameObject endRef = this.transform.GetChild(1).gameObject;
        endRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.endSpriteID, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, this.endSpriteID, endRef.GetComponent<SpriteRenderer>()));
        endRef.transform.localPosition = new Vector3(2, 0, 0);

        foreach (Transform child in parent){
            if(child != clonReference.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.length; i++){
            GameObject clon = Instantiate(clonReference, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteID, TileManager.TilesetType.ObjectsTileset);
            clon.transform.position = clonReference.transform.position + new Vector3(i, 0);
            endRef.transform.position = clon.transform.position + new Vector3(1, 0);

            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, this.middleSpriteID, clon.GetComponent<SpriteRenderer>()));
        }

        BlockField targetBlockField = this.myBlockField;
        for (int i = 0; i <= this.length; i++){
            targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField.blockFieldNumber + 1);
            LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField, this.gameObject, UMM.BlockData.BlockID.SPEEDMOVEBLOCK, this.myBlockField.blockFieldNumber);
        }

        this.transform.GetChild(6).localPosition = new Vector3(2 + this.length, 0, 0);
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

    public void ChangeDirection(){
        if (this.direction == LiftHelper.Direction.LEFT)
            this.direction = LiftHelper.Direction.RIGHT;
        else
            this.direction = LiftHelper.Direction.LEFT;
        LoadDirection();
    }

    public void ChangeOnOffState(){
        if (this.onOffState == SpeedMoveBlock.OnOffState.NONE)
            this.onOffState = SpeedMoveBlock.OnOffState.Red;
        else if (this.onOffState == SpeedMoveBlock.OnOffState.Red)
            this.onOffState = SpeedMoveBlock.OnOffState.Blue;
        else if(this.onOffState == SpeedMoveBlock.OnOffState.Blue)
            this.onOffState = SpeedMoveBlock.OnOffState.NONE;

        LoadDirection();
    }

    public void ChangeIsFast(){
        if (this.fast == 0){
            this.transform.GetChild(4).transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.blue;
            this.fast = 1;
        }else{
            this.fast = 0;
            this.transform.GetChild(4).transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.red;
        }

        LoadDirection();
    }
    
    public void LoadDirection(){
        int firstSpriteID = 79;
        if (this.direction == LiftHelper.Direction.LEFT){
            this.transform.GetChild(3).GetComponent<SpriteRenderer>().flipX = false;
            this.middleSpriteID = 80;
            this.endSpriteID = 81;
            if(this.fast == 1){
                this.middleSpriteID = 86;
                this.endSpriteID = 87;
                firstSpriteID = 85;
            }

            if(this.onOffState == SpeedMoveBlock.OnOffState.Red){
                if(fast == 0){
                    firstSpriteID = 151;
                    this.middleSpriteID = 152;
                    this.endSpriteID = 153;
                }else{
                    firstSpriteID = 157;
                    this.middleSpriteID = 158;
                    this.endSpriteID = 159;
                }
            }else if(this.onOffState == SpeedMoveBlock.OnOffState.Blue){
                if(fast == 0){
                    firstSpriteID = 115;
                    this.middleSpriteID = 116;
                    this.endSpriteID = 117;
                }else{
                    firstSpriteID = 121;
                    this.middleSpriteID = 122;
                    this.endSpriteID = 123;
                }
            }
        }else{
            this.transform.GetChild(3).GetComponent<SpriteRenderer>().flipX = true;
            this.middleSpriteID = 83;
            this.endSpriteID = 84;
            firstSpriteID = 82;
            if (this.fast == 1){
                this.middleSpriteID = 89;
                this.endSpriteID = 90;
                firstSpriteID = 88;
            }

           if(this.onOffState == SpeedMoveBlock.OnOffState.Red){
                if(fast == 0){
                    firstSpriteID = 154;
                    this.middleSpriteID = 155;
                    this.endSpriteID = 156;
                }else{
                    firstSpriteID = 160;
                    this.middleSpriteID = 161;
                    this.endSpriteID = 162;
                }
            }else if(this.onOffState == SpeedMoveBlock.OnOffState.Blue){
                if(fast == 0){
                    firstSpriteID = 118;
                    this.middleSpriteID = 119;
                    this.endSpriteID = 120;
                }else{
                    firstSpriteID = 124;
                    this.middleSpriteID = 125;
                    this.endSpriteID = 126;
                }
            }
        }

        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(firstSpriteID, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, firstSpriteID, this.GetComponent<SpriteRenderer>()));
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
