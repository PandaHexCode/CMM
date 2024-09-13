using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : MonoBehaviour{

    private void OnEnable(){
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(212, TileManager.TilesetType.ObjectsTileset);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            this.StopAllCoroutines();
            StartCoroutine(WarnRotatingIE());
        }
    }

    private IEnumerator WarnRotatingIE(){
        Transform trans = this.transform;

        while (true){
            float targetZ = trans.eulerAngles.z + 4;
            while (trans.eulerAngles.z < targetZ){
                Debug.Log(1);
                trans.eulerAngles = trans.eulerAngles + new Vector3(0, 0, 8 * Time.deltaTime);
                yield return new WaitForSeconds(0);
            }

            for (int i = 0; i <= 1; i++){
                float targetZ2 = trans.eulerAngles.z - 4;
                while (trans.eulerAngles.z > targetZ2){
                    Debug.Log(2);
                    trans.eulerAngles = trans.eulerAngles - new Vector3(0, 0, 8 * Time.deltaTime);
                    yield return new WaitForSeconds(0);
                }
            }
            
            float targetZ3 = trans.eulerAngles.z + 4;
            while (trans.eulerAngles.z < targetZ3){
                Debug.Log(3);
                trans.eulerAngles = trans.eulerAngles + new Vector3(0, 0, 8 * Time.deltaTime);
                yield return new WaitForSeconds(0);
            }
            trans.eulerAngles = Vector3.zero;
        }
    }

}
