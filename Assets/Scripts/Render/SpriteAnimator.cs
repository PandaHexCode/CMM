using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour{

    public Sprite[] sprites;
    public float delay = 0.15f;
    private SpriteRenderer sp;

    private void OnEnable(){
        this.sp = GetComponent<SpriteRenderer>();
        StopAllCoroutines();
        if(this.sprites.Length > 1)
            StartCoroutine(AnCor());
    }

    private IEnumerator AnCor(){
        while (true){
            foreach (Sprite sprite in this.sprites){
                sp.sprite = sprite;
                yield return new WaitForSeconds(this.delay);
            }
        }
    }

}
