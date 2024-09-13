using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bully : Entity{

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        p.Jump(-1, true);

        if (p.GetPowerup() == PlayerController.Powerup.Mini)
            return;

        StopAllCoroutines();
        StartCoroutine(SetCanTriggerBack());

        if (!p.input.JUMP){
            p.GetComponent<Rigidbody2D>().velocity += new Vector2(0, 11f);
            p.dontManipulateJump = true;
        }

        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitGround);

        if (p.transform.position.x > this.transform.position.x + 0.1f)
            StartCoroutine(SimulateVelocity(-2f, -12, 3, -7));
        else
            StartCoroutine(SimulateVelocity(2f, 12, 3, 7));
    }

    public override void OnCollOther(Collision2D col){
        if(col.gameObject.layer == 9 && !col.gameObject.GetComponent<PlayerController>().isFreeze){
            StopAllCoroutines();
            this.canTrigger = true;
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = col.gameObject.transform.position;
            if (col.gameObject.transform.position.x > this.transform.position.x){
                col.rigidbody.velocity += new Vector2(15, 0);
                StartCoroutine(SimulateVelocity(-1, -8, 3, -3));
            }else{
                col.rigidbody.velocity += new Vector2(-15, 0);
                StartCoroutine(SimulateVelocity(1, 8, 3, 3));
            }
            col.gameObject.GetComponent<PlayerController>().BreakReturn();
            if (col.gameObject.GetComponent<Rigidbody2D>().velocity.y != 0)
                col.gameObject.GetComponent<PlayerController>().CancelJump();
            col.gameObject.GetComponent<PlayerController>().StartFreezingFromStamp(0.3f, true);
            
        }
    }

    public IEnumerator SimulateVelocity(float power, float maxSpeed, float speedMultipler, float startSpeed){
        this.canMove = false;

        StartAnimationClip(this.animationClips[1]);
        float targetX = this.transform.position.x + power;

        float speed = startSpeed;

        Vector3 direction = Vector3.right;
        this.direction = 1;
        this.sp.flipX = false;
        if (power < 0){
            this.sp.flipX = true;
            this.direction = 0;
            direction = Vector3.left;
        }

        while((power > 0 && this.transform.position.x < targetX) | (power < 0 && this.transform.position.x > targetX)){
            if (power > 0 && speed < maxSpeed)
                speed = speed + speedMultipler * Time.deltaTime;
            else if(power < 0 && speed > maxSpeed)
                speed = speed - speedMultipler * Time.deltaTime;


            if (SceneManager.EntityWallCheckRay(this.transform, direction))
                break;

            this.transform.Translate(speed * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0);
        }

        this.canMove = true;
        StartAnimationClip(this.animationClips[0]);
        StartCoroutine(SceneManager.EntitySpeedNaturalRestorer(this, 1.5f, 1, 0.5f));
    }

    private void OnDestroy(){
        UnlockKey();
    }

}
