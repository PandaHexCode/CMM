using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.Mystery;

/*https://lospec.com/palette-list/cica26*/
public class LevelEditorMysteryMushrom : MonoBehaviour{

    public int costumeNumber = 0;
    public GameObject button;

    private void Awake(){
        if (!LevelEditorManager.instance.isInEditorLevelLoad && Time.timeScale != 0){
            InitButtons();
            GameManager.instance.sceneManager.levelEditorCursor.SetCanBuild(false);
            GameManager.instance.sceneManager.levelEditorCursor.canChangeCanBuild = false;
            LevelEditorManager.canSwitch = false;
            MenuManager.canOpenMenu = false;
        }else
            Destroy(this.transform.GetChild(0).gameObject);
    }

    public void OnCostumeClick(GameObject t){
        this.costumeNumber = GameManager.StringToInt(t.name);
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

    private void InitButtons(){
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < GameManager.instance.mysteryCostumesManager.mysteryCostumes.Length; i++) {
            MysteryCostume mysteryCostume = GameManager.instance.mysteryCostumesManager.mysteryCostumes[i];
            if (!mysteryCostume.isUnused){
                GameObject btn = Instantiate(this.button, this.transform.GetChild(0).transform.GetChild(0));
                btn.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = mysteryCostume.sprites.stand[0];
                btn.GetComponent<GuiButton>().onClickEvent.AddListener(delegate { OnCostumeClick(btn); });
                btn.transform.position = btn.transform.position + offset;
                btn.name = i.ToString();
                if (mysteryCostume.isSpecial)
                    btn.GetComponent<SpriteRenderer>().sprite = GameManager.instance.mysteryCostumesManager.buttonSprites[1];
                if(mysteryCostume.isPatreon)
                    btn.GetComponent<SpriteRenderer>().sprite = GameManager.instance.mysteryCostumesManager.buttonSprites[2];
                if ((mysteryCostume.isUnlockable && !GameManager.instance.unlockManager.unlockableThings[(int)mysteryCostume.unlockID].isUnlocked)){
                    Destroy(btn.transform.GetChild(0).gameObject);
                    Destroy(btn.GetComponent<GuiButton>());
                }
                offset = offset + new Vector3(1.5f, 0, 0);
                if (btn.transform.position.x >= 2.5f)
                    offset = new Vector3(0, offset.y - 2, 0);
            }
        }
    }

    public void ScrollButtonsUp(){
        if (this.transform.GetChild(0).transform.GetChild(0).position.y != 0)
            this.transform.GetChild(0).transform.GetChild(0).position = this.transform.GetChild(0).transform.GetChild(0).transform.position + new Vector3(0, 2, 0);
    }

    public void ScrollButtonsDown(){
        
            this.transform.GetChild(0).transform.GetChild(0).position = this.transform.GetChild(0).transform.GetChild(0).transform.position + new Vector3(0, -2, 0);
    }

}
