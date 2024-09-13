using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenAnimator : MonoBehaviour{

    public Sprite[] sprites;
    public float delay = 0.15f;
    private SpriteRenderer[] childrens;

    private void OnEnable(){
        List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
        foreach (Transform child in this.transform)
            spriteRenderers.Add(child.GetComponent<SpriteRenderer>());

        this.childrens = spriteRenderers.ToArray();
        StopAllCoroutines();
        StartCoroutine(AnCor());
    }

    private IEnumerator AnCor(){
        while (true){
            foreach (Sprite sprite in this.sprites){
                foreach (SpriteRenderer sp in this.childrens)
                    sp.sprite = sprite;
                yield return new WaitForSeconds(this.delay);
            }
        }
    }

}
