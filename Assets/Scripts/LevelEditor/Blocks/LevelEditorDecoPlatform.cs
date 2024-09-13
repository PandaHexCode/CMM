using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorDecoPlatform : MonoBehaviour{

    public int lengthX = 1;
    public int lengthY = 1;

    [System.NonSerialized]public int middleSpriteIDX = 56;
    private int endSpriteIDX = 57;
    private int beginIDY = 71;
    private int middleSpriteIDY = 72;
    private int endSpriteIDY = 73;
    private int lastBeginIDY = 103;
    private int lastMiddleSpriteIDY = 104;
    private int lastEndSpriteIDY = 105;

    public void LoadLength(){
        List<GameObject> yObjects = new List<GameObject>();
        Transform parent = this.transform.GetChild(5);

        GameObject clonReference1 = this.transform.GetChild(0).gameObject;
        clonReference1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteIDX, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.middleSpriteIDX, clonReference1.GetComponent<SpriteRenderer>()));
        
        GameObject clonReference2 = this.transform.GetChild(3).gameObject;
        clonReference2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.middleSpriteIDY, clonReference2.GetComponent<SpriteRenderer>()));
        yObjects.Add(clonReference2);

        GameObject endRef1 = this.transform.GetChild(1).gameObject;
        endRef1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.endSpriteIDX, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.endSpriteIDX, endRef1.GetComponent<SpriteRenderer>()));
        endRef1.transform.localPosition = new Vector3(2, 0, 0);

        GameObject endRef2 = this.transform.GetChild(4).gameObject;
        endRef2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.endSpriteIDY, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.endSpriteIDY, endRef2.GetComponent<SpriteRenderer>()));
        endRef2.transform.localPosition = new Vector3(2, -1, 0);

        GameObject beginRef = this.transform.GetChild(2).gameObject;
        beginRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.beginIDY, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.beginIDY, beginRef.GetComponent<SpriteRenderer>()));

        foreach (Transform child in parent){
            if (child != clonReference1.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.lengthX; i++){
            GameObject clon = Instantiate(clonReference1, parent);
           // clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteIDX, TileManager.TilesetType.MainTileset);
            clon.transform.position = clonReference1.transform.position + new Vector3(i, 0);
            endRef1.transform.position = clon.transform.position + new Vector3(1, 0);

            GameObject clon2 = Instantiate(clonReference2, parent);
          //  clon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
            clon2.transform.position = clonReference2.transform.position + new Vector3(i, 0);
            endRef2.transform.position = clon2.transform.position + new Vector3(1, 0);
            yObjects.Add(clon2);

            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.middleSpriteIDX, clon.GetComponent<SpriteRenderer>()));
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.middleSpriteIDY, clon2.GetComponent<SpriteRenderer>()));
        }

        for (int f = 1; f < this.lengthY; f++){
            GameObject beginClon = Instantiate(beginRef, parent);
         //   beginClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.beginIDY, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.beginIDY, beginClon.GetComponent<SpriteRenderer>()));
            beginClon.transform.position = beginRef.transform.position + new Vector3(0, -f);

            GameObject endClon = Instantiate(endRef2, parent);
         //   endClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.endSpriteIDY, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.endSpriteIDY, endClon.GetComponent<SpriteRenderer>()));
            endClon.transform.position = endClon.transform.position + new Vector3(0, -f);

            foreach(GameObject gm in yObjects){
                GameObject clon = Instantiate(gm, parent);
              //  clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
                TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.middleSpriteIDY, clon.GetComponent<SpriteRenderer>()));
                clon.transform.position = clon.transform.position + new Vector3(0, -f);
            }
        }

        GameObject lastClon = Instantiate(beginRef, parent);
        lastClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.lastBeginIDY, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.lastBeginIDY, lastClon.GetComponent<SpriteRenderer>()));
        lastClon.transform.position = beginRef.transform.position + new Vector3(0, -this.lengthY);

        GameObject lastClon2 = Instantiate(endRef1, parent);
        lastClon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.lastEndSpriteIDY, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.lastEndSpriteIDY, lastClon2.GetComponent<SpriteRenderer>()));
        lastClon2.transform.position = endRef2.transform.position + new Vector3(0, -this.lengthY);

        foreach (GameObject gm in yObjects){
            GameObject clon = Instantiate(gm, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(this.lastMiddleSpriteIDY, TileManager.TilesetType.MainTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, this.lastMiddleSpriteIDY, clon.GetComponent<SpriteRenderer>()));
            clon.transform.position = clon.transform.position + new Vector3(0, -this.lengthY);
        }

        this.transform.GetChild(7).localPosition = new Vector3(2 + this.lengthX, 0, 0);
        this.transform.GetChild(8).localPosition = new Vector3(2 + this.lengthX, -this.lengthY - 1.3f, 0);
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
        if (this.lengthY == 1)
            return;
        this.lengthY--;
        LoadLength();
    }

    public void ChangeType(){
        int beginIDX = 55;
        this.beginIDY = 71;
        this.lastBeginIDY = 103;

        if(this.middleSpriteIDX == 56){
            beginIDX = 58;
            this.beginIDY = 74;
            this.lastBeginIDY = 106;
        }else if(this.middleSpriteIDX == 59){
            beginIDX = 61;
            this.beginIDY = 77;
            this.lastBeginIDY = 109;
        }

        this.middleSpriteIDX = beginIDX + 1;
        this.endSpriteIDX = this.middleSpriteIDX + 1;
        this.middleSpriteIDY = this.beginIDY + 1;
        this.endSpriteIDY = this.middleSpriteIDY + 1;
        this.lastMiddleSpriteIDY = this.lastBeginIDY + 1;
        this.lastEndSpriteIDY = this.lastMiddleSpriteIDY + 1;

        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(beginIDX, TileManager.TilesetType.MainTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.MainTileset, beginIDX, this.GetComponent<SpriteRenderer>()));
        LoadLength();
    }

    public void StartMovingLength(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        button.GetComponent<SpriteRenderer>().sprite = LevelEditorManager.instance.blockWidget1;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        if (button == this.transform.GetChild(8).gameObject){
            this.transform.GetChild(7).GetComponent<SpriteRenderer>().sprite = LevelEditorManager.instance.blockWidget2;
            this.transform.GetChild(7).GetComponent<SpriteRenderer>().enabled = true;
            StartCoroutine(MovingLengthIE(button, true));
        }else{
            this.transform.GetChild(8).GetComponent<SpriteRenderer>().sprite = LevelEditorManager.instance.blockWidget2;
            this.transform.GetChild(8).GetComponent<SpriteRenderer>().enabled = true;
            StartCoroutine(MovingLengthIE(button, false));
        }
    }

    private IEnumerator MovingLengthIE(GameObject button, bool Y = false){
        int orgX = 0;
        int orgY = 0;
        orgX = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
        orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;

        int lastX = orgX;
        int lastY = orgY;

        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x > lastX){
                lastX = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.lengthX++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x < lastX && this.lengthX != 0){
                lastX = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;
                this.lengthX--;
                LoadLength();
            }

            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY && Y){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.lengthY++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY && this.lengthY != 1 && Y){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.lengthY--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }

        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
        this.transform.GetChild(7).GetComponent<SpriteRenderer>().enabled = false;
        this.transform.GetChild(8).GetComponent<SpriteRenderer>().enabled = false;
    }

}
