using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenTileAnimator : MonoBehaviour{
    
    public int[] sprites;
    public float delay = 0.15f;
    private SpriteRenderer[] childrens;
    private TileManager tileManager;

    private void OnEnable(){
        List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        foreach (Transform child in this.transform)
            spriteRenderers.Add(child.GetComponent<SpriteRenderer>());

        this.childrens = spriteRenderers.ToArray();
        this.tileManager = TileManager.instance;
        StopAllCoroutines();
        StartCoroutine(AnCor());
    }

    private IEnumerator AnCor(){
        while (true){
            foreach (int i in this.sprites){
                foreach (SpriteRenderer sp in this.childrens)
                    sp.sprite = this.tileManager.GetSpriteFromTileset(i, TileManager.TilesetType.ObjectsTileset);
                yield return new WaitForSeconds(this.delay);
            }
        }
    }

    private void OnDisable(){
        StopAllCoroutines();
    }

}
