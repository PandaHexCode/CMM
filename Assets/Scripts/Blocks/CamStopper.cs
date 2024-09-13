using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamStopper : MonoBehaviour{

    private void OnEnable(){
        if (LevelEditorManager.isLevelEditor && !LevelEditorManager.instance.isPlayMode)
            Destroy(this);

        StartCoroutine(BX());
    }

    private IEnumerator BX(){
        yield return new WaitForSeconds(1f);
        GetComponent<BoxCollider2D>().enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 19 | collision.gameObject.layer == 11)
            StartCoroutine(WaitForPos());
    }

    private IEnumerator WaitForPos(){
        Vector3 newPos = new Vector3(this.transform.position.x, GameManager.instance.sceneManager.playerCamera.transform.position.y, GameManager.instance.sceneManager.playerCamera.transform.position.z);
        Transform player = GameManager.instance.sceneManager.players[0].transform;
        while (player.position.x < newPos.x){
             yield return new WaitForSeconds(0);
         }

        GameManager.instance.sceneManager.playerCamera.transform.position = newPos;
        GameManager.instance.sceneManager.playerCamera.FreezeCamera();
    }

}
