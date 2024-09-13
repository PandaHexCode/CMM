using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;
using UMM.BlockData;

public class LevelEditorCursor : MonoBehaviour{

    public Color color;/*Color for the current BlockField*/
    public Color color2;
    public Color colorChooseBlockFields;
    public Sprite cursorSprite;
    public Sprite eraserCursorSprite;

    [System.NonSerialized] public BlockField currentBlockField;
    [System.NonSerialized] public bool canChangeCanBuild = true;

    public BlockID currentBlock = 0;
    private BlockID lastBlockBeforeEraser = BlockID.ERASER;
    private bool canBuild = true;
    [System.NonSerialized]public Coroutine ActionCor;

    public enum CursorAction { NOTHING = 0, CHECK = 1, PLACE = 2, MOVE = 3, ERASER = 4, START_CHOOSE_BLOCKFIELDS = 5, CHOOSE_BLOCKFIELDS_WAIT_FOR_ACTION = 6, CHANGE_BLOCK_ACTION = 7}
    public CursorAction currentAction = CursorAction.NOTHING;

    private BlockID movingBlockId = 0;
    private GameObject currentMovingBlock = null;

    private int lastPlaceNumber = -1;

    private BlockField chooseBlockFieldsFirstBlockField = null;
    private BlockField[] chooseBlockFieldsBlockFields = null;

    private Transform _transform;
    private LevelEditorManager levelEditorManager;
    private Camera cam;
    private AudioSource audioSource;
    public Transform showcaseRenderer;

    private void Awake(){
        this._transform = this.transform;
        this.levelEditorManager = LevelEditorManager.instance;
        this.currentBlockField = this.levelEditorManager.blockFieldManager.blockFields[0];
        this.cam = Camera.main;
        this.audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable(){
        this.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void OnDisable(){
        if (this._transform.childCount > 0){
            if (!this._transform.GetChild(0).gameObject.name.Equals("StartPoint(Clone)"))
                Destroy(this._transform.GetChild(0).gameObject);
            else
                PlaceMovingBlock();
        }
    }

    private void Update(){
        if (Input.GetKeyDown(KeyCode.K))
            levelEditorManager.blockFieldManager.DebugBlockField(this.currentBlockField);

        Vector3 pz = cam.ScreenToWorldPoint(Input.mousePosition);
        pz.z = 0;
        _transform.position = pz;

        if (this.currentAction == CursorAction.CHANGE_BLOCK_ACTION){
            this.GetComponent<SpriteRenderer>().sprite = this.cursorSprite;
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) && (this.currentAction == CursorAction.NOTHING | this.currentAction == CursorAction.PLACE)){
            this.lastBlockBeforeEraser = this.currentBlock;
            this.currentBlock = BlockID.ERASER;
            this.currentAction = CursorAction.ERASER;
            this.GetComponent<SpriteRenderer>().sprite = this.eraserCursorSprite;
            StartCoroutine(EraserLoopIE());
        }

        if (this.currentAction == CursorAction.PLACE | (Input.GetMouseButton(0) && this.currentAction == CursorAction.ERASER)){
            if (this.lastPlaceNumber != this.currentBlockField.blockFieldNumber | this.currentAction == CursorAction.ERASER){
                if(!this.audioSource.isPlaying)
                    this.audioSource.Play();
                if (this.currentBlock == BlockID.ERASER){
                    this.levelEditorManager.blockFieldManager.CheckBlockField(this.currentBlockField, false, -1, 0, BlockID.GROUND, true);
                    this.levelEditorManager.blockFieldManager.CheckBlockField(this.currentBlockField, false, -1, 1, BlockID.GROUND, true);
                    this.levelEditorManager.blockFieldManager.CheckBlockField(this.currentBlockField, false, -1, 2, BlockID.GROUND, true);
                    GroundTileConnector.SetAllWithoutPos();
                    return;
                }
                this.lastPlaceNumber = this.currentBlockField.blockFieldNumber;
                this.levelEditorManager.PlaceBlock(this.currentBlock, this.currentBlockField);
                if (!GameManager.instance.blockDataManager.blockDatas[(int)this.currentBlock].canSpam)
                    this.currentAction = CursorAction.CHECK;
            }
            return;
        }

        if (Input.GetMouseButtonDown(2) && this.currentAction == CursorAction.NOTHING)
            StartMovingBlock();

        if (Input.GetMouseButtonDown(0) && this.currentAction == CursorAction.MOVE)
            PlaceMovingBlock();

        if (this.currentAction == CursorAction.START_CHOOSE_BLOCKFIELDS){
            if (this.chooseBlockFieldsBlockFields != null){
                foreach (BlockField blockField in this.chooseBlockFieldsBlockFields){
                    if (blockField.blockFieldSpriteRendererOpt == null){
                        this.chooseBlockFieldsBlockFields = null;
                        break;
                    }
                    blockField.blockFieldSpriteRendererOpt.color = Color.white;
                    blockField.blockFieldSpriteRendererOpt.enabled = false;
                }
            }

            this.chooseBlockFieldsBlockFields = this.levelEditorManager.blockFieldManager.CalculateBlockFieldsFromDistance(this.chooseBlockFieldsFirstBlockField, this.currentBlockField);
            foreach (BlockField blockField in this.chooseBlockFieldsBlockFields){
                blockField.blockFieldSpriteRendererOpt.color = this.colorChooseBlockFields;
                blockField.blockFieldSpriteRendererOpt.enabled = true;
            }
            if (Input.GetMouseButtonUp(1))
                this.currentAction = CursorAction.CHOOSE_BLOCKFIELDS_WAIT_FOR_ACTION;
        }

        if(this.currentAction == CursorAction.CHOOSE_BLOCKFIELDS_WAIT_FOR_ACTION){
            if (Input.GetKey(KeyCode.E)){
                foreach (BlockField blockField in this.chooseBlockFieldsBlockFields){
                    this.levelEditorManager.blockFieldManager.CheckBlockField(blockField, false, -1, 0, BlockID.GROUND, true);
                    blockField.blockFieldSpriteRendererOpt.color = Color.white;
                    blockField.blockFieldSpriteRendererOpt.enabled = false;
                }

                GroundTileConnector.SetAllWithoutPos();
                this.currentAction = CursorAction.NOTHING;
            }else if (Input.GetMouseButtonDown(0)){
                foreach (BlockField blockField in this.chooseBlockFieldsBlockFields){
                    this.levelEditorManager.PlaceBlock(this.currentBlock, blockField);
                    blockField.blockFieldSpriteRendererOpt.color = Color.white;
                    blockField.blockFieldSpriteRendererOpt.enabled = false;
                }

                GroundTileConnector.SetAllWithoutPos();
                this.currentAction = CursorAction.NOTHING;
            }
        }

        if (!this.canBuild)
            return;

        if (Input.GetMouseButton(0) && this.currentAction == CursorAction.NOTHING){
            this.ActionCor = StartCoroutine(CheckAction());
            return;
        }else if (Input.GetMouseButton(1) && this.currentAction == CursorAction.NOTHING){
            this.currentAction = CursorAction.START_CHOOSE_BLOCKFIELDS;
            this.chooseBlockFieldsFirstBlockField = this.currentBlockField;
        } 
    }
    
    public void PlaceMovingBlock(){
        if(this.currentMovingBlock == null){
            this.currentAction = CursorAction.NOTHING;
            return;
        }

        this.audioSource.Play();
        GameManager.SetAllChildrenBoxCollidersState(this.currentMovingBlock.transform, true);
            foreach (Transform child in this.currentMovingBlock.transform){
                if (child.gameObject.tag == "EditorBlockWidget"){
                    child.GetComponent<SpriteRenderer>().enabled = false;
                    child.GetComponent<SpriteRenderer>().sprite = this.levelEditorManager.blockWidget1;
                }
                foreach (Transform child2 in child){
                    if (child2.gameObject.tag == "EditorBlockWidget"){
                        child2.GetComponent<SpriteRenderer>().enabled = false;
                        child2.GetComponent<SpriteRenderer>().sprite = this.levelEditorManager.blockWidget1;
                    }
                }
            }
            this.levelEditorManager.PlaceActiveBlock(this.currentMovingBlock, this.currentBlockField, this.movingBlockId);
            StartCoroutine(SetIsBlockMovingBack());
    }

    private IEnumerator EraserLoopIE(){
        while (!Input.GetKeyUp(KeyCode.E)){
            yield return new WaitForSeconds(0);
        }

        this.GetComponent<SpriteRenderer>().sprite = this.cursorSprite;
        this.currentBlock = this.lastBlockBeforeEraser;
        this.currentAction = CursorAction.NOTHING;
    }

    private IEnumerator CheckAction(){
        this.currentAction = CursorAction.CHECK;

        if (this.currentBlockField.currentBlock[0][this.levelEditorManager.currentArea] != null | (this.currentBlockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea] != -1 && this.levelEditorManager.blockFieldManager.GetBlockFieldAt(this.currentBlockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea]).currentBlock[0][this.levelEditorManager.currentArea] != null)){
            int i = 0;
            int blockFieldNumber = this.currentBlockField.blockFieldNumber;
            while (!Input.GetMouseButtonUp(0) && this.currentBlockField.blockFieldNumber == blockFieldNumber && i != 5){
                yield return new WaitForSecondsRealtime(0.05f);
                i++;
            }

            if (i == 5 && Input.GetMouseButton(0))
                StartMovingBlock();
            else
                this.currentAction = CursorAction.NOTHING;
        }else{
            this.lastPlaceNumber = -1;
            this.currentAction = CursorAction.PLACE;
            while (!Input.GetMouseButtonUp(0)){
                yield return new WaitForSecondsRealtime(0);
            }
            this.currentAction = CursorAction.NOTHING;
        }
    }

    private void StartMovingBlock() {
        if (this.currentAction == CursorAction.CHANGE_BLOCK_ACTION)
            return;

        try{
            if (!this.canBuild |
                (this.currentBlockField.currentBlock[0][this.levelEditorManager.currentArea] == null &&
                (this.currentBlockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea] == -1 &&
                this.levelEditorManager.blockFieldManager.GetBlockFieldAt(this.currentBlockField.notOfficalRealBlockIds[this.levelEditorManager.currentArea]).currentBlock[0][this.levelEditorManager.currentArea] == null))){
                this.currentAction = CursorAction.NOTHING;
                return;
            }
        }catch(System.Exception e){
            this.levelEditorManager.blockFieldManager.CheckBlockField(this.currentBlockField);
            this.currentAction = CursorAction.NOTHING;
        }

        this.currentAction = CursorAction.MOVE;
        if (this.currentBlockField.blockId[0][this.levelEditorManager.currentArea] == BlockID.CONNECTED_BLOCK) {
            BlockField b = this.levelEditorManager.blockFieldManager.GetBlockFieldAt(this.currentBlockField.mainBlockFieldNumber[0][this.levelEditorManager.currentArea]);
            this.currentMovingBlock = b.currentBlock[0][this.levelEditorManager.currentArea];
            this.movingBlockId = b.blockId[0][this.levelEditorManager.currentArea];
        }else{
            if (this.currentBlockField.currentBlock[0][this.levelEditorManager.currentArea] != null){
                this.currentMovingBlock = this.currentBlockField.currentBlock[0][this.levelEditorManager.currentArea];
                this.movingBlockId = this.currentBlockField.blockId[0][this.levelEditorManager.currentArea];
            }else{
                this.currentMovingBlock = this.currentBlockField.notOfficalBlocks[this.levelEditorManager.currentArea];
                this.movingBlockId = this.currentBlockField.notOfficalBlcokIds[this.levelEditorManager.currentArea];
            }
        }
        
        foreach (Transform child in this.currentMovingBlock.transform){
            if (child.gameObject.tag == "EditorBlockWidget"){
                child.GetComponent<SpriteRenderer>().enabled = true;
                child.GetComponent<SpriteRenderer>().sprite = this.levelEditorManager.blockWidget2;
            }
            foreach(Transform child2 in child){
                    if (child2.gameObject.tag == "EditorBlockWidget"){
                        child2.GetComponent<SpriteRenderer>().enabled = true;
                        child2.GetComponent<SpriteRenderer>().sprite = this.levelEditorManager.blockWidget2;
                    }
            }
        }

        GameManager.SetAllChildrenBoxCollidersState(this.currentMovingBlock.transform, false);
        this.currentMovingBlock.transform.SetParent(this._transform);
        this.currentMovingBlock.transform.localPosition = Vector3.zero + GameManager.instance.blockDataManager.blockDatas[(int)this.movingBlockId].placeOffset;
        if (this.currentBlockField.currentBlock[0][this.levelEditorManager.currentArea] != null)
            this.levelEditorManager.blockFieldManager.CheckBlockField(this.currentBlockField, true);
        this.currentBlockField.blockFieldSpriteRendererOpt.color = Color.white;
        this.currentBlockField.blockFieldSpriteRendererOpt.enabled = false;
        /*if (this.currentBlockField.currentBlock[0][levelEditorManager.currentArea] != null){
            foreach (SpriteRenderer sp in this.currentBlockField.currentBlock[0][levelEditorManager.currentArea].GetComponentsInChildren<SpriteRenderer>())
                sp.color = Color.white;
        }*/
    }

    private IEnumerator SetIsBlockMovingBack(){
        int blockFieldNumber = this.currentBlockField.blockFieldNumber;
        while(!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1) && this.currentBlockField.blockFieldNumber == blockFieldNumber){
            yield return new WaitForSeconds(0);
        }
        this.currentAction = CursorAction.NOTHING;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 12){
            if (this.currentBlockField != null && collision.gameObject != this.currentBlockField.blockFieldGameObject){
                this.currentBlockField.blockFieldSpriteRendererOpt.color = Color.white;
                this.currentBlockField.blockFieldSpriteRendererOpt.enabled = false;
                /*  if (this.currentBlockField.currentBlock[0][levelEditorManager.currentArea] != null){
                      foreach (SpriteRenderer sp in this.currentBlockField.currentBlock[0][levelEditorManager.currentArea].GetComponentsInChildren<SpriteRenderer>())
                          sp.color = Color.white;
                  }*/
            }

            this.currentBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldFromGameObject(collision.gameObject);
            this.showcaseRenderer.position = this.currentBlockField.blockFieldGameObject.transform.position;
            if (this.currentAction != CursorAction.MOVE) {
             this.currentBlockField.blockFieldSpriteRendererOpt.color =color;
        this.currentBlockField.blockFieldSpriteRendererOpt.enabled = true;

               /* if (this.currentBlockField.currentBlock[0][levelEditorManager.currentArea] != null){
                    foreach (SpriteRenderer sp in this.currentBlockField.currentBlock[0][levelEditorManager.currentArea].GetComponentsInChildren<SpriteRenderer>())
                        sp.color = this.color2;
                }*/
            }
        }
    }

    public void SetCanBuild(bool state){
        if(this.canChangeCanBuild)
            this.canBuild = state;
    }

    public void SetCurrentBlock(BlockID blockID){
        this.currentBlock = blockID;

        if ((int)blockID < 0){
            this.showcaseRenderer.GetComponent<SpriteRenderer>().sprite = null;
            return;
        }
        BlockData blockData = GameManager.instance.blockDataManager.blockDatas[(int)blockID];
        this.showcaseRenderer.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(blockData.spriteId, blockData.tilesetType);
    }

    private void OnGUI(){
        if (!DevGameManager.SHOWDEBUGINFORMATIONS)
            return;

        string text = "\nVersion: " + GameManager.instance.buildData.VERSION_STRING + "\nCanBuild: " + this.canBuild + ", CurrentArea: " + this.levelEditorManager.currentArea + "\nCursorPos: " + _transform.position + ", CurrentAct: " + this.currentAction;
        if (this.currentBlockField != null) {
            text = text + "\nCurrentBlockField: " + this.currentBlockField.blockFieldNumber + ", MainBlockField: " + this.currentBlockField.mainBlockFieldNumber[0][this.levelEditorManager.currentArea];
            if (this.currentBlockField.currentBlock[0][0] != null)
                text = text + "\nBlockID[Area0]: " + this.currentBlockField.blockId[0][0];
            if (this.currentBlockField.currentBlock[0][1] != null)
                text = text + "\nBlockID[Area1]: " + this.currentBlockField.blockId[0][1];
            if (this.currentBlockField.notOfficalBlocks[0] != null)
                text = text + "\nNotOfficalBlockID[Area1]: " + this.currentBlockField.notOfficalBlcokIds[0];
        }
        GUI.Box(new Rect(0, 0, 300, 120), "Debug");
        GUI.Label(new Rect(0, 0, 300, 180), text);
    }

}
