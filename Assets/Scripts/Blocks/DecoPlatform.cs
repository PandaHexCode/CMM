using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecoPlatform : MonoBehaviour{

    public int lengthX = 1;
    public int lengthY = 1;

    private int middleSpriteIDX = 56;
    private int endSpriteIDX = 57;
    private int beginIDY = 71;
    private int middleSpriteIDY = 72;
    private int endSpriteIDY = 73;
    private int lastBeginIDY = 103;
    private int lastMiddleSpriteIDY = 104;
    private int lastEndSpriteIDY = 105;

    public static int count = 0;

    public void LoadLength(int area){
        List<GameObject> yObjects = new List<GameObject>();
        Transform parent = this.transform.GetChild(5);

        GameObject clonReference1 = this.transform.GetChild(0).gameObject;
        clonReference1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.middleSpriteIDX, TileManager.TilesetType.MainTileset);

        GameObject clonReference2 = this.transform.GetChild(3).gameObject;
        clonReference2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
        yObjects.Add(clonReference2);

        GameObject endRef1 = this.transform.GetChild(1).gameObject;
        endRef1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.endSpriteIDX, TileManager.TilesetType.MainTileset);
        endRef1.transform.localPosition = new Vector3(2, 0, 0);

        GameObject endRef2 = this.transform.GetChild(4).gameObject;
        endRef2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.endSpriteIDY, TileManager.TilesetType.MainTileset);
        endRef2.transform.localPosition = new Vector3(2, -1, 0);

        GameObject beginRef = this.transform.GetChild(2).gameObject;
        beginRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.beginIDY, TileManager.TilesetType.MainTileset);
        for (int i = 1; i <= this.lengthX; i++){
            GameObject clon = Instantiate(clonReference1, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.middleSpriteIDX, TileManager.TilesetType.MainTileset);
            clon.transform.position = clonReference1.transform.position + new Vector3(i, 0);
            endRef1.transform.position = clon.transform.position + new Vector3(1, 0);

            GameObject clon2 = Instantiate(clonReference2, parent);
            clon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
            clon2.transform.position = clonReference2.transform.position + new Vector3(i, 0);
            endRef2.transform.position = clon2.transform.position + new Vector3(1, 0);
            yObjects.Add(clon2);
        }

        for (int f = 1; f < this.lengthY; f++){
            GameObject beginClon = Instantiate(beginRef, parent);
            beginClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.beginIDY, TileManager.TilesetType.MainTileset);
            beginClon.transform.position = beginRef.transform.position + new Vector3(0, -f);

            GameObject endClon = Instantiate(endRef2, parent);
            endClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.endSpriteIDY, TileManager.TilesetType.MainTileset);
            endClon.transform.position = endClon.transform.position + new Vector3(0, -f);

            foreach(GameObject gm in yObjects){
                GameObject clon = Instantiate(gm, parent);
                clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.middleSpriteIDY, TileManager.TilesetType.MainTileset);
                clon.transform.position = clon.transform.position + new Vector3(0, -f);
            }
        }

        GameObject lastClon = Instantiate(beginRef, parent);
        lastClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.lastBeginIDY, TileManager.TilesetType.MainTileset);
        lastClon.transform.position = beginRef.transform.position + new Vector3(0, -this.lengthY);

        GameObject lastClon2 = Instantiate(endRef1, parent);
        lastClon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.lastEndSpriteIDY, TileManager.TilesetType.MainTileset);
        lastClon2.transform.position = endRef2.transform.position + new Vector3(0, -this.lengthY);

        foreach (GameObject gm in yObjects){
            GameObject clon = Instantiate(gm, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, this.lastMiddleSpriteIDY, TileManager.TilesetType.MainTileset); 
            clon.transform.position = clon.transform.position + new Vector3(0, -this.lengthY);
        }

        BoxCollider2D bx = GetComponent<BoxCollider2D>();
        bx.size = new Vector2(3 + this.lengthX, 1);
        bx.offset = new Vector2(1 + (0.5f * this.lengthX), 0);

        GetComponent<SpriteRenderer>().sortingOrder = -6 - DecoPlatform.count;

        foreach (Transform sp in this.transform){
            if(sp.GetComponent<SpriteRenderer>() != null)
                sp.GetComponent<SpriteRenderer>().sortingOrder = -6 - DecoPlatform.count;
        }

        foreach(Transform sp in parent){
            sp.GetComponent<SpriteRenderer>().sortingOrder = -6 - DecoPlatform.count;
        }

        DecoPlatform.count++;

        Destroy(this);
    }


    public void LoadType(int beginIDX, int area){
        this.beginIDY = 71;
        this.lastBeginIDY = 103;

        if (beginIDX == 58){
            this.beginIDY = 74;
            this.lastBeginIDY = 106;
        }else if (beginIDX == 61){
            this.beginIDY = 77;
            this.lastBeginIDY = 109;
        }

        this.middleSpriteIDX = beginIDX + 1;
        this.endSpriteIDX = this.middleSpriteIDX + 1;
        this.middleSpriteIDY = this.beginIDY + 1;
        this.endSpriteIDY = this.middleSpriteIDY + 1;
        this.lastMiddleSpriteIDY = this.lastBeginIDY + 1;
        this.lastEndSpriteIDY = this.lastMiddleSpriteIDY + 1;


        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, beginIDX, TileManager.TilesetType.MainTileset);
    }

}
