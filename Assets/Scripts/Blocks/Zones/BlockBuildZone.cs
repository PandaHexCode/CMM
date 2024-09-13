using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBuildZone : MonoBehaviour{

    public bool disableObjectAfterExit = false;
    private LevelEditorCursor cursor;

    private void Awake(){
        this.cursor = GameManager.instance.sceneManager.levelEditorCursor;
    }

    private void OnTriggerStay2D(Collider2D collision){
        if (collision.gameObject.layer == 13)
            this.cursor.SetCanBuild(false);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 13){
            if (this.disableObjectAfterExit)
                this.gameObject.SetActive(false);
            this.cursor.SetCanBuild(true);
        }
    }

}
