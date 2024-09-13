using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Linq;

public class MenuManager : MonoBehaviour{

    public GameObject menuParent;
    public GameObject editorMainMenu;
    public GameObject playModeMainMenu;
    public GameObject[] subMenus;
    public SubMenuData[] subMenuDatas;
    public MenuButton grayedRestartButton;
    private MenuButton[] buttons = new MenuButton[] { };
    public Dropdown levelDropdown;
    public Slider musicSlider;
    public Slider soundsSlider;
    public Button possibleButton;
    public Image possibleSprite;
    public Sprite[] possibleSprites;
    public GameObject levelWarningText;
    public GameObject editButton;
    public Text userNameText;
    public Text versionText;
    public Text levelSaveInfo;
    public Text devLevelsButton;

    private int currentButton = 0;
    [System.NonSerialized] public bool isInMenu = false;
    public bool canRestart = true;
    [System.NonSerialized] public static bool canOpenMenu = true;
    [System.NonSerialized] public bool canClose = true;

    private InputManager input;

    [Header("AudioClips")]
    public AudioClip openMenuAudio;
    public AudioClip exitMenuAudio;
    public AudioClip scrollAudio;

    [System.Serializable]
    public class MenuButton{
        public Button button;
        public Dropdown dropdown = null;
        public AudioClip eventAudio;
        public bool isOptionButton = false;
        public string optionValueKey = "buttonText|optionValueKey";
    }

    [System.Serializable]
    public class SubMenuData{
        public string subMenuName;
        public GameObject subMenu;
        public MenuButton[] buttons;
    }

    private void Awake(){
        canOpenMenu = true;
        this.input = InputManager.instances[0];

        foreach(SubMenuData subMenu in this.subMenuDatas){
            foreach(MenuButton menuButton in subMenu.buttons){
                menuButton.button.onClick.AddListener(() => SearchAudioClipFromClick(menuButton.button.gameObject));
            }
        }
    }
    
    public void SearchAudioClipFromClick(GameObject button){
        foreach (SubMenuData subMenu in this.subMenuDatas){
            foreach (MenuButton menuButton in subMenu.buttons){
                if (menuButton.button.gameObject == button)
                    SoundManager.PlayAudioClip(menuButton.eventAudio);
            }
        }
    }

    private MenuButton[] lastButtons = null;
    private void Update(){
        if (this.input.MENU)
            OpenOrCloseMainMenu();

        if (!this.isInMenu)
            return;

        Time.timeScale = 0;

        if (Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.L))
            DevGameManager.SHOWDEBUGINFORMATIONS = !DevGameManager.SHOWDEBUGINFORMATIONS;

        if (this.input.LEFT && this.buttons != this.subMenuDatas[0].buttons && this.buttons[this.currentButton].dropdown == null){
            this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
            this.lastButtons = this.buttons;
            this.buttons = this.subMenuDatas[0].buttons;
            this.currentButton = 0;
            this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.red;
        }else if (this.input.RIGHT && this.lastButtons != null && this.buttons[this.currentButton].dropdown == null){
            this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
            this.buttons = this.lastButtons;
            this.lastButtons = null;
            this.currentButton = 0;
            this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.red;
        }

        if(this.buttons[this.currentButton].dropdown != null && this.input.RIGHT_DOWN)
            this.buttons[this.currentButton].dropdown.value++;
        else if (this.buttons[this.currentButton].dropdown != null && this.input.LEFT_DOWN)
            this.buttons[this.currentButton].dropdown.value--;

        if (this.input.DOWN_DOWN){
            if (this.currentButton != this.buttons.Length - 1 && this.buttons[this.currentButton + 1].button.transform.parent.gameObject.active){
                this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
                this.currentButton++;
                if (this.buttons[this.currentButton].button.GetComponent<Image>().color != Color.white && this.buttons[this.currentButton].button.GetComponent<Image>().color != Color.red)
                    this.currentButton++;
            }else{
                for (int i = 0; i < this.buttons.Length; i++){
                    if (this.buttons[i].button.transform.parent.gameObject.active){
                        this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
                        this.currentButton = i;
                        i = this.buttons.Length;
                        this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.red;
                    }
                }
            }
        }else if (this.input.UP_DOWN){
            if (this.currentButton != 0 && this.buttons[this.currentButton - 1].button.transform.parent.gameObject.active){
                this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
                if (this.currentButton != 0)
                    this.currentButton--;
                if (this.buttons[this.currentButton].button.GetComponent<Image>().color != Color.white && this.buttons[this.currentButton].button.GetComponent<Image>().color != Color.red)
                    this.currentButton--;
            }
            else{
                for (int i = this.buttons.Length-1; i > 0; i--) {
                    if (this.buttons[i].button.transform.parent.gameObject.active){
                        this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
                        this.currentButton = i;
                        this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.red;
                        i = 0;
                    }
                }
            }
        }

        if(this.input.UP_DOWN | this.input.DOWN_DOWN)
            this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.red;

        if (this.input.UP_DOWN | this.input.DOWN_DOWN | this.input.LEFT_DOWN | this.input.RIGHT_DOWN)
            SoundManager.PlayAudioClip(this.scrollAudio);

        if (this.input.JUMP_DOWN)
            this.buttons[this.currentButton].button.onClick.Invoke();
    }

    public void OpenOrCloseMainMenu(){
        if (!this.isInMenu)
            OpenMainMenu();
        else
            CloseMainMenu();
    }

    public void OpenMainMenu(){
        if (LevelEditorManager.isLevelEditor && !LevelEditorManager.canSwitch)
            return;
        if (!canOpenMenu | GameManager.instance.isInMainMenu)
            return;

        this.menuParent.GetComponent<Animator>().Play(0);
        if (this.canClose)
            StartCoroutine(OpenMainMenuWaitForAnimation());
        SoundManager.PlayAudioClip(this.openMenuAudio);
        LoadOptionButtonsTexts();
        ReloadLevelDropdown();
        GameManager.StopTimeScale();
        this.menuParent.SetActive(true);
        OpenMainSubMenu();

        this.levelSaveInfo.gameObject.SetActive(false);

        this.isInMenu = true;
        foreach (PlayerController p in GameManager.instance.sceneManager.players)
            p.isFreeze = true;
    }

    public void CloseMainMenu(){
        if (!this.canClose && this.isInMenu)
            return;

        SoundManager.PlayAudioClip(this.exitMenuAudio);
        this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
        this.isInMenu = false;
        GameManager.ResumeTimeScale();
        StartCoroutine(CloseMainMenuWaitForAnimation());

        if (LevelEditorManager.isLevelEditor)
            LevelEditorManager.canSwitch = true;

        foreach (PlayerController p in GameManager.instance.sceneManager.players)
            p.isFreeze = false;
    }

    private IEnumerator OpenMainMenuWaitForAnimation(){
        this.canClose = false;
        yield return new WaitForSecondsRealtime(0.25f);
        this.canClose = true;
    }

    private IEnumerator CloseMainMenuWaitForAnimation(){
        this.canClose = false;
        this.menuParent.GetComponent<Animator>().Play("CloseMenu");
        yield return new WaitForSecondsRealtime(0.35f);
        this.menuParent.SetActive(false);
        CloseAllMenus();
        this.canClose = true;
    }

    private void CloseAllMenus(){
        this.editorMainMenu.SetActive(false);
        this.playModeMainMenu.SetActive(false);
        foreach (GameObject menu in this.subMenus){
            menu.SetActive(false);
        }
    }

    public void OpenSubMenu(GameObject menu){
        if (!this.isInMenu)
            return;

        for (int i = 0; i < this.buttons.Length; i++){
            if (this.buttons[i].button.transform.parent.gameObject.active){
                this.currentButton = i;
                this.buttons[this.currentButton].button.GetComponent<Image>().color = Color.white;
            }
        }

        CloseAllMenus();
        menu.SetActive(true);
        if (menu.transform.parent != this.menuParent)
            menu.transform.parent.gameObject.SetActive(true);
        SearchButtonsFromMenu(menu);
    }

    public void OpenMainSubMenu(){
        CloseAllMenus();
        if (LevelEditorManager.isLevelEditor){
            this.editorMainMenu.SetActive(true);
            SearchButtonsFromMenu(this.editorMainMenu);
            LevelEditorManager.canSwitch = false;
        }else{
            this.playModeMainMenu.SetActive(true);
            SearchButtonsFromMenu(this.playModeMainMenu);
        }
    }

    private MenuButton s;
    private void SearchButtonsFromMenu(GameObject menu){
        foreach(SubMenuData data in this.subMenuDatas){
            if (menu == data.subMenu){
                this.buttons = data.buttons;
                if (!this.canRestart && data.subMenuName.Equals("PlayModeMainMenu")){
                    this.buttons[1].button.gameObject.SetActive(false);
                    if (this.buttons[1] != this.grayedRestartButton)
                        s = this.buttons[1];
                    this.buttons[1] = this.grayedRestartButton;
                    this.grayedRestartButton.button.gameObject.SetActive(true);
                    this.grayedRestartButton.button.GetComponentInChildren<Image>().color = new Color(0.6792453f, 0.6375934f, 0.6375934f, 0.4901961f);
                }else if (data.subMenuName.Equals("PlayModeMainMenu")){
                    if(s != null)
                        this.buttons[1] = s;
                    this.buttons[1].button.gameObject.SetActive(true);
                    this.grayedRestartButton.button.gameObject.SetActive(false);
                }
            }
        }
        this.currentButton = 0;
    }

    public void LoadOptionButtonsTexts(){
        foreach (SubMenuData data in this.subMenuDatas){
            foreach (MenuButton optionButton in data.buttons){     
                if (optionButton.isOptionButton){
                    string[] args = null;
                    if (optionButton.optionValueKey.Contains("|"))
                        args = optionButton.optionValueKey.Split('|');
                    else
                        args = new string[2] { string.Empty, optionButton.optionValueKey};
                    object option = SettingsManager.instance.GetOption(args[1]);
                    switch (args[1]) {
                        case "InputDeviceType":
                            option = (InputManager.InputDeviceType)option;
                            break;
                        case "VSync":
                        case "FullScreen":
                        case "SpriteShadow":
                            if ((bool)option == true)
                                option = "On";
                            else
                                option = "Off";
                            break;
                        case "ScaleMethod":
                            option = (PlayerCamera.ScaleMethod)option;
                            break;
                        case "PlayerButton":
                            if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.LUIGI){
                                optionButton.button.GetComponentInChildren<Text>().text = "L";
                                optionButton.button.GetComponentInChildren<Text>().color = Color.green;
                            }else if(GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.MARIO){
                                optionButton.button.GetComponentInChildren<Text>().text = "M";
                                optionButton.button.GetComponentInChildren<Text>().color = Color.red;
                            }else if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.BLUETOAD){
                                optionButton.button.GetComponentInChildren<Text>().text = "T";
                                optionButton.button.GetComponentInChildren<Text>().color = Color.blue;
                            }else if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.REDTOAD){
                                optionButton.button.GetComponentInChildren<Text>().text = "T";
                                optionButton.button.GetComponentInChildren<Text>().color = Color.red;
                            }else{
                                optionButton.button.GetComponentInChildren<Text>().text = "T";
                                optionButton.button.GetComponentInChildren<Text>().color = Color.green;
                            }
                            continue;
                    }
                    optionButton.button.GetComponentInChildren<Text>().text = args[0] + option.ToString();
                }
            }
        }

        this.musicSlider.SetValueWithoutNotify((float)SettingsManager.instance.GetOption("MusicVolume"));
        this.soundsSlider.SetValueWithoutNotify((float)SettingsManager.instance.GetOption("SoundsVolume"));
        SettingsManager.instance.UpdateAudioMixers();
    }

    /*Settings Buttons*/
    public void OnClickChangeDeviceType(){
        if (this.input.inputDeviceType == InputManager.InputDeviceType.CONTROLLER){
            SettingsManager.instance.SetOption("InputDeviceType", 0);
            this.input.inputDeviceType = InputManager.InputDeviceType.KEYBOARD;
        }else{
            this.canClose = false;
            this.input.ConnectController(this);
            OpenSubMenu(this.subMenus[4]);
        }

        this.LoadOptionButtonsTexts();
    }

    public void OnClickChangeSpriteShadow(){
        SettingsManager.instance.SetSpriteShadow(!(bool)SettingsManager.instance.GetOption("SpriteShadow"));
        this.LoadOptionButtonsTexts();
    }

    public void OnClickChangeFullScreen(){
        Screen.fullScreen = !Screen.fullScreen;
        SettingsManager.instance.SetOption("FullScreen", Screen.fullScreen);
        this.LoadOptionButtonsTexts();
    }

    public void OnClickChangeScaleMethod(){
        if (GameManager.instance.sceneManager.playerCamera.GetScaleMethod() == PlayerCamera.ScaleMethod.Aspect_Ratio)
            GameManager.instance.sceneManager.playerCamera.ChangeScaleMethod(PlayerCamera.ScaleMethod.Stretch);
        else
            GameManager.instance.sceneManager.playerCamera.ChangeScaleMethod(PlayerCamera.ScaleMethod.Aspect_Ratio);

        SettingsManager.instance.SetOption("ScaleMethod", (int)GameManager.instance.sceneManager.playerCamera.GetScaleMethod());
        this.LoadOptionButtonsTexts();
    }

    public void OnClickChangeVSync(){
        if (QualitySettings.vSyncCount == 0){
            GameManager.instance.sceneManager.playerCamera.SetVSync(true);
            SettingsManager.instance.SetOption("VSync", true);
        }else{
            GameManager.instance.sceneManager.playerCamera.SetVSync(false);
            SettingsManager.instance.SetOption("VSync", false);
        }

        this.LoadOptionButtonsTexts();
    }

    public void OnClickDefaultInputSettings(){
        if (!this.canClose)
            return;

        foreach (InputManager inputManager in InputManager.instances){
            if (inputManager != null)
                inputManager.LoadDefaultKeyCodes();
        }

        this.LoadOptionButtonsTexts();
    }

    public void StartChangingKeyCode(Button btn){
        if (!this.canClose)
            return;

        this.canClose = false;

        StartCoroutine(ChangeKeyCodeIE(btn));
    }

    private IEnumerator ChangeKeyCodeIE(Button btn){
        btn.GetComponentInChildren<Text>().text = "...";

        foreach (InputManager inputManager in InputManager.instances){
            if (inputManager != null)
                inputManager.allowInput = false;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        KeyCode key = KeyCode.None;
        while (key == KeyCode.None){
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
                if (vKey == KeyCode.Mouse0 | vKey.ToString().StartsWith("Joy"))
                    continue;
                if (Input.GetKey(vKey)){
                    key = vKey;
                }
            }

            yield return new WaitForSecondsRealtime(0);
        }

        this.canClose = true;
        SettingsManager.instance.SetOption(btn.name, key.ToString());
        foreach (InputManager inputManager in InputManager.instances){
            if (inputManager != null){
                inputManager.allowInput = true;
                inputManager.LoadKeyCodes();
            }
        }

        this.LoadOptionButtonsTexts();
    }

    public void OnClickOpenLevelPath(){
        string path = GameManager.LEVEL_PATH.Replace(@"/", @"\");
        Application.OpenURL(path);
    }

    public void OnClickOpenDcShareChannel(){
        Application.OpenURL("https://discord.com/channels/908731397827592193/1025801048801157220");
    }

    public void OnChangeVolumeSlider(Slider slider){
        if (!this.isInMenu)
            return;
        if (slider == this.musicSlider)
            SettingsManager.instance.SetOption("MusicVolume", slider.value);
        else
            SettingsManager.instance.SetOption("SoundsVolume", slider.value);

        SettingsManager.instance.UpdateAudioMixers();
        LoadOptionButtonsTexts();
    }

    /**/

    /*Multiplayer*/
    private Coroutine findCor;
    private PlayerController currentSearch = null;

    public void OnClickStartLocalMultiplayer(Text text){
        GameManager.instance.sceneManager.TryToStopLocalMultiplayer();
        this.findCor = StartCoroutine(FindLocalPlayersIE(text));
    } 

    private IEnumerator FindLocalPlayersIE(Text text){
        bool keyboard = false;
        text.text = "";
        for (int i = 0; i <= 3; i++){
            string orgText = text.text;
            text.text = text.text + "P" + (i + 1) + " - Wait...";
            if (i != 0)
                this.currentSearch = GameManager.instance.sceneManager.InitNewPlayer();
            InputManager.instances[i].inputDeviceType = InputManager.InputDeviceType.KEYBOARD;
            Coroutine cor =  InputManager.instances[i].ConnectController(null, !keyboard);
            while (InputManager.instances[i].inputDeviceType == InputManager.InputDeviceType.KEYBOARD && (!Input.GetKeyDown(KeyCode.Space) | keyboard)){
                yield return new WaitForSeconds(0);
            }
            if(cor != null)
                StopCoroutine(cor);
            if (InputManager.instances[i].inputDeviceType == InputManager.InputDeviceType.KEYBOARD)
                keyboard = true;
            text.text = orgText + "P" + (i + 1) + " - " + InputManager.instances[i].inputDeviceType.ToString() + "\n";
            yield return new WaitForSeconds(0);
        }
    }

    public void OnClickFinishLocalMultiplayer(){
        if(this.findCor != null){
            StopCoroutine(this.findCor);
            GameManager.instance.sceneManager.players.Remove(this.currentSearch);
            Destroy(this.currentSearch.gameObject);
            GameManager.instance.sceneManager.playerCamera.StartMultiplayerCam();
        }
    }
    /**/

    public void OnClickSaveLevel(InputField nameInput){
        this.levelSaveInfo.gameObject.SetActive(true);
        if (nameInput.text.Length > 2 && !nameInput.text.Contains(":")){
            try{
                string path = GameManager.LEVEL_PATH + nameInput.text + ".umm";
                LevelEditorManager.instance.SaveLevelAsFile(path);
                if (!File.Exists(path)){
                    this.levelSaveInfo.text = "Something went wrong.\nCan't create log.";
                    return;
                }else 
                    this.levelSaveInfo.text = "Level was saved successfully.";
            }catch(Exception ex){
                this.levelSaveInfo.text = "Something went wrong.\nCreated log.";
                GameManager.SaveFile(GameManager.LEVEL_PATH + nameInput.text + ".log", ex.Message + "\n" + ex.StackTrace);
            }
        }else
            this.levelSaveInfo.text = "Level name is too short or not allowed.";
        ReloadLevelDropdown();
    }

    public void OnClickPlayLevel(){
        CloseMainMenu();

        string path = GameManager.LEVEL_PATH + this.levelDropdown.options[this.levelDropdown.value].text;
        if (this.isDevLevelsEnabled){
            if (File.Exists(Application.streamingAssetsPath + "\\TitleLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text))
                path = Application.streamingAssetsPath + "\\TitleLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text;
            else
                path = Application.streamingAssetsPath + "\\DevLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text;
        }

        GameManager.instance.sceneManager.StartOnlyPlayModeLevel(path, true, this.levelDropdown.options[this.levelDropdown.value].text.Replace(".umm", ""));
    }

    public void OnClickUploadLevel(){
        CloseMainMenu();
        GameManager.instance.GetComponent<OnlineLevelManager>().UploadLevel(GameManager.LEVEL_PATH + this.levelDropdown.options[this.levelDropdown.value].text);
    }

    public void OnClickEditLevel(){
        if (!LevelEditorManager.isLevelEditor){
            GameManager.instance.LoadEditorScene("--editLevel-" + this.levelDropdown.options[this.levelDropdown.value].text);
            return;
        }

        Time.timeScale = 0.1f;
        LevelEditorManager.instance.LoadLevelInEditor(GameManager.LEVEL_PATH + this.levelDropdown.options[this.levelDropdown.value].text);
        Time.timeScale = 0;
    } 

    public void OnClickDeleteLevel(){
        System.IO.File.Delete(GameManager.LEVEL_PATH + this.levelDropdown.options[this.levelDropdown.value].text);
        ReloadLevelDropdown();
    }

    private bool isDevLevelsEnabled = false;
    public void ReloadLevelDropdown(){
        this.levelDropdown.ClearOptions();
        if (this.isDevLevelsEnabled){
            this.levelDropdown.AddOptions(GameManager.GetAllSavedLevels(Application.streamingAssetsPath + "\\TitleLevels\\"));
            this.levelDropdown.AddOptions(GameManager.GetAllSavedLevels(Application.streamingAssetsPath + "\\DevLevels\\"));
            try{
                GameManager.SortDropdownOptions(this.levelDropdown);
            }catch(Exception ex){

            }
            this.devLevelsButton.text = "Saved - Levels";
        }else{
            this.levelDropdown.AddOptions(GameManager.GetAllSavedLevels());
            this.devLevelsButton.text = "Dev - Levels";
        }

        if(this.levelDropdown.options.Any())
            OnLevelDropDownChanged();
        else{
            this.possibleButton.gameObject.SetActive(true);
            DisableEditButton();
        }
    }

    public void OnDevButtonClick(){
        this.isDevLevelsEnabled = !this.isDevLevelsEnabled;
        ReloadLevelDropdown();
    }

    public void OnClickRestart(){
        GameManager.instance.RestartCurrentLevel();
        CloseMainMenu();
    }

    public void OnTagToggle(Toggle toggle){
        int i = 0;
        foreach(Toggle toggle1 in GameManager.instance.sceneManager.saveLevelTagToggles){
            if (toggle1.isOn)
                i++;
        }

        if (i > 5)
            toggle.SetIsOnWithoutNotify(false);
        if (i == 0)
            toggle.SetIsOnWithoutNotify(true);
    }

    public void OnLevelDropDownChanged(){
        this.levelWarningText.SetActive(false);
        this.possibleSprite.sprite = this.possibleSprites[1];
        this.possibleButton.gameObject.SetActive(true);
        this.editButton.GetComponent<Button>().interactable = true;
        this.editButton.GetComponentInChildren<Text>().color = new Color32(0, 0, 0, 255);
        this.userNameText.text = "Level created by %UserName%";
        this.versionText.text = "Created in game version %Version%";
        string path = GameManager.LEVEL_PATH + this.levelDropdown.options[this.levelDropdown.value].text;

        if (this.isDevLevelsEnabled){
            if (File.Exists(Application.streamingAssetsPath + "\\TitleLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text))
                path = Application.streamingAssetsPath + "\\TitleLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text;
            else
                path = Application.streamingAssetsPath + "\\DevLevels\\" + this.levelDropdown.options[this.levelDropdown.value].text;
        }

        if (!File.Exists(path))
            return;

        string fileContent = GameManager.GetFileIn(path);
        if (!fileContent.Substring(1, 1).Contains(":")){
            try{
                fileContent = GameManager.Decrypt(fileContent, GameManager.Decrypt(GameManager.instance.buildData.levelKey, GameManager.instance.buildData.levelKey2));
            }catch (Exception ex){
                this.levelWarningText.SetActive(true);
                return;
            }
        }

        if (this.isDevLevelsEnabled)
            DisableEditButton();

        string[] lines = fileContent.Split('\n');
        string[] args1 = lines[0].Split(':');
        try{
            bool isPossible = GameManager.IntToBool(GameManager.StringToInt(args1[12]));
            if (isPossible | this.isDevLevelsEnabled){
                this.possibleSprite.sprite = this.possibleSprites[0];
                this.possibleButton.gameObject.SetActive(false);
            }
            
            string[] args2 = args1[13].Split(new string[] { "#$§#_+" }, System.StringSplitOptions.None);

            if (args2[1].Equals("NULL") && this.isDevLevelsEnabled)
                args2[1] = "PandaHexCode";

            this.userNameText.text = this.userNameText.text.Replace("%UserName%", args2[1]);
            if (args2[1] != GameManager.instance.buildData.USERNAME)
                DisableEditButton();
            this.versionText.text = this.versionText.text.Replace("%Version%", args2[2]);
            this.userNameText.gameObject.SetActive(true);
            this.versionText.gameObject.SetActive(true);
        }catch(Exception ex){
            this.levelWarningText.SetActive(true);
            this.userNameText.gameObject.SetActive(false);
            this.versionText.gameObject.SetActive(false);
            this.possibleButton.gameObject.SetActive(false);
        }
    }

    public void DisableEditButton(){
        if (GameManager.instance.buildData.isAdminBuild)
            return;

        this.editButton.GetComponent<Button>().interactable = false;
        this.editButton.GetComponentInChildren<Text>().color = new Color32(0, 0, 0, 150);
    }

    public void OnStartPossibleTest(){
        CloseMainMenu();
        GameManager.instance.isInPossibleCheck = true;
        GameManager.instance.sceneManager.StartOnlyPlayModeLevel(this.levelDropdown.options[this.levelDropdown.value].text);
    }

}
