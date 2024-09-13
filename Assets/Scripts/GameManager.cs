using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;
using System.IO;
using System.Text;
using TMPro;
using System;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;

public class GameManager : MonoBehaviour{

    public static GameManager instance;
    public BuildData buildData;
    public BlockDataManager blockDataManager;
    public MysteryCostumesManager mysteryCostumesManager;
    public UnlockManager unlockManager;
    public SceneManager sceneManager = new SceneManager();

    [System.NonSerialized]public bool isInPossibleCheck = false;

    [Header("Other")]
    public LayerMask entityGroundMask;
    public LayerMask entityWandMask;
    public LayerMask entityMask;
    public LayerMask shellWandMask;
    public LayerMask blockHitTriggerMask;
    public LayerMask stampMask;
    public LayerMask lazyEnemyMask;//replace that maybe later idkk
    public LayerMask waterMask;
    [System.NonSerialized]public bool isInMainMenu = false;

    [System.NonSerialized]public string currentLevelPath;

    public static string LOAD_AGRUMENTS = string.Empty;

    /**/
    public static string LEVEL_PATH;
    public static float SERVER_VERSION;
    public static bool CAN_ONLINE = true;

    public static int fps; private int frameCount; private float pollingTime = 1f; private float time;

    public static void CapFPS(){
        if (DevGameManager.SHOWDEBUGINFORMATIONS){
            Application.targetFrameRate = 999;
            return;
        }

        Application.targetFrameRate = 60;
        Debug.Log("FPSCap: 60");
    }

    private void Awake(){
        if (instance != null)
            Destroy(this);

        instance = this;
        this.isInPossibleCheck = false;
        this.unlockManager.LoadFile();
        this.sceneManager.Awake();
        this.sceneManager.blackScreen.SetActive(true);
        LEVEL_PATH = Application.persistentDataPath + "\\MyLevels\\";
        if (Application.isEditor && !buildData.VERSION_STRING.StartsWith("Dev"))
            buildData.VERSION_STRING = "Dev-" + buildData.VERSION_STRING;
        if (!Directory.Exists(LEVEL_PATH))
            Directory.CreateDirectory(LEVEL_PATH);
        StartCoroutine(LoadAgruments());
        CapFPS();

        if(this.buildData.isPBuild && !PManager.IsAllowed()){
            Application.Quit();
            Debug.LogError("Not allowed");
            Application.targetFrameRate = 5;
        }

       // InitDiscord();
    }

    private IEnumerator LoadAgruments(){
        yield return new WaitForSecondsRealtime(0.1f);
        while (Application.isLoadingLevel){
            yield return new WaitForSecondsRealtime(0.1f);
        }

        SettingsManager.instance.UpdateAudioMixers();
        GameManager.ResumeTimeScale();

        if (LOAD_AGRUMENTS.StartsWith("--editLevel-")){
            LOAD_AGRUMENTS = LOAD_AGRUMENTS.Replace("--editLevel-", "");
            LevelEditorManager.instance.LoadLevelInEditor(LEVEL_PATH + LOAD_AGRUMENTS);
        }else if (LOAD_AGRUMENTS.StartsWith("--style-")){
            LOAD_AGRUMENTS = LOAD_AGRUMENTS.Replace("--style-", "");
            TileManager.instance.LoadStyleWithId((TileManager.StyleID)StringToInt(LOAD_AGRUMENTS));
        }else if (LOAD_AGRUMENTS.StartsWith("--randomize")){
            LOAD_AGRUMENTS = LOAD_AGRUMENTS.Replace("--randomize", "");
            GetComponent<RandomizerManager>().airProbSlider.SetValueWithoutNotify(StringToFloat(LOAD_AGRUMENTS.Split('|')[0]));
            GetComponent<RandomizerManager>().groundProbSlider.SetValueWithoutNotify(StringToFloat(LOAD_AGRUMENTS.Split('|')[1]));
            TileManager.instance.LoadTilesetWithId((TileManager.TilesetID)StringToInt(LOAD_AGRUMENTS.Split('|')[3]));
            TileManager.instance.LoadStyleWithId((TileManager.StyleID)StringToInt(LOAD_AGRUMENTS.Split('|')[2]));
            GetComponent<RandomizerManager>().GenerateFromMenu();
        }else if (LOAD_AGRUMENTS.StartsWith("--mainmenu")){
            string path = string.Empty;
            try{
                if (LOAD_AGRUMENTS.Split('|').Length > 1)
                    path = LOAD_AGRUMENTS.Split('|')[1];
                else{
                    if (GetAllSavedLevels().Count > 2){
                        if (UnityEngine.Random.Range(0, 10) > 6)
                            path = GetRandomTitleLevel();
                        else
                            path = LEVEL_PATH + GetAllSavedLevels()[UnityEngine.Random.Range(0, GetAllSavedLevels().Count - 1)];
                    }else
                        path = GetRandomTitleLevel();
                }
            }catch(Exception e){
                Debug.Log(e.Message + "|" + e.StackTrace);
            }

            if (!File.Exists(path)){
                Debug.LogWarning(path + " not found for MainMenu!");
                path = GetRandomTitleLevel();
            }

            LevelEditorManager.instance.LoadLevelInEditor(path);
            while (LevelEditorManager.instance.isInEditorLevelLoad | Application.isLoadingLevel)
                yield return new WaitForSeconds(0);

            yield return new WaitForSeconds(0.1f);
            LevelEditorManager.instance.SwitchMode();
            yield return new WaitForSeconds(0.1f);
            LevelEditorManager.instance.SwitchMode();
            this.sceneManager.mainMenu.SetActive(true);
            this.sceneManager.mainMenu.GetComponent<MainMenu>().Activate();

            this.isInMainMenu = true;
        }

        if (!this.isInMainMenu)
            Destroy(this.sceneManager.mainMenu);

        LOAD_AGRUMENTS = string.Empty;
        this.sceneManager.blackScreen.SetActive(false);
        GameObject trans = this.sceneManager.InitTransision1(this.sceneManager.players[0].transform);
        trans.transform.position = this.sceneManager.players[0].transform.position;
        trans.GetComponent<Animator>().Play("Transision1Back");
        yield return new WaitForSeconds(1f);
        Destroy(trans);
    }

    private void OnApplicationQuit(){
        this.unlockManager.SaveFile();
    }

    public static int StringToInt(string str){
        int output = 0;
        int.TryParse(str, out output);
        return output;
    }

    public static float StringToFloat(string str){
        float output = 0;
        float.TryParse(str, out output);
        return output;
    }

    public static string IntToGoodString(int number){
        if (number < 10)
            return "0" + number;
        else
            return number.ToString();
    }

    public static string BigIntToGoodString(int number){
        if (number < 10)
            return "00" + number;
        else if (number < 100)
            return "0" + number;
        else
            return number.ToString();
    }

    public static int BoolToInt(bool boolean){
        if (!boolean)
            return 0;
        else
            return 1;
    }

    public static bool IntToBool(int integer){
        if (integer == 0)
            return false;
        else
            return true;
    }

    public static bool IsInLayerMask(GameObject obj, LayerMask layerMask){
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

    public static void SwitchGameObjectActiveState(GameObject obj){
        obj.SetActive(!obj.active);
    }

    public static void SetAllChildrenBoxCollidersState(Transform parent, bool state){
        foreach (Transform child in parent){
            if (child.GetComponent<BoxCollider2D>() != null)
                child.GetComponent<BoxCollider2D>().enabled = state;
            foreach (Transform child2 in child){
                if (child2.GetComponent<BoxCollider2D>() != null)
                    child2.GetComponent<BoxCollider2D>().enabled = state;
            }
        }
    }

    public Coroutine PlayBlockHitAnimation(GameObject block, bool isHitDown = false){
        return StartCoroutine(BlockHitAnimation(block, isHitDown));
    }

    private IEnumerator BlockHitAnimation(GameObject block, bool isHitDown = false){
        if (!block.GetComponent<Animator>()){
            CheckBlockHitTrigger(block, isHitDown);
            Animator an = block.AddComponent<Animator>();
            an.runtimeAnimatorController = this.sceneManager.hitBlockAnimator;
            if (isHitDown)
                an.Play("HitBlockDown");
            else
                an.Play("HitBlock");
            yield return new WaitForSeconds(0.25f);
            if (block != null && block.GetComponent<Animator>())
                Destroy(block.GetComponent<Animator>());
        }
    }

    public void CheckBlockHitTrigger(GameObject block, bool isHitDown = false){
        Physics2D.queriesStartInColliders = true;
        bool t = Physics2D.queriesHitTriggers;
        Physics2D.queriesHitTriggers = true;
        Vector2 offset = Vector2.up;
        if (isHitDown)
            offset = Vector2.down;
        RaycastHit2D ray1 = Physics2D.Raycast(block.transform.position + new Vector3(-0.37f, 1f, 0f), offset, 0.6f, this.blockHitTriggerMask);
        RaycastHit2D ray2 = Physics2D.Raycast(block.transform.position + new Vector3(0.37f, 1f, 0f), offset, 0.6f, this.blockHitTriggerMask);
        RaycastHit2D ray3 = Physics2D.Raycast(block.transform.position + new Vector3(0f, 1f, 0f), offset, 0.6f, this.blockHitTriggerMask);
        Physics2D.queriesStartInColliders = false;
        Physics2D.queriesHitTriggers = t;

        GameObject gm = null;
        if (ray1.collider != null)
            gm = ray1.collider.gameObject;
        if (ray2.collider != null)
            gm = ray2.collider.gameObject;
        if (ray3.collider != null)
            gm = ray3.collider.gameObject;

        if (gm == null)
            return;

        if (gm.layer == 14){/*Enemy*/
            if(gm.GetComponent<Bowser>() == null)
                gm.GetComponent<Entity>().StartCoroutine(gm.GetComponent<Entity>().ShootDieAnimation(block));
            else 
                 gm.transform.Translate(0, 15 * Time.deltaTime, 0);
        }else if (gm.layer == 15)/*Coin*/
            gm.GetComponent<Coin>().Collect();
        else if (gm.GetComponent<Entity>() != null && gm.GetComponent<PowerupItem>() != null){
            gm.transform.Translate(0, 15 * Time.deltaTime, 0);
            if (gm.GetComponent<Entity>().direction == 0)
                gm.GetComponent<Entity>().direction = 1;
            else
                gm.GetComponent<Entity>().direction = 0;
        }
    }

    public static string GetFileIn(string path){
        if (File.Exists(path)){
            StreamReader readStm2 = new StreamReader(path);
            string fileIn2 = readStm2.ReadToEnd();
            readStm2.Close();

            return fileIn2;
        }else
            return string.Empty;
    }


    public static void SaveFile(string path, string content){
        if (File.Exists(path))
            File.Delete(path);

        StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
        sw.Write(content);
        sw.Close();
    }

    public void RestartCurrentLevel(){
        foreach (PlayerController p in GameManager.instance.sceneManager.players)
            p.enabled = false;
        ClearCurrentLevel();
        StartCoroutine(RestartCurrentLevelIE());
    }

    private IEnumerator RestartCurrentLevelIE(){
        yield return new WaitForSecondsRealtime(0.1f);
        LoadLevelFile(GameManager.instance.currentLevelPath);
        foreach (PlayerController p in GameManager.instance.sceneManager.players){
            p.enabled = true;
            p.ResetPlayer();
        }
    }

    public bool isInLevelLoad = false;
    public void LoadLevelFile(string path, bool dontResetPlayer = false){/*blockId:blockFieldNumber:x:y*/
        this.isInLevelLoad = true;
        TileManager tileManager = TileManager.instance;
        string fileContent = GetFileIn(path);

        if (!fileContent.Substring(1, 1).Contains(":")){
            try{
                fileContent = GameManager.Decrypt(fileContent, GameManager.Decrypt(this.buildData.levelKey, this.buildData.levelKey2));
            }catch (Exception ex){
                Debug.LogException(ex);
                LoadLevelFile(Application.streamingAssetsPath + "\\ErrorLevel.umm");
                return;
            }
        }

        if (1.4f.ToString().Contains(","))
            fileContent = fileContent.Replace(".", ",");
        else
            fileContent = fileContent.Replace(",", ".");

        string[] lines = fileContent.Split('\n');
        fileContent = string.Empty;
        int savedArea = 0;
        if (LevelEditorManager.isLevelEditor)
            savedArea = LevelEditorManager.instance.currentArea;

        this.sceneManager.TryToStopTimer();
        GameManager.instance.sceneManager.playerCamera.maxX = 391.4f;

        /**/
        if (!LevelEditorManager.isLevelEditor){
            this.sceneManager.playerCamera.FreezeCamera();
            this.sceneManager.playerCamera.transform.position = new Vector3(0, 0, 10);

            string[] firstArgs = lines[0].Split(':');
            tileManager.LoadStyleWithInt(StringToInt(firstArgs[0]));

            string[] tilesets = lines[1].Split(':');
             for (int i = 0; i < tilesets.Length; i++){/*Load tilesets*/
                if (tilesets[i].Contains("=#=")){
                    TileManager.Tileset tileset = ModManager.GetTilesetFromCustomId(tilesets[i].Split(new string[] { "=#=" }, System.StringSplitOptions.None)[1]);
                    if(tileset == null)
                        tileManager.LoadTilesetToMemoryWithId(TileManager.TilesetID.Plain, i);
                    else
                        tileManager.LoadTilesetToMemory(tileset, i);
                }else
                        tileManager.LoadTilesetToMemoryWithId((TileManager.TilesetID)GameManager.StringToInt(tilesets[i]), i);
            }
            tileManager.LoadTilesetFromMemory(0);

            int timer = GameManager.StringToInt(firstArgs[3]);
            if (timer == 0)
                timer = 999;
            this.sceneManager.timer = timer;
            this.sceneManager.TryToStartTimer();

            this.sceneManager.fullLevelLiquidIdArea0 = GameManager.StringToInt(firstArgs[4]);
            this.sceneManager.fullLevelLiquidIdArea1 = GameManager.StringToInt(firstArgs[5]);
            try{
                this.sceneManager.fullLevelLiquidSizeArea0 = GameManager.StringToInt(firstArgs[6]);
                this.sceneManager.fullLevelLiquidSizeArea1 = GameManager.StringToInt(firstArgs[7]);
            }catch(Exception e){
                this.sceneManager.fullLevelLiquidSizeArea0 = 0;
                this.sceneManager.fullLevelLiquidSizeArea1 = 0;
                Debug.LogError(e.Message + "|" + e.StackTrace);
            }
            if (firstArgs.Length > 8){
                this.sceneManager.isAreaDark[0] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[8]));
                this.sceneManager.isAreaDark[1] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[9]));
                this.sceneManager.hasAreaCloudsFog[0] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[10]));
                this.sceneManager.hasAreaCloudsFog[1] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[11]));
            }else{
                this.sceneManager.isAreaDark[0] = false;
                this.sceneManager.isAreaDark[1] = false;
                this.sceneManager.hasAreaCloudsFog[0] = false;
                this.sceneManager.hasAreaCloudsFog[1] = false;
            }
        }

        lines[0] = string.Empty;
        lines[1] = string.Empty;

        SceneManager.onOffObjects.Clear();
        SceneManager.respawnableEntities.Clear();
        SceneManager.DestroyDestroyAfterNewLoadList();
        SceneManager.destroyAfterNewLoad.Clear();
        SceneManager.checkpoints.Clear();
        SceneManager.StopPSwitch();
        SceneManager.pSwitchObjects.Clear();
        SceneManager.enemiesDamageColliders.Clear();
        Trampoline.targetGameObjects.Clear();
        DecoPlatform.count = 0;

        this.sceneManager.spawnZone.GetComponent<BoxCollider2D>().enabled = false;
        this.sceneManager.playerCamera.UnfreezeCamera();
        foreach (PlayerController p in this.sceneManager.players){
            if(!LevelEditorManager.isLevelEditor && !dontResetPlayer)
                p.ResetPlayer();
            if(!dontResetPlayer)
                p.SetPowerup(PlayerController.Powerup.Small, true);
        }

        float hammerBros = 0;
        float bowsers = 0;
        lines[2] = "";
        foreach (string line in lines){
            try{
#if UNITY_EDITOR
                if (DevGameManager.levelLoadDebug){
                    Debug.Log(line);
                    System.Diagnostics.Debugger.Break();
                }
#endif
                if (!string.IsNullOrEmpty(line)){
                string[] args = line.Split(':');
                int id = StringToInt(args[0]);
                float x = StringToFloat(args[2]);
                float y = StringToFloat(args[3]);
                int area = StringToInt(args[4]);
                BlockData blockData = this.blockDataManager.blockDatas[id];

                if((BlockID)id == BlockID.STARTPOINT){
                    if (!LevelEditorManager.isLevelEditor){
                        foreach (PlayerController p in this.sceneManager.players)
                            p.transform.position = new Vector3(x, y, 0);
                    }
                    continue;
                }

                if (GameManager.instance.GetComponent<RandomizerManager>().isRandomized){
                    foreach (PlayerController p in this.sceneManager.players)
                        p.transform.position = GameManager.instance.GetComponent<RandomizerManager>().newStartPos;
                }

                if (blockData.onlyForSMB1 && tileManager.currentStyle.id != TileManager.StyleID.SMB1)
                    continue;

                GameObject block = Instantiate(blockData.prefarb);
                block.transform.SetParent(this.sceneManager.GetAreaParent(area));
                block.transform.position = new Vector3(x, y, 0);
                if (block.GetComponent<SpriteRenderer>() == null && !blockData.keepOriginalSprite)
                    block.GetComponentInChildren<SpriteRenderer>().sprite = tileManager.GetSpriteFromPreLoadedTileset(area, blockData.spriteId, blockData.tilesetType);
                else if (!blockData.keepOriginalSprite)
                    block.GetComponent<SpriteRenderer>().sprite = tileManager.GetSpriteFromPreLoadedTileset(area, blockData.spriteId, blockData.tilesetType);

                if (blockData.dontRenderInPlayMode)
                    block.GetComponent<SpriteRenderer>().enabled = false;

                SceneManager.RespawnableEntity respawnableEntity = null;
                if (blockData.canRespawn)
                     respawnableEntity = new SceneManager.RespawnableEntity(block, (BlockID)id, new Vector3(x, y, 0), area);
                if (blockData.glowInDark && this.sceneManager.isAreaDark[area]){
                     GameObject mask = Instantiate(GameManager.instance.sceneManager.entityDarkMask);
                     if(block.transform.childCount > 0)
                        mask.transform.SetParent(block.transform.GetChild(0));
                     else
                        mask.transform.SetParent(block.transform);

                     mask.transform.localScale = blockData.darkGlowMaskSize;
                     mask.transform.localPosition = new Vector3(0, 0, 0);
                }

                switch ((BlockID)id){
                    case BlockID.FAKEWALL:
                    case BlockID.GROUND:
                        int tileID = GameManager.StringToInt(args[5]);
                        block.GetComponent<SpriteRenderer>().sprite = tileManager.loadedTilesets[area].mainTileset[tileID];
                        if (GameManager.StringToInt(args[6]) != -1){
                             GameObject clon = Instantiate(this.sceneManager.groundBlockMask, block.transform);
                             clon.transform.position = block.transform.position;
                             block.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                             clon.GetComponent<SpriteRenderer>().sprite = tileManager.loadedTilesets[area].mainTileset[GameManager.StringToInt(args[6])];
                        }

                        if(id == (int)BlockID.GROUND && block.transform.position.y == 31.3f)
                            {
                                block.GetComponent<BoxCollider2D>().offset = new Vector2(0, 5.132374f);
                                block.GetComponent<BoxCollider2D>().size = new Vector2(1, 11.26475f);
                            }
                        break;
                    case BlockID.COIN:
                        SceneManager.pSwitchObjects.Add(block);
                        break;
                    case BlockID.BREAK_BLOCK:
                    case BlockID.QUESTION_BLOCK:
                    case BlockID.INVISBLE_QUESTION_BLOCK:
                    case BlockID.LONGQUESTIONBLOCK:
                        if ((BlockID)id == BlockID.BREAK_BLOCK)
                            SceneManager.pSwitchObjects.Add(block);
                        BlockID contentID = (BlockID)GameManager.StringToInt(args[5]);
                        block.GetComponent<ItemBlock>().contentBlock = contentID;
                        if (contentID == BlockID.MYSTERY_MUSHROM)
                            block.GetComponent<ItemBlock>().extraNumber = GameManager.StringToInt(args[6]);
                        break;
                    case BlockID.MINI_PIPE:
                    case BlockID.PIPE:
                        Pipe pipe = block.GetComponent<Pipe>();
                        pipe.pipeLength = GameManager.StringToInt(args[5]);
                        pipe.LoadLength(area);
                        pipe.contentBlock = (BlockID)GameManager.StringToInt(args[6]);
                        block.transform.eulerAngles = new Vector3(block.transform.eulerAngles.x, block.transform.eulerAngles.y, GameManager.StringToInt(args[7]));
                        if (GameManager.StringToInt(args[7]) != 0){
                            foreach(Transform child in pipe.transform){
                                child.GetComponent<SpriteRenderer>().sortingOrder = 0; 
                            }
                        }
                        if(args.Length > 8){/*Connecting Pipe*/
                            float connectedPipeX = GameManager.StringToFloat(args[9]);
                            float connectedPipeY = GameManager.StringToFloat(args[10]);
                            int connectedPipeArea = GameManager.StringToInt(args[11]);
                            int connectedPipeLength = GameManager.StringToInt(args[12]);
                            int connectedPipeContentBlock = GameManager.StringToInt(args[13]);
                            int connectedPipeRotationZ = GameManager.StringToInt(args[14]);

                            Pipe connectedPipe = Instantiate(blockData.prefarb).GetComponent<Pipe>();
                            connectedPipe.transform.SetParent(this.sceneManager.GetAreaParent(connectedPipeArea));
                            connectedPipe.transform.position = new Vector3(connectedPipeX, connectedPipeY, 0);
         
                            connectedPipe.pipeLength = connectedPipeLength;
                            connectedPipe.contentBlock = (BlockID)connectedPipeContentBlock;
                            connectedPipe.LoadLength(connectedPipeArea);
                            connectedPipe.transform.eulerAngles = new Vector3(connectedPipe.transform.eulerAngles.x, connectedPipe.transform.eulerAngles.y, connectedPipeRotationZ);
                            connectedPipe.transform.position = new Vector3(connectedPipeX, connectedPipeY, 0);

                            pipe.connectedPipe = connectedPipe;
                            connectedPipe.connectedPipe = pipe;
                        }
                        break;
                    case BlockID.KEY_DOOR:
                    case BlockID.PSWITCH_DOOR:
                    case BlockID.DOOR:
                    case BlockID.WARPBOX:
                        float warpBox2X = GameManager.StringToFloat(args[5]);
                        float warpBox2Y = GameManager.StringToFloat(args[6]);
                        GameObject warpBox2 = Instantiate(blockData.prefarb);
                        warpBox2.GetComponent<SpriteRenderer>().sprite = tileManager.GetSpriteFromTileset(blockData.spriteId, blockData.tilesetType);
                        warpBox2.transform.position = new Vector3(warpBox2X, warpBox2Y, 0);
                        warpBox2.transform.SetParent(this.sceneManager.GetAreaParent(area));
                        if ((BlockID)id == BlockID.WARPBOX){
                            block.GetComponent<WarpBox>().otherWarpBox = warpBox2;
                            warpBox2.GetComponent<WarpBox>().otherWarpBox = block;
                        }else{
                            block.GetComponent<Door>().otherDoor = warpBox2;
                            warpBox2.GetComponent<Door>().otherDoor = block;
                        }
                        if ((BlockID)id == BlockID.PSWITCH_DOOR){
                            SceneManager.pSwitchObjects.Add(block);
                            SceneManager.pSwitchObjects.Add(warpBox2);
                            block.GetComponent<Door>().DeactivatePSwitchDoor();
                            warpBox2.GetComponent<Door>().DeactivatePSwitchDoor();
                        }
                        break;
                    case BlockID.MESSAGEBLOCK:
                        block.GetComponent<MessageBlock>().text = args[5];
                        break;
                    case BlockID.CHECKPOINT:
                        block.GetComponent<Checkpoint>().myNumber = SceneManager.checkpoints.Count;
                        SceneManager.checkpoints.Add(block.GetComponent<Checkpoint>());
                        break;
                    case BlockID.AIRBUBBLES:
                    case BlockID.BURNER:
                    case BlockID.DISABLEDBURNER:
                    case BlockID.BIGBILLENEMY:
                    case BlockID.REDBIGBILLENEMY:
                    case BlockID.DECOARROW:
                    case BlockID.ONEWAYWALL:
                        float rotateZ = GameManager.StringToFloat(args[5]);
                        if ((id == (int)BlockID.BIGBILLENEMY | id == (int)BlockID.REDBIGBILLENEMY)){
                                if(rotateZ == 180)
                                    block.GetComponent<SpriteRenderer>().flipY = true;
                                respawnableEntity.saveVar = rotateZ;
                        }

                        block.transform.eulerAngles = new Vector3(block.transform.eulerAngles.x, block.transform.eulerAngles.y, rotateZ);
                        if (id == (int)BlockID.AIRBUBBLES)
                            block.GetComponent<AirBubbles>().Load();
                        break;
                    case BlockID.MYSTERY_MUSHROM:
                        block.GetComponent<MysteryMushrom>().costumeNumber = GameManager.StringToInt(args[5]);
                        respawnableEntity.saveVar = block.GetComponent<MysteryMushrom>().costumeNumber;
                        break;
                    case BlockID.BLUEPLATFORM:
                    case BlockID.PLATFORM:
                        if ((BlockID)id == BlockID.PLATFORM){
                            block.GetComponent<PlatformLift>().direction = (LiftHelper.Direction)GameManager.StringToInt(args[5]);
                            block.GetComponent<PlatformLift>().Register();
                        }
                        block.GetComponent<PlatformLift>().length = GameManager.StringToInt(args[6]);
                        block.GetComponent<PlatformLift>().LoadLength();
                        break;
                    case BlockID.SPEEDMOVEBLOCK:
                        block.GetComponent<SpeedMoveBlock>().length = GameManager.StringToInt(args[5]);
                        block.GetComponent<SpeedMoveBlock>().onOffState = (SpeedMoveBlock.OnOffState)GameManager.StringToInt(args[8]);
                        if (block.GetComponent<SpeedMoveBlock>().onOffState != SpeedMoveBlock.OnOffState.NONE)
                            SceneManager.onOffObjects.Add(block);
                        block.GetComponent<SpeedMoveBlock>().LoadIsFast(GameManager.StringToInt(args[7]));
                        block.GetComponent<SpeedMoveBlock>().LoadDirection((LiftHelper.Direction)GameManager.StringToInt(args[6]));
                        block.GetComponent<SpeedMoveBlock>().LoadLength();
                        break;
                    case BlockID.DECOPLATFORM:
                        block.GetComponent<DecoPlatform>().lengthX = GameManager.StringToInt(args[5]);
                        block.GetComponent<DecoPlatform>().lengthY = GameManager.StringToInt(args[6]);
                        block.GetComponent<DecoPlatform>().LoadType(GameManager.StringToInt(args[7]) - 1, area);
                        block.GetComponent<DecoPlatform>().LoadLength(area);
                        break;
                    case BlockID.VINE:
                        block.GetComponent<Vine>().length = GameManager.StringToInt(args[5]); 
                        block.GetComponent<Vine>().LoadLength(area);
                        break;
                    case BlockID.DECOMUSHROOM_PLATFORM:
                        block.GetComponent<MushroomPlatform>().lengthX = GameManager.StringToInt(args[5]);
                        block.GetComponent<MushroomPlatform>().lengthY = GameManager.StringToInt(args[6]);
                        block.GetComponent<MushroomPlatform>().type = GameManager.StringToInt(args[7]);
                        block.GetComponent<MushroomPlatform>().LoadLength(area);
                        break;
                    case BlockID.FIRE_BAR:
                        block.GetComponent<FireBar>().lengthY = GameManager.StringToInt(args[5]);
                        block.GetComponent<FireBar>().LoadLength(area);
                        block.transform.GetChild(0).eulerAngles = new Vector3(0, 0, GameManager.StringToFloat(args[6]));
                        if (GameManager.StringToInt(args[7]) == 1)
                            block.GetComponent<FireBar>().speed = -block.GetComponent<FireBar>().speed;
                        break;
                    case BlockID.REDBILLBLASTER:
                    case BlockID.BILLBLASTER:
                        block.GetComponent<BillBlaster>().length = GameManager.StringToInt(args[5]);
                        block.GetComponent<BillBlaster>().LoadLength();
                        respawnableEntity.saveVar = block.GetComponent<BillBlaster>().length;
                        break;
                    case BlockID.BIGHAMMERBRO:
                    case BlockID.HAMMERBRO:
                        if (hammerBros == 1){
                             block.GetComponent<HammerBro>().startDirection = 0;
                             hammerBros = 0;
                        }else
                             hammerBros = 1;
                        break;
                    case BlockID.BOWSER:
                        block.GetComponent<Bowser>().extraSeconds = bowsers;
                        if (bowsers < 0.5f) 
                             bowsers = bowsers + 0.1f;
                        else
                             bowsers = 0;
                        break;
                    case BlockID.GREENSKYROTATEBLOCK:
                    case BlockID.REDSKYROTATEBLOCK:
                        block.transform.localScale = new Vector3(GameManager.StringToInt(args[5]), GameManager.StringToInt(args[5]), 1);
                        break;
                    case BlockID.PSWITCHBLOCK:
                        SceneManager.pSwitchObjects.Add(block);
                        break;
                    case BlockID.PSWITCHBLOCK1:
                            SceneManager.pSwitchObjects.Add(block);
                        break;
                    }

                if(args[args.Length - 1].Equals("key")){
                        if (block.GetComponent<Entity>() != null){
                            block.GetComponent<Entity>().hasKey = true;
                            respawnableEntity.hasKey = true;
                        }
                }
            }
            }catch(Exception e){
                Debug.LogError("ERROR LOADING LEVEL: " + e.Message + "|" + e.StackTrace);
                continue;
            }
        }

        if (LevelEditorManager.isLevelEditor){
            Checkpoint.currentCheckpoint = -1;
            this.sceneManager.ChageArea(savedArea, true);
        }else{
            if (this.currentLevelPath != path)
                Checkpoint.currentCheckpoint = -1;
            this.sceneManager.ChageArea(0);
            if (this.currentLevelPath == path && Checkpoint.currentCheckpoint != -1){
                this.sceneManager.ChageArea(StringToInt(SceneManager.checkpoints[Checkpoint.currentCheckpoint].transform.parent.name));
                SceneManager.checkpoints[Checkpoint.currentCheckpoint].CollectCheckPoint();
                foreach (PlayerController p in this.sceneManager.players)
                    p.transform.position = SceneManager.checkpoints[Checkpoint.currentCheckpoint].transform.position;
            }
        }
        if (GetComponent<RandomizerManager>().isRandomized){
            foreach (PlayerController p in this.sceneManager.players){
                p.StartCoroutine(p.DamageAnimation());
            }
        }

        if (LevelEditorManager.isLevelEditor)
            sceneManager.spawnZone.GetComponent<BoxCollider2D>().enabled = true;
        else{
            GameManager.instance.StartCoroutine(sceneManager.SpawnZoneNewCheckIE());
            GameManager.instance.StartCoroutine(sceneManager.LevelStartRespawnHelperCheckIE());
        }
        SceneManager.onOffState = false;
        SceneManager.SwitchOnOffState(true);
        this.sceneManager.playerCamera.UnfreezeCamera();
        this.currentLevelPath = path;

        if (this.sceneManager.isAreaDark[this.sceneManager.currentArea])
            StartCoroutine(ActivateDarkScreenSmoothIE());

        //if (this.sceneManager.hasAreaCloudsFog[this.sceneManager.currentArea])
            //StartCoroutine(ActivateHasFogScreenSmoothIE());

        foreach (PlayerController p in this.sceneManager.players){
            if (p.CheckIfOverPlayerIsABlock())
                p.TryEnableDuck();
        }
    }

    private IEnumerator ActivateDarkScreenSmoothIE(){
        this.sceneManager.darkScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        this.sceneManager.darkScreen.SetActive(true);

        while(this.sceneManager.darkScreen.GetComponent<SpriteRenderer>().color.a < 1.2f && this.sceneManager.darkScreen.active){
            this.sceneManager.darkScreen.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, this.sceneManager.darkScreen.GetComponent<SpriteRenderer>().color.a+ 3 *Time.deltaTime);
            yield return new WaitForSeconds(0);
        }
    }

    private IEnumerator ActivateHasFogScreenSmoothIE(){
        this.sceneManager.fogClouds.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        this.sceneManager.fogClouds.SetActive(true);

        while (this.sceneManager.fogClouds.GetComponent<SpriteRenderer>().color.a < 0.7647059f && this.sceneManager.fogClouds.active){
            this.sceneManager.fogClouds.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, this.sceneManager.fogClouds.GetComponent<SpriteRenderer>().color.a + 3 * Time.deltaTime);
            foreach (Transform child in this.sceneManager.fogClouds.transform){
                child.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, child.GetComponent<SpriteRenderer>().color.a + 3 * Time.deltaTime);
            }
            yield return new WaitForSeconds(0);
        }

        this.sceneManager.fogClouds.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 195);
        foreach (Transform child in this.sceneManager.fogClouds.transform){
            child.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 195);
        }
    }

    public string GetRandomTitleLevel(){
        List<string> fileNames = new List<string>();
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.streamingAssetsPath + "\\TitleLevels\\");
        FileInfo[] directoryFiles = directoryInfo.GetFiles("*.umm");
        foreach (FileInfo fileInfo in directoryFiles){
            fileNames.Add(fileInfo.FullName);
        }

        return fileNames[UnityEngine.Random.Range(0, fileNames.Count)];
    }
    public static void SortDropdownOptions(Dropdown dropdown){
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>(dropdown.options);

        options.Sort((a, b) => a.text.CompareTo(b.text));

        dropdown.ClearOptions();

        dropdown.AddOptions(options);
    }

    public static List<string> GetAllSavedLevels(string customPath = ""){
        List<string> fileNames = new List<string>();

        DirectoryInfo directoryInfo;
        if (string.IsNullOrEmpty(customPath))
            directoryInfo = new DirectoryInfo(LEVEL_PATH);
        else
            directoryInfo = new DirectoryInfo(customPath);

        FileInfo[] directoryFiles = directoryInfo.GetFiles("*.umm");
        foreach (FileInfo fileInfo in directoryFiles){
            fileNames.Add(fileInfo.Name);
        }

        return fileNames;
    }

    public static List<string> GetDirectories(string path, string searchPattern){
        try{
            return Directory.GetDirectories(path, searchPattern).ToList();
        }catch (UnauthorizedAccessException){
            return new List<string>();
        }
    }


    public void ClearCurrentLevel(){
        this.sceneManager.ResetCoins();
        foreach (Transform child in this.sceneManager.levelBlocksParent){
            Destroy(child.gameObject);
        }
    }

    public static string GetB(){
        byte[] bytes = Encoding.ASCII.GetBytes("hlrez242");
        string outString = string.Empty;
        foreach(byte by in bytes){
            outString = outString + by;
        }

        return outString;
    }

    public static void StopTimeScale(){
        Time.timeScale = 0;
    }

    public static void ResumeTimeScale(){
        Time.timeScale = 1;
    }

    public static IEnumerator EnterPipe(Pipe pipe, GameObject player1){
        if (player1.GetComponent<PlayerController>().GetCanMove()){ 
        SceneManager sceneManager = GameManager.instance.sceneManager;
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        int layerID = player1.GetComponent<SpriteRenderer>().sortingLayerID;
        foreach (PlayerController p in sceneManager.players){
            p.GetComponent<PlayerController>().SetCanMove(false);
            p.GetComponent<PlayerController>().BreakReturn();
            p.GetComponent<BoxCollider2D>().enabled = false;
            p.GetComponent<SpriteRenderer>().sortingLayerID = pipe.GetComponent<SpriteRenderer>().sortingLayerID;
            p.GetComponent<SpriteRenderer>().sortingOrder = -2;
            p.GetComponent<Rigidbody2D>().isKinematic = true;
            p.GetComponent<Rigidbody2D>().simulated = false;
            p.GetComponent<PlayerController>().PlayAnimation(p.GetComponent<PlayerController>().currentPlayerSprites.stand, PlayerController.AnimationID.Stand);
            p.DisableDuck();
        }
        if(!pipe.isSmallPipe)
            player1.transform.position = pipe.transform.position + pipe.GetPlayerOffset();
        else
                player1.transform.position = pipe.transform.position;
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerdown);

        Vector3 offset2 = -pipe.GetOffset2();

            if (offset2.x > 0)
                player1.GetComponent<SpriteRenderer>().flipX = false;
            else
                player1.GetComponent<SpriteRenderer>().flipX = true;
            for (int i = 0; i < 25; i++){
            player1.transform.Translate(offset2);
            if(offset2.y == 0)
                player1.GetComponent<PlayerController>().PlayAnimation(player1.GetComponent<PlayerController>().currentPlayerSprites.walk, PlayerController.AnimationID.Walk);
            yield return new WaitForSeconds(0.01f);
        }

        GameObject trans = GameManager.instance.sceneManager.InitTransision1(player1.transform);
        yield return new WaitForSeconds(1f);
        GameManager.instance.sceneManager.playerCamera.UnfreezeCamera();
        sceneManager.ChageArea(GameManager.StringToInt(pipe.connectedPipe.transform.parent.name));
        SceneManager.DestroyDestroyAfterNewLoadList();
        sceneManager.RespawnEntities();
        Vector3 offset = pipe.connectedPipe.GetPlayerOffset();
        if (Pipe.GetLookDirection(pipe.connectedPipe.gameObject) == Vector3.up | Pipe.GetLookDirection(pipe.connectedPipe.gameObject) == Vector3.down)
            offset.y = -offset.y;
        else
            offset.x = -offset.x;

         foreach (PlayerController p in sceneManager.players){
                if(!pipe.connectedPipe.isSmallPipe)
                    p.transform.position = pipe.connectedPipe.transform.position + offset;
                else
                    p.transform.position = pipe.connectedPipe.transform.position;
         }

        trans.transform.position = player1.transform.position;
        trans.GetComponent<Animator>().Play("Transision1Back");
        yield return new WaitForSeconds(1f);
        Destroy(trans);
        SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.powerdown);

        int count = 38;
        if(pipe.connectedPipe.isSmallPipe)
            count = 15;


            Vector3 offset3 = pipe.connectedPipe.GetOffset2();
            if (offset3.y == 0)
                count = 55;
            for (int i = 0; i < count; i++){
                foreach (PlayerController p in sceneManager.players){
                    p.transform.Translate(offset3);
                    if (offset3.y == 0){
                        p.GetComponent<PlayerController>().PlayAnimation(p.GetComponent<PlayerController>().currentPlayerSprites.walk, PlayerController.AnimationID.Walk);
                        if (offset3.x > 0)
                            p.GetComponent<SpriteRenderer>().flipX = false;
                        else
                            p.GetComponent<SpriteRenderer>().flipX = true;
                    }
                }
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(0.1f);

        foreach (PlayerController p in sceneManager.players){
            p.transform.eulerAngles = Vector3.zero;
            p.GetComponent<BoxCollider2D>().enabled = true;
            p.GetComponent<SpriteRenderer>().sortingLayerID = layerID;
            p.GetComponent<SpriteRenderer>().sortingLayerName = "Player";
            p.GetComponent<Rigidbody2D>().isKinematic = false;
            p.GetComponent<Rigidbody2D>().simulated = true;
                p.GetComponent<PlayerController>().PlayAnimation(p.GetComponent<PlayerController>().currentPlayerSprites.stand, PlayerController.AnimationID.Stand);
                p.StartCoroutine(p.FreezeMovementForSeconds(0.2f));
            if (p.CheckIfOverPlayerIsABlock())
                p.TryEnableDuck();
        }
        LevelEditorManager.canSwitch = true;
        MenuManager.canOpenMenu = true;
        }
    }

    public void LoadEditorScene(string loadAgruments = ""){
        LevelEditorManager.isLevelEditor = true;
        LevelEditorManager.canSwitch = true;
        LOAD_AGRUMENTS = loadAgruments;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void LoadMainMenu(){
        StartCoroutine(LoadMainMenuIE());
    }

    private IEnumerator LoadMainMenuIE(){
        ResumeTimeScale();
        LevelEditorManager.canSwitch = false;
        this.GetComponent<MenuManager>().CloseMainMenu();

        GameObject trans = this.sceneManager.InitTransision1(this.sceneManager.players[0].transform);
        trans.transform.position = this.sceneManager.players[0].transform.position;
        yield return new WaitForSeconds(1f);

        LevelEditorManager.canSwitch = true;
        LoadEditorScene("--mainmenu|" + this.currentLevelPath);
    }

    private static Discord.Discord discord = null;
    public static void InitDiscord(){
        if (discord != null)
            return;

        discord = new Discord.Discord(995978856852901900, (UInt64)Discord.CreateFlags.Default);
        var activityManager = discord.GetActivityManager();
        var lobbyManager = discord.GetLobbyManager();

        var activity = new Discord.Activity {
            State = "Custom Mario Maker",
            Details = "testing",
            Assets ={
                LargeImage = "img",
                LargeText = "img",
                SmallImage = "img",
                SmallText = "img",
            },
            Instance = true,
        };

        activityManager.UpdateActivity(activity, result =>{
            Debug.Log("Update Activity" + result.ToString());
        });
    }

    private void Update(){
        time += Time.deltaTime;
        frameCount++;
        if (time >= pollingTime){
            fps = Mathf.RoundToInt(frameCount / time);
            time -= pollingTime;
            frameCount = 0;
        }
    }

    /*https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp*/
    private const int Keysize = 256;
    private const int DerivationIterations = 1000;
    public static string Encrypt(string plainText, string passPhrase){
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations)){
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged()){
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)){
                    using (var memoryStream = new MemoryStream()){
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)){
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    public static string Decrypt(string cipherText, string passPhrase){
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations)){
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged()){
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)){
                    using (var memoryStream = new MemoryStream(cipherTextBytes)){
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)){
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }
    public static byte[] Generate256BitsOfRandomEntropy(){
        var randomBytes = new byte[32];
        using (var rngCsp = new RNGCryptoServiceProvider()){
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}

[System.Serializable]
public class SceneManager{
    [Header("Link to Objects")]
    public List<PlayerController> players = new List<PlayerController>();
    public PlayerCamera playerCamera;
    public MenuManager menuManager;
    public LevelEditorCursor levelEditorCursor;
    public Transform levelBlocksParent;
    public SpriteRenderer[] backgroundRenderers;
    public SpriteRenderer[] secondBackgroundRenderers;
    public TextMeshProUGUI coinsCountText;
    public TextMeshProUGUI timerText;
    public GroundTileConnector emptyGroundTileConnector;
    public GameObject switchModeButton;
    public SpriteRenderer styleButton;
    public SpriteRenderer backgroundButton;
    public TextMeshPro editorAreaSwitchButtonText;
    public GameObject levelStartScreen;
    public TextMeshProUGUI levelStartScreenLevelNameText;
    public Text levelStartScreenTagsText;
    public SpriteRenderer mkItemSlotContentRenderer;
    public Text textBoxText;
    public GameObject spawnZone;
    public BoxCollider2D despawnZone;
    public GameObject levelStartRespawnHelper;
    public GameObject fullLevelLiquid;
    public InputField levelNameInput;
    public GameObject finishScreen;
    public GameObject snowEffect;
    public GameObject mainMenu;
    public GameObject blackScreen;
    public TextMeshPro lavaEnabledStateText;
    public SpriteRenderer fullLevelLiquidImage;
    public Sprite[] fullLevelLiquidSprites;
    public SpriteRenderer[] editorBackgrounds;
    public Toggle[] saveLevelTagToggles;
    public SpriteRenderer isDarkImage;
    public Sprite[] isDarkSprites;
    public GameObject darkScreen;
    public GameObject entityDarkMask;
    public GameObject fogClouds;
    public GameObject fogIcon;
    /*Objects created in runtime*/
    [System.NonSerialized]public BackgroundMusicManager backgroundMusicManager;
    [System.NonSerialized]public static List<GroundTileConnector> groundTileConnectors = new List<GroundTileConnector>();
    [System.NonSerialized]public static List<GameObject> onOffObjects = new List<GameObject>();
    [System.NonSerialized]public static List<RespawnableEntity> respawnableEntities = new List<RespawnableEntity>();
    [System.NonSerialized]public static List<GameObject> destroyAfterNewLoad = new List<GameObject>();/*Destroy After Enter an Pipe or Door*/
    [System.NonSerialized]public static List<Checkpoint> checkpoints = new List<Checkpoint>();
    [System.NonSerialized]public static List<GameObject> pSwitchObjects = new List<GameObject>();
    [System.NonSerialized]public static List<BoxCollider2D> enemiesDamageColliders = new List<BoxCollider2D>();
    [Header("PreLoadedPrefarbs")]
    public GameObject fireBallPrefarb;
    public GameObject firePiranhaFireBall;
    public GameObject iceBallPrefarb;
    public GameObject iceFlowerBlock;
    public GameObject smashFirePrefarb;
    public GameObject transision1;
    public GameObject destroyEffect;
    public GameObject coinEffect;
    public GameObject explodeEffect;public GameObject explodeEffect2;
    public GameObject hitEffect;
    public GameObject levelEditorSaveBlock;
    public GameObject groundBlockMask;
    public GameObject bonePrefarb;
    public GameObject waitForEntityRespawnPrefarb;
    public GameObject SMWNoShellPrefarb;
    public GameObject hammer;
    public GameObject friendlyHammer;
    public GameObject bowserFire;
    public GameObject friendlyBowserFire;
    public GameObject SMWBowserFallFire;
    public GameObject elcHitEffect;
    public GameObject waterEffect;
    public GameObject gunBullet;
    public GameObject booEffect;
    public GameObject pumkinEffect;
    [Header("Other")]
    public Sprite missingSpriteSprite;
    public RuntimeAnimatorController hitBlockAnimator;
    public RuntimeAnimatorController dropGrabedObjectAnimator;
    public AudioMixerGroup mixerGroup;
    public static AudioMixerGroup audioMixerGroup;
    /*States*/
    [System.NonSerialized]public int currentArea = 0;
    [System.NonSerialized]public static bool onOffState = true;/*true = On, false = Off*/
    [System.NonSerialized]public static bool pSwitchState = false;/*true = On, false = Off*/
    /**/

    [System.NonSerialized]private int coins = 0;
    [System.NonSerialized]public int timer = 999;
    [System.NonSerialized]public int fullLevelLiquidIdArea0 = 0/*0 == Off, 1 = Lava*/;
    [System.NonSerialized]public int fullLevelLiquidIdArea1 = 0/*0 = Off, 1 = Lava*/;
    [System.NonSerialized]public int fullLevelLiquidSizeArea0 = 0;
    [System.NonSerialized]public int fullLevelLiquidSizeArea1 = 0;
    [System.NonSerialized]public bool[] isAreaDark = new bool[2] { false, false };
    [System.NonSerialized]public bool[] hasAreaCloudsFog = new bool[2] { false, false };

    [System.NonSerialized]private static Coroutine currentPSwitchCor = null;
    [System.NonSerialized]private static AudioSource pSwitchAudioSource = null;

    public class RespawnableEntity{

        public GameObject currentEntity = null;
        public int area;
        public BlockID id;
        public Vector3 position;
        public object saveVar;
        public bool dontRespwan = false;
        public bool hasKey = false;

        public RespawnableEntity(GameObject currentEntity, BlockID id, Vector3 position, int area){
            this.currentEntity = currentEntity;
            this.id = id;
            this.position = position;
            this.area = area;
            SceneManager.respawnableEntities.Add(this);
        }

        public void Respawn(bool wasDespawned = false){
            if (this.currentEntity != null && this.currentEntity.transform.parent.GetComponent<PlayerController>() != null)
                return;

            if(this.currentEntity != null)
                GameManager.Destroy(this.currentEntity);
            if (dontRespwan)
                return;

            if (wasDespawned){
                GameObject r = GameManager.Instantiate(GameManager.instance.sceneManager.waitForEntityRespawnPrefarb, GameManager.instance.sceneManager.GetAreaParent(this.area));
                r.transform.position = this.position;
                r.GetComponent<WaitForRespawn>().respawnableEntity = this;
                return;
            }

            BlockData blockData = GameManager.instance.blockDataManager.blockDatas[(int)this.id];
            GameObject clon = GameManager.Instantiate(blockData.prefarb);

            if (this.hasKey)
                clon.GetComponent<Entity>().hasKey = this.hasKey;

            /*saveVar Checks*/
            if (this.id == BlockID.BIGBILLENEMY | this.id == BlockID.REDBIGBILLENEMY){
                float rotateZ = (float)this.saveVar;
                if (rotateZ == 180)
                    clon.GetComponent<SpriteRenderer>().flipY = true;

                clon.transform.eulerAngles = new Vector3(clon.transform.eulerAngles.x, clon.transform.eulerAngles.y, rotateZ);
            }else if (this.id == BlockID.BILLBLASTER | this.id == BlockID.REDBILLBLASTER){
                clon.GetComponent<BillBlaster>().length = (int)this.saveVar;
                clon.GetComponent<BillBlaster>().LoadLength();
            }else if (this.id == BlockID.MYSTERY_MUSHROM){
                clon.GetComponent<MysteryMushrom>().costumeNumber = (int)this.saveVar;
            }

            clon.transform.SetParent(GameManager.instance.sceneManager.GetAreaParent(this.area));
            clon.transform.position = position;
            clon.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(this.area, blockData.spriteId, blockData.tilesetType);
            this.currentEntity = clon;
        }

    }

    public void Awake(){
        this.players[0].GetComponentInChildren<InputManager>().RegisterInstance(0);
        audioMixerGroup = mixerGroup;
        this.backgroundMusicManager = GameManager.instance.gameObject.AddComponent<BackgroundMusicManager>();
        this.backgroundMusicManager.playModeSource = GameManager.instance.gameObject.AddComponent<AudioSource>();
        this.backgroundMusicManager.playModeSource.loop = true;
        this.backgroundMusicManager.editModeSource = GameManager.instance.gameObject.AddComponent<AudioSource>();
        this.backgroundMusicManager.editModeSource.loop = true;
        string _OutputMixer = "Music";
        this.backgroundMusicManager.playModeSource.outputAudioMixerGroup = SceneManager.audioMixerGroup.audioMixer.FindMatchingGroups(_OutputMixer)[0];
        this.backgroundMusicManager.editModeSource.outputAudioMixerGroup = SceneManager.audioMixerGroup.audioMixer.FindMatchingGroups(_OutputMixer)[0];
    }

    public PlayerController InitNewPlayer(bool dontRegisterInput = false){
        GameObject nP = GameManager.Instantiate(this.players[0].gameObject);
        this.players.Add(nP.GetComponent<PlayerController>());
        if(!dontRegisterInput)
            nP.GetComponentInChildren<InputManager>().RegisterInstance(this.players.Count-1);
        nP.GetComponent<PlayerController>().input = nP.GetComponentInChildren<InputManager>();
        nP.GetComponent<PlayerController>().playerType = PlayerController.PlayerType.LUIGI;
        nP.GetComponent<PlayerController>().SetPowerup(PlayerController.Powerup.Small);
        if (LevelEditorManager.instance != null){
            LevelEditorManager.instance.monoBehavioursDisableInEditor[this.players.Count] = nP.GetComponent<PlayerController>();
            LevelEditorManager.instance.monoBehavioursEnableInEditor[this.players.Count + 1] = nP.GetComponent<LevelEditorPlayer>();
        }
        nP.GetComponentInChildren<InputManager>().ConnectController();
        this.playerCamera.StartMultiplayerCam();
        nP.GetComponent<SpriteRenderer>().sprite = nP.GetComponent<PlayerController>().currentPlayerSprites.stand[0];
        return nP.GetComponent<PlayerController>();
    }

    public void TryToStopLocalMultiplayer(){
        this.playerCamera.StartSinglePlayerCam();
        for (int i = 1; i < this.players.Count; i++){
            GameManager.Destroy(this.players[i].gameObject);
        }
        PlayerController p1 = this.players[0];
        this.players.Clear();
        this.players.Add(p1);
    }

    public GameObject SpawnFireBall(Vector3 startPos, float speed){
        GameObject fireBall = GameManager.Instantiate(this.fireBallPrefarb);
        fireBall.transform.position = startPos;
        fireBall.GetComponent<FireBall>().VelocityX = speed;
        return fireBall;
    }

    public GameObject SpawnIceBall(Vector3 startPos, float speed){
        GameObject iceBall = GameManager.Instantiate(this.iceBallPrefarb);
        iceBall.transform.position = startPos;
        iceBall.GetComponent<FireBall>().VelocityX = speed;
        return iceBall;
    }

    public void AddCoins(int amount = 1){
        this.coins = this.coins + amount;
        if (this.coins > 99)
            ResetCoins();
        RefreshUITexts();
    }

    public void ResetCoins(){
        this.coins = 0;
        RefreshUITexts();
    }

    public void RefreshUITexts(){
        this.coinsCountText.text = "x" + GameManager.IntToGoodString(this.coins);
    }


    private bool wasSpeed = false;
    private Coroutine timerCor;
    private IEnumerator TimerIE(){
        int time = this.timer;
        
        for (int i = 0; i < this.timer; i++){

            if (time < 102 && !wasSpeed){
                this.wasSpeed = true;
                AudioSource[] audios = GameManager.instance.GetComponents<AudioSource>();
                SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hurryUp);
                audios[0].pitch = 1.1f;
                audios[1].pitch = 1.1f;
            }


            time--;
            this.timerText.text = GameManager.BigIntToGoodString(time);
            yield return new WaitForSeconds(1);
        }

        foreach (PlayerController p in this.players)
            p.Death();
    }

    public void TryToStartTimer(){
        if (this.timer == 999 | (LevelEditorManager.instance != null && !LevelEditorManager.instance.isPlayMode)){
            this.timerText.text = "***";
            return;
        }

        TryToStopTimer();

        this.wasSpeed = false;
        this.timerCor = GameManager.instance.StartCoroutine(this.TimerIE());
    }

    public void TryToStopTimer(){
        AudioSource[] audios = GameManager.instance.GetComponents<AudioSource>();
        audios[0].pitch = 1f;
        audios[1].pitch = 1f;

        if (this.timerCor != null)
            GameManager.instance.StopCoroutine(this.timerCor);
    }

    public void TryToggleCastleLavaFromState(){
        int area = this.currentArea;
        if (LevelEditorManager.instance != null && LevelEditorManager.isLevelEditor)
            area = LevelEditorManager.instance.currentArea;
        LoadFullLevelLiquidSize();
        if ((area == 0 && this.fullLevelLiquidIdArea0 != 0) | (area == 1 && this.fullLevelLiquidIdArea1 != 0)){
            foreach (Lava lava in this.fullLevelLiquid.GetComponents<Lava>())
                GameManager.Destroy(lava);
            foreach (WaterStay water in this.fullLevelLiquid.GetComponents<WaterStay>())
                GameManager.Destroy(water);

            this.fullLevelLiquid.layer = 0;
            if ((area == 0 && this.fullLevelLiquidIdArea0 == 1) | (area == 1 && this.fullLevelLiquidIdArea1 == 1)){
                this.fullLevelLiquid.AddComponent<Lava>();
                this.fullLevelLiquid.GetComponentInChildren<ChildrenTileAnimator>().sprites = new int[4] { 169, 170, 171, 172 };
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(0))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(169, TileManager.TilesetType.ObjectsTileset);
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(1))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(173, TileManager.TilesetType.ObjectsTileset);
            }else if((area == 0 && this.fullLevelLiquidIdArea0 == 2) | (area == 1 && this.fullLevelLiquidIdArea1 == 2)){
                this.fullLevelLiquid.AddComponent<Lava>();
                this.fullLevelLiquid.GetComponentInChildren<ChildrenTileAnimator>().sprites = new int[4] { 185, 186, 187, 188 };
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(0))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(185, TileManager.TilesetType.ObjectsTileset);
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(1))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(189, TileManager.TilesetType.ObjectsTileset);
            }else if((area == 0 && this.fullLevelLiquidIdArea0 == 3) | (area == 1 && this.fullLevelLiquidIdArea1 == 3)){
                this.fullLevelLiquid.AddComponent<WaterStay>();
                this.fullLevelLiquid.layer = 4;
                this.fullLevelLiquid.GetComponentInChildren<ChildrenTileAnimator>().sprites = new int[4] { 190, 191, 192, 193 };
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(0))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(190, TileManager.TilesetType.ObjectsTileset);
                foreach (Transform child in this.fullLevelLiquid.transform.GetChild(1))
                    child.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(194, TileManager.TilesetType.ObjectsTileset);
            }

            GameManager.instance.sceneManager.fullLevelLiquid.SetActive(true);
        }else
            GameManager.instance.sceneManager.fullLevelLiquid.SetActive(false);
    }

    public void LoadFullLevelLiquidSize(){
        int area = this.currentArea;
        if (LevelEditorManager.instance != null && LevelEditorManager.isLevelEditor)
            area = LevelEditorManager.instance.currentArea;
        int size = 0;
        if (area == 0)
            size = this.fullLevelLiquidSizeArea0;
        else
            size = this.fullLevelLiquidSizeArea1;

        this.fullLevelLiquid.GetComponent<BackgroundParallax>().yOffset = size;
        foreach (Transform child in this.fullLevelLiquid.transform.GetChild(1).transform)
            GameManager.Destroy(child.gameObject);

        for (int i = 1; i <= size; i++){
            foreach (Transform child in this.fullLevelLiquid.transform.GetChild(0).transform){
                GameObject clon = GameManager.Instantiate(child.gameObject, this.fullLevelLiquid.transform.GetChild(1));
                clon.transform.position = new Vector3(clon.transform.position.x, clon.transform.position.y - i, clon.transform.position.z);
                int id = 173;
                if ((area == 0 && this.fullLevelLiquidIdArea0 == 2) | (area == 1 && this.fullLevelLiquidIdArea1 == 2))
                    id = 189;
                else if ((area == 0 && this.fullLevelLiquidIdArea0 == 3) | (area == 1 && this.fullLevelLiquidIdArea1 == 3))
                    id = 194;
                clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(id, TileManager.TilesetType.ObjectsTileset);
            }
        }
        this.fullLevelLiquid.GetComponentInChildren<BoxCollider2D>().size = new Vector2(40.9f, 1.5f + size);
        this.fullLevelLiquid.GetComponentInChildren<BoxCollider2D>().offset = new Vector2(-0.02f, -0.69f - (size * 0.5f));
    }

    public static bool CheckBlockToUseOrDestroy(GameObject block, bool isHitDown) {
        if (block.name.Equals("Sprite"))
            block = block.transform.parent.gameObject;

        if (block.GetComponent<ItemBlock>() != null)
            block.GetComponent<ItemBlock>().UseItemBlock(GameManager.instance.sceneManager.players[0].gameObject, isHitDown, true);
        else if (block.GetComponent<OnOffSwitcher>() != null)
            block.GetComponent<OnOffSwitcher>().UseOnOffSwitcher(isHitDown);
        else if (block.GetComponent<MessageBlock>() != null)
            block.GetComponent<MessageBlock>().UseMessageBlock(isHitDown);
        else if (block.GetComponent<Coin>() != null)
            block.GetComponent<Coin>().Collect();
        else
            return false;

        return true;
    }

    public void ChageArea(int area, bool dontChangeMusic = false){
        foreach (Transform child in this.levelBlocksParent.transform){
            child.gameObject.SetActive(false);
        }

        GameObject parent = GetAreaParent(area).gameObject;
        parent.SetActive(true);
        bool yCamera = false;
        foreach (Transform child in parent.transform){
            if (child.position.y >= 26.7)
                yCamera = true;
        }
        this.playerCamera.yCamera = yCamera;

        this.playerCamera.UnfreezeCamera();
        TileManager.instance.LoadTilesetFromMemory(area, dontChangeMusic);
        this.currentArea = area;
        if (LevelEditorManager.isLevelEditor)
            LevelEditorManager.instance.ChangeLevelEditorArea(area, dontChangeMusic);
        if (TileManager.instance.currentTileset.id == TileManager.TilesetID.Airship)
            this.playerCamera.yCamera = true;

        TryToggleCastleLavaFromState();
        this.darkScreen.SetActive(this.isAreaDark[this.currentArea]);
        this.fogClouds.SetActive(this.hasAreaCloudsFog[this.currentArea]);
        SceneManager.onOffState = !SceneManager.onOffState;
        SceneManager.SwitchOnOffState(true);/*To fix some sprite and animations bugs*/
    }

    public Transform GetAreaParent(int area){
        foreach (Transform child in this.levelBlocksParent.transform){
            if (child.name.Equals(area.ToString()))
                return child;
        }

        GameObject gm = new GameObject(area.ToString());
        gm.transform.SetParent(this.levelBlocksParent.transform);
        return gm.transform;
    }

    public GameObject InitTransision1(Transform player){
        GameObject clon = GameManager.Instantiate(this.transision1);
        clon.transform.position = player.transform.position;
        return clon;
    }

    public void StartOnlyPlayModeLevel(string levelName, bool withoutLevelPath = false, string customName = "null"){
        PlayerController.DEATHS = 0;
        GameManager.instance.StartCoroutine(this.StartOnlyPlayModeLevelIE(levelName, withoutLevelPath, customName));
    }

    private IEnumerator StartOnlyPlayModeLevelIE(string levelName, bool withoutLevelPath = false, string customName = "null"){
        this.backgroundMusicManager.StopCurrentBackgroundMusic();
        this.levelStartScreen.SetActive(true);
        string path = GameManager.LEVEL_PATH + levelName;
        if (withoutLevelPath)
            path = levelName;

        if (customName != "null")
            this.levelStartScreenLevelNameText.text = customName;
        else
            this.levelStartScreenLevelNameText.text = levelName.Replace(".umm", "");

        this.levelStartScreenTagsText.text = "No tags";
        try{
            string[] line = GameManager.GetFileIn(path).Split('\n')[2].Split(':');
            string tags = string.Empty;
            foreach(string arg in line){
                if (string.IsNullOrEmpty(arg))
                    continue;
                int i = -1;
                i = GameManager.StringToInt(arg);
                if (i != -1)
                    tags = tags + (UMM.LevalDatas.Tag)i + ",";
            }
            this.levelStartScreenTagsText.text = tags.Remove(tags.Length - 1);
        }catch(Exception e){
            Debug.LogError(e.Message + "|" + e.StackTrace);
        }

        yield return new WaitForSecondsRealtime(0.05f);
        if(LevelEditorManager.isLevelEditor)
            LevelEditorManager.instance.DisableLevelEditor();
        GameManager.instance.ClearCurrentLevel();
        GameManager.StopTimeScale();
        yield return new WaitForSecondsRealtime(3f);

        GameManager.instance.LoadLevelFile(path);
        yield return new WaitForSecondsRealtime(0.1f);
        this.backgroundMusicManager.StartPlayingBackgroundMusic();
        this.levelStartScreen.SetActive(false);
        GameManager.ResumeTimeScale();
    }

    public static void SwitchOnOffState(bool noSound = false){
        if(!noSound)
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.onOffSwitch);
        SceneManager.onOffState = !SceneManager.onOffState;
        foreach(GameObject onOffObject in SceneManager.onOffObjects){
            if (onOffObject != null && onOffObject.GetComponent<OnOffObject>() != null)
                onOffObject.GetComponent<OnOffObject>().OnSwitch(SceneManager.onOffState);
            else if (onOffObject != null && onOffObject.GetComponent<SpeedMoveBlock>() != null)
                onOffObject.GetComponent<SpeedMoveBlock>().ChangeDirectionFromOnOff();
        }
    }

    /*TextBox*/
    public void StartTextBoxText(string text){
        if (Time.timeScale == 0)
            return;

        text = text.Replace("#n", "\n");

        GameManager.StopTimeScale();
        LevelEditorManager.canSwitch = false;
        MenuManager.canOpenMenu = false;
        this.textBoxText.text = string.Empty;
        this.textBoxText.transform.parent.transform.parent.gameObject.SetActive(true);
        this.textBoxText.transform.parent.transform.parent.gameObject.GetComponent<Animator>().enabled = true;
        this.textBoxText.transform.parent.transform.parent.gameObject.GetComponent<Animator>().Play(0);
        GameManager.instance.StartCoroutine(this.TextBoxTextIE(text));
    }

    private IEnumerator TextBoxTextIE(string text){
        yield return new WaitForSecondsRealtime(0.1f);
        InputManager input = InputManager.instances[0];

        for (int i = 0; i <= text.Length; i++){
            float sec = 0.05f;

            if (input.MENU | input.RUN | input.LEVELEDITOR_SWITCHMODE)
                sec = 0;

            this.textBoxText.text = text.Substring(0, i);
            yield return new WaitForSecondsRealtime(sec);
        }

        while(!input.MENU && !input.RUN && !input.LEVELEDITOR_SWITCHMODE){
            yield return new WaitForSecondsRealtime(0);
        }

        EndTextBoxText();
    }

    public void EndTextBoxText(){
        this.textBoxText.transform.parent.transform.parent.gameObject.GetComponent<Animator>().Play("TextBoxClose");
        GameManager.instance.StartCoroutine(EndTextBoxIE());
    }

    private IEnumerator EndTextBoxIE(){
        yield return new WaitForSecondsRealtime(0.15f);
        this.textBoxText.transform.parent.transform.parent.gameObject.GetComponent<Animator>().enabled = false;
        this.textBoxText.transform.parent.transform.parent.gameObject.SetActive(false);
        this.textBoxText.text = string.Empty;
        GameManager.ResumeTimeScale();
        LevelEditorManager.canSwitch = true;
        MenuManager.canOpenMenu = true;
        foreach(PlayerController p in this.players){
            if (p.CheckIfOverPlayerIsABlock())
                p.TryEnableDuck();
        }
    }

    public void RespawnEntities(){
        foreach(RespawnableEntity entity in respawnableEntities){
            try{
                entity.Respawn();
            }catch(Exception e){
                continue;
            }
        }

        GameManager.instance.StartCoroutine(this.SpawnZoneNewCheckIE());
    }

    public static RespawnableEntity GetRespawnableEntityFromEntity(GameObject entity){
        foreach (RespawnableEntity rEntity in respawnableEntities){
            if (rEntity.currentEntity == entity)
                return rEntity;
        }

        return null;
    }

    public static bool EntityWallCheckRay(Transform en, Vector3 vt){
        BoxCollider2D collider = en.GetComponent<BoxCollider2D>();

        if (collider != null && collider.size.y > 1f){
            bool hitSomething = false;

            for (int i = 0; i < collider.size.y; i++){
                Vector3 offset = new Vector3(0.1f, 0.3f - i, 0f);
                RaycastHit2D ray = Physics2D.Raycast(en.position + offset, vt, 0.5f, GameManager.instance.entityWandMask);

                if (ray.collider != null){
                    hitSomething = true;
                }
            }

            return hitSomething;
        }


        RaycastHit2D ray1 = Physics2D.Raycast(en.position + new Vector3(0.1f, 0.3f, 0f), vt, 0.5f, GameManager.instance.entityWandMask);
        return ray1;
    }

    public IEnumerator SpawnZoneNewCheckIE(){
        this.spawnZone.SetActive(false);
        this.spawnZone.GetComponent<BoxCollider2D>().enabled = false;
        yield return new WaitForSecondsRealtime(0.1f);
        this.spawnZone.SetActive(true);
        this.spawnZone.GetComponent<BoxCollider2D>().enabled = true;
        GameManager.instance.isInLevelLoad = false;
    }

    public IEnumerator DeSpawnZoneNewCheckIE(){
        Vector2 size = this.despawnZone.size;
        Vector2 offset = this.despawnZone.offset;
        this.despawnZone.size = new Vector2(38, 63);
        this.despawnZone.offset = new Vector2(4, -17);
        yield return new WaitForSecondsRealtime(0.1f);
        this.despawnZone.size = size;
        this.despawnZone.offset = offset;
    }

    public IEnumerator LevelStartRespawnHelperCheckIE(){
        this.levelStartRespawnHelper.SetActive(true);
        yield return new WaitForSecondsRealtime(0.1f);
        this.levelStartRespawnHelper.SetActive(false);
    }

    public static void DestroyDestroyAfterNewLoadList(){
        foreach(GameObject gm in destroyAfterNewLoad){
            if (gm != null)
                GameManager.Destroy(gm);
        }

        destroyAfterNewLoad.Clear();
    }

    public static void ActivatePSwitch(){
        StopPSwitch();
        BackgroundMusicManager.instance.StopCurrentBackgroundMusic();

        if (!pSwitchState){
            List<GameObject> generated = new List<GameObject>();
            foreach(GameObject gm in pSwitchObjects){
                if (gm.tag.Equals("GeneratedFromPSwitch"))
                    continue;

                if(gm.GetComponent<BreakBlock>() != null){
                    if (gm.GetComponent<BreakBlock>().contentBlock != BlockID.ERASER)
                        continue;

                    GameObject coin = GameManager.Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)BlockID.COIN].prefarb);
                    coin.transform.SetParent(gm.transform.parent);
                    coin.transform.position = gm.transform.position;
                    gm.transform.SetParent(coin.transform, false);
                    coin.gameObject.tag = "GeneratedFromPSwitch";
                    generated.Add(coin);
                    gm.SetActive(false);
                }else if(gm.GetComponent<Coin>() != null){
                    GameObject breakBlock = GameManager.Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)BlockID.BREAK_BLOCK].prefarb);
                    breakBlock.transform.SetParent(gm.transform.parent);
                    breakBlock.transform.position = gm.transform.position;
                    breakBlock.GetComponentInChildren<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(GameManager.StringToInt(gm.transform.parent.ToString()), 1, TileManager.TilesetType.MainTileset);
                    gm.transform.SetParent(breakBlock.transform, false);
                    breakBlock.gameObject.tag = "GeneratedFromPSwitch";
                    generated.Add(breakBlock);
                    CheckToKillEnemy(breakBlock);
                    gm.SetActive(false);
                }else if(gm.GetComponent<Door>() != null)
                    gm.GetComponent<Door>().ActivatePSwitchDoor();
                else if (gm.tag.Equals("PSwitchBlock")){
                    gm.GetComponent<BoxCollider2D>().enabled = false;
                    gm.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(210, TileManager.TilesetType.ObjectsTileset);
                }else if (gm.tag.Equals("PSwitchBlock1")){
                    CheckToKillEnemy(gm);
                    gm.GetComponent<BoxCollider2D>().enabled = true;
                    gm.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(209, TileManager.TilesetType.ObjectsTileset);
                }
            }
            pSwitchObjects.AddRange(generated);
        }

        currentPSwitchCor = GameManager.instance.StartCoroutine(PSwitchIE());
        pSwitchAudioSource = SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.pSwitch);
    }

    private static IEnumerator PSwitchIE(){
        yield return new WaitForSeconds(13);
        StopPSwitch();
        currentPSwitchCor = null;
    }

    public static void CheckToKillEnemy(GameObject gm){
        RaycastHit2D ray1 = Physics2D.Raycast(gm.transform.position + new Vector3(0, 0.5f), Vector2.down, 0.8f, GameManager.instance.stampMask);
            if (ray1 && ray1.collider.gameObject != null){
                if (ray1.collider.gameObject.layer == 9 && ray1.collider.gameObject.GetComponent<PlayerController>().GetOnGround())
                    ray1.collider.gameObject.GetComponent<PlayerController>().Death();
                else if (ray1.collider.gameObject.GetComponent<Entity>())
                    ray1.collider.gameObject.GetComponent<Entity>().StartCoroutine(ray1.collider.gameObject.GetComponent<Entity>().ShootDieAnimation(gm));
       }
    }

    public static void StopPSwitch(){
        if (currentPSwitchCor != null)
            GameManager.instance.StopCoroutine(currentPSwitchCor);
        currentPSwitchCor = null;
        if (pSwitchAudioSource != null)
            GameManager.Destroy(pSwitchAudioSource);
        pSwitchAudioSource = null;

        bool star = false;
        foreach (PlayerController p in GameManager.instance.sceneManager.players)
            if (p.GetIsStar())
                star = true;
        if(!star)
            BackgroundMusicManager.instance.StartPlayingBackgroundMusic();

        List<GameObject> remove = new List<GameObject>();
        foreach (GameObject gm in pSwitchObjects){
            try{
                if (gm == null){
                    remove.Add(gm);
                    continue;
                }

                if (gm.tag.Equals("GeneratedFromPSwitch")){
                    int childIndex = 0;
                    if (gm.GetComponent<BreakBlock>() != null){
                        childIndex = 1;
                    }else{
                        CheckToKillEnemy(gm);
                    }
                    gm.transform.GetChild(childIndex).gameObject.SetActive(true);
                    gm.transform.GetChild(childIndex).transform.SetParent(gm.transform.parent, false);
                    remove.Add(gm);
                    GameManager.Destroy(gm);
                }
                else if (gm.GetComponent<Door>() != null)
                    gm.GetComponent<Door>().DeactivatePSwitchDoor();
                else if (gm.tag.Equals("PSwitchBlock")){
                    gm.GetComponent<BoxCollider2D>().enabled = true;
                    gm.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(209, TileManager.TilesetType.ObjectsTileset);
                    CheckToKillEnemy(gm);
                }else if (gm.tag.Equals("PSwitchBlock1")){
                    gm.GetComponent<BoxCollider2D>().enabled = false;
                    gm.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(210, TileManager.TilesetType.ObjectsTileset);
                }
            }
            catch(Exception e){
                continue;
            }
        }

        foreach(GameObject gm in remove){
            pSwitchObjects.Remove(gm);
        }
    }

    public static void ShakeCamera(){
        if(TileManager.instance.currentTileset.id == TileManager.TilesetID.Airship)
            Camera.main.GetComponent<Animator>().Play("CameraShakeAirship");
        else
            Camera.main.GetComponent<Animator>().Play("CameraShake");
    }

    public static IEnumerator DropGameObject(GameObject gm, float xSpeed, float gravity = 8){
        if (gm != null)
            gm.GetComponent<EntityGravity>().enabled = false;

        bool grounded = false;
        bool cancel = false;
        bool isIceBlock = false;
        if (gm.GetComponent<Entity>() != null)
            isIceBlock = gm.GetComponent<Entity>().isFreezedFromIceBall;
        float grav = 0;
        float iceSpeed = 0;

        float X_onground = 0.31f;
        float Y_onground = 0;

        if(gm.GetComponent<EntityGravity>() != null){
            X_onground = gm.GetComponent<EntityGravity>().X_onground;
            Y_onground = gm.GetComponent<EntityGravity>().Y_onground;
        }

        while ((!grounded | isIceBlock) && !cancel){
            Vector3 vt = Vector3.right;
            if (xSpeed < 0)
                vt = Vector3.left;
            bool dRay = SceneManager.EntityWallCheckRay(gm.transform, vt);
           
            if (dRay){
                if (gm.GetComponent<Entity>() != null && gm.GetComponent<Entity>().isFreezedFromIceBall){
                    gm.GetComponent<Entity>().DestroyIceBlock();
                    break;
                }
                xSpeed = -(xSpeed / 2);
            }

            if (grounded && isIceBlock){
                if (xSpeed > 0){
                    if (iceSpeed < 0)
                        isIceBlock = false;
                    iceSpeed = iceSpeed - 8 * Time.deltaTime;
                }else{
                    if (iceSpeed > 0)
                        isIceBlock = false;
                    iceSpeed = iceSpeed + 8 * Time.deltaTime;
                }
                  gm.transform.Translate(iceSpeed * Time.deltaTime, 0, 0);
            }

            float length = 0.5f;
            if (GameManager.fps < 60)
                length = 0.95f;
            RaycastHit2D ray1 = Physics2D.Raycast(gm.transform.position + new Vector3(X_onground, Y_onground, 0f), Vector2.down, length, GameManager.instance.entityGroundMask);
            
            if (ray1 && !grounded){
                grounded = true;
                iceSpeed = xSpeed;
                    if(isIceBlock)
                        gm.transform.position = new Vector3(gm.transform.position.x, (int)gm.transform.position.y + gm.GetComponent<EntityGravity>().onGroundAdd, gm.transform.position.z);
                }

                if(!grounded)
                    gm.transform.Translate(xSpeed * Time.deltaTime, 0, 0);
            if (!ray1){
                gm.transform.Translate(0, -gravity * grav * Time.deltaTime, 0);
                grav = grav + 8 * Time.deltaTime;
            }
            
            if (gm.transform.parent.gameObject.layer == 9 | (isIceBlock && !gm.GetComponent<Entity>().isFreezedFromIceBall))
                cancel = true;
            yield return new WaitForSeconds(0);
        }

        if (!cancel){
            gm.transform.position = new Vector3(gm.transform.position.x, (int)gm.transform.position.y + 0.7f, gm.transform.position.z);
            if (gm.GetComponent<EntityGravity>() != null){
                gm.GetComponent<EntityGravity>().enabled = true;
                gm.GetComponent<EntityGravity>().onGround = false;
            }
            if (gm.GetComponent<BombEnemy>() != null)
                gm.GetComponent<BombEnemy>().canHit = true;
        }
    }

    public static IEnumerator KillLaserIE(GameObject gm, PlayerController p, bool drop){
        if (gm.GetComponent<Trampoline>() == null){

            yield return new WaitForSeconds(0.01f);
            while (!gm.GetComponent<EntityGravity>().enabled){
                Vector2 vt = Vector2.right;
                if ((p.currentGrabedObject == gm && p.GetComponent<SpriteRenderer>().flipX) | (p.currentGrabedObject != gm && gm.GetComponentInChildren<SpriteRenderer>().flipX))
                    vt = Vector2.left;

                float yOffset = 0;
                if (p.GetPowerup() != PlayerController.Powerup.Small && !drop)
                    yOffset = -2f;
                vt.y = vt.y + yOffset;

                RaycastHit2D ray1 = Physics2D.Raycast(gm.transform.position, vt, 0.6f);
                if (ray1 && ray1.collider.GetComponent<Entity>() != null && ray1.collider.gameObject != gm && ray1.collider.transform.parent != gm.transform && (ray1.collider.GetComponent<Entity>().canDieFromFireBall | ray1.collider.GetComponent<Entity>().canDieFromShell)){
                    if (ray1.collider.GetComponent<Bowser>() != null){
                        ray1.collider.GetComponent<Bowser>().DamageBowser(1);
                        break;
                    }

                    if (!drop)
                        gm.GetComponentInChildren<SpriteRenderer>().flipX = !ray1.collider.GetComponentInChildren<SpriteRenderer>().flipX;
                    ray1.collider.GetComponent<Entity>().StartCoroutine(ray1.collider.GetComponent<Entity>().ShootDieAnimation(ray1.collider.gameObject));
                    if (p.currentGrabedObject == gm){
                        p.UngrabCurrentObject(true);
                        gm.GetComponent<Entity>().StartCoroutine(gm.GetComponent<Entity>().ShootDieAnimation(gm));
                    }
                }
                yield return new WaitForSeconds(0);
            }
        }
    }

    public static void RemoveAllRespawnableEntities(){/*For ending a level*/
        foreach(RespawnableEntity respawnableEntity in respawnableEntities){
            if (respawnableEntity.currentEntity == null)
                continue;

            GameObject eff = GameManager.Instantiate(GameManager.instance.sceneManager.destroyEffect);
            eff.transform.position = respawnableEntity.currentEntity.transform.position;
            GameManager.Destroy(respawnableEntity.currentEntity);
        }

        respawnableEntities.Clear();
    }

    public static IEnumerator EntitySpeedNaturalRestorer(Entity en, float targetSpeed, float multiplerSpeed, float startSpeed){
        en.moveSpeed = startSpeed;

        while(en.moveSpeed < targetSpeed){
            en.moveSpeed = en.moveSpeed + multiplerSpeed * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        en.moveSpeed = targetSpeed;
    }

}
