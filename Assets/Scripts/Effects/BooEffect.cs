using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooEffect : MonoBehaviour{

    private void Awake(){
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(26, TileManager.TilesetType.EnemyTileset);
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);
        StartCoroutine(EffectIE());
    }

    private IEnumerator EffectIE(){
        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(0.5f);
        while(sp.color.a > 0.1f){
            sp.color = new Color(sp.color.r, sp.color.g, sp.color.b, sp.color.a - 5 * Time.deltaTime);
            yield return new WaitForSeconds(0);
        }
        Destroy(this.gameObject);
    }

    private void Update(){
        this.transform.Translate(-7 * Time.deltaTime, 0, 0);
    }

}
