using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour{
    [System.NonSerialized] public SaveValue[] options = { new SaveValue("InputDeviceType", (int)InputManager.InputDeviceType.KEYBOARD), new SaveValue("SpriteShadow", (bool)true), new SaveValue("FullScreen", (bool)true), new SaveValue("PlayerType", (int)PlayerController.PlayerType.MARIO), new SaveValue("ScaleMethod", (int)PlayerCamera.ScaleMethod.Aspect_Ratio), new SaveValue("VSync", (bool)true),
     new SaveValue("Left", KeyCode.LeftArrow.ToString()), new SaveValue("Right", KeyCode.RightArrow.ToString()), 
     new SaveValue("Up", KeyCode.UpArrow.ToString()), new SaveValue("Down", KeyCode.DownArrow.ToString()),  new SaveValue("Jump", KeyCode.X.ToString()),
     new SaveValue("Run", KeyCode.A.ToString()),  new SaveValue("UsePowerup_ZR", KeyCode.V.ToString()), new SaveValue("ItemSlot", KeyCode.B.ToString()),
     new SaveValue("Menu", KeyCode.Escape.ToString()), new SaveValue("Editor_SwitchMode", KeyCode.T.ToString()), new SaveValue("Editor_ChangeView", KeyCode.R.ToString()),
     new SaveValue("Spin", KeyCode.R.ToString()),
     new SaveValue("MusicVolume", (float)0.03f), new SaveValue("SoundsVolume", (float)3.27f)};

    [Header("Link to Objects for settings")]
    public Material spriteMaterial;
    public Shader shadowShader;
    public Material defaultShaderMaterial;/*Material for get the Shader because we cant link the default shader*/
    public UnityEngine.Audio.AudioMixerGroup soundsMixerGroup;
    public UnityEngine.Audio.AudioMixerGroup musicMixerGroup;

    public static SettingsManager instance = null;

    private string optionsFilePath = string.Empty;

    [System.Serializable]
    public class SaveValue{
        public string name;
        public object value;

        public SaveValue(string name, object value){
            this.name = name;
            this.value = value;
        }
    }

    private void Awake(){
        if (instance != null)
            Destroy(this);
        else{
            this.optionsFilePath = Application.persistentDataPath + "\\options.dat";
            instance = this;
            LoadOptionsFile();
        }
    }

    public void SaveFile(string filePath, SaveValue[] saveValues){
        string content = string.Empty;
        foreach(SaveValue option in saveValues){
            content = content + option.name + ":" + option.value + "\n";
        }
        GameManager.SaveFile(filePath, content);
    }

    public void LoadOptionsFile() {
        if (File.Exists(this.optionsFilePath)) {
            string[] lines = GameManager.GetFileIn(this.optionsFilePath).Split('\n');

            for (int i = 0; i < lines.Length; i++) {
                lines[i] = lines[i].Replace('\n', ' ');
                if (lines[i] != string.Empty) {
                    string[] line = lines[i].Split(':');
                    SetOption(line[0], line[1]);
                }
            }
        }

        /*Set the values*/
        if ((InputManager.InputDeviceType)GetOption("InputDeviceType") == InputManager.InputDeviceType.KEYBOARD)
            InputManager.instances[0].inputDeviceType = InputManager.InputDeviceType.KEYBOARD;
        else
            InputManager.instances[0].ConnectController();

        this.SetSpriteShadow((bool)GetOption("SpriteShadow"));
        this.GetComponentInParent<PlayerCamera>().ChangeScaleMethod((PlayerCamera.ScaleMethod)GetOption("ScaleMethod"));
        this.GetComponentInParent<PlayerCamera>().SetVSync((bool)GetOption("VSync"));
        UpdateAudioMixers();
        foreach (InputManager inputManager in InputManager.instances){
            if(inputManager != null)
                inputManager.LoadKeyCodes();
        }

        if (LevelEditorManager.instance != null && LevelEditorManager.instance.helperText != null){
            LevelEditorManager.instance.helperText.text = LevelEditorManager.instance.helperText.text.Replace("%Del%", "E");
            LevelEditorManager.instance.helperText.text = LevelEditorManager.instance.helperText.text.Replace("%Hide%", SettingsManager.instance.GetOption("Editor_ChangeView").ToString());
        }
    }

    public void UpdateAudioMixers(){
        GetComponent<MenuManager>().musicSlider.SetValueWithoutNotify((float)GetOption("MusicVolume"));
        GetComponent<MenuManager>().soundsSlider.SetValueWithoutNotify((float)GetOption("SoundsVolume"));
        this.musicMixerGroup.audioMixer.SetFloat("MusicVolume", (float)GetOption("MusicVolume"));
        this.musicMixerGroup.audioMixer.SetFloat("MusicVolume2", (float)GetOption("MusicVolume"));
        this.soundsMixerGroup.audioMixer.SetFloat("SoundsVolume", (float)GetOption("SoundsVolume"));
    }

    public void SetOption(string optionName, object value){
        SaveValue[] saveValues = this.SetValue(this.options, optionName, value);
        if (saveValues != null)
            this.options = saveValues;
    }

    private SaveValue[] SetValue(SaveValue[] values,string valueName, object value){
        for (int i = 0; i < values.Length; i++){
            SaveValue option = values[i];
            if (option.name.Equals(valueName)){
                if (value.GetType().Equals(typeof(string))){
                    if (option.value.GetType().Equals(typeof(float)))
                        value = GameManager.StringToFloat((string)value);
                    else if (option.value.GetType().Equals(typeof(int)))
                        value = GameManager.StringToInt((string)value);
                    else if (option.value.GetType().Equals(typeof(bool))){
                        string var = (string)value;
                        if (var.Equals("true", System.StringComparison.OrdinalIgnoreCase))
                            value = (bool)true;
                        else
                            value = (bool)false;
                    }
                }

                values[i].value = value;
                return values;
            }
        }

        return null;
    }

    public object GetOption(string optionName){
        return GetValue(this.options, optionName);
    }

    private object GetValue(SaveValue[] values ,string valueName){
        foreach (SaveValue option in values){
            if (option.name == valueName)
                return option.value;
        }

        return null;
    }

    private void OnApplicationQuit(){
        this.SaveFile(this.optionsFilePath, this.options);
    }

    private void OnDestroy(){
        this.SaveFile(this.optionsFilePath, this.options);
    }

    /**/
    public void SetSpriteShadow(bool state){
        if (state)
            this.spriteMaterial.shader = this.shadowShader;
        else
            this.spriteMaterial.shader = this.defaultShaderMaterial.shader;

        SetOption("SpriteShadow", (bool)state);
    }
}
