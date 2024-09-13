using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePiranha : Entity{

    private Transform player;
    private Transform _transform;

    public static AudioSource currentAudioSource = null;

    private List<GameObject> fireballs = new List<GameObject>();

    private void OnEnable(){
        OnEnableTileAnimator();
        StartCoroutine(FireBallIE());

        _transform = this.transform;
        player = GameManager.instance.sceneManager.players[0].transform;
    }

    public IEnumerator FireBallIE(){
        yield return new WaitForSeconds(3);
        if (this.isSpawned){
            SpawnFireBall(false);
        }
        StartCoroutine(FireBallIE());
    }

    public void SpawnFireBall(bool friendlyFireBall){
        if (friendlyFireBall){
            List<GameObject> removeTo = new List<GameObject>();
            foreach(GameObject gm in fireballs){
                if (gm == null)
                    removeTo.Add(gm);
            }
            foreach(GameObject gm in removeTo){
                this.fireballs.Remove(gm);
            }
            if (this.fireballs.Count > 5)
                return;
        }

        if (currentAudioSource == null)
            currentAudioSource = SoundManager.PlayAudioClipIfPlayerIsInNear(SoundManager.currentSoundEffects.throwFireBall, this._transform.position);
        GameObject clon = Instantiate(GameManager.instance.sceneManager.firePiranhaFireBall);
        clon.transform.position = this.transform.position + new Vector3(0, 0.2f, 0);
        float numb = 210;
        if (sp.flipX)
            numb = -numb;

        if (currentAnimation == 0)
            clon.transform.rotation = Quaternion.Euler(0, 0, -numb);
        else
            clon.transform.rotation = Quaternion.Euler(0, 0, numb);

        if (sp.flipX)
            clon.GetComponent<PiranhaFireBall>().speed = -clon.GetComponent<PiranhaFireBall>().speed;
        clon.GetComponent<PiranhaFireBall>().isFriendly = friendlyFireBall;
        if (friendlyFireBall)
            this.fireballs.Add(clon);
    }

    private void Update(){
        if (this.isCaptured)
            return;

        if (_transform.position.y + 3 < player.transform.position.y && currentAnimation != 0){
            StartAnimationClip(animationClips[0]);
        }else if (_transform.position.y + 3 > player.transform.position.y && currentAnimation != 1)
            StartAnimationClip(animationClips[1]);

        if (_transform.position.x + 1 < player.transform.position.x)
            sp.flipX = true;
        else if (_transform.position.x + 1 > player.transform.position.x)
            sp.flipX = false;
    }

    public override void OnTriggerPlayer(PlayerController p){
        if ((int)p.transform.position.y == (int)this.transform.position.y && p.GetOnGround())
            return;

        if (p.isInSpin){
            StartCoroutine(ReactivateDamage());
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spin);
            p.Jump(-1, true);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
            eff.transform.position = p.transform.position;
            return;
        }
    }

    private IEnumerator ReactivateDamage(){
        this.damageOnCollision = false;
        yield return new WaitForSeconds(0.05f);
        this.damageOnCollision = true;
    }

}
