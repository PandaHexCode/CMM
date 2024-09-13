using UnityEngine;

public class Coin : TileAnimator{

    public int amount = 1;

    public void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 | collision.gameObject.layer == 18)
            Collect();
    }

    public void Collect(){
        GameObject effect = Instantiate(GameManager.instance.sceneManager.coinEffect);
        effect.transform.position = this.transform.position;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.coin);
        GameManager.instance.sceneManager.AddCoins(this.amount);
        Destroy(this.gameObject);
    }

}
