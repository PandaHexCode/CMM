using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpBox : MonoBehaviour{

    public GameObject otherWarpBox;
    [System.NonSerialized] public GameObject targetEnemy = null;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !collision.GetComponent<PlayerController>().isInGoal)
            StartCoroutine(UseWarpBox(collision.gameObject));
        else if (GameManager.IsInLayerMask(collision.gameObject, GameManager.instance.entityMask)){
            if (collision.gameObject == this.targetEnemy)
                return;
            if ((collision.gameObject.layer == 23 | collision.gameObject.layer == 8) && collision.gameObject.GetComponent<EntityGravity>() == null)
                return;
            StartCoroutine(EnemyUseWarpBox(collision.gameObject));
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject == this.targetEnemy)
            this.targetEnemy = null;
    }

    private IEnumerator EnemyUseWarpBox(GameObject enemy){
        GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
        eff.GetComponent<SpriteRenderer>().color = Color.red;
        eff.transform.position = this.transform.position;
        if (enemy.layer == 26){
            enemy = enemy.transform.parent.gameObject;
            enemy.GetComponent<Animator>().Play(0);
        }

        this.otherWarpBox.GetComponent<WarpBox>().targetEnemy = enemy;
        enemy.transform.position = this.otherWarpBox.transform.position;
        GameObject eff2 = Instantiate(GameManager.instance.sceneManager.hitEffect);
        eff2.GetComponent<SpriteRenderer>().color = Color.red;
        eff2.transform.position = otherWarpBox.transform.position;
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator UseWarpBox(GameObject player){
        GameManager.instance.sceneManager.playerCamera.UnfreezeCamera();
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.warpBoxEnter);
        GameObject eff = Instantiate(GameManager.instance.sceneManager.coinEffect);
        eff.GetComponent<SpriteRenderer>().color = Color.red;
        eff.transform.position = this.transform.position;
        this.GetComponent<SpriteRenderer>().enabled = false;
        this.GetComponent<BoxCollider2D>().enabled = false;
        this.otherWarpBox.GetComponent<BoxCollider2D>().enabled = false;
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        player.GetComponent<PlayerController>().SetCanMove(false);
        player.GetComponent<Rigidbody2D>().isKinematic = true;
        player.GetComponent<Rigidbody2D>().simulated = false;
        GameObject trans = GameManager.instance.sceneManager.InitTransision1(player.transform);
        yield return new WaitForSeconds(0.52f);
        player.transform.position = this.otherWarpBox.transform.position;
        trans.transform.position = player.transform.position;
        trans.GetComponent<Animator>().Play("Transision1Back");
        yield return new WaitForSeconds(0.5f);
        Destroy(trans);

        LevelEditorManager.canSwitch = true;
        MenuManager.canOpenMenu = true;
        player.GetComponent<PlayerController>().SetCanMove(true);
        player.GetComponent<Rigidbody2D>().isKinematic = false;
        player.GetComponent<Rigidbody2D>().simulated = true;
        GameObject eff2 = Instantiate(GameManager.instance.sceneManager.coinEffect);
        eff2.GetComponent<SpriteRenderer>().color = Color.red;
        eff2.transform.position = otherWarpBox.transform.position;
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.warpBoxExit);
        Destroy(this.otherWarpBox);
        Destroy(this.gameObject);
    }

}
