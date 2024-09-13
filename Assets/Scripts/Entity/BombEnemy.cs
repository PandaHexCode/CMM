using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombEnemy : Entity{

    private bool isInExplosion = false;

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.input.RUN && this.isInExplosion && TileManager.instance.currentStyleID !=  TileManager.StyleID.SMB1){
            p.GrabObject(this.gameObject);
            return;
        }

        if (this.moveSpeed == 0){
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[0], 0.3f);
            p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[1], 0.3f);
            if (p.isInSpin){
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
                p.Jump(-1, true);
            }
        }else
            p.Jump(-1, true);

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);   

        HitBomb(p);
        CheckBomb();
    }

    public void CheckBomb(){
        if (!this.isInExplosion){
            this.isInExplosion = true;
            StartAnimationClip(this.animationClips[1]);
            this.canMove = false;
            this.moveSpeed = 0;
            this.damageOnCollision = false;
            StartCoroutine(ExplodeIE());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9){
            PlayerController p = collision.gameObject.GetComponent<PlayerController>();
            if (this.damageOnCollision)
                p.Damage();
            else
                HitBomb(p);
        }else if (collision.gameObject.layer == 10 | collision.gameObject.layer == 25){
            if (collision.gameObject.layer == 25)
                collision.gameObject.GetComponent<PiranhaFireBall>().Explode();
            else
                collision.gameObject.GetComponent<FireBall>().Explode();
            CheckBomb();
        }else if (collision.gameObject.layer == 18){
            StartCoroutine(ShootDieAnimation(collision.gameObject));
        }else if (collision.gameObject.layer == 26){
            CheckBomb();
        }
    }

    public override void OnTriggerOther(Collider2D col){
        if (col.gameObject.layer == 26)
            CheckBomb();
    }

    public bool canHit = true;
    public void HitBomb(PlayerController p){
        if (!this.canHit | !this.isInExplosion)
            return;
        this.gameObject.layer = 28;
        this.canHit = false;
        this.canMove = false;
        this.moveSpeed = 0;
        this.GetComponent<EntityGravity>().enabled = false;
        this.GetComponent<SpriteRenderer>().flipX = !p.GetComponent<SpriteRenderer>().flipX;
        this.GetComponent<EntityGravity>().StopAllCoroutines();
        this.GetComponent<EntityGravity>().StartCoroutine(p.DropGrabedObject(this.gameObject, p.GetComponent<SpriteRenderer>().flipX));
    }

    private IEnumerator ExplodeIE(){
        this.canDespawn = false;
        for (int i = 0; i < 7; i++){
            this.sp.color = Color.white;
            yield return new WaitForSeconds(0.3f);
            this.sp.color = Color.yellow;
            yield return new WaitForSeconds(0.3f);
            this.sp.color = Color.red;
            yield return new WaitForSeconds(0.3f);
        }

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.explode);

        Vector3 pos = transform.position;

        GameObject explodeEff = Instantiate(GameManager.instance.sceneManager.explodeEffect);
        explodeEff.transform.position = pos;

        GameObject eff1  = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff1.transform.position = pos + new Vector3(1, 1,0);
        GameObject eff2 = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff2.transform.position = pos + new Vector3(-1, 1, 0);
        GameObject eff3 = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff3.transform.position = pos + new Vector3(1, -1, 0);
        GameObject eff4 = Instantiate(GameManager.instance.sceneManager.destroyEffect);
        eff4.transform.position = pos + new Vector3(-1, -1, 0);

        UnlockKey();
        Destroy(this.gameObject);
    }

}
