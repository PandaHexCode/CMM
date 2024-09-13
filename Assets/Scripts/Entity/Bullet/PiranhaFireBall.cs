using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiranhaFireBall : TileAnimator{

    public LayerMask layerMask;
    public float speed;
    public bool isFriendly;

    public static AudioSource currentAudioSource = null;

    private void Awake(){
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);
    }

    private void Update(){
        transform.Translate(speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (IsInLayerMask(collision.gameObject, layerMask)){
            if (currentAudioSource == null)
                currentAudioSource = SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
            Explode();
        }else if (collision.gameObject.layer == 9 && !this.isFriendly){
            collision.gameObject.GetComponent<PlayerController>().Damage();
        }else if (collision.gameObject.layer == 11)
            Explode();
    }

    public bool IsInLayerMask(GameObject obj, LayerMask layerMask){
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

    public void Explode(){
        GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff.transform.position = transform.position;
        Destroy(gameObject);
    }
}
