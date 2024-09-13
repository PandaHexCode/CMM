using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorMessageBlock : MonoBehaviour{

    public string text;

    private void Awake(){
        if (!LevelEditorManager.instance.isInEditorLevelLoad && Time.timeScale != 0){
            GameManager.instance.sceneManager.levelEditorCursor.SetCanBuild(false);
            GameManager.instance.sceneManager.levelEditorCursor.canChangeCanBuild = false;
           GameManager.StopTimeScale();
            LevelEditorManager.canSwitch = false;
            MenuManager.canOpenMenu = false;
        }else
            Destroy(this.transform.GetChild(0).gameObject);
    }

    public void OnFinishClick(){
        this.text = this.transform.GetChild(0).GetComponentInChildren<UnityEngine.UI.InputField>().text;
        this.text = this.text.Replace(":", "");
        this.text = this.text.Replace("\n", "#n");
        Destroy(this.transform.GetChild(0).gameObject);
        GameManager.ResumeTimeScale();
        LevelEditorManager.canSwitch = true;
        MenuManager.canOpenMenu = true;
        GameManager.instance.sceneManager.levelEditorCursor.SetCanBuild(true);
        GameManager.instance.sceneManager.levelEditorCursor.canChangeCanBuild = true;
    }

    private void OnDestroy(){
        if (this.transform.GetChild(0) != null){
            Destroy(this.transform.GetChild(0).gameObject);
            GameManager.instance.sceneManager.levelEditorCursor.SetCanBuild(true);
            GameManager.instance.sceneManager.levelEditorCursor.canChangeCanBuild = true;
        }
    }

}
