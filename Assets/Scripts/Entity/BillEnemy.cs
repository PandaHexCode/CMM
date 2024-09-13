using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillEnemy : Entity{

    public float speed = -7;
    public bool isBig;

    public BillBlaster spawner;

    private void OnEnable(){
        if (this.isBig){
            switch (this.transform.eulerAngles.z){
                case 270:
                    GetComponents<BoxCollider2D>()[0].offset = new Vector2(0.7604061f, -0.03006744f);
                    GetComponents<BoxCollider2D>()[0].size = new Vector2(2.524434f, 3.984577f);

                    GetComponents<BoxCollider2D>()[1].offset = new Vector2(-1.354629f, -0.07037926f);
                    GetComponents<BoxCollider2D>()[1].size = new Vector2(1.446404f, 4.007286f);
                    break;
                case 180:
                    GetComponents<BoxCollider2D>()[0].offset = new Vector2(0.0295198f, 0.8779914f);
                    GetComponents<BoxCollider2D>()[0].size = new Vector2(3.986207f, 2.167143f);

                    GetComponents<BoxCollider2D>()[1].offset = new Vector2(0.02058792f, -1.191606f);
                    GetComponents<BoxCollider2D>()[1].size = new Vector2(4.196837f, 1.600412f);
                    break;
                case 90:
                    GetComponents<BoxCollider2D>()[0].offset = new Vector2(-0.4044533f, -0.008593559f);
                    GetComponents<BoxCollider2D>()[0].size = new Vector2(3.11826f, 4.027525f);

                    GetComponents<BoxCollider2D>()[1].offset = new Vector2(1.7210562f, -0.05200529f);
                    GetComponents<BoxCollider2D>()[1].size = new Vector2(0.7959023f, 4.109204f);
                    break;
            }
        }
    }

    private void Update(){
        if (this.isCaptured)
            return;

        if (this.isSpawned && this.canMove)
            this.transform.Translate(this.speed * Time.deltaTime, 0, 0);
    }

    public override void OnTriggerPlayer(PlayerController p){
        if (p.transform.position.y < this.transform.position.y){
            p.Damage();
            return;
        }

        p.Jump(-1, true);

        if (p.GetPowerup() == PlayerController.Powerup.Mini)
            return;

        UnlockKey();

        if (p.isInSpin){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.spinKick);
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            Destroy(this.gameObject);
            return;
        }

        StartCoroutine(this.ShootDieAnimation(this.gameObject));
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11 && !GameManager.instance.isInLevelLoad)
            Destroy(this.gameObject);
    }

    private void OnDestroy(){
        if(this.spawner != null)
             this.spawner.count = this.spawner.count - 1;
    }

}
