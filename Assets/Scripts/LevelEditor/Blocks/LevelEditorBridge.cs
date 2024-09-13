using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorBridge : MonoBehaviour{

    private void Update(){
        if (!LevelEditorManager.isLevelEditor)
            Destroy(this.gameObject);
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            Destroy(this.gameObject);
    }

}
