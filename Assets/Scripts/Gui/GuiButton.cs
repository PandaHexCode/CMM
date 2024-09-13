using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GuiButton : MonoBehaviour{

    public UnityEvent onClickEvent;
    public bool setCanBuild = true;

    public bool canPress = false;
    private LevelEditorCursor cursor;

    private void Awake(){
        this.cursor = GameManager.instance.sceneManager.levelEditorCursor;
    }

    private void Update(){
        if (this.canPress && Input.GetMouseButtonDown(0)){
            this.onClickEvent.Invoke();
            StopCoroutine(GameManager.instance.sceneManager.levelEditorCursor.ActionCor);
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 13){
            this.canPress = true;
            if (this.setCanBuild)
                this.cursor.SetCanBuild(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 13){
            this.canPress = false;
            if(this.setCanBuild)
                this.cursor.SetCanBuild(true);
        }
    }

}
