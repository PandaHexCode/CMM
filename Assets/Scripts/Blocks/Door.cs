using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : TileAnimator{

    [Header("Door")]
    public GameObject otherDoor;
    public bool isPSwitchDoor = false;
    public bool isKeyDoor = false;
    public bool isFakeDoor = false;

    private bool canEnter = false;
    private bool isClose = false;
    private PlayerController p;

    private void Update(){
        if(this.canEnter && this.p.input.UP_DOWN && !this.isClose)
            OpenDoor();
    }

    private void OpenDoor(){
        if (Time.timeScale == 0 | !p.GetOnGround())
            return;

        if(this.isKeyDoor && !p.HasKey()){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.doorLocked);
            StopAllCoroutines();
            StartCoroutine(KeyDoorNotOpenRotation());
            return;
        }else if(this.isKeyDoor && p.HasKey()){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.doorUnlock);
            StopAllCoroutines();
            StartCoroutine(OpenKeyDoorIE());
            return;
        }else if (this.isFakeDoor){
            GameObject eff = Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = this.transform.position;
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.booLaught);
            GameObject booEff1 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff1.transform.position = this.transform.position;
            booEff1.transform.rotation = Quaternion.Euler(0, 0, -40);
            GameObject booEff2 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff2.transform.position = this.transform.position;
            booEff2.transform.rotation = Quaternion.Euler(0, -180, -40);
            GameObject booEff3 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff3.transform.position = this.transform.position;
            booEff3.transform.rotation = Quaternion.Euler(0, 0, 0);
            GameObject booEff4 = Instantiate(GameManager.instance.sceneManager.booEffect);
            booEff4.transform.position = this.transform.position;
            booEff4.transform.rotation = Quaternion.Euler(0, -180, 0);
            Destroy(this.gameObject);
            return;
        }

        GameManager.instance.sceneManager.playerCamera.UnfreezeCamera();
        this.isClose = true;
        this.canEnter = false;
        StartAnimationClip(this.animationClips[0]);
        StartCoroutine(OpenDoorIE(p));
    }

    private IEnumerator OpenKeyDoorIE(){
        this.isKeyDoor = false;
        this.canEnter = false;
        p.GetComponent<PlayerController>().BreakReturn();
        p.GetComponent<PlayerController>().SetCanMove(false);
        p.RemoveKey();
        this.sp.sprite = TileManager.instance.GetSpriteFromTileset(182, TileManager.TilesetType.ObjectsTileset);
        this.otherDoor.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(182, TileManager.TilesetType.ObjectsTileset);
        this.otherDoor.GetComponent<Door>().isKeyDoor = false;
        while(this.transform.localScale.x < 1.3f){
            this.transform.localScale = new Vector3(this.transform.localScale.x + 5 * Time.deltaTime, this.transform.localScale.y + 5 * Time.deltaTime, this.transform.localScale.z);
            yield return new WaitForSeconds(0f);
        }

        while (this.transform.localScale.x > 1){
            this.transform.localScale = new Vector3(this.transform.localScale.x - 5 * Time.deltaTime, this.transform.localScale.y - 5 * Time.deltaTime, this.transform.localScale.z);
            yield return new WaitForSeconds(0f);
        }

        this.transform.localScale = new Vector3(1, 1, this.transform.localScale.z);
        this.isClose = true;
        StartAnimationClip(this.animationClips[0]);
        StartCoroutine(OpenDoorIE(p));
    }

    public IEnumerator KeyDoorNotOpenRotation(){
        this.transform.rotation = Quaternion.Euler(0, 0, 0);

        for (int i = 0; i < 2; i++){
            while(this.transform.rotation.z > -0.1449779){
                this.transform.Rotate(0, 0, -255 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z < 0){
                this.transform.Rotate(0, 0, 255 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z < 0.1449779){
                this.transform.Rotate(0, 0, 255 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            while (this.transform.rotation.z > 0){
                this.transform.Rotate(0, 0, -255 * Time.deltaTime);
                yield return new WaitForSeconds(0f);
            }

            this.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private IEnumerator OpenDoorIE(PlayerController player){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.doorOpen);
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        player.GetComponent<PlayerController>().BreakReturn();
        player.GetComponent<PlayerController>().SetCanMove(false);
        player.GetComponent<Rigidbody2D>().isKinematic = true;
        player.GetComponent<Rigidbody2D>().simulated = false;
        GameObject trans = GameManager.instance.sceneManager.InitTransision1(player.transform);
        yield return new WaitForSeconds(0.52f);
        Vector3 offset = new Vector3(0, -0.5f, 0);
        if (player.GetPowerup() != PlayerController.Powerup.Small)
            offset = new Vector3(0, 0, 0);
        player.transform.position = this.otherDoor.transform.position + offset;
        this.otherDoor.GetComponent<TileAnimator>().StartAnimationClip(this.animationClips[0]);
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.doorClose);
        trans.transform.position = player.transform.position;
        trans.GetComponent<Animator>().Play("Transision1Back");
        SceneManager.DestroyDestroyAfterNewLoadList();
        GameManager.instance.sceneManager.RespawnEntities();
        yield return new WaitForSeconds(0.5f);
        Destroy(trans);

        LevelEditorManager.canSwitch = true;
        MenuManager.canOpenMenu = true;
        player.GetComponent<PlayerController>().SetCanMove(true);
        player.GetComponent<Rigidbody2D>().isKinematic = false;
        player.GetComponent<Rigidbody2D>().simulated = true;
        this.isClose = false;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            this.p = collision.gameObject.GetComponent<PlayerController>();
            this.canEnter = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.canEnter = false;
    }

    public void DeactivatePSwitchDoor(){
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(45, TileManager.TilesetType.ObjectsTileset);
    }

    public void ActivatePSwitchDoor(){
        this.GetComponent<BoxCollider2D>().enabled = true;
        this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(46, TileManager.TilesetType.ObjectsTileset);
    }
}
