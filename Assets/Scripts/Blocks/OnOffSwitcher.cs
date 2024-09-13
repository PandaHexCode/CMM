using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnOffSwitcher : OnOffObject{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && collision.GetComponent<Rigidbody2D>().velocity.y > -5){
            if (collision.gameObject.transform.position.x - 0.8f < this.transform.position.x && collision.gameObject.transform.position.x + 0.8f > this.transform.position.x)
                UseOnOffSwitcher(false);
        }
    }

    public void UseOnOffSwitcher(bool isHitDown){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
        GameManager.instance.PlayBlockHitAnimation(this.gameObject, isHitDown);
        SceneManager.SwitchOnOffState();
    }

}
