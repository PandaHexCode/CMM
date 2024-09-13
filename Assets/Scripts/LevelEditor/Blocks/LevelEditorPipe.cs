using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;
using UMM.BlockData;
using System;

public class LevelEditorPipe : MonoBehaviour{

    public int pipeLength = 1;
    public bool isSmallPipe = false;
    public BlockID contentBlock = BlockID.ERASER;

    public bool isMainPipe = false;
    public LevelEditorPipe connetedPipe = null;
    public BlockField myBlockField = null;

    [System.NonSerialized]public int extraNumber = 0;

    public void LoadLength(BlockField blockField = null){
        BlockField targetBlockField2 = this.myBlockField;

        if(targetBlockField2.blockFieldNumber != 0){ 
            for (int i = 0; i <= this.pipeLength; i++){
                switch (this.transform.eulerAngles.z){
                    case 0:
                        if (i == 0)
                            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2));
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1), this.gameObject, UMM.BlockData.BlockID.PIPE);
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2);
                        break;
                    case -90:
                    case 270:
                        if (i == 0)
                            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber - 2);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2), this.gameObject, UMM.BlockData.BlockID.PIPE);
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber - 1);
                        break;
                    case 180:
                        if (i == 0)
                            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField2));
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1), this.gameObject, UMM.BlockData.BlockID.PIPE);
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField2);
                        break;
                    case 90:
                        if (i == 0)
                            targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 2);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE);
                        LevelEditorManager.instance.blockFieldManager.RemoveNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2), this.gameObject, UMM.BlockData.BlockID.PIPE);
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1);
                        break;
                }
            }
        }

        if (blockField != null)
            this.myBlockField = blockField;

        Vector3 ogRot = this.transform.eulerAngles;
        this.transform.eulerAngles = Vector3.zero;
        int leftID = 30;
        int rightID = 31;
        if (this.isSmallPipe)
            leftID = 164;
        GameObject clonReference = this.transform.GetChild(0).gameObject;
        Transform parent = this.transform.GetChild(1);

        if (!this.isSmallPipe){
            clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(15, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, 15, clonReference.GetComponent<SpriteRenderer>()));
        }

        foreach (Transform child in parent){
            if(child != clonReference.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.pipeLength; i++){
            GameObject leftClon = Instantiate(clonReference, parent);
            leftClon.GetComponent<SpriteRenderer>().enabled = true;
         leftClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(leftID, TileManager.TilesetType.MainTileset);
            leftClon.transform.position = clonReference.transform.position + new Vector3(-1, -i);
            if (!this.isSmallPipe){
                GameObject rightClon = Instantiate(clonReference, parent);
                rightClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(rightID, TileManager.TilesetType.MainTileset);
                rightClon.transform.position = clonReference.transform.position + new Vector3(0, -i);
                TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, rightID, rightClon.GetComponent<SpriteRenderer>()));
                rightClon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }

            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, leftID, leftClon.GetComponent<SpriteRenderer>()));
           
            leftClon.GetComponent<SpriteRenderer>().sortingOrder = 1;  
        }

        this.transform.eulerAngles = ogRot;
        this.transform.GetChild(5).localPosition = new Vector3(this.transform.GetChild(5).localPosition.x, -this.pipeLength - 0.3f, 0);

        return;
        targetBlockField2 = this.myBlockField;
        for (int i = 0; i < (this.pipeLength - 1); i++){
            switch (this.transform.eulerAngles.z){
                case 0:
                    if (i == 0)
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2));
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1), this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2);
                    break;
                case -90:
                case 270:
                    if (i == 0)
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber - 2);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2), this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber - 1);
                    break;
                case 180:
                    if (i == 0)
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField2));
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1), this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField2);
                    break;
                case 90:
                    if (i == 0)
                        targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 2);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(targetBlockField2, this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    LevelEditorManager.instance.blockFieldManager.AddNotOfficalBlockData(LevelEditorManager.instance.blockFieldManager.GetBlockFieldUnderBlockField(targetBlockField2), this.gameObject, UMM.BlockData.BlockID.PIPE, this.myBlockField.blockFieldNumber);
                    targetBlockField2 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField2.blockFieldNumber + 1);
                    break;
            }
        }

    }

    public void AddLength(){
        this.pipeLength++;
        LoadLength();
    }

    public void RemoveLength(){
        if (this.pipeLength == 1)
            return;
        this.pipeLength--;
        LoadLength();
    }

    public void SetContentBlock(BlockID id, GameObject from = null){
        this.contentBlock = id;
        if(id == BlockID.ERASER)
            this.transform.GetChild(2).gameObject.SetActive(false);
        else
            this.transform.GetChild(2).gameObject.SetActive(true);

        if (id == BlockID.MYSTERY_MUSHROM && from != null)
            this.extraNumber = from.GetComponent<LevelEditorMysteryMushrom>().costumeNumber;
    }

    public void RotatePipe(){
        switch (this.transform.eulerAngles.z){
            case 0:
                this.transform.eulerAngles = new Vector3(0, 0, -90);
                if(!this.isSmallPipe)
                    this.transform.position = this.transform.position + new Vector3(1, 0, 0);
                break;
            case -90:
            case 270:
                this.transform.eulerAngles = new Vector3(0, 0, 180);
                if (!this.isSmallPipe)
                    this.transform.position = this.transform.position + new Vector3(0, -1, 0);
                break;
            case 180:
                this.transform.eulerAngles = new Vector3(0, 0, 90);
                if (!this.isSmallPipe)
                    this.transform.position = this.transform.position + new Vector3(-1, 0, 0);
                break;
            case 90:
                this.transform.eulerAngles = new Vector3(0, 0, 0);
                if (!this.isSmallPipe)
                    this.transform.position = this.transform.position + new Vector3(0, 1, 0);
                break;
        }
    }

    public static Vector3 GetRotateOffset(GameObject pipe){
        switch (pipe.transform.eulerAngles.z){
            case 0:
                return new Vector3(1, 0, 0);
            case -90:
            case 270:
                return new Vector3(0, -1, 0);
            case 180:
                return new Vector3(-1, 0, 0);
                break;
            case 90:
                return new Vector3(0, 1, 0);
                break;
        }

        return Vector3.zero;
    }

    public void StartRegisterAreaSwitchPipeButton(){
        LevelEditorManager.instance.StartRegisterPipeArea(this.gameObject, this.isSmallPipe);
    }

    public void ConnectPipe(bool isMainPipe, LevelEditorPipe otherPipe){
        this.isMainPipe = isMainPipe;
        this.connetedPipe = otherPipe;
        this.transform.GetChild(4).gameObject.SetActive(false);
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
            if (this.transform.eulerAngles.z == 0){
                if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                    this.pipeLength++;
                    LoadLength();   
                }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY && this.pipeLength != 1){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                    this.pipeLength--;
                    LoadLength();
                }
            }else if (this.transform.eulerAngles.z == -90 | this.transform.eulerAngles.z == 270){
                if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x < lastY){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                    this.pipeLength++;
                    LoadLength();
                }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x > lastY && this.pipeLength != 1){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                    this.pipeLength--;
                    LoadLength();
                }
            }else if (this.transform.eulerAngles.z == 90){
                if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x > lastY){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                    this.pipeLength++;
                    LoadLength();
                }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x < lastY && this.pipeLength != 1){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                    this.pipeLength--;
                    LoadLength();
                }
            }else if (this.transform.eulerAngles.z == 180){
                if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                    this.pipeLength++;
                    LoadLength();
                }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY && this.pipeLength != 1){
                    lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                    this.pipeLength--;
                    LoadLength();
                }
            }
           
            yield return new WaitForSeconds(0);
        }
        
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnDestroy(){
        if (this.connetedPipe != null){
            try
            {
                this.connetedPipe.transform.GetChild(7).gameObject.SetActive(true);
            }catch(Exception e)
            {

            }
            this.connetedPipe.isMainPipe = false;
        }
    }
}
