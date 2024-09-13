using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;
using UMM.BlockField;
using UnityEngine.UI;
using TMPro;
using System;

public class RandomizerManager : MonoBehaviour{

    public Slider airProbSlider;
    public Slider groundProbSlider;
    public Transform inLevelParent;

    public Vector3 newStartPos;

    public bool isRandomized = false;

    private string lvlPath;

    private void Awake(){
        this.lvlPath = Application.persistentDataPath + "\\tempRandomLevel.lvl";
    }

    public void GenerateFromMenu(){
        if (LevelEditorManager.isLevelEditor)
            GenerateRandomLevel(this.airProbSlider.value, this.groundProbSlider.value);
        else
            GameManager.instance.LoadEditorScene("--randomize" + this.airProbSlider.value + "|" + this.groundProbSlider.value + "|" + (int)TileManager.instance.currentStyleID + "|" + (int)TileManager.instance.currentTileset.id);
    }

    private void GenerateRandomLevel(float airProb, float groundProb){
        try{
            GameManager.instance.sceneManager.levelStartScreen.SetActive(true);
            GetComponent<MenuManager>().CloseMainMenu();
        }catch(Exception e){
            
        }
        LevelEditorManager levelEditor = LevelEditorManager.instance;
        levelEditor.DeleteCurrentEditorLevel(false);
        levelEditor.isInEditorLevelLoad = true;
        BlockDataManager blockDataManager = GameManager.instance.blockDataManager;

        
        foreach(BlockField blockField in levelEditor.blockFieldManager.blockFields){
            try{
                float airP = UnityEngine.Random.Range(0, 100);
            if (airProb < airP){
                float groundP = UnityEngine.Random.Range(0, 100);
                int id = 0;

                if (groundProb < groundP)
                    id = UnityEngine.Random.Range(0, blockDataManager.blockDatas.Length - 1);

                BlockData blockData = blockDataManager.blockDatas[id];
                GameObject g = null;
                if (blockData.canSpawnInRandomizer && !blockData.dontDisplayButtonInEditor && blockData.needSize == Vector2.zero)
                      g = levelEditor.PlaceBlock((BlockID)id, blockField, 0);
                    if ((BlockID)id == BlockID.QUESTION_BLOCK) {
                        int id2 = UnityEngine.Random.Range(0, blockDataManager.blockDatas.Length - 1);
                            if (blockDataManager.blockDatas[id2].canSpawnInRandomizer)
                                g.GetComponent<LevelEditorItemBlock>().SetContentBlock((BlockID)id2);
                     }
                }
            }catch (Exception e){
                Debug.Log(e.Message + "|" + e.StackTrace);
            }
        }

        levelEditor.isInEditorLevelLoad = false;
        StartCoroutine(RandomizerPlayIE());
    }

    public IEnumerator RandomizerPlayIE(){
        GroundTileConnector.SetAllWithoutPos();
        yield return new WaitForSeconds(0.05f);
        GroundTileConnector.SetAllWithoutPos();
        yield return new WaitForSeconds(0.05f);
        LevelEditorManager.instance.SaveLevelAsFile(this.lvlPath);
        GameManager.instance.sceneManager.StartOnlyPlayModeLevel(this.lvlPath, true, "Randomized Level");
        yield return new WaitForSeconds(0.05f);
        this.inLevelParent.gameObject.SetActive(true);
        TextMeshPro counter = this.inLevelParent.GetChild(0).GetComponent<TextMeshPro>();
        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            p.enabled = false;
            p.GetComponent<Rigidbody2D>().isKinematic = true;
            p.GetComponent<Rigidbody2D>().simulated = false;
        }

        this.isRandomized = true;
        Coroutine cor = StartCoroutine(PlayerPosChangeIE());
        for (int i = 5; i > 0; i--){
            foreach (PlayerController p in GameManager.instance.sceneManager.players){
                p.enabled = false;
                p.GetComponent<Rigidbody2D>().isKinematic = true;
                p.GetComponent<Rigidbody2D>().simulated = false;
            }
            counter.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        StopCoroutine(cor);
        this.newStartPos = GameManager.instance.sceneManager.players[0].transform.position;
        this.inLevelParent.gameObject.SetActive(false);
        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            p.enabled = true;
            p.GetComponent<Rigidbody2D>().isKinematic = false;
            p.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

    private IEnumerator PlayerPosChangeIE(){
        Vector3 orgPos = GameManager.instance.sceneManager.players[0].transform.position;
        InputManager input = GameManager.instance.sceneManager.players[0].input;
        Vector3 newPos = orgPos;

        while (true){
            if (input.RIGHT)
                 newPos = new Vector3(orgPos.x + 1, orgPos.y, orgPos.z);
            else if (input.LEFT)
                 newPos = new Vector3(orgPos.x - 1, orgPos.y, orgPos.z);
            else if(input.UP)
                newPos = new Vector3(orgPos.x, orgPos.y + 1, orgPos.z);
            else if (input.DOWN)
                newPos = new Vector3(orgPos.x, orgPos.y - 1, orgPos.z);

            foreach (PlayerController p in GameManager.instance.sceneManager.players)
                p.transform.position = newPos;
            yield return new WaitForSeconds(0);
        }
    }

}
