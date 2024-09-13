using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineInputSimulator : InputManager, IPunObservable{

    public bool isSender = false;

    [System.NonSerialized]public int id = 0;

    private void Awake(){
        this.allowInput = true;
    }

    public void Update(){
        if (this.isSender){
            if (!this.allowInput)
                return;

            if (this.inputDeviceType == InputDeviceType.KEYBOARD)
                KeyboardUpdate();
            else
                ControllerUpdate();

            OnlineMultiplayerManager.instance.SendRPCToAllOthers("ReceiveInputStates", this.id, new bool[] { this.LEFT, this.LEFT_DOWN,
            this.RIGHT, this.RIGHT_DOWN, this.UP, this.UP_DOWN, this.DOWN, this.DOWN_DOWN, this.JUMP, this.JUMP_DOWN, this.RUN,
            this.USEPOWERUP, this.USEPOWERUP_ZR, this.USEMKITEMSLOT, this.SPIN, this.SPIN_DOWN, this.MENU, this.LEVELEDITOR_SWITCHMODE, this.LEVELEDITOR_CHANGE_VIEW});
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        throw new System.NotImplementedException();
    }

}
