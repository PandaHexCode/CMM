using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooRing : MonoBehaviour{

    private Transform _transform;

    private void OnEnable(){
        if (this.transform.GetChild(0).childCount > 0)
            return;

        foreach(Transform child in this.transform){
            child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(26, TileManager.TilesetType.EnemyTileset);
        }

        if (GameManager.instance.sceneManager.isAreaDark[GameManager.instance.sceneManager.currentArea]){
            foreach(Transform child in this.transform){
                GameObject mask = Instantiate(GameManager.instance.sceneManager.entityDarkMask);
                mask.transform.SetParent(child.transform);

                mask.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                mask.transform.localPosition = new Vector3(0, 0, 0);
            }
        }
    }

    private void Awake(){
        this._transform = this.transform;
    }

    private void Update(){
        this._transform.Rotate(0, 0, -55 * Time.deltaTime);
        foreach (Transform child in this._transform)
            child.transform.Rotate(0, 0, 55 * Time.deltaTime);
    }

}
