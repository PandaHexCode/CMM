using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;

public class Pipe : MonoBehaviour{

    public int pipeLength = 1;
    public bool isSmallPipe = false;
    public BlockID contentBlock = BlockID.ERASER;
    public Pipe connectedPipe = null;

    private bool canSpawnContent = true;
    private List<GameObject> spawnedContent = new List<GameObject>();

    [System.NonSerialized]public int extraNumber = 0;
    [System.NonSerialized]public bool canEnterPipe = false;
    [System.NonSerialized]public Direction directionKey = Direction.UP;
    public enum Direction { UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3};

    private PipePlayerCheck myPipePlayerCheck;
    [System.NonSerialized]public InputManager input = null;

    private void OnEnable(){
        RegisterKey();
        if (this.connectedPipe != null && this.myPipePlayerCheck == null){
            GameObject pipePlayerCheck = new GameObject("PipePlayerCheck");
            pipePlayerCheck.transform.SetParent(this.transform);
            pipePlayerCheck.transform.localPosition = Vector3.zero;
            pipePlayerCheck.transform.localEulerAngles = Vector3.zero;
            pipePlayerCheck.transform.SetParent(this.transform.parent, true);
            BoxCollider2D bx = pipePlayerCheck.AddComponent<BoxCollider2D>();
            bx.isTrigger = true;
            if(this.isSmallPipe)
                bx.offset = new Vector2(0f, 0.5f);
            else
                bx.offset = new Vector2(0.5f, 0.5f);
            bx.size = new Vector2(1.1f, 1f);
            this.myPipePlayerCheck = pipePlayerCheck.AddComponent<PipePlayerCheck>();
            this.myPipePlayerCheck.myPipe = this;
        }
        this.spawnedContent.Clear();
        StartCoroutine(SpawnContentLoop());
    }

    private void OnDisable(){
        foreach(GameObject gm in this.spawnedContent){
            Destroy(gm);
        }
        this.spawnedContent.Clear();
    }

    private void RegisterKey(){
        Vector3 lookDirection = GetLookDirection(this.gameObject);
        if (lookDirection == Vector3.up)
            this.directionKey = Direction.DOWN;
        else if (lookDirection == Vector3.down)
            this.directionKey = Direction.UP;
        else if (lookDirection == Vector3.left)
            this.directionKey = Direction.RIGHT;
        else if (lookDirection == Vector3.right)
            this.directionKey = Direction.LEFT;
    }

    private void Update(){
        if (this.canEnterPipe && CheckInput())
            GameManager.instance.StartCoroutine(GameManager.EnterPipe(this, this.input.GetComponentInParent<PlayerController>().gameObject));
    }

    private bool CheckInput(){
        switch (this.directionKey){
            case Direction.UP:
                return this.input.UP;
            case Direction.DOWN:
                return this.input.DOWN;
            case Direction.LEFT:
                return this.input.LEFT;
                break;
            case Direction.RIGHT:
                return this.input.RIGHT;
        }

        return false;
    }

    public IEnumerator SpawnContentLoop(){
        yield return new WaitForSeconds(2);
        if (this.contentBlock == BlockID.ERASER)
            StopAllCoroutines();
        else{
            int maxSpawnSize = GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].maxPipeSpawnSize;
            while (this.spawnedContent.Count == maxSpawnSize){
                yield return new WaitForSeconds(0);
            }
            while (!this.canSpawnContent){
                yield return new WaitForSeconds(0);
            }
            yield return new WaitForSeconds(1);

            if(Physics2D.Raycast(this.transform.position , GetLookDirection(this.gameObject), 0.5f, GameManager.instance.entityGroundMask)){
                StartCoroutine(SpawnContentLoop());
            }else
                StartCoroutine(SpawnContent());
        }
    }

    public IEnumerator SpawnContent(){
        GameObject clon = Instantiate(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].prefarb, this.gameObject.transform.parent);
        clon.transform.position = this.transform.position + GetOffset();
        this.spawnedContent.Add(clon);
        if (this.contentBlock == BlockID.MYSTERY_MUSHROM)
            clon.GetComponent<MysteryMushrom>().costumeNumber = this.extraNumber;
        StartCoroutine(CheckSpawnedContentLoop(clon));

        SpriteRenderer clonSp = clon.GetComponentInChildren<SpriteRenderer>();
        clonSp.sprite = TileManager.instance.GetSpriteFromTileset(GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].spriteId, GameManager.instance.blockDataManager.blockDatas[(int)this.contentBlock].tilesetType);
        int savedOrder = clonSp.sortingOrder;
        clonSp.sortingOrder = -1;
        bool wasEntity = false;
        if (clon.GetComponent<Entity>() != null){
            clon.GetComponent<Entity>().Spawn();
            if (clon.GetComponent<Entity>().canMove){
                clon.GetComponent<Entity>().canMove = false;
                wasEntity = true;
            }
        }
        if (this.contentBlock == BlockID.PROBELLER | this.contentBlock == BlockID.FIRE_ENEMY){
            clon.GetComponent<Animator>().StopPlayback();
            clon.GetComponent<Animator>().enabled = false;
        }

        if (clon.GetComponent<EntityGravity>() != null)
            clon.GetComponent<EntityGravity>().enabled = false;

        bool wasRB = false;
        if (clon.GetComponent<Rigidbody2D>() != null && !clon.GetComponent<Rigidbody2D>().isKinematic){
            wasRB = true;
            clon.GetComponent<Rigidbody2D>().isKinematic = true;
            clon.GetComponent<Rigidbody2D>().simulated = false;
        }
        if (clon.GetComponent<StarItem>() != null)
            clon.GetComponent<StarItem>().enabled = false;


        clon.transform.rotation = this.transform.rotation;
        if (this.contentBlock == BlockID.AIRBUBBLES)
            clon.GetComponent<AirBubbles>().Load();

        if(this.contentBlock == BlockID.AIRBUBBLES){
            yield return new WaitForSeconds(4f);
            for (int i = 0; i < 50; i++){
                if (clon != null)
                    clon.transform.Translate(0, 0.035f, 0);
                yield return new WaitForSeconds(0.01f);
            }
            Destroy(clon.gameObject);
        }else if(this.contentBlock == BlockID.PIRANHA | this.contentBlock == BlockID.FIRE_PIRANHA | this.contentBlock == BlockID.MUNCHER){
            int savedX = (int)clon.transform.position.x;
            int length = 50;
            if (this.contentBlock == BlockID.MUNCHER)
                length = 35;
            if (GetLookDirection(this.gameObject) == Vector3.down)
                length = (int) (length * 1.65f);

            for (int i = 0; i < length; i++){
                if (clon != null)
                    clon.transform.Translate(0, 0.015f, 0);
                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(2.5f);
            int length2 = length + 30;
            for (int i = 0; i < length2; i++){
                if(clon.GetComponent<Entity>() != null && clon.GetComponent<Entity>().isFreezedFromIceBall){
                    while (clon.GetComponent<Entity>().isFreezedFromIceBall)
                        yield return new WaitForSeconds(0);
                    if ((clon != null && savedX != (int)clon.transform.position.x)){
                        this.contentBlock = BlockID.ERASER;
                        this.StopAllCoroutines();
                    }
                }
                if (clon != null)
                    clon.transform.Translate(0, -0.015f, 0);
                yield return new WaitForSeconds(0.01f);
            }

            if (clon == null | (clon != null && savedX != (int)clon.transform.position.x)){
                this.contentBlock = BlockID.ERASER;
                this.StopAllCoroutines();
            }
            Destroy(clon.gameObject);
        }
        else{
            SceneManager.destroyAfterNewLoad.Add(clon);
            for (int i = 0; i < 35; i++){
                if(clon != null)
                    clon.transform.Translate(0, 0.015f, 0);
                yield return new WaitForSeconds(0.01f);
            }

            if (clon != null){
                clon.transform.rotation = Quaternion.Euler(0, 0, 0);

                clonSp.sortingOrder = savedOrder;
                if (wasEntity)
                    clon.GetComponent<Entity>().canMove = true;
                if (clon.GetComponent<EntityGravity>() != null)
                    clon.GetComponent<EntityGravity>().enabled = true;
                if (this.contentBlock == BlockID.PROBELLER | this.contentBlock == BlockID.FIRE_ENEMY){
                    clon.GetComponent<Animator>().enabled = true;
                    clon.GetComponent<Animator>().Play(0);
                }
                if (wasRB){
                    clon.GetComponent<Rigidbody2D>().isKinematic = false;
                    clon.GetComponent<Rigidbody2D>().simulated = true;
                }
                if (clon.GetComponent<StarItem>() != null)
                    clon.GetComponent<StarItem>().enabled = true;
            }
        }

        StartCoroutine(SpawnContentLoop());
    }

    public void LoadLength(int area){
        int leftID = 30;
        int rightID = 31;
        if (this.isSmallPipe){
            leftID = 164;
            this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, 148, TileManager.TilesetType.MainTileset);
        }else
            this.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, 14, TileManager.TilesetType.MainTileset);
        GameObject clonReference = this.transform.GetChild(0).gameObject;

        if(!this.isSmallPipe)
            clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, 15, TileManager.TilesetType.MainTileset);

        foreach (Transform child in this.transform){
            if (child != clonReference.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.pipeLength; i++){
            GameObject leftClon = Instantiate(clonReference, this.transform);
            leftClon.GetComponent<SpriteRenderer>().enabled = true;
            leftClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, leftID, TileManager.TilesetType.MainTileset);
            leftClon.transform.position = clonReference.transform.position + new Vector3(-1, -i);

            if (!this.isSmallPipe){
                GameObject rightClon = Instantiate(clonReference, this.transform);
                rightClon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromPreLoadedTileset(area, rightID, TileManager.TilesetType.MainTileset);
                rightClon.transform.position = clonReference.transform.position + new Vector3(0, -i);
                rightClon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            }

            leftClon.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        BoxCollider2D bx = GetComponent<BoxCollider2D>();
        if (this.isSmallPipe){
            bx.size = new Vector2(1, this.pipeLength + 0.6f);
            bx.offset = new Vector2(0, -0.200f - (0.5f * (this.pipeLength)));
        }else{
            bx.size = new Vector2(2, this.pipeLength + 1);
            bx.offset = new Vector2(0.5f, -(0.5f * this.pipeLength));
        }

        BoxCollider2D bx2 = GetComponents<BoxCollider2D>()[1];
        bx2.size = new Vector2(2.8f, this.pipeLength + 1);
        bx2.offset = new Vector2(0.5f, -(0.5f * this.pipeLength));
    }

    private void OnCollisionExit2D(Collision2D collision){
        this.canEnterPipe = false;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.canSpawnContent = false;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.canSpawnContent = true;
    }

    private IEnumerator CheckSpawnedContentLoop(GameObject contentObject){
        while(contentObject != null){
            yield return new WaitForSeconds(0);
        }
        this.spawnedContent.Remove(contentObject);
    }

    private void OnDestroy(){
        StopAllCoroutines();
    }

    public static Vector3 GetLookDirection(GameObject pipe){
        switch (pipe.transform.eulerAngles.z){
            case 270:
                return Vector3.right;
                break;
            case 180:
                return Vector3.down;
                break;
            case 90:
                return Vector3.left;
                break;
        }

        return Vector3.up;
    }

    public Vector3 GetOffset(){
        Vector3 direction = GetLookDirection(this.gameObject);
        if (direction == Vector3.right)
            return new Vector3(0.5f, -0.5f, 0);
        else if (direction == Vector3.down)
            return new Vector3(-0.5f, 0, 0);
        else if (direction == Vector3.left)
            return new Vector3(-0.5f, 0.5f, 0);
        return new Vector3(0.5f, 0.5f, 0);
    }

    public Vector3 GetOffset2(){
        Vector3 direction = GetLookDirection(this.gameObject);
        if (direction == Vector3.right)
            return new Vector3(0.035f, 0, 0);
        else if (direction == Vector3.down)
            return new Vector3(0, -0.035f, 0);
        else if (direction == Vector3.left)
            return new Vector3(-0.035f, 0, 0);
        return new Vector3(0, 0.035f, 0);
    }

    public Vector3 GetPlayerOffset(){
        Vector3 direction = GetLookDirection(this.gameObject);
        if (direction == Vector3.right)
            return new Vector3(1f, -0.5f, 0);
        else if (direction == Vector3.down)
            return new Vector3(-0.5f, -1, 0);
        else if (direction == Vector3.left)
            return new Vector3(-1f, 0.5f, 0);

        return new Vector3(0.5f, 0.5f, 0);
    }

}

public class PipePlayerCheck : MonoBehaviour{

    public Pipe myPipe;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.myPipe.input = collision.gameObject.GetComponentInChildren<InputManager>();

        if (collision.gameObject.layer == 9 && this.myPipe.directionKey == Pipe.Direction.UP && ((myPipe.isSmallPipe && collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini) | !myPipe.isSmallPipe))
            this.myPipe.canEnterPipe = true;
        else if (collision.gameObject.layer == 9 && this.myPipe.directionKey == Pipe.Direction.DOWN && ((myPipe.isSmallPipe && collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini) | !myPipe.isSmallPipe))
            this.myPipe.canEnterPipe = true;
        else if (collision.gameObject.layer == 9 && collision.gameObject.GetComponent<PlayerController>().GetOnGround() && ((myPipe.isSmallPipe && collision.gameObject.GetComponent<PlayerController>().GetPowerup() == PlayerController.Powerup.Mini) | !myPipe.isSmallPipe))
            this.myPipe.canEnterPipe = true;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9)
            this.myPipe.canEnterPipe = false;
    }
}
