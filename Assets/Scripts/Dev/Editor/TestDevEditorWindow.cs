using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class TestDevEditorWindow : EditorWindow{

    private string levelPath = string.Empty;
    private string informations = string.Empty;
    private bool autoReload = false;
    private bool cheater = false;
    private bool infJump = false;

    private void Awake(){
        this.levelPath = Application.persistentDataPath + "/devTestLevel.lvl";
    }

    [MenuItem("UMM/TestDevWindow")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(TestDevEditorWindow));
    }

    private void Update(){
        if(this.autoReload)
            Repaint();
    }

    private void OnGUI(){
        if (!Application.isPlaying){
            GUILayout.Label("Please enter PlayMode!");
            return;
        }
        GUILayout.Label("Ingame Settings\n");
        DevGameManager.SHOWDEBUGINFORMATIONS = GUILayout.Toggle(DevGameManager.SHOWDEBUGINFORMATIONS, "ShowDebugInformationsIngame");
        GUILayout.Label("LevelSavePath");
        GameManager.LEVEL_PATH = GUILayout.TextField(GameManager.LEVEL_PATH);
        GUILayout.Label("\nTestLevelFunctions\n");
        GUILayout.Label("DevLevelPath");
        this.levelPath = GUILayout.TextField(this.levelPath);

        if (GUILayout.Button("SaveLevel"))
            LevelEditorManager.instance.SaveLevelAsFile(this.levelPath);
        if (GUILayout.Button("ClearCurrentEditorLevel"))
            LevelEditorManager.instance.DeleteCurrentEditorLevel();
        if (GUILayout.Button("LoadLevelInEditor"))
            LevelEditorManager.instance.LoadLevelInEditor(this.levelPath);
        if (GUILayout.Button("LoadDebugTileset"))
            TileManager.instance.LoadTilesetWithIntButton(27);
        if (GUILayout.Button("60FPS Limit"))
            Application.targetFrameRate = 60;
        if (GUILayout.Button("30FPS Limit"))
            Application.targetFrameRate = 30;
        if (GUILayout.Button("10FPS Limit"))
            Application.targetFrameRate = 10;
        if (GUILayout.Button("5FPS Limit"))
            Application.targetFrameRate = 5;
        if (GUILayout.Button("1FPS Limit"))
            Application.targetFrameRate = 1;
        if (GUILayout.Button("999FPS Limit"))
            Application.targetFrameRate = 999;
        if (GUILayout.Button("Dev Screenshot"))
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/devScreenshot.png");
        if (GUILayout.Button("SavePlayerPos")){
            PlayerPrefs.SetFloat("DebugPosX", GameManager.instance.sceneManager.players[0].transform.position.x);
            PlayerPrefs.SetFloat("DebugPosY", GameManager.instance.sceneManager.players[0].transform.position.y);
            PlayerPrefs.Save();
        }else if (GUILayout.Button("LoadPlayerPos")){
            GameManager.instance.sceneManager.players[0].transform.position = new Vector3(PlayerPrefs.GetFloat("DebugPosX"), PlayerPrefs.GetFloat("DebugPosY"), GameManager.instance.sceneManager.players[0].transform.position.z);
        }

        if (GUILayout.Button("ChangeArea")){
            if (GameManager.instance.sceneManager.currentArea == 0)
                GameManager.instance.sceneManager.ChageArea(1);
            else
                GameManager.instance.sceneManager.ChageArea(0);
        }

        if (GUILayout.Button("TestOnlineLevel"))
            GameManager.instance.GetComponent<OnlineLevelManager>().PlayLevel("Airship", "NULL");
        if (GUILayout.Button("LoadTestMod"))
            ModManager.LoadMod(@"C:\Users\marco\AppData\LocalLow\PandaHexCode\Custom Mario Maker\Mods\Test");
        if (GUILayout.Button("LoadTestCustomTheme")){
            TileManager.Tileset tileset = ModManager.GetTilesetFromCustomId("tjejt325&!");
            TileManager.instance.LoadTileset(tileset);
            TileManager.instance.LoadTilesetToMemory(tileset, LevelEditorManager.instance.currentArea);
        }
        DevGameManager.levelLoadDebug = GUILayout.Toggle(DevGameManager.levelLoadDebug, "LevelLoadDebug");

        GUILayout.Label("\nInformations\n");
        GUILayout.Label(this.informations +"\n");

        this.autoReload = GUILayout.Toggle(this.autoReload, "AutoReloadInformations");

        if (!this.autoReload && GUILayout.Button("ReloadInformations"))
            ReloadInformations();
        else if(this.autoReload)
            ReloadInformations();

        this.cheater = GUILayout.Toggle(this.cheater, "Cheater");
        if (this.cheater){
            GameManager.instance.sceneManager.players[0].canGetDamage = GUILayout.Toggle(GameManager.instance.sceneManager.players[0].canGetDamage, "CanGetDamage");
            this.infJump = GUILayout.Toggle(this.infJump, "InfJump");
            if (this.infJump && Input.GetKey(KeyCode.X))
                GameManager.instance.sceneManager.players[0].Jump();
        }

        GUILayout.Label("Dont forget backups!");
    }

    public void ReloadInformations(){
        this.informations = "CurrentScene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "\nCurrentStyle: " + TileManager.instance.currentStyleID + "\nCurrentTileset: " + TileManager.instance.currentTileset.id + "\nLoadedTilesets size: " + TileManager.instance.loadedTilesets.Length;
        foreach (TileManager.Tileset tilest in TileManager.instance.loadedTilesets){
            this.informations = this.informations + "\n     " + tilest.id;
        }
    }
}
