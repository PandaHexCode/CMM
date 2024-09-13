using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBlock : MonoBehaviour{

    public string text;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && collision.GetComponent<Rigidbody2D>().velocity.y > -5)
            UseMessageBlock(false);
    }

    public void UseMessageBlock(bool isHitDown){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.messageBlock);
        GameManager.instance.PlayBlockHitAnimation(this.gameObject, isHitDown);
        StartCoroutine(UseMessageBlockIE());
    }

    private IEnumerator UseMessageBlockIE(){
        yield return new WaitForSeconds(0.1f);
        GameManager.instance.sceneManager.StartTextBoxText(this.text);
    }

}
