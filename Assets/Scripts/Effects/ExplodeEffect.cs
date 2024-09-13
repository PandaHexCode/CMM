using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeEffect : TileAnimator{

    public GameObject standartBlockDestroyEffect;
    public bool canDamagePlayer = true;
    public bool canKillEnemies = true;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.isTrigger)
            return;

        if (collision.gameObject.layer == 9 && this.canDamagePlayer)
            collision.gameObject.GetComponent<PlayerController>().Damage();
        else if (collision.gameObject.tag.Equals("DestroyableBlock")){
            GameObject eff = Instantiate(this.standartBlockDestroyEffect);
            eff.transform.position = collision.gameObject.transform.position + new Vector3(4, 2, 0);
            Destroy(collision.gameObject);
        }else if (collision.gameObject.GetComponent<Entity>() != null && this.canKillEnemies){ 
            if(collision.gameObject.GetComponent<Bowser>() != null){
                collision.gameObject.GetComponent<Bowser>().DamageBowser(2);
                return;
            }
            collision.gameObject.GetComponent<Entity>().StartCoroutine(collision.gameObject.GetComponent<Entity>().ShootDieAnimation(this.gameObject));
        }else
            SceneManager.CheckBlockToUseOrDestroy(collision.gameObject, false);
    }

    public override void FinishedAnimationClip(AnimationClip clip){
        Destroy(this.gameObject);
    }

}
