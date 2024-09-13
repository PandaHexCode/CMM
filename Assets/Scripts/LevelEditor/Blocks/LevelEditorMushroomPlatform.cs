using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorMushroomPlatform : MonoBehaviour{

    public int lengthX = 0;
    public int lengthY = 0;

    public int type = 0;

    public void LoadLength(){
        int XLeft = 35;
        if (this.type == 1)
            XLeft = 51;
        else if (this.type == 2)
            XLeft = 67;

        Transform parent = this.transform.GetChild(0);
        GameObject clonRef1 = this.transform.GetChild(1).gameObject;

        clonRef1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(XLeft + 1, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, XLeft + 1, clonRef1.GetComponent<SpriteRenderer>()));

        foreach (Transform child in parent){
            Destroy(child.gameObject);
        }

        for (int i = 1; i < (this.lengthX + 1); i++){
            GameObject clon = Instantiate(clonRef1, parent);
            clon.transform.position = clonRef1.transform.position + new Vector3(i, 0, 0);
            GameObject clon2 = Instantiate(clonRef1, parent);
            clon2.transform.position = clonRef1.transform.position + new Vector3(-i, 0, 0);

            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(XLeft + 1, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, XLeft + 1, clon.GetComponent<SpriteRenderer>()));
            clon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(XLeft + 1, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, XLeft + 1, clon2.GetComponent<SpriteRenderer>()));
        }

        this.transform.GetChild(2).gameObject.transform.localPosition = new Vector3(-this.lengthX, 0, 0);
        this.transform.GetChild(3).gameObject.transform.localPosition = new Vector3(2 + this.lengthX, 0, 0);
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(XLeft, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, XLeft, this.transform.GetChild(2).GetComponent<SpriteRenderer>()));
        this.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(XLeft + 2, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, XLeft + 2, this.transform.GetChild(3).GetComponent<SpriteRenderer>()));

        for (int i = 1; i < (this.lengthY + 1); i++){
            int id = 70;
            if (i == 1)
                id = 54;
            GameObject clon = Instantiate(clonRef1, parent);
            clon.transform.position = clonRef1.transform.position + new Vector3(0, -i, 0);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(id, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, id, clon.GetComponent<SpriteRenderer>()));
        }

        this.transform.GetChild(5).localPosition = new Vector3(2 + this.lengthX, 0, 0);
        this.transform.GetChild(6).localPosition = new Vector3(1, -this.lengthY, 0);
    }

    public void AddLengthX(){
        this.lengthX++;
        LoadLength();
    }

    public void RemoveLengthX(){
        if (this.lengthX == 0)
            return;
        this.lengthX--;
        LoadLength();
    }

    public void AddLengthY(){
        this.lengthY++;
        LoadLength();
    }

    public void RemoveLengthY(){
        if (this.lengthY == 0)
            return;
        this.lengthY--;
        LoadLength();
    }

    public void ChangeType(){
        if (this.type == 0)
            this.type = 1;
        else if (this.type == 1)
            this.type = 2;
        else if (this.type == 2)
            this.type = 0;

        LoadLength();
    }

    public void StartMovingLengthX(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        StartCoroutine(MovingLengthIEX(button));
    }

    private IEnumerator MovingLengthIEX(GameObject button){
        int orgY = 0;
        orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x > lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.lengthX++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x < lastY && this.lengthX != 0){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.lengthX--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }

        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }

     public void StartMovingLengthY(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        StartCoroutine(MovingLengthIEY(button));
    }

   private IEnumerator MovingLengthIEY(GameObject button){
        int orgY = 0;
        orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.lengthY++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY && this.lengthY != 0){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.lengthY--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }

        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }
}
