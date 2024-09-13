using UnityEngine;

public class PowerupItem : EntityGravity{

    [Header("PowerupItem")]
    public PlayerController.Powerup powerup;
    public bool isOneUp = false;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            if (this.isOneUp){
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.oneUp);
                SceneManager.respawnableEntities.Remove(SceneManager.GetRespawnableEntityFromEntity(this.gameObject));
                Destroy(this.gameObject);
                return;
            }

            if (this.powerup == PlayerController.Powerup.Big && (collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Small | collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini))
                collision.gameObject.GetComponent<PlayerController>().SetPowerup(this.powerup);
            else if (this.powerup != PlayerController.Powerup.Big)
                collision.gameObject.GetComponent<PlayerController>().SetPowerup(this.powerup);
            else
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerup);
            if (this.powerup == PlayerController.Powerup.Probeller){
                SceneManager.respawnableEntities.Remove(SceneManager.GetRespawnableEntityFromEntity(this.transform.parent.gameObject));
                Destroy(this.transform.parent.gameObject);
            }else{
                SceneManager.respawnableEntities.Remove(SceneManager.GetRespawnableEntityFromEntity(this.gameObject));
               Destroy(this.gameObject);
            }
        }
    }

    public void OnAnimationFinish(){
        Destroy(this.transform.parent.gameObject);
    }

}
