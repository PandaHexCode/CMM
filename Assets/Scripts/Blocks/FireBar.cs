using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBar : MonoBehaviour{

    public int lengthY = 2;
    private Transform parent;

    [System.NonSerialized]public float speed = 90;

    public void LoadLength(int area){
        this.parent = this.transform.GetChild(0).transform;

        for (int i = 0; i < this.lengthY; i++){
            GameObject clon = Instantiate(this.transform.GetChild(1).gameObject, parent);
            clon.transform.position = this.transform.GetChild(1).position + new Vector3(0, (i * 0.45f), 0);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(175, TileManager.TilesetType.ObjectsTileset);
        }

        if (GameManager.instance.sceneManager.isAreaDark[area]){
            GameObject mask = Instantiate(GameManager.instance.sceneManager.entityDarkMask, parent);
            mask.transform.localScale = new Vector2(0.5f, 0.5f * (this.lengthY - 1) - 0.5f);
            mask.transform.localPosition = new Vector2(0, 0.2f * this.lengthY + 0.2f);
        }

        this.parent.GetComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.5f * (this.lengthY - 1) - 0.5f);
        this.parent.GetComponent<BoxCollider2D>().offset = new Vector2(0, 0.2f * this.lengthY + 0.2f);
        Destroy(this.transform.GetChild(1).gameObject);
    }

    private void Update(){
        this.parent.Rotate(0, 0, this.speed * Time.deltaTime);
    }
}
