using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class NewTrailerHelper : EditorWindow{

    public string[] levelPaths;

    private bool isEditFiles = false;

    [MenuItem("UMM/NewTrailerHelper")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(NewTrailerHelper));
    }

    private void OnGUI(){
        GUILayout.Label("CMM - New Trailer Helper");

        this.isEditFiles = GUILayout.Toggle(this.isEditFiles, "Edit");
 
        if (this.isEditFiles){
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("levelPaths");

            EditorGUILayout.PropertyField(stringsProperty, true);
            so.ApplyModifiedProperties();
        }

        if (!Application.isPlaying){
            GUILayout.Label("\nPlease enter PlayMode!");
            return;
        }

        if (GUILayout.Button("Init"))
            GameManager.instance.gameObject.AddComponent<LevelRuntimeChanger>().newTrailerHelper = this;

    }
}

public class LevelRuntimeChanger : MonoBehaviour{

    public NewTrailerHelper newTrailerHelper;

    private void Update(){
        if (Input.GetKeyDown(KeyCode.U)){
            GameManager.StopTimeScale();
            int cur = -1;
            for (int i = 0; i < this.newTrailerHelper.levelPaths.Length; i++){
                if (this.newTrailerHelper.levelPaths[i].Equals(GameManager.instance.currentLevelPath)){
                    cur = i;
                    break;
                }
            }

            if (cur == -1){
                Debug.LogError("CurrentLevelPath is not in trailerLevelPaths!");
                cur = -1;
            }

            StartCoroutine(LoadLevelIE(this.newTrailerHelper.levelPaths[cur + 1]));
        }
    }

    private IEnumerator LoadLevelIE(string path){
        GameManager.instance.ClearCurrentLevel();
        GameManager.instance.sceneManager.blackScreen.SetActive(true);

        yield return new WaitForSecondsRealtime(0.3f);

        GameManager.instance.sceneManager.blackScreen.SetActive(false);

        Vector3 playerPos = GameManager.instance.sceneManager.players[0].transform.position;
        PlayerController.Powerup playerPowerup = GameManager.instance.sceneManager.players[0].GetPowerup();
        GameManager.instance.LoadLevelFile(path);
        GameManager.instance.sceneManager.players[0].transform.position = playerPos;
        GameManager.instance.sceneManager.players[0].SetPowerup(playerPowerup, true, true);
        GameManager.ResumeTimeScale();       
    }
}