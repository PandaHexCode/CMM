using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuiButton2 : GuiButton{

    private void Update(){
        if (this.canPress && Input.GetKeyDown(KeyCode.G)){
            this.onClickEvent.Invoke();
            StopCoroutine(GameManager.instance.sceneManager.levelEditorCursor.ActionCor);
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        }
    }

}
