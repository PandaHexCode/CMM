using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UMM.BlockData;
using UMM.Unlock;
using UnityEngine;
using UnityEngine.UI;
using XInputDotNetPure;

public class MainMenu : MonoBehaviour{

    public AudioClip[] titleThemesNormal;
    public AudioClip titleThemeHalloween;

    public GameObject titleScreenParent;
    public GameObject mainScreen;
    public GameObject waitScreen;
    public GameObject unlockableScreen;
    public SubMenuData[] subMenuDatas;

    public UnlockManager unlockManager;
    public GameObject unlockableThingPrefarb;
    public TextMeshProUGUI versionText;
    public Image logo;
    public Sprite[] logos;

    private Button[] buttons = new Button[] { };

    private InputManager input;
    private int currentButton = 0;
    private TileManager tileManager;

    [System.Serializable]
    public class SubMenuData{
        public string subMenuName;
        public GameObject subMenu;
        public Button[] buttons;
    }

    public void Activate(){
        this.tileManager = TileManager.instance;
        this.versionText.text = "Ver. " + GameManager.instance.buildData.VERSION_STRING;
        this.versionText.text = this.versionText.text + "\nLogged as " + GameManager.instance.buildData.USERNAME;

        this.input = InputManager.instances[0];
        SearchButtonsFromMenu(this.subMenuDatas[0].subMenu);
        EventCheck();
        InitUnlockableThings();
        BackgroundMusicManager.instance.StopCurrentBackgroundMusic();
        MenuManager.canOpenMenu = false;
        LevelEditorManager.instance.ActivatePlayMode();
        GameManager.instance.sceneManager.switchModeButton.SetActive(false);
    }

    public void OnClickEdit(){
        GameManager.instance.isInMainMenu = false;
        if (LevelEditorManager.instance.isPlayMode)
            LevelEditorManager.instance.SwitchMode();
        GameManager.instance.sceneManager.switchModeButton.SetActive(true);
        MenuManager.canOpenMenu = true;
        BackgroundMusicManager.instance.StartPlayingBackgroundMusic();
        Destroy(this.gameObject);
    }

    public void OnLetterButtonClick(int id){
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.kicked);

        switch (id){
            case 0:/*EButton*/
                List<BlockData> blockDatas = new List<BlockData>();
                foreach(BlockData blockData in GameManager.instance.blockDataManager.blockDatas){
                    if (blockData.canRespawn)
                        blockDatas.Add(blockData);
                }

                BlockData blockData1 = blockDatas[UnityEngine.Random.Range(0, blockDatas.Count - 1)];
                if (blockData1.dontDisplayButtonInEditor | blockData1.id == BlockID.THWOMP | blockData1.id == BlockID.DONUTBLOCK | blockData1.id == BlockID.GROUNDBOO | (blockData1.onlyForSMB1 && TileManager.instance.currentStyleID != TileManager.StyleID.SMB1))
                    return;
                GameObject en = Instantiate(blockData1.prefarb, GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea));
                en.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(blockData1.spriteId, blockData1.tilesetType);
                en.transform.position = Camera.main.ScreenToWorldPoint(this.logo.transform.GetChild(1).GetChild(0).position);
                GameObject eff = Instantiate(GameManager.instance.sceneManager.hitEffect);
                eff.transform.position = en.transform.position;

                if (en.GetComponent<EntityGravity>() == null)
                    return;
              
                en.GetComponent<EntityGravity>().enabled = false;
                
                float speed = UnityEngine.Random.Range(5, 22);
                if (UnityEngine.Random.Range(0, 10) > 5){
                    if(en.GetComponent<Entity>() != null)
                        en.GetComponent<Entity>().direction = 1;

                    en.GetComponent<SpriteRenderer>().flipX = true;
                    speed = -speed;
                }

                new SceneManager.RespawnableEntity(en, blockData1.id, en.transform.position, GameManager.instance.sceneManager.currentArea).dontRespwan = true;
                en.GetComponent<EntityGravity>().StartCoroutine(SceneManager.DropGameObject(en, speed));
                Time.timeScale = 1;
                break;
            case 1:/*KButton*/
                SceneManager.RemoveAllRespawnableEntities();
                break;
            case 2:/*RButton*/
                SceneManager.RemoveAllRespawnableEntities();
                GameManager.instance.sceneManager.RespawnEntities();
                break;
        }
    }

    public void SearchButtonsFromMenu(GameObject menu){
        if (this.buttons.Length > 1)
            this.buttons[this.currentButton].GetComponent<Image>().color = Color.white;
        foreach (SubMenuData data in this.subMenuDatas){
            if (menu == data.subMenu){
                this.buttons = data.buttons;
            }
        }
        this.currentButton = 0;
    }

    private void Update(){

        if (GameManager.instance.sceneManager.isAreaDark[GameManager.instance.sceneManager.currentArea] | tileManager.currentTileset.id == TileManager.TilesetID.Underground | tileManager.currentTileset.id == TileManager.TilesetID.Castle | tileManager.currentTileset.id == TileManager.TilesetID.HauntedHouse | tileManager.currentTileset.id == TileManager.TilesetID.SMB3_EXTRA1){
            if (this.logo.sprite == this.logos[2])
                this.logo.sprite = this.logos[7];
            else if (this.logo.sprite == this.logos[0])
                this.logo.sprite = this.logos[6];
            else if(this.logo.sprite == this.logos[8])
                this.logo.sprite = this.logos[9];
            this.logo.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }else{
            if (this.logo.sprite == this.logos[7])
                this.logo.sprite = this.logos[2];
            else if (this.logo.sprite == this.logos[6])
                this.logo.sprite = this.logos[0];
            else if (this.logo.sprite == this.logos[9])
                this.logo.sprite = this.logos[8];
            this.logo.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        }

        for (int i = 0; i < 4; ++i) {
            PlayerIndex testPlayerIndex = (PlayerIndex)i;
            GamePadState testState = GamePad.GetState(testPlayerIndex);
            if (testState.IsConnected && (testState.Buttons.A == ButtonState.Pressed | testState.Buttons.B == ButtonState.Pressed | testState.DPad.Left == ButtonState.Pressed | testState.DPad.Right == ButtonState.Pressed)) {
                input.SetControllerPlayerIndex(testPlayerIndex);
                input.SetControllerGamePadState(testState);
                input.inputDeviceType = InputManager.InputDeviceType.CONTROLLER;
                SettingsManager.instance.SetOption("InputDeviceType", 0);
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) | Input.GetKey(KeyCode.RightArrow) | Input.GetKey(KeyCode.X) | Input.GetKey(KeyCode.Space)){
            input.inputDeviceType = InputManager.InputDeviceType.KEYBOARD;
            SettingsManager.instance.SetOption("InputDeviceType", 1);
        }

       if(this.waitScreen.active && ((this.input.inputDeviceType == InputManager.InputDeviceType.CONTROLLER && this.input.IsButtonPressed(this.input.controllerState.Buttons.RightShoulder) && this.input.IsButtonPressed(this.input.controllerState.Buttons.LeftShoulder)) | (this.input.inputDeviceType == InputManager.InputDeviceType.KEYBOARD && Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.R)))){
            this.waitScreen.SetActive(false);
            this.mainScreen.SetActive(true);
       }

        if (this.waitScreen.active)
            return;


         if (this.input.LEFT_DOWN){
            if (this.currentButton != this.buttons.Length - 1 && this.buttons[this.currentButton + 1].transform.parent.gameObject.active){
                this.buttons[this.currentButton].GetComponent<Image>().color = Color.white;
                this.currentButton++;
            }else{
                for (int i = 0; i < this.buttons.Length; i++){
                    if (this.buttons[i].transform.parent.gameObject.active){
                        this.buttons[this.currentButton].GetComponent<Image>().color = Color.white;
                        this.currentButton = i;
                        i = this.buttons.Length;
                        this.buttons[this.currentButton].GetComponent<Image>().color = Color.red;
                    }
                }
            }
        }else if (this.input.RIGHT_DOWN){
            if (this.currentButton != 0 && this.buttons[this.currentButton - 1].transform.parent.gameObject.active){
                this.buttons[this.currentButton].GetComponent<Image>().color = Color.white;
                if (this.currentButton != 0)
                    this.currentButton--;
            }
            else{
                for (int i = this.buttons.Length-1; i > 0; i--) {
                    if (this.buttons[i].transform.parent.gameObject.active){
                        this.buttons[this.currentButton].GetComponent<Image>().color = Color.white;
                        this.currentButton = i;
                        this.buttons[this.currentButton].GetComponent<Image>().color = Color.red;
                        i = 0;
                    }
                }
            }
        }

        if(this.input.RIGHT_DOWN | this.input.LEFT_DOWN)
            this.buttons[this.currentButton].GetComponent<Image>().color = Color.red;

        if (this.input.JUMP_DOWN && this.buttons[this.currentButton].GetComponent<Image>().color == Color.red)
            this.buttons[this.currentButton].onClick.Invoke();
    }

    private void InitUnlockableThings(){
        Vector3 offset = Vector3.zero;
        foreach (UnlockableThing unlockableThing in this.unlockManager.unlockableThings){
            GameObject btn = Instantiate(this.unlockableThingPrefarb, this.unlockableScreen.transform.GetChild(1));
            btn.transform.localPosition = btn.transform.localPosition + offset;
            if (unlockableThing.isUnlocked && unlockableThing.previewSprite != null)
            {
                btn.transform.GetChild(0).GetComponent<Image>().sprite = unlockableThing.previewSprite;
                btn.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
            }
            offset = offset + new Vector3(270f, 0, 0);
            if (btn.transform.localPosition.x >= 360f)
                offset = new Vector3(0, offset.y - 150, 0);
        }
    }

    public void EventCheck(){
        /*Logo*/
        if (DateTime.Today.Month == 6)
            this.logo.sprite = this.logos[2];
        else if(DateTime.Today.Month == 10)
            this.logo.sprite = this.logos[8];
        else{
            if (UnityEngine.Random.Range(0, 10) > 5){
                this.logo.sprite = this.logos[2];
            }else if (UnityEngine.Random.Range(0, 10) > 4){
                if (UnityEngine.Random.Range(0, 10) > 5)
                    this.logo.sprite = this.logos[4];
                else
                    this.logo.sprite = this.logos[1];
            }else{
               this.logo.sprite = this.logos[0];
            }
        }

        /*Music*/
        if (DateTime.Today.Month == 10)
            this.GetComponent<AudioSource>().clip = this.titleThemeHalloween;
        else{/*Randomize*/
            this.GetComponent<AudioSource>().clip = this.titleThemesNormal[UnityEngine.Random.Range(0, this.titleThemesNormal.Length)];
        }

        this.GetComponent<AudioSource>().Play();
    }

    public void OpenLink(string url){
        Application.OpenURL(url);
    }

    }




