using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;
using UMM.BlockField;
using System;
using System.IO;

public class LevelEditorManager : MonoBehaviour{

    public static bool isLevelEditor = true;
    public static bool canSwitch = true;

    public MonoBehaviour[] monoBehavioursDisableInEditor;
    public MonoBehaviour[] monoBehavioursEnableInEditor;
    public GameObject editorParent;
    public GameObject playModeCanvas;
    public GameObject blockButtonsParent;
    public GameObject blockButtonPrefarb;
    public GameObject pipeAreButton;
    public GameObject blockFieldPrefarb;
    public GameObject[] sortTypeButtons;
    public AudioClip cutSceneAudioClip;
    public Transform savedPlayerMoveParent;public bool savePlayerMoves = false;private Vector3 savedPlayerMoveLastPos = Vector3.zero;private List<SavedPlayerMove> savedPlayerMoves = new List<SavedPlayerMove>();
    public Sprite blockWidget1;
    public Sprite blockWidget2;
    public LevelEditorBlocksSlider blocksSlider;
    public Color[] editorBackgroundColors;
    public TextMeshPro helperText;

    [System.NonSerialized]public bool isPlayMode = false;
    [System.NonSerialized]public static LevelEditorManager instance;
    [System.NonSerialized]public BlockFieldManager blockFieldManager;
    [System.NonSerialized]public int currentArea = 0;
    [System.NonSerialized]public bool isInEditorLevelLoad = false;

    private string tempLevelPath;
    private bool isInPipeAreaRegister = false;private LevelEditorPipe currentPipeAreaRegisterPipe;
    private GameObject currentStartPoint = null;
    private SortType currentSortType = SortType.BLOCK;

    private BlockDataManager blockDataManager;
    private GameManager gameManager;
    private InputManager input;
    private TileManager tileManager;

    [System.Serializable]
    public class SavedPlayerMove{
        public Vector3 pos;
        public Sprite sprite;
        public bool flip;
    }

    public void Awake(){
        if (instance != null)
            Destroy(this);
        instance = this;
        this.savePlayerMoves = false;
        LevelEditorManager.isLevelEditor = true;
        LevelEditorManager.canSwitch = true;
        this.gameManager = GameManager.instance;
        this.input = InputManager.instances[0];
        this.tileManager = TileManager.instance;
        this.blockDataManager = this.gameManager.blockDataManager;
        this.blockFieldManager = new BlockFieldManager();
        this.blockFieldManager.levelEditorManager = this;
        this.blockFieldManager.blockDataManager = this.blockDataManager;
        this.blockFieldManager.GenerateBlockFields(this.blockFieldPrefarb);
        GameManager.ResumeTimeScale();
        this.currentStartPoint = PlaceBlock(BlockID.STARTPOINT, this.blockFieldManager.GetBlockFieldAt(((2 * this.blockFieldManager.weidth) + 2)));
        this.tempLevelPath = Application.persistentDataPath + "\\tempLevel.lvl";
        this.isPlayMode = true;
        SwitchMode();
        InitBlockButtons(SortType.NULL, this.sortTypeButtons[0], true);
    }

    public void InitBlockButtons(SortType sortType = SortType.NULL, GameObject button = null, bool change = false){
        if (sortType == SortType.NULL){
            sortType = this.currentSortType;
        }else{
            this.blockButtonsParent.transform.localPosition = Vector3.zero;
            this.blocksSlider.SetCurrentValue(0);
        }
        if (button == null)
            button = this.sortTypeButtons[(int)this.currentSortType];

        GameObject lastButton = this.sortTypeButtons[(int)this.currentSortType];
        lastButton.transform.localPosition = new Vector3(lastButton.transform.localPosition.x, 4.87f, lastButton.transform.localPosition.z);
        button.transform.localPosition = new Vector3(button.transform.localPosition.x, 4.4f, 10);
        this.currentSortType = sortType;
        DeleteBlockButtons();
        float x = -3.5f;
        BlockID[] blockIDs = null;
        if (this.currentSortType == SortType.BLOCK)
            blockIDs = this.gameManager.blockDataManager.levelEditorBlocksOrder;
        else if (this.currentSortType == SortType.DECO)
            blockIDs = this.gameManager.blockDataManager.levelEditorDecoOrder;
        else if (this.currentSortType == SortType.ENEMY)
            blockIDs = this.gameManager.blockDataManager.levelEditorEnemiesOrder;
        else if (this.currentSortType == SortType.ITEM)
            blockIDs = this.gameManager.blockDataManager.levelEditorItemsOrder;
        else if (this.currentSortType == SortType.SPECIAL)
            blockIDs = this.gameManager.blockDataManager.levelEditorSpecialOrder;
        if (change)
            this.gameManager.sceneManager.levelEditorCursor.currentBlock = blockIDs[0];

        try
        {
            this.blocksSlider.maxValue = blockIDs.Length + 4;
            foreach (BlockID blockID in blockIDs){
                BlockData blockData = this.gameManager.blockDataManager.blockDatas[(int)blockID];
                if (!blockData.dontDisplayButtonInEditor && blockData.sortType == sortType && (!blockData.onlyForSMB1 | this.tileManager.currentStyle.id == TileManager.StyleID.SMB1)){
                    GameObject btn = Instantiate(this.blockButtonPrefarb);
                    btn.transform.SetParent(this.blockButtonsParent.transform);
                    btn.transform.localPosition = new Vector3(x, 3.8f, 10);
                    if (blockData.spriteId == 0)
                        btn.GetComponentsInChildren<SpriteRenderer>()[1].sprite = blockData.prefarb.GetComponent<SpriteRenderer>().sprite;
                    else
                        btn.GetComponentsInChildren<SpriteRenderer>()[1].sprite = tileManager.GetSpriteFromTileset(blockData.spriteId, blockData.tilesetType);
                    btn.GetComponent<BlockButton>().blockID = blockData.id;
                    x = x + 2f;
                    if (blockData.smallIcon)
                        btn.transform.GetChild(0).transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    else if (blockData.id == BlockID.BIGBILLENEMY | blockData.id == BlockID.REDBIGBILLENEMY)
                        btn.transform.GetChild(0).transform.localScale = new Vector3(0.8f, 0.8f, 1);
                    else if (blockData.id == BlockID.DECOCASTLE)
                        btn.transform.GetChild(0).transform.localScale = new Vector3(0.5f, 0.5f, 1);
                    if (blockData.id == BlockID.BOORING){
                        GameObject c = Instantiate(btn.transform.GetChild(0), btn.transform.GetChild(0)).gameObject;                
                        c.transform.localPosition = btn.transform.GetChild(0).transform.localPosition + new Vector3(0.4f, 0.3f, 0);
                        c.transform.localScale = new Vector3(1,1, 0);
                    }
                    if(blockData.blockGUICustomSize != Vector3.zero)
                        btn.transform.GetChild(0).transform.localScale = blockData.blockGUICustomSize;
                    if (blockID == this.gameManager.sceneManager.levelEditorCursor.currentBlock)
                        btn.GetComponent<BlockButton>().Choose();

                    if (blockData.customEditorPrefarb != null && blockData.customEditorPrefarb.GetComponent<SpriteRenderer>() != null)
                        btn.transform.GetChild(0).GetComponent<SpriteRenderer>().color = blockData.customEditorPrefarb.GetComponent<SpriteRenderer>().color;
                }
            }
        }catch (Exception e){
        }
    }

    public void ChangeSortType(int sortType){
        InitBlockButtons((SortType)sortType, this.sortTypeButtons[sortType], true);
    }

    private void DeleteBlockButtons(){
        foreach (Transform child in this.blockButtonsParent.transform){
            GameObject.Destroy(child.gameObject);
        }
    }

    private void Update(){
        if (this.input.LEVELEDITOR_SWITCHMODE && !this.gameManager.isInMainMenu)
            SwitchMode();
        if (!this.isPlayMode && this.input.LEVELEDITOR_CHANGE_VIEW)
            this.blockButtonsParent.transform.parent.gameObject.SetActive(!this.blockButtonsParent.transform.parent.gameObject.active);

        if (!this.isPlayMode | !this.savePlayerMoves)
            return;

       
       if ((int)this.savedPlayerMoveLastPos.x != (int)this.gameManager.sceneManager.players[0].transform.position.x | (int)this.savedPlayerMoveLastPos.y != (int)this.gameManager.sceneManager.players[0].transform.position.y - 1f){
            foreach(SavedPlayerMove sv in this.savedPlayerMoves){
                if (sv.pos == new Vector3((int)this.gameManager.sceneManager.players[0].transform.position.x, (int)this.gameManager.sceneManager.players[0].transform.position.y, 0) && sv.flip == this.gameManager.sceneManager.players[0].GetComponent<SpriteRenderer>().flipX)
                    return;
            }
            
            GameObject clon = new GameObject("SavedPlayerMove");
            clon.transform.SetParent(this.savedPlayerMoveParent);
            clon.AddComponent<SpriteRenderer>().sprite = this.gameManager.sceneManager.players[0].GetComponent<SpriteRenderer>().sprite;
            clon.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            clon.GetComponent<SpriteRenderer>().flipX = this.gameManager.sceneManager.players[0].GetComponent<SpriteRenderer>().flipX;
            clon.transform.position = this.gameManager.sceneManager.players[0].transform.position;
            this.savedPlayerMoveLastPos = this.gameManager.sceneManager.players[0].transform.position;
            SavedPlayerMove playerMove = new SavedPlayerMove();
            playerMove.pos = new Vector3((int)this.gameManager.sceneManager.players[0].transform.position.x, (int)this.gameManager.sceneManager.players[0].transform.position.y, 0);
            playerMove.sprite = this.gameManager.sceneManager.players[0].GetComponent<SpriteRenderer>().sprite;
            playerMove.flip = this.gameManager.sceneManager.players[0].GetComponent<SpriteRenderer>().flipX;
            this.savedPlayerMoves.Add(playerMove);
       }
    }

    public bool CheckSetContentBlock(BlockData blockData, BlockField blockField, int area, BlockID blockID, GameObject block = null, int arrayId = 0) {
        if (blockData.canPutInItemBlock && blockField.currentBlock[0][area] != null | blockData.canPutInPipe && blockField.currentBlock[0][area] != null){
            if (blockField.blockId[arrayId][area] == BlockID.QUESTION_BLOCK | blockField.blockId[arrayId][area] == BlockID.BREAK_BLOCK | blockField.blockId[arrayId][area] == BlockID.INVISBLE_QUESTION_BLOCK | blockField.blockId[arrayId][area] == BlockID.LONGQUESTIONBLOCK) {
                blockField.currentBlock[0][area].GetComponent<LevelEditorItemBlock>().SetContentBlock(blockID, block);
                return true;
            }else if(blockField.blockId[arrayId][area] == BlockID.PIPE){
                blockField.currentBlock[0][area].GetComponent<LevelEditorPipe>().SetContentBlock(blockID, block);
                return transform;
            }
        }

        if(blockID == BlockID.KEY  && blockField.currentBlock[0][area] != null && this.blockDataManager.blockDatas[(int)blockField.blockId[arrayId][area]].sortType == SortType.ENEMY){
            GameObject keyInEnemy = Instantiate(block);
            keyInEnemy.name = "KeyInEnemy";
            keyInEnemy.transform.position = blockField.currentBlock[0][area].transform.position;
            keyInEnemy.GetComponent<SpriteRenderer>().sortingOrder = 4;
            keyInEnemy.transform.SetParent(blockField.currentBlock[0][area].transform);
            blockField.currentBlock[0][area].layer = 15;/*To save the key in the enemy without to much performance checks*/
            Destroy(block);
            return true;
        }
        return false;
    }

    public GameObject PlaceBlock(BlockID blockID, BlockField blockField, int area = -1, int arrayId = 0){
        if (area == -1)
            area = this.currentArea;
        if (blockID == BlockID.FAKEWALL)
            arrayId = 1;
        if (Time.timeScale == 0 | (blockField.blockId[arrayId][area] != BlockID.CONNECTED_BLOCK && this.blockDataManager.blockDatas[(int)blockField.blockId[arrayId][area]].cantDestroy))
            return null;

        BlockData blockData = this.blockDataManager.blockDatas[(int)blockID];
        bool c = CheckSetContentBlock(blockData, blockField, area, blockID,null , arrayId);
        if (c)
            return null;
        this.blockFieldManager.CheckBlockField(blockField, false, area, arrayId, blockID);

        GameObject block = null;
        if (blockData.customEditorPrefarb != null)
            block = Instantiate(blockData.customEditorPrefarb);
        else
            block = Instantiate(this.blockDataManager.editorTempBlockPrefarb);
        block.transform.position = blockField.blockFieldGameObject.transform.position + blockData.placeOffset;
        block.transform.SetParent(this.editorParent.transform);
        if(!blockData.keepOriginalSprite)
            block.GetComponent<SpriteRenderer>().sprite = tileManager.GetSpriteFromTileset(blockData.spriteId, blockData.tilesetType);

        blockField.currentBlock[arrayId][area] = block;
        blockField.blockId[arrayId][area] = blockID;

        foreach(BlockField blockField1 in this.blockFieldManager.GetBlockFieldsFromNeedSize(blockData.needSize, blockField)){
            if(blockField1.currentBlock[arrayId][area] != null && blockField1.blockId[arrayId][area] == BlockID.STARTPOINT){
                Destroy(block);
                return null;
            }
            this.blockFieldManager.CheckBlockField(blockField1,false,-1, 0,blockID);
            blockField1.currentBlock[arrayId][area] = block;
            blockField1.mainBlockFieldNumber[arrayId][area] = blockField.blockFieldNumber;
            blockField1.blockId[arrayId][area] = BlockID.CONNECTED_BLOCK;
        }

        if(blockID != BlockID.GROUND && blockID != BlockID.FAKEWALL && !blockData.keepOriginalSprite)
            new TileManager.Tile(blockData.tilesetType, blockData.spriteId, block.GetComponent<SpriteRenderer>());
        PlaceBlockSpecialCheck(block, blockField, blockID);
        if(!this.isInEditorLevelLoad)
            StartCoroutine(GroundTileConnect(block.transform.position.x));


        blockField.mainBlockFieldNumber[arrayId][area] = -1;

        /*Check Pipe Area Register*/
        if (this.isInPipeAreaRegister){
            this.gameManager.sceneManager.levelEditorCursor.SetCanBuild(false);
            this.gameManager.sceneManager.levelEditorCursor.canChangeCanBuild = false;
            canSwitch = true;
            ChangeLevelEditorAreaButton();
            block.GetComponent<LevelEditorPipe>().ConnectPipe(false, this.currentPipeAreaRegisterPipe);
            this.currentPipeAreaRegisterPipe.ConnectPipe(true, block.GetComponent<LevelEditorPipe>());
        }
        return block;
    }

    public void PlaceActiveBlock(GameObject block, BlockField blockField, BlockID blockId){
        BlockData blockData = this.blockDataManager.blockDatas[(int)blockId];

        bool c = CheckSetContentBlock(blockData, blockField, this.currentArea, blockId, block);

        try{
            if (c | this.blockDataManager.blockDatas[(int)blockField.blockId[0][this.currentArea]].cantDestroy){
                Destroy(block);
                return;
            }
        }catch(Exception e){
            Destroy(block);
        }

        if (blockField.currentBlock[0][this.currentArea] != block)
            this.blockFieldManager.CheckBlockField(blockField, false, -1, 0, blockId);

        block.transform.position = blockField.blockFieldGameObject.transform.position + blockData.placeOffset;
        block.transform.SetParent(this.editorParent.transform);
        blockField.currentBlock[0][this.currentArea] = block;
        blockField.blockId[0][this.currentArea] = blockId;

        foreach (BlockField blockField1 in this.blockFieldManager.GetBlockFieldsFromNeedSize(blockData.needSize, blockField)){
            if (blockField1.currentBlock[0][this.currentArea] != null && blockField1.blockId[0][this.currentArea] == BlockID.STARTPOINT){
                Destroy(block);
                return;
            }
            this.blockFieldManager.CheckBlockField(blockField1, false, -1, 0, blockId);
            blockField1.currentBlock[0][this.currentArea] = block;
            blockField1.mainBlockFieldNumber[0][this.currentArea] = blockField.blockFieldNumber;
            blockField1.blockId[0][this.currentArea] = BlockID.CONNECTED_BLOCK;
        }

        blockField.mainBlockFieldNumber[0][this.currentArea] = -1;

        PlaceBlockSpecialCheck(block, blockField, blockId, true);
        StartCoroutine(GroundTileConnect(block.transform.position.x));
    }

    public void PlaceBlockSpecialCheck(GameObject block, BlockField blockField, BlockID blockId, bool isAlreadyActiveBlock = false){
        switch (blockId) {
            case BlockID.GROUND:
                block.GetComponent<GroundTileConnector>().blockFieldNumber = blockField.blockFieldNumber;
                block.GetComponent<GroundTileConnector>().fakeWallMaskSpriteID = -1;
                break;
            case BlockID.FAKEWALL:
                block.GetComponent<GroundTileConnector>().blockFieldNumber = blockField.blockFieldNumber;
                block.GetComponent<GroundTileConnector>().fakeWallMaskSpriteID = -1;
                GroundTileConnector.SetAllWithoutPos();
                break;
            case BlockID.MINI_PIPE:
            case BlockID.PIPE:
                block.GetComponent<LevelEditorPipe>().LoadLength(blockField);
                block.GetComponent<LevelEditorPipe>().myBlockField = blockField;
                break;
            case BlockID.LAVA:
                BlockID blockID1 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber + this.blockFieldManager.weidth).blockId[0][this.currentArea];
                if (blockID1 == BlockID.LAVA_BLOCK | blockID1 == BlockID.LAVA) {
                    this.blockFieldManager.CheckBlockField(blockField);
                    PlaceBlock(BlockID.LAVA_BLOCK, blockField);
                }
                BlockField blockField1 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber - this.blockFieldManager.weidth);
                if (blockField1.blockId[0][this.currentArea] == BlockID.LAVA | blockField1.blockId[0][this.currentArea] == BlockID.LAVA_BLOCK){
                    this.blockFieldManager.CheckBlockField(blockField1);
                    PlaceBlock(BlockID.LAVA_BLOCK, blockField1);
                }
                break;
            case BlockID.POISON:
                BlockID blockID2 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber + this.blockFieldManager.weidth).blockId[0][this.currentArea];
                if (blockID2 == BlockID.POISON_BLOCK | blockID2 == BlockID.POISON) {
                    this.blockFieldManager.CheckBlockField(blockField);
                    PlaceBlock(BlockID.POISON_BLOCK, blockField);
                }
                BlockField blockField2 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber - this.blockFieldManager.weidth);
                if (blockField2.blockId[0][this.currentArea] == BlockID.POISON | blockField2.blockId[0][this.currentArea] == BlockID.POISON_BLOCK){
                    this.blockFieldManager.CheckBlockField(blockField2);
                    PlaceBlock(BlockID.POISON_BLOCK, blockField2);
                }
                break;
            case BlockID.WATER:
                BlockID blockID3 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber + this.blockFieldManager.weidth).blockId[0][this.currentArea];
                if (blockID3 == BlockID.WATER_BLOCK | blockID3 == BlockID.WATER) {
                    this.blockFieldManager.CheckBlockField(blockField);
                    PlaceBlock(BlockID.WATER_BLOCK, blockField);
                }
                BlockField blockField3 = this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber - this.blockFieldManager.weidth);
                if (blockField3.blockId[0][this.currentArea] == BlockID.WATER | blockField3.blockId[0][this.currentArea] == BlockID.WATER_BLOCK){
                    this.blockFieldManager.CheckBlockField(blockField3);
                    PlaceBlock(BlockID.WATER_BLOCK, blockField3);
                }
                break;
                break;
            case BlockID.KEY_DOOR:
            case BlockID.PSWITCH_DOOR:
            case BlockID.DOOR:
            case BlockID.WARPBOX:
                if (this.isInEditorLevelLoad | isAlreadyActiveBlock)
                    return;
                this.isInEditorLevelLoad = true;
                GameObject warpBox = PlaceBlock(blockId, this.blockFieldManager.GetBlockFieldAt(blockField.blockFieldNumber + 1));
                this.isInEditorLevelLoad = false;
                block.GetComponent<LevelEditorConnector>().ConnectBlock(true, warpBox);
                if (warpBox == null){
                    Destroy(block);
                    return;
                }
                warpBox.GetComponent<LevelEditorConnector>().ConnectBlock(false, block);
                break;
            case BlockID.BIGBILLENEMY:
            case BlockID.REDBIGBILLENEMY:
                block.GetComponent<LevelEditorRotate>().flipX = true;
                break;
            case BlockID.SPEEDMOVEBLOCK:
                block.GetComponent<LevelEditorSpeedMoveBlock>().LoadLength(blockField);
                break;
            case BlockID.DECOPLATFORM:
                block.GetComponent<LevelEditorDecoPlatform>().LoadLength();
                break;
            case BlockID.VINE:
                block.GetComponent<LevelEditorVine>().LoadLength(blockField);
                break;
            case BlockID.DECOMUSHROOM_PLATFORM:
                block.GetComponent<LevelEditorMushroomPlatform>().LoadLength();
                break;
            case BlockID.FIRE_BAR:
                block.GetComponent<LevelEditorFireBar>().LoadLength();
                break;
            case BlockID.BLUEPLATFORM:
            case BlockID.PLATFORM:
                block.GetComponent<LevelEditorPlatfromLift>().LoadLength(blockField);
                break;
            case BlockID.SPINNY:
            case BlockID.BUZZYBEETLE:
                if (this.blockFieldManager.GetBlockFieldOverBlockField(blockField, true).currentBlock[0][this.currentArea] != null)
                    block.GetComponent<SpriteRenderer>().flipY = true;
                else
                    block.GetComponent<SpriteRenderer>().flipY = false;
                break;
            case BlockID.GREENSKYROTATEBLOCK:
            case BlockID.REDSKYROTATEBLOCK:
                block.GetComponent<LevelEditorSkyRotateBlock>().LoadSize(block.transform.position, blockField);
                break;
        }
    }

    public void SwitchMode(){
        if (!canSwitch)
            return;

        SoundManager.PlayAudioClip(this.cutSceneAudioClip);
        if (!this.gameManager.isInMainMenu)
            BackgroundMusicManager.instance.StartPlayingBackgroundMusic();
        this.isPlayMode = !this.isPlayMode;
        if (this.isPlayMode){
            this.gameManager.sceneManager.levelEditorCursor.transform.localPosition = new Vector3(11, -6, 10);
            SaveLevelAsFile(this.tempLevelPath, true);
            GameManager.instance.LoadLevelFile(this.tempLevelPath);
            ActivatePlayMode();
        }else{
            SceneManager.DestroyDestroyAfterNewLoadList();
            this.savedPlayerMoveParent.gameObject.SetActive(this.savePlayerMoves);
            this.gameManager.sceneManager.TryToStopTimer();
            this.gameManager.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scrooling = false;
            this.gameManager.sceneManager.playerCamera.GetComponent<Animator>().Play("Null");
            SceneManager.StopPSwitch();
            this.gameManager.ClearCurrentLevel();
            if (!this.gameManager.isInMainMenu){
                this.editorParent.SetActive(true);
                this.blockButtonsParent.transform.parent.gameObject.SetActive(true);
                this.playModeCanvas.SetActive(false);
                
            }
            this.gameManager.sceneManager.backgroundMusicManager.StartListingToEditSource();
            this.gameManager.sceneManager.ResetCoins();
            this.tileManager.UpdateTiles();
            GroundTileConnector.SetAllWithoutPos();
            this.gameManager.sceneManager.playerCamera.yCamera = true;
            this.gameManager.sceneManager.playerCamera.UnfreezeCamera();
            GameManager.instance.sceneManager.playerCamera.minY = 19;
            GameManager.instance.sceneManager.fullLevelLiquid.GetComponentInChildren<ChildrenTileAnimator>().enabled = false;
            GameManager.instance.sceneManager.darkScreen.SetActive(false);
            GameManager.instance.sceneManager.fogClouds.SetActive(false);

            if (this.tileManager.currentTileset.id == TileManager.TilesetID.Snow)
                GameManager.instance.sceneManager.snowEffect.GetComponent<ParticleSystem>().Pause();

            if (this.helperText != null && SettingsManager.instance != null){
                this.helperText.text = this.helperText.text.Replace("%Del%", "E");
                this.helperText.text = this.helperText.text.Replace("%Hide%", SettingsManager.instance.GetOption("Editor_ChangeView").ToString());
            }

            foreach (SpriteRenderer sp in GameManager.instance.sceneManager.backgroundRenderers){
                foreach (Transform child in sp.transform){
                    if (child.GetComponent<SpriteRenderer>() == null)
                        Destroy(child.gameObject);
                }
            }

            foreach (MonoBehaviour mono in this.monoBehavioursDisableInEditor){
                if(mono != null)
                    mono.enabled = false;
            }

            foreach(MonoBehaviour mono in this.monoBehavioursEnableInEditor){
                if (mono != null)
                    mono.enabled = true;
            }

            if(this.gameManager.isInMainMenu)
                this.gameManager.sceneManager.backgroundMusicManager.StopCurrentBackgroundMusic();

            if (this.gameManager.isInMainMenu){
                this.gameManager.sceneManager.players[0].transform.position = this.currentStartPoint.transform.position;
                StartCoroutine(MainMenuRestartIE());
                return;
            }
        }
    }

    private IEnumerator MainMenuRestartIE(){
        this.gameManager.isInMainMenu = false;
        if (this.isPlayMode)
            LevelEditorManager.instance.SwitchMode();
        this.gameManager.sceneManager.backgroundMusicManager.StopCurrentBackgroundMusic();
        while (LevelEditorManager.instance.isInEditorLevelLoad | Application.isLoadingLevel)
            yield return new WaitForSeconds(0);
        yield return new WaitForSeconds(0.1f);
        if (!this.isPlayMode){
            LevelEditorManager.instance.SwitchMode();
        }
        this.gameManager.sceneManager.backgroundMusicManager.StopCurrentBackgroundMusic();
        this.gameManager.isInMainMenu = true;
    }

    private IEnumerator PlayModeWaitIE(){
        GameManager.StopTimeScale();
        while (Application.isLoadingLevel)
            yield return new WaitForSeconds(0);
        yield return new WaitForSecondsRealtime(0.01f);
        GameManager.ResumeTimeScale();
    }

    public void ActivatePlayMode(){
        this.savedPlayerMoveParent.gameObject.SetActive(false);
        this.savedPlayerMoves.Clear();
        this.savedPlayerMoveLastPos = this.gameManager.sceneManager.players[0].transform.position;
        foreach (Transform child in this.savedPlayerMoveParent)
            Destroy(child.gameObject);

        this.isPlayMode = true;
        this.editorParent.SetActive(false);
        this.blockButtonsParent.transform.parent.gameObject.SetActive(false);
        this.playModeCanvas.SetActive(true);
        StartCoroutine(PlayModeWaitIE()); 
        this.gameManager.sceneManager.backgroundMusicManager.StartListingToPlayModeSource();

        this.gameManager.sceneManager.TryToStartTimer();
        GameManager.instance.sceneManager.fullLevelLiquid.GetComponentInChildren<ChildrenTileAnimator>().enabled = true;
        if (this.tileManager.currentTileset.id == TileManager.TilesetID.Snow)
            GameManager.instance.sceneManager.snowEffect.GetComponent<ParticleSystem>().Play();

        if (this.gameManager.isInMainMenu)
            this.gameManager.sceneManager.backgroundMusicManager.StopCurrentBackgroundMusic();

        foreach (MonoBehaviour mono in this.monoBehavioursDisableInEditor){
            if (mono != null)
                mono.enabled = true;
        }

        foreach (MonoBehaviour mono in this.monoBehavioursEnableInEditor){
            if (mono != null)
                mono.enabled = false;
        }
    }

    public void ToggleSavePlayerMovesButton(SpriteRenderer sp){
        this.savePlayerMoves = !this.savePlayerMoves;
        if (this.savePlayerMoves)
            sp.color = new Color(1, 0.7960784f, 0, 1);
        else{
            sp.color = Color.white;
            foreach (Transform child in this.savedPlayerMoveParent){
                Instantiate(this.gameManager.sceneManager.destroyEffect).transform.position = child.transform.position;
                Destroy(child.gameObject);
            }
        }
    }

    public void SaveLevelAsFile(string path, bool tempLevel = false){/*blockId:blockFieldNumber:x:y:area*/
        string content = string.Empty;
        content = (int)TileManager.instance.currentStyleID + ":" + this.blockFieldManager.weidth + ":" + this.blockFieldManager.height + ":" + this.gameManager.sceneManager.timer + ":" +  gameManager.sceneManager.fullLevelLiquidIdArea0 + ":" + gameManager.sceneManager.fullLevelLiquidIdArea1 + ":" + gameManager.sceneManager.fullLevelLiquidSizeArea0 + ":" + gameManager.sceneManager.fullLevelLiquidSizeArea1 + ":" + GameManager.BoolToInt(gameManager.sceneManager.isAreaDark[0]) + ":" + GameManager.BoolToInt(gameManager.sceneManager.isAreaDark[1]) + ":" + GameManager.BoolToInt(gameManager.sceneManager.hasAreaCloudsFog[0]) + ":" + GameManager.BoolToInt(gameManager.sceneManager.hasAreaCloudsFog[1]) + ":0" + ":#$§#_+" + GameManager.instance.buildData.USERNAME + "#$§#_+" + this.gameManager.buildData.VERSION_STRING + "\n";/*Style:BlockFieldWeidth;BlockFieldHeight;Timer;isCastleLavaEnabeldArea0, isCatleLavaEnabledArea1, size0, size1*/
        foreach(TileManager.Tileset tileset in this.tileManager.loadedTilesets){/*Save tilesets*/
            if(tileset.id != TileManager.TilesetID.Custom)
                content = content + (int)tileset.id + ":";
            else
                content = content + (int)tileset.id + "=#=" + tileset.customId + ":";
        }
        content = content.Remove(content.Length-1);

        content = content + "\n";

        foreach (UnityEngine.UI.Toggle toggle in GameManager.instance.sceneManager.saveLevelTagToggles){
            if (toggle.isOn)
                content = content + (int)UMM.LevalDatas.LevelDataConverter.StringToTag(toggle.GetComponentInChildren<UnityEngine.UI.Text>().text) + ":";
        }

        bool hasStartPoint = false;

        foreach (BlockField blockField in this.blockFieldManager.blockFields){
            for (int f = 0; f < blockField.currentBlock.Length; f++){
                for(int i = 0; i < blockField.currentBlock[f].Length; i++){ 
                    GameObject block = blockField.currentBlock[f][i];
                    BlockID id = blockField.blockId[f][i];

                    if (block != null && id != BlockID.CONNECTED_BLOCK){
                        string line = (int)id + ":" + blockField.blockFieldNumber + "&" + f + ":" + block.transform.position.x + ":" + block.transform.position.y + ":" + i;

                        switch (id){
                            case BlockID.STARTPOINT:
                                hasStartPoint = true;
                                break;
                            case BlockID.FAKEWALL:
                            case BlockID.GROUND:
                                line = line + ":" + block.GetComponent<GroundTileConnector>().groundTileID + ":" + block.GetComponent<GroundTileConnector>().fakeWallMaskSpriteID;
                                break;
                            case BlockID.BREAK_BLOCK:
                            case BlockID.QUESTION_BLOCK:
                            case BlockID.INVISBLE_QUESTION_BLOCK:
                            case BlockID.LONGQUESTIONBLOCK:
                                line = line + ":" + (int)block.GetComponent<LevelEditorItemBlock>().GetContentBlock() + ":" + block.GetComponent<LevelEditorItemBlock>().extraNumber;
                                break;
                            case BlockID.MINI_PIPE:
                            case BlockID.PIPE:
                                LevelEditorPipe pipe = block.GetComponent<LevelEditorPipe>();
                                if (pipe.connetedPipe != null) {
                                    if (pipe.isMainPipe) {
                                        string mainPipeLine = line + ":" + pipe.pipeLength + ":" + (int)pipe.contentBlock + ":" + pipe.gameObject.transform.eulerAngles.z;
                                        LevelEditorPipe connectedPipe = pipe.connetedPipe;
                                        string connectedPipeLine = ":" + connectedPipe.myBlockField.blockFieldNumber + ":" + connectedPipe.transform.position.x
                                            + ":" + connectedPipe.transform.position.y
                                            + ":" + this.blockFieldManager.GetAreaFromBlockFieldBlock(connectedPipe.myBlockField, connectedPipe.gameObject)
                                            + ":" + connectedPipe.pipeLength + ":" + (int)connectedPipe.contentBlock
                                            + ":" + connectedPipe.gameObject.transform.eulerAngles.z;
                                        line = mainPipeLine + connectedPipeLine;
                                    }
                                    else
                                        line = string.Empty;
                                } else
                                    line = line + ":" + pipe.pipeLength + ":" + (int)pipe.contentBlock + ":" + pipe.gameObject.transform.eulerAngles.z;
                                break;
                            case BlockID.KEY_DOOR:
                            case BlockID.PSWITCH_DOOR:
                            case BlockID.DOOR:
                            case BlockID.WARPBOX:
                                if (block.GetComponent<LevelEditorConnector>().isMainConnector){
                                    if (block.GetComponent<LevelEditorConnector>().connectedBlock == null){
                                        Destroy(block);
                                        continue;
                                    }
                                    Transform connectedWarpBox = block.GetComponent<LevelEditorConnector>().connectedBlock.transform;
                                    line = line + ":" + connectedWarpBox.position.x + ":" + connectedWarpBox.position.y + ":" + this.blockFieldManager.GetBlockFieldDirectFromCurrentBlock(connectedWarpBox.gameObject, i).blockFieldNumber;
                                }else
                                    line = string.Empty;
                                break;
                            case BlockID.MESSAGEBLOCK:
                                line = line + ":" + block.GetComponent<LevelEditorMessageBlock>().text;
                                break;
                            case BlockID.AIRBUBBLES:
                            case BlockID.BURNER:
                            case BlockID.DISABLEDBURNER:
                            case BlockID.BIGBILLENEMY:
                            case BlockID.REDBIGBILLENEMY:
                            case BlockID.DECOARROW:
                            case BlockID.ONEWAYWALL:
                                line = line + ":" + block.gameObject.transform.eulerAngles.z;
                                break;
                            case BlockID.MYSTERY_MUSHROM:
                                line = line + ":" + block.GetComponent<LevelEditorMysteryMushrom>().costumeNumber;
                                break;
                            case BlockID.BLUEPLATFORM:
                            case BlockID.PLATFORM:
                                line = line + ":" + (int)block.GetComponent<LevelEditorPlatfromLift>().direction + ":" + block.GetComponent<LevelEditorPlatfromLift>().length;
                                break;
                            case BlockID.SPEEDMOVEBLOCK:
                                line = line + ":" + block.GetComponent<LevelEditorSpeedMoveBlock>().length + ":" + (int)block.GetComponent<LevelEditorSpeedMoveBlock>().direction + ":" + block.GetComponent<LevelEditorSpeedMoveBlock>().fast + ":" + (int)block.GetComponent<LevelEditorSpeedMoveBlock>().onOffState;
                                break;
                            case BlockID.DECOPLATFORM:
                                line = line + ":" + block.GetComponent<LevelEditorDecoPlatform>().lengthX + ":" + block.GetComponent<LevelEditorDecoPlatform>().lengthY + ":" + block.GetComponent<LevelEditorDecoPlatform>().middleSpriteIDX;
                                break;
                            case BlockID.VINE:
                                line = line + ":" + block.GetComponent<LevelEditorVine>().length;
                                break;
                            case BlockID.DECOMUSHROOM_PLATFORM:
                                line = line + ":" + block.GetComponent<LevelEditorMushroomPlatform>().lengthX + ":" + block.GetComponent<LevelEditorMushroomPlatform>().lengthY + ":" + block.GetComponent<LevelEditorMushroomPlatform>().type;
                                break;
                            case BlockID.FIRE_BAR:
                                line = line + ":" + block.GetComponent<LevelEditorFireBar>().lengthY + ":" + block.gameObject.transform.GetChild(0).eulerAngles.z + ":" + block.GetComponent<LevelEditorFireBar>().direction;
                                break;
                            case BlockID.BILLBLASTER:
                                line = line + ":" + block.GetComponent<LevelEditorBillBlaster>().length;
                                break;
                            case BlockID.GREENSKYROTATEBLOCK:
                            case BlockID.REDSKYROTATEBLOCK:
                                line = line + ":" + block.GetComponent<LevelEditorSkyRotateBlock>().size;
                                break;
                        }

                        if (block.layer == 15)
                            line = line + ":key";

                        if (!string.IsNullOrEmpty(line))
                            content = content + "\n" + line;
                        
                    }
                }
            }
        }

        if (!hasStartPoint){
            this.currentStartPoint = PlaceBlock(BlockID.STARTPOINT, this.blockFieldManager.GetBlockFieldAt(((2 * this.blockFieldManager.weidth) + 2)));
            Debug.LogError("No Startpoint was found!");
        }

        if (this.gameManager.buildData.encryptLevels){
            content = GameManager.Encrypt(content, GameManager.Decrypt(this.gameManager.buildData.levelKey, this.gameManager.buildData.levelKey2));
        }

        GameManager.SaveFile(path, content);
    }

    public void DeleteCurrentEditorLevel(bool dontGenerateStartPoint = false){
        try{
            if (this.currentStartPoint != null){
                BlockField blockField = this.blockFieldManager.GetBlockFieldDirectFromCurrentBlock(this.currentStartPoint);
                if(blockField != null)
                    blockField.blockId[0][0] = BlockID.GROUND;

                Destroy(this.currentStartPoint);
            }
            this.currentStartPoint = null;
        foreach (BlockField blockField in this.blockFieldManager.blockFields){
            this.blockFieldManager.DestroyBlockFieldCurrentBlock(blockField, false, 0, 0);
            this.blockFieldManager.DestroyBlockFieldCurrentBlock(blockField, false, 1, 0);
            this.blockFieldManager.DestroyBlockFieldCurrentBlock(blockField, false, 0, 1);
            this.blockFieldManager.DestroyBlockFieldCurrentBlock(blockField, false, 1, 1);
        }
        ChangeLevelEditorArea(0);

        foreach(Transform trans in this.editorParent.transform){
                if (trans.CompareTag("S"))
                    Destroy(trans.gameObject);
            }    
        
        if (!dontGenerateStartPoint)
            this.currentStartPoint = PlaceBlock(BlockID.STARTPOINT, this.blockFieldManager.GetBlockFieldAt(((2 * this.blockFieldManager.weidth) + 2)));
        this.blockFieldManager.weidth = 420;
        this.blockFieldManager.GenerateBlockFields(this.blockFieldPrefarb);
        }catch (Exception ex){
            Debug.LogException(ex);
        }
    }

    public void LoadLevelInEditor(string path){
        string fileContent = GameManager.GetFileIn(path);
        if (!fileContent.Substring(1, 1).Contains(":")){
            try{
                fileContent = GameManager.Decrypt(fileContent, GameManager.Decrypt(this.gameManager.buildData.levelKey, this.gameManager.buildData.levelKey2));
            }catch(Exception ex){
                Debug.LogException(ex);
                LoadLevelInEditor(Application.streamingAssetsPath + "\\ErrorLevel.umm");
                return;
            }
        }

        this.isInEditorLevelLoad = true;
        DeleteCurrentEditorLevel(true);

        if (1.4f.ToString().Contains(","))
            fileContent = fileContent.Replace(".", ",");
        else
            fileContent = fileContent.Replace(",", ".");

        string[] lines = fileContent.Split('\n');
        fileContent = string.Empty;

        /**/
        string[] firstArgs = lines[0].Split(':');
        this.tileManager.LoadStyleWithInt(GameManager.StringToInt(firstArgs[0]));
        int blockFieldWeidth = GameManager.StringToInt(firstArgs[1]);
        int blockFieldHeight = GameManager.StringToInt(firstArgs[2]);
        if (blockFieldWeidth != this.blockFieldManager.weidth | blockFieldHeight != this.blockFieldManager.height){
            this.blockFieldManager.weidth = blockFieldWeidth;
            this.blockFieldManager.height = blockFieldHeight;
            this.blockFieldManager.GenerateBlockFields(this.blockFieldPrefarb);
        }
        int timer = GameManager.StringToInt(firstArgs[3]);
        if (timer == 0)
            timer = 999;
        this.gameManager.sceneManager.timer = timer;

        gameManager.sceneManager.fullLevelLiquidIdArea0 = GameManager.StringToInt(firstArgs[4]);
        gameManager.sceneManager.fullLevelLiquidIdArea1 = GameManager.StringToInt(firstArgs[5]);
        try{
            gameManager.sceneManager.fullLevelLiquidSizeArea0 = GameManager.StringToInt(firstArgs[6]);
            gameManager.sceneManager.fullLevelLiquidSizeArea1 = GameManager.StringToInt(firstArgs[7]);
        }catch(Exception e){
            gameManager.sceneManager.fullLevelLiquidSizeArea0 = 0;
            gameManager.sceneManager.fullLevelLiquidSizeArea1 = 0;
            Debug.LogError(e.Message + "|" + e.StackTrace);
        }
        if (firstArgs.Length > 8){
            gameManager.sceneManager.isAreaDark[0] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[8]));
            gameManager.sceneManager.isAreaDark[1] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[9]));
            gameManager.sceneManager.hasAreaCloudsFog[0] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[10]));
            gameManager.sceneManager.hasAreaCloudsFog[1] = GameManager.IntToBool(GameManager.StringToInt(firstArgs[11]));
        }else{
            gameManager.sceneManager.isAreaDark[0] = false;
            gameManager.sceneManager.isAreaDark[1] = false;
            gameManager.sceneManager.hasAreaCloudsFog[0] = false;
            gameManager.sceneManager.hasAreaCloudsFog[1] = false;
        }
        UpdateHasFog();
        UpdateIsDark();
        string[] tilesets = lines[1].Split(':');
        for (int i = 0; i < tilesets.Length; i++){/*Load tilesets*/
            if (tilesets[i].Contains("=#=")){
                TileManager.Tileset tileset = ModManager.GetTilesetFromCustomId(tilesets[i].Split(new string[] { "=#=" }, System.StringSplitOptions.None)[1]);
                if(tileset == null)
                    this.tileManager.LoadTilesetToMemoryWithId(TileManager.TilesetID.Plain, i);
                else
                    this.tileManager.LoadTilesetToMemory(tileset, i);
            }else
                this.tileManager.LoadTilesetToMemoryWithId((TileManager.TilesetID)GameManager.StringToInt(tilesets[i]), i);
        }
        this.tileManager.LoadTilesetFromMemory(0);

        foreach (UnityEngine.UI.Toggle toggle in this.gameManager.sceneManager.saveLevelTagToggles){
            toggle.SetIsOnWithoutNotify(false);
        }

        foreach (string arg in lines[2].Split(':')){
            if (string.IsNullOrEmpty(arg))
                continue;
            int i = -1;
            i = GameManager.StringToInt(arg);
            if(i != -1){
                UMM.LevalDatas.Tag tag = (UMM.LevalDatas.Tag)i;
                foreach (UnityEngine.UI.Toggle toggle in this.gameManager.sceneManager.saveLevelTagToggles) {
                    if (tag == UMM.LevalDatas.LevelDataConverter.StringToTag(toggle.GetComponentInChildren<UnityEngine.UI.Text>().text)){
                        toggle.SetIsOnWithoutNotify(true);
                        break;
                    }
                }
            }
        }

        lines[0] = string.Empty;
        lines[1] = string.Empty;
        lines[2] = string.Empty;
        foreach (string line in lines){
            try { 
            if (!string.IsNullOrEmpty(line)){
                string[] args = line.Split(':');
                int id = GameManager.StringToInt(args[0]);
                if (!args[1].Contains("&"))
                    args[1] = args[1] + "&" + "0";
                int blockFieldId = GameManager.StringToInt(args[1].Split('&')[0]);
                int blockFieldArrayId = GameManager.StringToInt(args[1].Split('&')[1]);
                int area = GameManager.StringToInt(args[4]);
                GameObject block = PlaceBlock((BlockID)id, this.blockFieldManager.GetBlockFieldAt(blockFieldId), area, blockFieldArrayId);
                switch ((BlockID)id){
                    case BlockID.FAKEWALL:
                    case BlockID.GROUND:
                        int tileID = GameManager.StringToInt(args[5]);
                        block.GetComponent<GroundTileConnector>().blockFieldNumber = blockFieldId;
                        // block.GetComponent<SpriteRenderer>().sprite = this.tileManager.loadedTilesets[area].mainTileset[tileID];
                        block.GetComponent<GroundTileConnector>().spWithoutStart(tileID);
                        block.GetComponent<GroundTileConnector>().fakeWallMaskSpriteID = GameManager.StringToInt(args[6]);
                            break;
                    case BlockID.STARTPOINT:
                        this.currentStartPoint = block;
                        break;
                    case BlockID.BREAK_BLOCK:
                    case BlockID.QUESTION_BLOCK:
                    case BlockID.INVISBLE_QUESTION_BLOCK:
                    case BlockID.LONGQUESTIONBLOCK:
                        BlockID contentID = (BlockID)GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorItemBlock>().SetContentBlock(contentID);
                            if (contentID == BlockID.MYSTERY_MUSHROM){
                               /* if (GameManager.instance.mysteryCostumesManager.mysteryCostumes[GameManager.StringToInt(args[6])].isPatreon && !GameManager.IS_PATREON){
                                    block.GetComponent<LevelEditorItemBlock>().extraNumber = 0;
                                    continue;
                                }*/

                                block.GetComponent<LevelEditorItemBlock>().extraNumber = GameManager.StringToInt(args[6]);
                            }
                        break;
                   case BlockID.MINI_PIPE:
                   case BlockID.PIPE:
                        LevelEditorPipe pipe = block.GetComponent<LevelEditorPipe>();

                        pipe.pipeLength = GameManager.StringToInt(args[5]);
                        pipe.GetComponent<LevelEditorPipe>().SetContentBlock((BlockID)GameManager.StringToInt(args[6]));
                        pipe.LoadLength(this.blockFieldManager.GetBlockFieldAt(blockFieldId));
                        block.transform.eulerAngles = new Vector3(block.transform.eulerAngles.x, block.transform.eulerAngles.y, GameManager.StringToInt(args[7]));
                        block.transform.position = new Vector3(GameManager.StringToFloat(args[2]), GameManager.StringToFloat(args[3]), 0);
                        if(args.Length > 9){/*Connecting Pipe*/
                            int connectedPipeBlockFieldId = GameManager.StringToInt(args[8]);
                            float connectedPipeX = GameManager.StringToFloat(args[9]);
                            float connectedPipeY = GameManager.StringToFloat(args[10]);
                            int connectedPipeArea = GameManager.StringToInt(args[11]);
                            int connectedPipeLength = GameManager.StringToInt(args[12]);
                            int connectedPipeContentBlock = GameManager.StringToInt(args[13]);
                            int connectedPipeRotationZ = GameManager.StringToInt(args[14]);

                            ChangeLevelEditorArea(connectedPipeArea);
                            LevelEditorPipe connectedPipe = PlaceBlock((BlockID)id, this.blockFieldManager.GetBlockFieldAt(connectedPipeBlockFieldId), connectedPipeArea).GetComponent<LevelEditorPipe>();

                            connectedPipe.pipeLength = connectedPipeLength;
                            connectedPipe.SetContentBlock((BlockID)connectedPipeContentBlock);
                            connectedPipe.LoadLength(this.blockFieldManager.GetBlockFieldAt(connectedPipeBlockFieldId));
                            connectedPipe.transform.eulerAngles = new Vector3(connectedPipe.transform.eulerAngles.x, connectedPipe.transform.eulerAngles.y, connectedPipeRotationZ);
                            connectedPipe.transform.position = new Vector3(connectedPipeX, connectedPipeY, 0);

                            pipe.ConnectPipe(true, connectedPipe);
                            connectedPipe.ConnectPipe(false, pipe);
                        }
                        break;
                    case BlockID.KEY_DOOR:
                    case BlockID.PSWITCH_DOOR:
                    case BlockID.DOOR:
                    case BlockID.WARPBOX:
                        int warpBox2FieldID = GameManager.StringToInt(args[7]);
                        GameObject warpbox2 = PlaceBlock((BlockID)id, this.blockFieldManager.GetBlockFieldAt(warpBox2FieldID), area);
                        block.GetComponent<LevelEditorConnector>().ConnectBlock(true, warpbox2);
                        warpbox2.GetComponent<LevelEditorConnector>().ConnectBlock(false, block);
                        break;
                    case BlockID.MESSAGEBLOCK:
                        block.GetComponent<LevelEditorMessageBlock>().text = args[5];
                        break;
                    case BlockID.AIRBUBBLES:
                    case BlockID.BURNER:
                    case BlockID.DISABLEDBURNER:
                    case BlockID.BIGBILLENEMY:
                    case BlockID.REDBIGBILLENEMY:
                    case BlockID.DECOARROW:
                    case BlockID.ONEWAYWALL:
                        float rotateZ = GameManager.StringToFloat(args[5]);
                        if ((id == (int)BlockID.BIGBILLENEMY | id == (int)BlockID.REDBIGBILLENEMY) && rotateZ == 180)
                            block.GetComponent<SpriteRenderer>().flipY = true;

                        block.transform.eulerAngles = new Vector3(block.transform.eulerAngles.x, block.transform.eulerAngles.y, rotateZ);
                        block.transform.position = new Vector3(GameManager.StringToFloat(args[2]), GameManager.StringToFloat(args[3]), 0);
                        break;
                    case BlockID.MYSTERY_MUSHROM:
                        /*if(GameManager.instance.mysteryCostumesManager.mysteryCostumes[GameManager.StringToInt(args[5])].isPatreon && !GameManager.IS_PATREON){
                            block.GetComponent<LevelEditorMysteryMushrom>().costumeNumber = 0;
                            continue;
                        }*/
                        
                        block.GetComponent<LevelEditorMysteryMushrom>().costumeNumber = GameManager.StringToInt(args[5]);
                        break;
                    case BlockID.BLUEPLATFORM:
                    case BlockID.PLATFORM:
                        block.GetComponent<LevelEditorPlatfromLift>().ChangeDirection((LiftHelper.Direction)GameManager.StringToInt(args[5]));
                        block.GetComponent<LevelEditorPlatfromLift>().length = GameManager.StringToInt(args[6]);
                        block.GetComponent<LevelEditorPlatfromLift>().LoadLength(this.blockFieldManager.GetBlockFieldAt(blockFieldId));
                        break;
                    case BlockID.SPEEDMOVEBLOCK:
                        block.GetComponent<LevelEditorSpeedMoveBlock>().length = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorSpeedMoveBlock>().LoadLength(this.blockFieldManager.GetBlockFieldAt(blockFieldId));
                        if (GameManager.StringToInt(args[7]) == 1)
                            block.GetComponent<LevelEditorSpeedMoveBlock>().ChangeIsFast();
                        block.GetComponent<LevelEditorSpeedMoveBlock>().onOffState = (SpeedMoveBlock.OnOffState)GameManager.StringToFloat(args[8]);
                        block.GetComponent<LevelEditorSpeedMoveBlock>().direction = (LiftHelper.Direction)GameManager.StringToInt(args[6]);
                        block.GetComponent<LevelEditorSpeedMoveBlock>().LoadDirection();
                        break;
                    case BlockID.DECOPLATFORM:
                        block.GetComponent<LevelEditorDecoPlatform>().lengthX = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorDecoPlatform>().lengthY = GameManager.StringToInt(args[6]);
                        int middleSpriteID = GameManager.StringToInt(args[7]);
                        if (middleSpriteID == 59)
                            block.GetComponent<LevelEditorDecoPlatform>().ChangeType();
                        else if(middleSpriteID == 62){
                            block.GetComponent<LevelEditorDecoPlatform>().ChangeType();
                            block.GetComponent<LevelEditorDecoPlatform>().ChangeType();
                        }
                        block.GetComponent<LevelEditorDecoPlatform>().LoadLength();
                        break;
                    case BlockID.VINE:
                        block.GetComponent<LevelEditorVine>().length = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorVine>().LoadLength(this.blockFieldManager.GetBlockFieldAt(blockFieldId));
                        break;
                    case BlockID.DECOMUSHROOM_PLATFORM:
                        block.GetComponent<LevelEditorMushroomPlatform>().lengthX = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorMushroomPlatform>().lengthY = GameManager.StringToInt(args[6]);
                        block.GetComponent<LevelEditorMushroomPlatform>().type = GameManager.StringToInt(args[7]);
                        block.GetComponent<LevelEditorMushroomPlatform>().LoadLength();
                        break;
                    case BlockID.FIRE_BAR:
                        block.GetComponent<LevelEditorFireBar>().lengthY = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorFireBar>().LoadLength();
                        block.transform.GetChild(0).eulerAngles = new Vector3(0, 0, GameManager.StringToFloat(args[6]));
                        if (GameManager.StringToInt(args[7]) == 1)
                            block.GetComponent<LevelEditorFireBar>().ChangeDirection();
                        break;
                    case BlockID.BILLBLASTER:
                        block.GetComponent<LevelEditorBillBlaster>().length = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorBillBlaster>().LoadLength();
                        break;
                    case BlockID.GREENSKYROTATEBLOCK:
                    case BlockID.REDSKYROTATEBLOCK:
                        block.GetComponent<LevelEditorSkyRotateBlock>().size = GameManager.StringToInt(args[5]);
                        block.GetComponent<LevelEditorSkyRotateBlock>().LoadSize(block.transform.position, this.blockFieldManager.GetBlockFieldAt(blockFieldId));
                        break;
                }
                if (args[args.Length - 1].Equals("key")){
                        GameObject keyInEnemy = Instantiate(this.blockDataManager.editorTempBlockPrefarb);
                        keyInEnemy.GetComponent<SpriteRenderer>().sprite = tileManager.GetSpriteFromTileset(177, TileManager.TilesetType.ObjectsTileset);
                        keyInEnemy.name = "KeyInEnemy";
                        keyInEnemy.GetComponent<SpriteRenderer>().sortingOrder = 4;
                        keyInEnemy.transform.position = block.transform.position;
                        keyInEnemy.transform.SetParent(block.transform);
                        block.layer = 15;
                    }
                }
            }catch(Exception e){
                Debug.LogError("ERROR LOADING EDITOR-LEVEL: " + e.Message + "|" + e.StackTrace);
                continue;
            }
        }
        this.isInEditorLevelLoad = false;
        ChangeLevelEditorArea(0, false);
        string name = path;
        name = name.Replace(GameManager.LEVEL_PATH, "");
        name = name.Replace(".umm", "");
        this.gameManager.sceneManager.levelNameInput.SetTextWithoutNotify(name);
        StartCoroutine(LoadLevelSafetlyIE());
        SetCastleLavaStateText();
    }

    private IEnumerator LoadLevelSafetlyIE(){
        while (Application.isLoadingLevel){
            yield return new WaitForSecondsRealtime(0);
        }

        yield return new WaitForSecondsRealtime(1);
        ChangeLevelEditorArea(0);
    }

    public IEnumerator GroundTileConnect(float x){
        GroundTileConnector.SetAll(x);
        yield return new WaitForSeconds(0.1f);
        GroundTileConnector.SetAll(x);
        yield return new WaitForSeconds(0.1f);
        GroundTileConnector.SetAll(x);
    }

    public void MoveBlockButtonsParent(float add){
        if (add > 0 && this.blockButtonsParent.transform.localPosition.x == 0)
            return;

        this.blockButtonsParent.transform.position = this.blockButtonsParent.transform.position + new Vector3(add, 0,0);
    }

    public void ChangeLevelEditorArea(int area, bool dontChangeMusic = false){
        foreach(BlockField blockField in this.blockFieldManager.blockFields){
            GameObject gm1 = blockField.currentBlock[0][this.currentArea];
            GameObject gm2 = blockField.currentBlock[1][this.currentArea];
            if (gm1 != null){
                gm1.SetActive(false);
                if (blockField.blockId[0][this.currentArea] == BlockID.GROUND){
                    gm1.layer = 0;
                    gm1.GetComponent<GroundTileConnector>().canSet = false;
                }
            }

            if (gm2 != null){
                gm2.SetActive(false);
                if (blockField.blockId[1][this.currentArea] == BlockID.FAKEWALL){
                    gm2.layer = 0;
                    gm2.GetComponent<GroundTileConnector>().canSet = false;
                }
            }

            if (blockField.currentBlock[0][area] != null){
                blockField.currentBlock[0][area].SetActive(true);
                if (blockField.blockId[0][area] == BlockID.GROUND){
                    blockField.currentBlock[0][area].layer = 16;
                    blockField.currentBlock[0][area].GetComponent<GroundTileConnector>().canSet = true;
                }
            }
            if (blockField.currentBlock[1][area] != null){
                blockField.currentBlock[1][area].SetActive(true);
                if (blockField.blockId[1][this.currentArea] == BlockID.FAKEWALL){
                    blockField.currentBlock[1][area].layer = 30;
                    blockField.currentBlock[1][area].GetComponent<GroundTileConnector>().canSet = true;
                }
            }
        }
        this.currentArea = area;
        this.tileManager.LoadTilesetFromMemory(area, dontChangeMusic);
        this.gameManager.sceneManager.editorAreaSwitchButtonText.text = area.ToString();

        gameManager.sceneManager.TryToggleCastleLavaFromState();
        SetCastleLavaStateText();
        UpdateIsDark();
        UpdateHasFog();
    }

    public void SetCastleLavaStateText(){
        int id = 0;
        if (this.currentArea == 0)
            id = gameManager.sceneManager.fullLevelLiquidIdArea0;
        else
            id = gameManager.sceneManager.fullLevelLiquidIdArea1;

        if (id == 1)
            this.gameManager.sceneManager.lavaEnabledStateText.text = "Lava";
        else if (id == 2)
            this.gameManager.sceneManager.lavaEnabledStateText.text = "Poison";
        else if (id == 3)
            this.gameManager.sceneManager.lavaEnabledStateText.text = "Water";
        else
            this.gameManager.sceneManager.lavaEnabledStateText.text = "Off";

        this.gameManager.sceneManager.fullLevelLiquidImage.sprite = this.gameManager.sceneManager.fullLevelLiquidSprites[id];
    }

     public void StartMovingFullLevelLiquidSize(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        StartCoroutine(MovingLengthFullLevelLiquidSizeIE(button));
    }

    private IEnumerator MovingLengthFullLevelLiquidSizeIE(GameObject button){
        int orgY = 0;
        if(this.transform.eulerAngles.z == 0)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
        else if (this.transform.eulerAngles.z == -90 | this.transform.eulerAngles.z == 270 | this.transform.eulerAngles.z == 90)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                if (this.currentArea == 0)
                    gameManager.sceneManager.fullLevelLiquidSizeArea0++;
                else
                    gameManager.sceneManager.fullLevelLiquidSizeArea1++;
                gameManager.sceneManager.LoadFullLevelLiquidSize();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY && ((this.currentArea == 0 && gameManager.sceneManager.fullLevelLiquidSizeArea0 != 1) | (this.currentArea == 1 && gameManager.sceneManager.fullLevelLiquidSizeArea1 != 1))){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                if (this.currentArea == 0)
                    gameManager.sceneManager.fullLevelLiquidSizeArea0--;
                else
                    gameManager.sceneManager.fullLevelLiquidSizeArea1--;
                gameManager.sceneManager.LoadFullLevelLiquidSize();
            }

            yield return new WaitForSeconds(0);
        }
        
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.transform.parent.gameObject.SetActive(false);
    }

    public void ResetFullLevelLiquidSite(){
        if (this.currentArea == 0)
            gameManager.sceneManager.fullLevelLiquidSizeArea0 = 0;
        else
            gameManager.sceneManager.fullLevelLiquidSizeArea1 = 0;
        gameManager.sceneManager.LoadFullLevelLiquidSize();
    }

    public void ChangeLevelEditorAreaButton(){
        if (this.currentArea == 0)
            ChangeLevelEditorArea(1);
        else
            ChangeLevelEditorArea(0);
        GroundTileConnector.SetAllWithoutPos();
    }

    /*Pipe Area Register*/
    public void StartRegisterPipeArea(GameObject pipe, bool isSmallPipe = false){
        if (this.isInPipeAreaRegister)
            return;

        this.isInPipeAreaRegister = true;
        this.gameManager.sceneManager.levelEditorCursor.SetCanBuild(false);
        this.gameManager.sceneManager.levelEditorCursor.canChangeCanBuild = false;
        canSwitch = false;
        this.blockButtonsParent.transform.parent.gameObject.SetActive(false);
        this.pipeAreButton.SetActive(true);
        if(isSmallPipe)
            this.gameManager.sceneManager.levelEditorCursor.SetCurrentBlock(BlockID.MINI_PIPE);
        else
            this.gameManager.sceneManager.levelEditorCursor.SetCurrentBlock(BlockID.PIPE);
        this.currentPipeAreaRegisterPipe = pipe.GetComponent<LevelEditorPipe>();
        ChangeLevelEditorAreaButton();
        StartCoroutine(PipeRegisterCheckPipeIsAlive(pipe));
    }
    
    public void EndRegisterPipeArea(GameObject pipe){
        this.isInPipeAreaRegister = false;
        canSwitch = true;
        this.pipeAreButton.SetActive(false);
        this.blockButtonsParent.transform.parent.gameObject.SetActive(true);
    }

    private IEnumerator PipeRegisterCheckPipeIsAlive(GameObject pipe){
        while (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1)){
            yield return new WaitForSeconds(0);
        }
        this.gameManager.sceneManager.levelEditorCursor.canChangeCanBuild = true;
        this.gameManager.sceneManager.levelEditorCursor.SetCanBuild(true);

        while (pipe != null && !canSwitch){
            yield return new WaitForSeconds(0);
        }

        while (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1)){
            yield return new WaitForSeconds(0);
        }
        this.gameManager.sceneManager.levelEditorCursor.canChangeCanBuild = true;
        this.gameManager.sceneManager.levelEditorCursor.SetCanBuild(true);

        EndRegisterPipeArea(pipe);
    }

    public void SetTimerUp(TextMeshPro guiTimer){
        if (this.gameManager.sceneManager.timer != 999 && this.gameManager.sceneManager.timer != 900 && this.gameManager.sceneManager.timer > 99)
            this.gameManager.sceneManager.timer = this.gameManager.sceneManager.timer + 100;
        else if (this.gameManager.sceneManager.timer == 900)
            this.gameManager.sceneManager.timer = 999;
        else if (this.gameManager.sceneManager.timer < 99)
            this.gameManager.sceneManager.timer = this.gameManager.sceneManager.timer + 10;
        guiTimer.text = this.gameManager.sceneManager.timer.ToString();
    }

    public void SetTimerDown(TextMeshPro guiTimer){
        if (this.gameManager.sceneManager.timer != 999 && this.gameManager.sceneManager.timer > 110)
            this.gameManager.sceneManager.timer = this.gameManager.sceneManager.timer - 100;
        else if (this.gameManager.sceneManager.timer == 999)
            this.gameManager.sceneManager.timer = 900;
        else if (this.gameManager.sceneManager.timer < 110 && this.gameManager.sceneManager.timer != 10)
            this.gameManager.sceneManager.timer = this.gameManager.sceneManager.timer - 10;
        guiTimer.text = this.gameManager.sceneManager.timer.ToString();
    }

    public void ToggleCastleLavaCurrentArea(){
        int org = 0;
        int end = 0;
        if (this.currentArea == 0)
            org = gameManager.sceneManager.fullLevelLiquidIdArea0;
        else
            org = gameManager.sceneManager.fullLevelLiquidIdArea1;

        if (org == 0)
            end = 1;
        else if (org == 1)
            end = 2;
        else if (org == 2)
            end = 3;
        else
            end = 0;

        if (this.currentArea == 0)
            gameManager.sceneManager.fullLevelLiquidIdArea0 = end;
        else
            gameManager.sceneManager.fullLevelLiquidIdArea1 = end;

        SetCastleLavaStateText();
        gameManager.sceneManager.TryToggleCastleLavaFromState();
    }

    public void ToggleIsDarkCurrentArea(){
        gameManager.sceneManager.isAreaDark[this.currentArea] = !gameManager.sceneManager.isAreaDark[this.currentArea];
        UpdateIsDark();
    }

    public void UpdateIsDark(){
        if (gameManager.sceneManager.isAreaDark[this.currentArea])
            gameManager.sceneManager.isDarkImage.sprite = gameManager.sceneManager.isDarkSprites[1];
        else
            gameManager.sceneManager.isDarkImage.sprite = gameManager.sceneManager.isDarkSprites[0];
    }

    public void ToggleHasFogCurrentArea(){
        gameManager.sceneManager.hasAreaCloudsFog[this.currentArea] = !gameManager.sceneManager.hasAreaCloudsFog[this.currentArea];
        UpdateHasFog();
    }

    public void UpdateHasFog(){
        if (gameManager.sceneManager.hasAreaCloudsFog[this.currentArea])
            gameManager.sceneManager.fogIcon.SetActive(true);
        else
            gameManager.sceneManager.fogIcon.SetActive(false);
    }

    public void DisableLevelEditor(){
        if (!this.isPlayMode)
            ActivatePlayMode();
        isLevelEditor = false;
        canSwitch = false;
        SceneManager.groundTileConnectors.Clear();
        Destroy(this.gameManager.sceneManager.switchModeButton);
        Destroy(this.editorParent.gameObject);
        Destroy(this.blockButtonsParent.transform.parent.gameObject);
        Destroy(this.gameManager.sceneManager.levelEditorCursor.gameObject);
        instance = null;
        Destroy(this);
    }

    public void SetBackgroundColor(Color color){
        foreach (SpriteRenderer sp in this.gameManager.sceneManager.editorBackgrounds)
            sp.color = color;
    }

}

namespace UMM.BlockField{
    public class BlockFieldManager{
        public List<BlockField> blockFields = new List<BlockField>();
        public int weidth = 420;
        public int height = 20;
        public LevelEditorManager levelEditorManager;
        public BlockDataManager blockDataManager;

        public void GenerateBlockFields(GameObject blockFieldPrefarb){
            ClearBlockFields();
            GameObject blockFieldParent = new GameObject("BlockFieldParent");
            blockFieldParent.transform.SetParent(this.levelEditorManager.editorParent.transform);

            int counter = 0;
            float y = 12.3f;
            for (int i = 0; i < this.height; i++){
                float x = 2.8f;
                for (int f = 0; f < this.weidth; f++){
                    GameObject blockField = GameManager.Instantiate(blockFieldPrefarb);
                    this.blockFields.Add(new BlockField(blockField, counter));
                    blockField.transform.position = new Vector3(x, y, 0);
                    blockField.transform.SetParent(blockFieldParent.transform);
                    counter = counter + 1;
                    x = x + 1;
                }
                y = y + 1;
            }

            GameManager.instance.sceneManager.playerCamera.maxX = (blockFieldParent.transform.GetChild(blockFieldParent.transform.childCount - 1).position.x) - 30.4f;

            GameManager.instance.sceneManager.levelEditorCursor.currentBlockField = blockFields[0];
        }

        private void ClearBlockFields(){
            foreach(BlockField blockField in this.blockFields){
                GameManager.Destroy(blockField.blockFieldGameObject);
            }

            this.blockFields.Clear();
            GameManager.instance.sceneManager.levelEditorCursor.currentBlockField = null;
        }

        public BlockField GetBlockFieldFromGameObject(GameObject blockFieldGameObject){
            string strNumber = blockFieldGameObject.name.Split('|')[1];
            return GetBlockFieldAt(GameManager.StringToInt(strNumber));
        }

        public BlockField GetBlockFieldDirectFromGameObject(GameObject blockFieldGameObject){
            foreach(BlockField blockField in this.blockFields){
                if (blockField.blockFieldGameObject == blockFieldGameObject)
                    return blockField;
            }

            return null;
        }

        public BlockField GetBlockFieldDirectFromCurrentBlock(GameObject currentBlock, int area = 0){
            foreach (BlockField blockField in this.blockFields) {
                if (blockField.currentBlock[0][area] == currentBlock)
                    return blockField;
            }

            return null;
        }

        public BlockField GetBlockFieldAt(int at){
            return this.blockFields[at];
        }

        public BlockField[] GetBlockFieldsFromNeedSize(Vector2 needSize, BlockField blockField){
            List<BlockField> fields = new List<BlockField>();

            if(needSize.x != 0){
                if(needSize.x > 0){
                    for (int i = 1; i <= needSize.x; i++){
                        fields.Add(GetBlockFieldAt(blockField.blockFieldNumber + i));
                    }
                }
                else{
                    float size = -needSize.x;
                    for (int i = 1; i <= size; i++){
                        fields.Add(GetBlockFieldAt(blockField.blockFieldNumber - i));  
                    }
                }
            }

            if(needSize.y != 0){
                if(needSize.y > 0){
                    for (int i = 1; i <= needSize.y; i++){
                        fields.Add(GetBlockFieldAt(blockField.blockFieldNumber + (i * weidth)));
                        if (needSize.x != 0){
                            if (needSize.x > 0){
                                for (int z = 1; i <= needSize.x; i++){
                                    fields.Add(GetBlockFieldAt(blockField.blockFieldNumber + (i * weidth) + z));
                                }
                            }
                            else{
                                float size = -needSize.x;
                                for (int z = 1; z <= size; z++){
                                    fields.Add(GetBlockFieldAt(blockField.blockFieldNumber + (i * weidth) - z));
                                }
                            }
                        }
                    }
                }
                else{
                    float size = -needSize.y;
                    for (int i = 1; i <= size; i++){
                        fields.Add(GetBlockFieldAt(blockField.blockFieldNumber - (i * weidth)));
                        if (needSize.x != 0){
                            if (needSize.x > 0){
                                for (int z = 1; i <= needSize.x; i++){
                                    fields.Add(GetBlockFieldAt(blockField.blockFieldNumber - (i * weidth) + z));
                                }
                            }
                            else{
                                float dsize = -needSize.x;
                                for (int z = 1; z <= dsize; z++){
                                    fields.Add(GetBlockFieldAt(blockField.blockFieldNumber - (i * weidth) - z));
                                }
                            }
                        }
                    }

                }
            }

            return fields.ToArray();
        }

        public void CheckNotOfficalBlock(BlockField blockField, BlockID newBlock, bool isEraser = false){
            int area = this.levelEditorManager.currentArea;
            if (blockField.notOfficalRealBlockIds[area] != -1 && (blockField.notOfficalBlcokIds[area] == newBlock | isEraser)){
                if(blockField.notOfficalBlocks[area] == GetBlockFieldAt(blockField.notOfficalRealBlockIds[area]).currentBlock[0][area])
                    CheckBlockField(GetBlockFieldAt(blockField.notOfficalRealBlockIds[area]));
                blockField.notOfficalBlocks[area] = null;
                blockField.notOfficalBlcokIds[area] = BlockID.GROUND;
                blockField.notOfficalRealBlockIds[area] = -1;
            }
        }

        public void CheckBlockField(BlockField blockField, bool dontDestroy = false, int area = -1, int arrayId = 0, BlockID newBlock = BlockID.GROUND, bool isEraser = false){
            if (area == -1)
                area = this.levelEditorManager.currentArea;
            DestroyBlockFieldCurrentBlock(blockField, dontDestroy, area, arrayId, newBlock, isEraser);
        }

        public void DestroyBlockFieldCurrentBlock(BlockField blockField, bool dontDestroy, int area, int arrayId, BlockID newBlock = BlockID.GROUND, bool isEraser = false){
            CheckNotOfficalBlock(blockField, newBlock);
            if (isEraser && blockField.currentBlock[arrayId][area] == null && blockField.mainBlockFieldNumber[arrayId][area] == -1)
                CheckNotOfficalBlock(blockField, newBlock, true);

            if (newBlock == BlockID.FAKEWALL && (blockField.currentBlock[0][area] != null && blockField.blockId[0][area] == BlockID.GROUND))
                DestroyBlockFieldCurrentBlock(blockField, dontDestroy, area, 0, BlockID.ERASER, false);
            if (newBlock == BlockID.GROUND && blockField.currentBlock[1][area] != null)
                DestroyBlockFieldCurrentBlock(blockField, dontDestroy, area, 1, BlockID.ERASER, false);

            if (blockField.currentBlock[arrayId][area] != null && blockField.mainBlockFieldNumber[arrayId][area] == -1) {
                if (this.blockDataManager.blockDatas[(int)blockField.blockId[arrayId][area]].cantDestroy && !dontDestroy)
                    return;

                UMM.BlockData.BlockData blockData = GameManager.instance.blockDataManager.blockDatas[(int)blockField.blockId[arrayId][area]];
                foreach (BlockField blockField1 in GetBlockFieldsFromNeedSize(blockData.needSize, blockField)) {
                    blockField1.mainBlockFieldNumber[arrayId][area] = -1;
                    blockField1.currentBlock[arrayId][area] = null;
                    blockField1.blockId[arrayId][area] = BlockID.GROUND;
                }

                if (!dontDestroy)
                    GameManager.Destroy(blockField.currentBlock[arrayId][area]);
                blockField.blockId[arrayId][area] = BlockID.GROUND;
                blockField.currentBlock[arrayId][area] = null;
            }else if (blockField.currentBlock[arrayId][area] != null)
                CheckBlockField(GetBlockFieldAt(blockField.mainBlockFieldNumber[arrayId][area]), dontDestroy);
        }

        public int GetAreaFromBlockFieldBlock(BlockField blockField, GameObject block){
            for (int i = 0; i < blockField.currentBlock.Length; i++) {
                if (blockField.currentBlock[0][i] == block)
                    return i;
            }

            return 0;
        }

        public BlockField[] CalculateBlockFieldsFromDistance(BlockField startBlockField, BlockField endBlockField){
            List<BlockField> blockFields = new List<BlockField>();

            int xDistance = 0;
            BlockField targetBlockField = startBlockField;
            BlockField end = endBlockField;

            int heightDistance = 0;
            if (endBlockField.blockFieldGameObject.transform.position.y < startBlockField.blockFieldGameObject.transform.position.y){
                targetBlockField = endBlockField;
                end = startBlockField;
            }else{
                targetBlockField = startBlockField;
                end = endBlockField;
            }

            while(targetBlockField.blockFieldGameObject.transform.position.y != end.blockFieldGameObject.transform.position.y + 1){
                heightDistance++;
                try{
                    targetBlockField = GetBlockFieldOverBlockField(targetBlockField, false);
                }catch(Exception e){
                    break;
                }
            }

            if (endBlockField.blockFieldGameObject.transform.position.x < startBlockField.blockFieldGameObject.transform.position.x){
                targetBlockField = endBlockField;
                end = startBlockField;
            }else{
                targetBlockField = startBlockField;
                end = endBlockField;
            }

            while (targetBlockField.blockFieldGameObject.transform.position.x != end.blockFieldGameObject.transform.position.x){
                xDistance++;
                targetBlockField = GetBlockFieldAt(targetBlockField.blockFieldNumber + 1);
            }

            if (endBlockField.blockFieldGameObject.transform.position.x < startBlockField.blockFieldGameObject.transform.position.x){
                targetBlockField = endBlockField;
                end = startBlockField;
            }else{
                targetBlockField = startBlockField;
                end = endBlockField;
            }

            if (endBlockField.blockFieldGameObject.transform.position.x > startBlockField.blockFieldGameObject.transform.position.x)
                xDistance++;

            for (int i = 0; i < heightDistance; i++){
                for (int x = 0; x < xDistance; x++){
                    blockFields.Add(GetBlockFieldAt(targetBlockField.blockFieldNumber + x));
                }

                if (end.blockFieldGameObject.transform.position.y < targetBlockField.blockFieldGameObject.transform.position.y +1)
                    targetBlockField = GetBlockFieldUnderBlockField(targetBlockField);
                else
                    targetBlockField = GetBlockFieldOverBlockField(targetBlockField, false);
            }

            return blockFields.ToArray();
        }

        public BlockField GetBlockFieldOverBlockField(BlockField blockField, bool checkSize = true){
            if (checkSize && blockField.blockFieldNumber + weidth >= this.blockFields.Count)
                return blockField;
            return GetBlockFieldAt(blockField.blockFieldNumber + weidth);
        }

        public BlockField GetBlockFieldUnderBlockField(BlockField blockField){
            int numb = blockField.blockFieldNumber - weidth;
            if (numb <= 0)
                return blockField;
            return GetBlockFieldAt(blockField.blockFieldNumber - weidth);
        }

        public void AddNotOfficalBlockData(BlockField blockField, GameObject gameObject, BlockID id, int realBlockFieldNumber){
            if (blockField.notOfficalBlocks[this.levelEditorManager.currentArea] != gameObject)
                 CheckNotOfficalBlock(blockField, id);

            blockField.notOfficalBlocks[this.levelEditorManager.currentArea] = gameObject;
            blockField.notOfficalBlcokIds[this.levelEditorManager.currentArea] = id;
            blockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea] = realBlockFieldNumber;
        }

        public void RemoveNotOfficalBlockData(BlockField blockField, GameObject gameObject, BlockID id){
            if (blockField.notOfficalBlocks[this.levelEditorManager.currentArea] != gameObject)
                CheckNotOfficalBlock(blockField, id);
            blockField.notOfficalBlocks[this.levelEditorManager.currentArea] = null;
            blockField.notOfficalBlcokIds[this.levelEditorManager.currentArea] = UMM.BlockData.BlockID.GROUND;
            blockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea] = -1;
        }

        public void DebugBlockField(BlockField blockField) {
            Debug.Log("DEBUG-BlockField | Number:" + blockField.blockFieldNumber);
            Debug.Log("DEBUG-BlockField | MainBlockFieldNumber[0]: " + blockField.mainBlockFieldNumber[0][0]);
            Debug.Log("DEBUG-Blockfield | BlockID[0]: " + blockField.blockId[0][0]);
            Debug.Log("DEBUG-Blockfield | BlockFieldGameObject " + blockField.blockFieldGameObject);
            Debug.Log("DEBUG-Blockfield | BlockFieldSpriteRenderer: " + blockField.blockFieldSpriteRendererOpt);
            Debug.Log("DEBUG-Blockfield | Currentblock[0]: " + blockField.currentBlock[0][0]);
        }
    }

    [System.Serializable]
    public class BlockField {
        public int blockFieldNumber;
        public int[][] mainBlockFieldNumber = new int[2][] { new int[2] { -1, -1 }, new int[2] { -1, -1 } };/*Only needed for connecting Blocks*/
        public BlockID[][] blockId = new BlockID[2][] { new BlockID[2], new BlockID[2] };
        public GameObject blockFieldGameObject;
        public SpriteRenderer blockFieldSpriteRendererOpt;
        public GameObject[][] currentBlock = new GameObject[2][] { new GameObject[2] { null, null }, new GameObject[2] { null, null } };/*CurrentBlock of all Areas Array*/
        public GameObject[] notOfficalBlocks = new GameObject[2] { null, null };
        public BlockID[] notOfficalBlcokIds = new BlockID[2];
        public int[] notOfficalRealBlockIds = new int[2] { -1, -1 };

        public BlockField(GameObject blockFieldGameObject, int blockFieldNumber){
            this.blockFieldGameObject = blockFieldGameObject;
            this.blockFieldNumber = blockFieldNumber;
            this.blockFieldGameObject.name = "BlockField|" + this.blockFieldNumber;
            this.blockFieldSpriteRendererOpt = this.blockFieldGameObject.GetComponent<SpriteRenderer>();
        }
    }
}