using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryModeManager : MonoBehaviour{

    public static StoryModeManager instance;

    public StoryArea[] storyAreas;

    public DialogText[] testDialog;

    [Header("Link to Objects")]
    public GameObject dialogBoxParent;
    public Text dialogBoxText;
    public Text dialogBoxNameText;
    public Image dialogBoxImage;
    public GameObject dialogPopupInfo;

    /*States*/
    private bool isInDialog = false;

    [System.Serializable]
    public class StoryArea{

        public int id = 0;
        public GameObject areaParent;
        public TileManager.TilesetID backgroundId;

        public void Load(){
            this.areaParent.SetActive(true);
            TileManager.instance.LoadTilesetWithId(this.backgroundId);
        }

    }

    [System.Serializable]
    public struct DialogText{
        public string text;
        public string name;
        public Sprite image;

        public float defaultTextSpeed;
    }

    private void Awake(){
        StoryModeManager.instance = this;

        this.storyAreas[0].Load();
        BackgroundMusicManager.instance.editModeSource.volume = 0;
        BackgroundMusicManager.instance.editModeSource.enabled = false;
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.T))
            StartCoroutine(SmoothMuteBGM());
        if (Input.GetKeyDown(KeyCode.G))
            StartCoroutine(SmoothStartBGM());
    }

    public void StartDialogBox(DialogText[] dialogTexts){
        if (this.isInDialog)
            return;

        this.isInDialog = true;
        MenuManager.canOpenMenu = false;
        foreach (PlayerController player in GameManager.instance.sceneManager.players){
            player.SetCanMove(false);
            player.BreakReturn();
            player.PlayAnimation(player.currentPlayerSprites.stand[0], PlayerController.AnimationID.Stand);
            player.isFreeze = true;
        }

        this.dialogBoxText.text = string.Empty;

        this.dialogBoxImage.sprite = dialogTexts[0].image;
        this.dialogBoxNameText.text = dialogTexts[0].name;

        this.dialogBoxParent.gameObject.SetActive(true);
        this.dialogBoxParent.GetComponent<Animator>().Play(0);
        StartCoroutine(StartDialogBoxIE(dialogTexts));
    }

    private IEnumerator StartDialogBoxIE(DialogText[] dialogTexts){
        while (this.dialogBoxParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("DialogBoxPopUp"))
            yield return new WaitForSeconds(0);
        this.dialogBoxParent.GetComponent<Animator>().StopPlayback();

        foreach (DialogText dialogText in dialogTexts){
            string text = OptimzeDialogString(dialogText.text);
            this.dialogBoxText.text = string.Empty;
            this.dialogBoxNameText.text = dialogText.name;
            this.dialogBoxImage.sprite = dialogText.image;
            for (int i = 0; i <= text.Length; i++){
                this.dialogBoxText.text = text.Substring(0, i);
                yield return new WaitForSecondsRealtime(dialogText.defaultTextSpeed);
            }

            InputManager input = InputManager.instances[0];
            while (!input.MENU && !input.RUN && !input.LEVELEDITOR_SWITCHMODE){
                yield return new WaitForSecondsRealtime(0);
            }
        }

        this.dialogBoxParent.GetComponent<Animator>().Play("DialogBoxClose");
        yield return new WaitForSeconds(0.1f);

        while (this.dialogBoxParent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("DialogBoxClose"))
            yield return new WaitForSeconds(0);

        this.dialogBoxParent.SetActive(false);
        this.isInDialog = false;
        MenuManager.canOpenMenu = true;
        foreach (PlayerController player in GameManager.instance.sceneManager.players){
            player.SetCanMove(true);
            player.isFreeze = false;
        }
    }

    public string OptimzeDialogString(string str){
        if (str.Contains("%Player%")){
            if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.MARIO)
                str = str.Replace("%Player%", "Mario");
            else if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.LUIGI)
                str = str.Replace("%Player%", "Luigi");
            else if (GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.BLUETOAD | GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.GREENTOAD | GameManager.instance.sceneManager.players[0].playerType == PlayerController.PlayerType.REDTOAD)
                str = str.Replace("%Player%", "Toad");
        }

        return str;
    }

    public IEnumerator SmoothMuteBGM(){
        StopCoroutine("SmoothStartBGM");
        AudioSource source = BackgroundMusicManager.instance.playModeSource;
        while(source.volume > 0){
            source.volume = source.volume - 1 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        source.volume = 0;
    }

    public IEnumerator SmoothStartBGM(){
        StopCoroutine("SmoothMuteBGM");
        AudioSource source = BackgroundMusicManager.instance.playModeSource;
        while (source.volume < 1){
            source.volume = source.volume + 0.5f * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        source.volume = 1;
    }

}
