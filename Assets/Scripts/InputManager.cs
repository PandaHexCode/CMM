using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class InputManager : MonoBehaviour{

    public InputDeviceType inputDeviceType = InputDeviceType.KEYBOARD;
    public enum InputDeviceType { KEYBOARD = 0, CONTROLLER = 1}
    
    /*KeyCode values are only for Keyboard*/
    public bool allowInput = true;
    public bool LEFT = false; private KeyCode left;
    public bool LEFT_DOWN = false;
    public bool RIGHT = false; private KeyCode right;
    public bool RIGHT_DOWN = false;
    public bool UP = false; private KeyCode up;
    public bool UP_DOWN = false;
    public bool DOWN = false; private KeyCode down;
    public bool DOWN_DOWN = false;
    public bool JUMP = false; private KeyCode jump;
    public bool JUMP_DOWN = false;
    public bool RUN = false; private KeyCode run;
    public bool USEPOWERUP = false; private KeyCode usepowerup;
    public bool USEPOWERUP_ZR = false; private KeyCode usepowerup_zr;
    public bool USEMKITEMSLOT = false; private KeyCode usemkitemslot;
    public bool SPIN = false; private KeyCode spin;
    public bool SPIN_DOWN = false;
    public bool MENU = false; private KeyCode menu;
    public bool LEVELEDITOR_SWITCHMODE = false;private KeyCode leveleditor_switchmode;
    public bool LEVELEDITOR_CHANGE_VIEW = false;private KeyCode leveleditor_change_view;

    /*Controller Values*/
    public float CONTROLLER_DEADZONE;

    private PlayerIndex controllerPlayerIndex;
    [System.NonSerialized]public GamePadState controllerState;
    private GamePadButtons lastButtons;
    private GamePadThumbSticks.StickValue lastStickValues;
    private GamePadDPad lastDpad;

    public static InputManager[] instances = new InputManager[4];

    public void RegisterInstance(int playerNumber){
        instances[playerNumber] = this;
        Debug.Log("Input instance (" + playerNumber + ") has registied!");
    }

    private void Awake(){
        this.allowInput = true;
    }

    private void Update(){
        if (!this.allowInput)
            return;

        if (this.inputDeviceType == InputDeviceType.KEYBOARD)
            KeyboardUpdate();
        else
            ControllerUpdate();
    }

    private void LateUpdate(){
        this.lastButtons = this.controllerState.Buttons;
        this.lastDpad = this.controllerState.DPad;
        this.lastStickValues = this.controllerState.ThumbSticks.Left;
    }

    /*Keyboard*/
    public void LoadKeyCodes(){
        SettingsManager settings = SettingsManager.instance;
        left = GetKeyCode(settings.GetOption("Left").ToString());
        right = GetKeyCode(settings.GetOption("Right").ToString());
        up = GetKeyCode(settings.GetOption("Up").ToString());
        down = GetKeyCode(settings.GetOption("Down").ToString());
        jump = GetKeyCode(settings.GetOption("Jump").ToString());
        run = GetKeyCode(settings.GetOption("Run").ToString());
        usepowerup = GetKeyCode(settings.GetOption("Run").ToString());
        usepowerup_zr = GetKeyCode(settings.GetOption("UsePowerup_ZR").ToString());
        usemkitemslot = GetKeyCode(settings.GetOption("ItemSlot").ToString());
        menu = GetKeyCode(settings.GetOption("Menu").ToString());
        leveleditor_switchmode = GetKeyCode(settings.GetOption("Editor_SwitchMode").ToString());
        leveleditor_change_view = GetKeyCode(settings.GetOption("Editor_ChangeView").ToString());
        spin = GetKeyCode(settings.GetOption("Spin").ToString());
    }

    public void LoadDefaultKeyCodes(){
        left = KeyCode.LeftArrow;
        right = KeyCode.RightArrow;
        up = KeyCode.UpArrow;
        down = KeyCode.DownArrow;
        jump = KeyCode.X;
        run = KeyCode.A;
        usepowerup = KeyCode.A;
        usepowerup_zr = KeyCode.V;
        usemkitemslot = KeyCode.B;
        menu = KeyCode.Escape;
        leveleditor_switchmode = KeyCode.T;
        leveleditor_change_view = KeyCode.R;
        spin = KeyCode.R;
        SaveKeyCodes();
    }

    public void SaveKeyCodes(){
        SettingsManager settings = SettingsManager.instance;
        settings.SetOption("Left", this.left.ToString());
        settings.SetOption("Right", this.right.ToString());
        settings.SetOption("Up", this.up.ToString());
        settings.SetOption("Down", this.down.ToString());
        settings.SetOption("Jump", this.jump.ToString());
        settings.SetOption("Run", this.run.ToString());
        settings.SetOption("UsePowerup_ZR", this.usepowerup_zr.ToString());
        settings.SetOption("ItemSlot", this.usemkitemslot.ToString());
        settings.SetOption("Menu", this.menu.ToString());
        settings.SetOption("Editor_SwitchMode", this.leveleditor_switchmode.ToString());
        settings.SetOption("Editor_ChangeView", this.leveleditor_change_view.ToString());
        settings.SetOption("Spin", this.spin.ToString());

        foreach (InputManager inputManager in InputManager.instances){
            if (inputManager != null)
                inputManager.LoadKeyCodes();
        }
    }

    public virtual void KeyboardUpdate(){
        this.LEFT = Input.GetKey(left);
        this.LEFT_DOWN = Input.GetKeyDown(left);
        this.RIGHT = Input.GetKey(right);
        this.RIGHT_DOWN = Input.GetKeyDown(right);
        this.UP = Input.GetKey(up);
        this.UP_DOWN = Input.GetKeyDown(up) | Input.GetKeyDown(KeyCode.W);
        this.DOWN = Input.GetKey(down);
        this.DOWN_DOWN = Input.GetKeyDown(down) | Input.GetKeyDown(KeyCode.S);
        this.JUMP = Input.GetKey(jump);
        this.JUMP_DOWN = Input.GetKeyDown(jump);
        this.RUN = Input.GetKey(run);
        this.USEPOWERUP = Input.GetKeyDown(usepowerup);
        this.USEPOWERUP_ZR = Input.GetKeyDown(usepowerup_zr);
        this.USEMKITEMSLOT = Input.GetKeyDown(usemkitemslot);
        this.SPIN = Input.GetKey(spin);
        this.SPIN_DOWN = Input.GetKeyDown(spin);
        this.MENU = Input.GetKeyDown(menu) | Input.GetKeyDown(KeyCode.Tab);
        this.LEVELEDITOR_SWITCHMODE = Input.GetKeyDown(leveleditor_switchmode);
        this.LEVELEDITOR_CHANGE_VIEW = Input.GetKeyDown(leveleditor_change_view);
    }

    public static KeyCode GetKeyCode(string st){
        foreach(KeyCode key in Enum.GetValues(typeof(KeyCode))){
            if (key == KeyCode.Mouse0)
                continue;

            if (key.ToString().Equals(st))
                return key;
        }

        return KeyCode.None;
    }

    /*Controller*/
    public void ControllerUpdate(){
        this.controllerState = GamePad.GetState(controllerPlayerIndex);

        this.UP = IsButtonPressed(this.controllerState.DPad.Up) | this.controllerState.ThumbSticks.Left.Y > 0 + CONTROLLER_DEADZONE;
        this.UP_DOWN = IsButtonDown(this.controllerState.DPad.Up, this.lastDpad.Up) | (this.lastStickValues.Y < 0 - CONTROLLER_DEADZONE && this.controllerState.ThumbSticks.Left.Y > 0 + CONTROLLER_DEADZONE);
        
        this.DOWN = IsButtonPressed(this.controllerState.DPad.Down) | this.controllerState.ThumbSticks.Left.Y < 0 - CONTROLLER_DEADZONE;
        this.DOWN_DOWN = IsButtonDown(this.controllerState.DPad.Down, this.lastDpad.Down) | (this.lastStickValues.Y > 0 + CONTROLLER_DEADZONE && this.controllerState.ThumbSticks.Left.Y < 0 - CONTROLLER_DEADZONE);

        this.LEFT = IsButtonPressed(this.controllerState.DPad.Left) | this.controllerState.ThumbSticks.Left.X < 0 - CONTROLLER_DEADZONE;
        this.LEFT_DOWN = IsButtonDown(this.controllerState.DPad.Left, this.lastDpad.Left) | (this.lastStickValues.X > 0 + CONTROLLER_DEADZONE && this.controllerState.ThumbSticks.Left.X < 0 - CONTROLLER_DEADZONE);
        
        this.RIGHT = IsButtonPressed(this.controllerState.DPad.Right) | this.controllerState.ThumbSticks.Left.X > 0 + CONTROLLER_DEADZONE;
        this.RIGHT_DOWN = IsButtonDown(this.controllerState.DPad.Right, this.lastDpad.Right) | (this.lastStickValues.X < 0 - CONTROLLER_DEADZONE && this.controllerState.ThumbSticks.Left.X > 0 + CONTROLLER_DEADZONE);

        this.JUMP = IsButtonPressed(this.controllerState.Buttons.A) | IsButtonPressed(this.controllerState.Buttons.B);
        this.JUMP_DOWN = IsButtonDown(this.controllerState.Buttons.A, this.lastButtons.A) | IsButtonDown(this.controllerState.Buttons.B, this.lastButtons.B);
        this.RUN = IsButtonPressed(this.controllerState.Buttons.X) | IsButtonPressed(this.controllerState.Buttons.Y);
        this.USEPOWERUP = IsButtonDown(this.controllerState.Buttons.X, this.lastButtons.X) | IsButtonDown(this.controllerState.Buttons.Y, this.lastButtons.Y);
        this.USEPOWERUP_ZR = this.controllerState.Triggers.Left > 0 | this.controllerState.Triggers.Right > 0;
        this.USEMKITEMSLOT = IsButtonDown(this.controllerState.Buttons.RightShoulder, this.lastButtons.RightShoulder);
        this.SPIN = IsButtonPressed(this.controllerState.Buttons.RightShoulder) | IsButtonPressed(this.controllerState.Buttons.LeftShoulder);
        this.SPIN_DOWN = IsButtonDown(this.controllerState.Buttons.RightShoulder, this.lastButtons.RightShoulder) | IsButtonDown(this.controllerState.Buttons.LeftShoulder, this.lastButtons.LeftShoulder);
        this.MENU = IsButtonDown(this.controllerState.Buttons.Start, this.lastButtons.Start);
        this.LEVELEDITOR_SWITCHMODE = Input.GetKeyDown(leveleditor_switchmode);
        this.LEVELEDITOR_CHANGE_VIEW = Input.GetKeyDown(leveleditor_change_view);
    }

    public Coroutine ConnectController(MenuManager menuManager = null, bool canCancel = true){
        return StartCoroutine(ConnectControllerLoopIE(menuManager, canCancel));
    }

    private IEnumerator ConnectControllerLoopIE(MenuManager menuManager, bool canCancel = true){
        bool isConnect = false;
        bool isCancel = false;

        Debug.Log("Please press A or B on your Controller!");
        while (!isConnect && !isCancel){
            for (int i = 0; i < 4; ++i){
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected && testState.Buttons.A == ButtonState.Pressed | testState.Buttons.B == ButtonState.Pressed){
                    bool alreadyInUse = false; 
                    foreach(InputManager input in instances){
                        if (input != null && input.IsControllerConnected() && input.controllerPlayerIndex == testPlayerIndex && input.inputDeviceType == InputDeviceType.CONTROLLER)
                            alreadyInUse = true;
                    }

                    if (!alreadyInUse){
                        this.controllerPlayerIndex = testPlayerIndex;
                        this.controllerState = testState;
                        Debug.Log("Controller connected: " + this.controllerPlayerIndex);
                        isConnect = true;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) && canCancel)
                isCancel = true;
            yield return new WaitForSeconds(0);
        }

        if (isConnect){
            this.inputDeviceType = InputDeviceType.CONTROLLER;
            if(this == instances[0])
                SettingsManager.instance.SetOption("InputDeviceType", 1);
        }else
            Debug.Log("Controller connection is canceld!");


        if (menuManager != null){
            menuManager.canClose = true;
            menuManager.OpenSubMenu(menuManager.subMenus[2]);
            menuManager.LoadOptionButtonsTexts();
        }
    }

    public bool IsButtonPressed(ButtonState button){
        if (button == ButtonState.Pressed)
            return true;
        else
            return false;
    }

    public bool IsButtonDown(ButtonState button, ButtonState lastButton){
        if (button == ButtonState.Pressed && lastButton == ButtonState.Released)
            return true;
        else
            return false;
    }

    public static string KeyCodeToString(KeyCode keyCode){
        return keyCode.ToString().Replace("KeyCode.", "");
    }

    public PlayerIndex GetControllerPlayerIndex(){
        return this.controllerPlayerIndex;
    }

    public void SetControllerPlayerIndex(PlayerIndex index){
        this.controllerPlayerIndex = index;
    }

    public void SetControllerGamePadState(GamePadState state){
        this.controllerState = state;
    }

    public bool IsControllerConnected(){
        return this.controllerState.IsConnected;
    }

}
