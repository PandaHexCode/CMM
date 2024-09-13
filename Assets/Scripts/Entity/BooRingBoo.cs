using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BooRingBoo : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            if (collision.gameObject.GetComponent<PlayerController>().GetIsStar()){
                GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
                eff.transform.position = this.transform.position;
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.booLaught);
                Destroy(this.gameObject);
            }else
                collision.gameObject.GetComponent<PlayerController>().Damage();
        }
    }

}
