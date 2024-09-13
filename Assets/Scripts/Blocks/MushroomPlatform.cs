using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomPlatform : MonoBehaviour{

    public int lengthX = 0;
    public int lengthY = 0;

    public int type = 0;

    public void LoadLength(int area){
        int XLeft = 35;
        if (this.type == 1)
            XLeft = 51;
        else if (this.type == 2)
            XLeft = 67;

        Transform parent = this.transform.GetChild(0);
        GameObject clonRef1 = this.transform.GetChild(1).gameObject;

        clonRef1.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, XLeft + 1, TileManager.TilesetType.MainTileset);

        for (int i = 1; i < (this.lengthX + 1); i++){
            GameObject clon = Instantiate(clonRef1, parent);
            clon.transform.position = clonRef1.transform.position + new Vector3(i, 0, 0);
            GameObject clon2 = Instantiate(clonRef1, parent);
            clon2.transform.position = clonRef1.transform.position + new Vector3(-i, 0, 0);

            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, XLeft + 1, TileManager.TilesetType.MainTileset);
            clon2.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, XLeft + 1, TileManager.TilesetType.MainTileset);
        }

        this.transform.GetChild(2).gameObject.transform.localPosition = new Vector3(-this.lengthX, 0, 0);
        this.transform.GetChild(3).gameObject.transform.localPosition = new Vector3(2 + this.lengthX, 0, 0);
        this.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, XLeft, TileManager.TilesetType.MainTileset);
        this.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, XLeft + 2, TileManager.TilesetType.MainTileset);

        for (int i = 1; i < (this.lengthY + 1); i++){
            int id = 70;
            if (i == 1)
                id = 54;
            GameObject clon = Instantiate(clonRef1, parent);
            clon.transform.position = clonRef1.transform.position + new Vector3(0, -i, 0);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, id, TileManager.TilesetType.MainTileset);
        }

        this.GetComponent<BoxCollider2D>().size = new Vector2(3 + (this.lengthX * 2), 1);

        Destroy(this);
    }

}
