using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBullet : MonoBehaviour{

    public float speed = 4;

    private Transform _transform;
    private LayerMask layerMask;

    private void Awake(){
        this._transform = this.transform;
        this.layerMask = GameManager.instance.sceneManager.fireBallPrefarb.GetComponent<FireBall>().layerMask;
    }

    private void Update(){
        this._transform.Translate(this.speed * Time.deltaTime, 0, 0);

        if (this.speed > 0 && this.speed < 15)
            this.speed = this.speed + 5 * Time.deltaTime;
        else if (this.speed < 0 && this.speed > -15)
            this.speed = this.speed - 5 * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (GameManager.IsInLayerMask(collision.gameObject, layerMask) && !collision.isTrigger && !collision.gameObject.tag.Equals("GrabNoWand"))
            Explode();
        else if(GameManager.IsInLayerMask(collision.gameObject, GameManager.instance.entityMask)){
            if (collision.gameObject.GetComponent<Entity>() != null){
                if (collision.gameObject.GetComponent<Thwomp>() != null && collision.isTrigger)
                    return;

                if (collision.gameObject.GetComponent<Bowser>() != null)
                    collision.gameObject.GetComponent<Bowser>().DamageBowser(3);
                else if (collision.gameObject.GetComponent<BombEnemy>() != null)
                    collision.gameObject.GetComponent<BombEnemy>().CheckBomb();
                else
                    collision.gameObject.GetComponent<Entity>().StartCoroutine(collision.gameObject.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
            }else
                return;

            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this._transform.position;
        }
    }

    public void Explode(){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitGround);
        GameObject eff = Instantiate(GameManager.instance.sceneManager.explodeEffect2);
        eff.transform.position = this._transform.position;
        Destroy(this.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11)
            Destroy(this.gameObject);       
    }

}
