

using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineMultiplayerManager : MonoBehaviourPunCallbacks, IPunObservable{

    public string appId = "39f1899d-6ca9-4789-96c6-53c9d9d95a8d";

    [Header("Link to UI Objects")]
    public InputField roomNameInput;
    public Button connectLeaveButton;
    public Text playerListText;

    public static OnlineMultiplayerManager instance;

    //
    private string[] roomChars = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

    [System.NonSerialized]public bool isConnectedToMaster = false;
    [System.NonSerialized]public bool isInRoom = false;

    private void Awake(){
        if (Application.isEditor)
            GameManager.CAN_ONLINE = true;

        instance = this;

        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0;
        this.connectLeaveButton.GetComponentInChildren<Text>().text = "Join";

        if (gameObject.GetComponent<PhotonView>() == null){
            PhotonView photonView = gameObject.AddComponent<PhotonView>();
            photonView.ViewID = 1;
        }else{
            photonView.ViewID = 1;
        }
    }

    public bool CheckIfCanCreateOrJoin(){
        if (!GameManager.CAN_ONLINE | GameManager.instance.sceneManager.players.Count > 1 | this.isInRoom)
            return false;

        if (!this.isConnectedToMaster | !PhotonNetwork.IsConnected){
            ConnectToPhoton();
            return false;
        }

        return true;
    }

    public void ConnectToPhoton(){
        Debug.Log("Try to connect to photon...");
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = this.appId;
        PhotonNetwork.NickName = GameManager.instance.buildData.USERNAME;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom(){
        if (!CheckIfCanCreateOrJoin())
            return;

        string code = GenerateRandomRoomCode();
        this.roomNameInput.SetTextWithoutNotify(code);
        this.roomNameInput.readOnly = true;
        this.isInRoom = true;
        PhotonNetwork.CreateRoom(code);
        Debug.Log("Created room with code " + code + "!");
    }

    public string GenerateRandomRoomCode(){
        int radomint1 = Random.Range(0, roomChars.Length); int radomint2 = Random.Range(0, roomChars.Length); int radomint3 = Random.Range(0, roomChars.Length); int radomint4 = Random.Range(0, roomChars.Length);
        string code = roomChars[radomint1] + roomChars[radomint2] + roomChars[radomint3] + roomChars[radomint4];
        return code;
    }

    public void JoinRoom(){
        if (!CheckIfCanCreateOrJoin()){
            PhotonNetwork.LeaveRoom();
            return;
        }

        PhotonNetwork.JoinRoom(this.roomNameInput.text);
        this.isInRoom = true;
    }

    public void ResetRoomNameInput(){
        this.isInRoom = false;
        this.roomNameInput.SetTextWithoutNotify("");
        this.roomNameInput.readOnly = false;
        this.connectLeaveButton.GetComponentInChildren<Text>().text = "Join";
        this.playerListText.text = string.Empty;
        PhotonNetwork.LeaveRoom();
    }

    public override void OnConnectedToMaster(){
        this.isConnectedToMaster = true;
        Debug.Log("Suscefully connected to Master!");
    }

    public override void OnJoinedRoom(){
        Destroy(GameManager.instance.sceneManager.players[0].input);
        GameManager.instance.sceneManager.players[0].input = GameManager.instance.sceneManager.players[0].gameObject.AddComponent<OnlineInputSimulator>();
        GameManager.instance.sceneManager.players[0].GetComponent<OnlineInputSimulator>().isSender = true;
        InputManager.instances[0] = GameManager.instance.sceneManager.players[0].input;

        this.connectLeaveButton.GetComponentInChildren<Text>().text = "Leave";
        if(!PhotonNetwork.LocalPlayer.IsMasterClient)
            this.playerListText.text = string.Empty;
    }

    public override void OnCreatedRoom(){
        this.connectLeaveButton.GetComponentInChildren<Text>().text = "Leave";
        this.playerListText.text = PhotonNetwork.NickName;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer){
        this.playerListText.text = this.playerListText.text + "\n" + newPlayer.NickName;
        SendRPCToPlayer(newPlayer, "ChangePlayerList", this.playerListText.text);
        PlayerController p = GameManager.instance.sceneManager.InitNewPlayer(true);
        Destroy(p.input);
        p.input = p.gameObject.AddComponent<OnlineInputSimulator>();
        p.input.RegisterInstance(1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        this.playerListText.text = this.playerListText.text.Replace("\n" + otherPlayer.NickName, "");
    }

    [PunRPC]
    private void ChangePlayerList(string list){
        this.playerListText.text = list;
    }

    public void SendRPC(string function, params object[] parameters){
        photonView.RPC(function, RpcTarget.All, parameters);
    }

    public void SendRPCToAllOthers(string function, params object[] parameters){
        photonView.RPC(function, RpcTarget.Others, parameters);
    }

    public void SendRPCToPlayer(Player targetPlayer, string function, params object[] parameters){
        photonView.RPC(function, targetPlayer, parameters);
    }

    [PunRPC]
    public void ReceiveInputStates(int id, bool[] states){
        OnlineInputSimulator input = InputManager.instances[1].GetComponent<OnlineInputSimulator>();
        input.LEFT = states[0];
        input.LEFT_DOWN = states[1];
        input.RIGHT = states[2];
        input.RIGHT_DOWN = states[3];
        input.UP = states[4];
        input.UP_DOWN = states[5];
        input.DOWN = states[6];
        input.DOWN_DOWN = states[7];
        input.JUMP = states[8];
        input.JUMP_DOWN = states[9];
        input.RUN = states[10];
        input.USEPOWERUP = states[11];
        input.USEPOWERUP_ZR = states[12];
        input.USEMKITEMSLOT = states[13];
        input.SPIN = states[14];
        input.SPIN_DOWN = states[15];
        input.MENU = states[16];
        input.LEVELEDITOR_SWITCHMODE = states[17];
        input.LEVELEDITOR_CHANGE_VIEW = states[18];
    }

    public override void OnJoinRoomFailed(short returnCode, string message){
        ResetRoomNameInput();
        Debug.LogError("Failed to join Room |" + returnCode + "|" + message + "!");
    }

    public override void OnJoinRandomFailed(short returnCode, string message){
        ResetRoomNameInput();
        Debug.LogError("Failed to join random Room |" + returnCode + "|" + message + "!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message){
        ResetRoomNameInput();
        Debug.LogError("Failed to create Room |" + returnCode + "|" + message + "!");
    }

    public override void OnLeftRoom(){
        ResetRoomNameInput();
        Debug.Log("Left Room.");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        throw new System.NotImplementedException();
    }

}
