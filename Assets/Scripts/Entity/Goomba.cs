using System.Collections;
using UnityEngine;

public class Goomba : Entity{

    private void OnEnable(){
        OnEnableTileAnimator();
        if (TileManager.instance.currentTileset.autoEnableIsWater)
            StartCoroutine(WaterIE());
    }

    private IEnumerator WaterIE(){
        while (!this.isSpawned)
            yield return new WaitForSeconds(0);

        UnStack();
        this.GetComponent<EntityGravity>().enabled = false;
        this.moveSpeed = 0;
        PlayerController[] players = GameManager.instance.sceneManager.players.ToArray();
        PlayerController nearPlayer = null;
        while(nearPlayer == null){
            foreach(PlayerController player in players){
                if ((int)player.transform.position.x > (int)this.transform.position.x - 4 && (int)player.transform.position.x < (int)this.transform.position.x + 4 && (int)player.transform.position.y > (int)this.transform.position.y - 5 && (int)player.transform.position.y < (int)this.transform.position.y + 5){
                    nearPlayer = player;
                }  
            }
            this.transform.Rotate(0, 0, 30 * Time.deltaTime);
            yield return new WaitForSeconds(0);
        }

        while (nearPlayer != null){
            PlayerController p = null;
            foreach (PlayerController player in players){
                if ((int)player.transform.position.x > (int)this.transform.position.x - 4 && (int)player.transform.position.x < (int)this.transform.position.x + 4 && (int)player.transform.position.y > (int)this.transform.position.y - 5 && (int)player.transform.position.y < (int)this.transform.position.y + 5){
                    p = player;
                }
            }
            nearPlayer = p;
            if (nearPlayer == null)
                break;

            Vector3 dir = nearPlayer.transform.position - this.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float z = Quaternion.AngleAxis(angle, Vector3.forward).z - 58;
            if (( z > 0 && this.transform.rotation.z < z))
                this.transform.Rotate(0, 0, -100 * Time.deltaTime);
            else if ((z < 0 && this.transform.rotation.z > z))
                this.transform.Rotate(0, 0, 100 * Time.deltaTime);

            if (Physics2D.Raycast(this.transform.position, dir, 2, this.wandMask))
                nearPlayer = null;
            else
                this.transform.Translate(0, 3 * Time.deltaTime, 0);
            yield return new WaitForSeconds(0);
        }

        StartCoroutine(WaterIE());
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.GetPowerup() == PlayerController.Powerup.Mini){
            p.Jump(-1, true);
            return;
        }

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            UnlockKey();
            Destroy(this.gameObject);
            return;
        }

        UnStack();
        this.moveSpeed = 0;
        this.canMove = false;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
        
        if (TileManager.instance.currentStyleID == TileManager.StyleID.SMW){
            this.gameObject.layer = 24;
            if (p.input.RUN && this.sp.flipY && p.currentGrabedObject == null){
                p.GrabObject(this.gameObject);
                return;
            }

            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = this.transform.position;

            if (this.sp.flipY && this.GetComponent<EntityGravity>().enabled){
                p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[0], 0.3f);
                p.StartIgnoreCollisionForSeconds(this.GetComponents<BoxCollider2D>()[1], 0.3f);
                this.GetComponent<EntityGravity>().enabled = false;
                this.GetComponent<EntityGravity>().StartCoroutine(p.DropGrabedObject(this.gameObject, p.GetComponent<SpriteRenderer>().flipX));
                StopCoroutine("SMWKickIE");
            }

            if(!this.sp.flipY)
                p.Jump(-1, true);

            if (this.GetComponent<EntityGravity>().enabled)
                StartCoroutine(SMWKickIE());
            return;
        }

        p.Jump(-1, true);

        StartAnimationClip(animationClips[1]);
        this.GetComponents<BoxCollider2D>()[0].enabled = false;
        this.GetComponents<BoxCollider2D>()[1].enabled = false;
        UnlockKey();
        StartCoroutine(DestroyIE());
    }

    public IEnumerator DestroyIE(){
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }

    private IEnumerator SMWKickIE(){
        this.sp.flipY = true;

        for (int i = 0; i < 20; i++){
            if (i > 10)
                this.animationClips[0].delay = this.animationClips[0].delay - 0.01f;
            yield return new WaitForSeconds(0.5f);
        }

        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            if (p.currentGrabedObject == this.gameObject){
                p.UngrabCurrentObject(true);
                p.Damage();
            }
        }

        this.GetComponent<EntityGravity>().enabled = true;
        this.GetComponents<BoxCollider2D>()[1].enabled = true;
        this.animationClips[0].delay = 0.16f;
        this.sp.flipY = false;
        this.canMove = true;
        this.moveSpeed = 1.7f;
        this.gameObject.layer = 14;
    }

    public override void OnDamagePlayer(PlayerController p){
        if(!this.sp.flipY)
            base.OnDamagePlayer(p);
        else if (p.input.RUN && p.currentGrabedObject == null){
            p.GrabObject(this.gameObject);
        }else if (this.GetComponent<EntityGravity>().enabled){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = this.transform.position;
            this.GetComponent<EntityGravity>().enabled = false;
            this.GetComponent<EntityGravity>().StartCoroutine(p.DropGrabedObject(this.gameObject, p.GetComponent<SpriteRenderer>().flipX));
            StopCoroutine("SMWKickIE");
        }
    }

}
