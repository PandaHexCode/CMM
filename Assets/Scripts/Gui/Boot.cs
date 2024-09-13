using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading;
using UMM.FTP;

public class Boot : MonoBehaviour{/*Titlescreen, Handle Intro Animations, Handle Credits,Handle Server Checks etc*/

    public BuildData buildData;

    public GameObject creditsScreenParent;
    public GameObject serverCheckScreen;

    public GameObject scroolBackground;
    public TextMeshProUGUI serverCheckStateText;
    public GameObject understandButton;
    public TMP_InputField userNameInput;

    private void Awake(){
        StartCreditsAnimation();
    }

    public void StartCreditsAnimation(){
        FTPManager.LL();
        this.creditsScreenParent.SetActive(true);
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().Play("Credits");
    }

    public void OpenTitleScreen(){
        if (this.serverState == ServerState.NO_USERNAME){
            if(this.userNameInput.text.Length < 3){
                this.serverCheckStateText.text = "This UserName is to short!";
                return;
            }else if(this.userNameInput.tag.Length > 15){
                this.serverCheckStateText.text = "This UserName is to long!";
                return;
            }

            if (FTPManager.CheckIfFileExists("htdocs/Users/" + this.userNameInput.text) | this.userNameInput.text.Contains(":")){
                this.serverCheckStateText.text = "This UserName already exits or is not allowed!";
                return;
            }else{
                string userName = this.userNameInput.text;
                GameManager.SaveFile(Application.persistentDataPath + "\\" + userName, userName);
                FTPManager.Upload(Application.persistentDataPath + "\\" + userName, "htdocs/Users/" + userName);
                PlayerPrefs.SetString("USERNAME4", userName);
                PlayerPrefs.Save();
                this.buildData.USERNAME = userName;
            }
        }

        FTPManager.BB();
        GameManager.LOAD_AGRUMENTS = "--mainmenu";
        LoadScene("GameScene");
        return;
    }

    public void OnFinishDailyIntroBegin(){/*TODO*/
        OnFinishDailyIntro();
        //Debug.Log(System.DateTime.Now.DayOfWeek);
        //this.GetComponent<Animator>().Play("DailyIntro" + System.DateTime.Now.DayOfWeek + "1");
    }

    public void OnFinishDailyIntro(){
        this.GetComponent<Animator>().Play("DailyIntroEnd"); 
        this.serverCheckScreen.SetActive(true);

        StartCoroutine(ServerCheckLoop());
    }

    private IEnumerator ServerCheckLoop(){
        GameManager.CAN_ONLINE = false;
        this.serverState = ServerState.NULL;
        this.serverCheckStateText.text = "Try connecting to Server...";
        Thread thread = new Thread(ServerCheck);
        thread.IsBackground = true;
        thread.Start();

        while(this.serverState == ServerState.NULL){
            yield return new WaitForSeconds(0);
        }

        switch (this.serverState){
            case ServerState.NO_USERNAME:
            case ServerState.PERFECT:
                if (GameManager.SERVER_VERSION > buildData.VERSION_FLOAT){
                    Debug.Log("Outdated Version detected!");
                    this.serverCheckStateText.text = "Your game version is out of date, there is/are already new update(s).\nPlease note if you dont update you can't use online features and you don't have new features or you find bugs that are already fixed.";
                    yield return new WaitForSecondsRealtime(2);
                    this.understandButton.SetActive(true);
                    Application.OpenURL("https://discord.gg/qvXQAMYXfY");
                }else{
                    GameManager.CAN_ONLINE = true;
                    this.serverCheckStateText.text = "You have the newest version!";
                    this.serverCheckStateText.GetComponent<RectTransform>().transform.localPosition = new Vector3(-478f, -31, 0);
                    yield return new WaitForSecondsRealtime(1);


                    if (PlayerPrefs.GetString("USERNAME4", "NULL") == "NULL")
                        this.serverState = ServerState.NO_USERNAME;
                    else if (FTPManager.CheckIfFileExists("htdocs/BannedUsers/" + PlayerPrefs.GetString("USERNAME4")))
                        this.serverState = ServerState.BANNED_USER;

                    if (this.serverState == ServerState.NO_USERNAME){
                        this.serverCheckStateText.text = "Please enter an Username!\nYou can't change it later.";
                        this.understandButton.SetActive(true);
                        this.understandButton.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
                        this.userNameInput.gameObject.SetActive(true);
                    }else if (this.serverState == ServerState.BANNED_USER){
                        GameManager.CAN_ONLINE = false;
                        this.buildData.USERNAME = PlayerPrefs.GetString("USERNAME4");
                        this.serverCheckStateText.text = "You are banned!";
                    }else{
                        this.buildData.USERNAME = PlayerPrefs.GetString("USERNAME4");
                        OpenTitleScreen();
                    }
                }
                break;

            case ServerState.ERROR_CANT_CONNECT_TO_SERVER:
                this.serverCheckStateText.text = "Can't connect to the server, can't check for updates.\nIf you still want to continue playing, note that you cannot use any online features and maybe you are running an old version.\nServerip: " + BuildData.hU;
                yield return new WaitForSecondsRealtime(2);
                this.understandButton.SetActive(true);
                break;
        }
    }

    private ServerState serverState = ServerState.NULL;
    private enum ServerState { NULL = -1, ERROR_CANT_CONNECT_TO_SERVER = 0, PERFECT = 1, NO_USERNAME = 2, BANNED_USER = 3,};
    public void ServerCheck(){
        GameManager.CAN_ONLINE = false;
        try{
            string ver = FTPManager.ReadFile("htdocs/currentVersion.txt");
            if (1.4f.ToString().Contains(","))
                ver = ver.Replace(".", ",");
            else
                ver = ver.Replace(",", ".");

            float serverVersion = GameManager.StringToFloat(ver);
            GameManager.SERVER_VERSION = serverVersion;
            this.serverState = ServerState.PERFECT;
        }
        catch (Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
            this.serverState = ServerState.ERROR_CANT_CONNECT_TO_SERVER;
        }
    }

    public void LoadScene(string name){
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }

}
