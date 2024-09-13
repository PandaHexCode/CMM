using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMoveBlock : TileAnimator{

    public int length = 1;
    public LayerMask layerMask;
    public OnOffState onOffState = OnOffState.NONE;
    public enum OnOffState { NONE = 0, Red = 1, Blue = 2};
    
    private int fast = 0;
    private bool lastOnOffState = true;

    private LayerMask wallMask;

    [System.NonSerialized]public float moveSpeed = -5.5f;
    private List<Transform> currentMovingObjects = new List<Transform>();

    private void Awake(){
        this.wallMask = GameManager.instance.entityWandMask;
    }

    private void Update(){
        foreach (Transform tr in this.currentMovingObjects){
            Vector2 vt = Vector2.left;
            if (this.moveSpeed > 0)
                vt = Vector2.right;
            bool ray1 = SceneManager.EntityWallCheckRay(tr, vt);
            if (!ray1)
                tr.Translate(this.moveSpeed * Time.deltaTime, 0, 0);
        }
    }

    public void LoadLength() {
        GameObject clonReference = this.transform.GetChild(0).gameObject;
        Transform parent = this.transform.GetChild(2);
        clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(56, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, 56, clonReference.GetComponent<SpriteRenderer>()));
        GameObject endRef = this.transform.GetChild(1).gameObject;
        endRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(57, TileManager.TilesetType.ObjectsTileset);
        TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, 57, endRef.GetComponent<SpriteRenderer>()));
        endRef.transform.localPosition = new Vector3(2, 0, 0);

        for (int i = 1; i <= this.length; i++) {
            GameObject clon = Instantiate(clonReference, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(56, TileManager.TilesetType.ObjectsTileset);
            clon.transform.position = clonReference.transform.position + new Vector3(i, 0);
            endRef.transform.position = clon.transform.position + new Vector3(1, 0);

            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, 56, clon.GetComponent<SpriteRenderer>()));
        }


        BoxCollider2D bx = GetComponent<BoxCollider2D>();
        bx.size = new Vector2(3 + this.length, 1);
        bx.offset = new Vector2(1 + (0.5f * this.length), 0);

        bx = GetComponents<BoxCollider2D>()[1];
        bx.size = new Vector2(3 + this.length, 0.07f);
        bx.offset = new Vector2(1 + (0.5f * this.length), 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (!collision.isTrigger && GameManager.IsInLayerMask(collision.gameObject, this.layerMask)){
            this.currentMovingObjects.Add(collision.gameObject.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (!collision.isTrigger && GameManager.IsInLayerMask(collision.gameObject, this.layerMask)){
            this.currentMovingObjects.Remove(collision.gameObject.transform);
            this.currentMovingObjects.Remove(collision.gameObject.transform);
            if (collision.gameObject.layer != 9){
                collision.gameObject.GetComponent<EntityGravity>().StopAllCoroutines();
                if(!collision.gameObject.GetComponent<EntityGravity>().onGround)
                    collision.gameObject.GetComponent<EntityGravity>().StartCoroutine(SceneManager.DropGameObject(collision.gameObject, moveSpeed / 1.5f));
            }
        }
    }

    public void LoadDirection(LiftHelper.Direction direction){
        if (this.onOffState == OnOffState.Red){
            this.animationClips[0].tileNumbers = new int[] { 127, 130, 133, 136, 139, 142, 145, 148 };
            this.transform.GetChild(0).GetComponent<TileAnimator>().animationClips[0].tileNumbers = new int[] { 128, 131, 134, 137, 140, 143, 146, 149 };
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].tileNumbers = new int[] { 129, 132, 135, 138, 141, 144, 147, 150 };
        }else if (this.onOffState == OnOffState.Blue){
            this.animationClips[0].tileNumbers = new int[] { 91, 94, 97, 100, 103, 106, 109, 112 };
            this.transform.GetChild(0).GetComponent<TileAnimator>().animationClips[0].tileNumbers = new int[] { 92, 95, 98, 101, 104, 107, 110, 113 };
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].tileNumbers = new int[] { 93, 96, 99, 102, 105, 108, 111, 114 };
        }

        if (direction == LiftHelper.Direction.RIGHT){
            if(this.moveSpeed < 0)
                this.moveSpeed = -this.moveSpeed;
          
            this.animationClips[0].backwards = true;
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].backwards = true;
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].backwards = true;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = true;
        }else{
            if (this.moveSpeed > 0)
                this.moveSpeed = -this.moveSpeed;
            this.animationClips[0].backwards = false;
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].backwards = false;
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].backwards = false;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = false;
        }

        if (this.fast == 1){
            this.animationClips[0].delay = 0.013f;
            this.transform.GetChild(1).GetComponent<TileAnimator>().animationClips[0].delay = 0.0139f;
            this.transform.GetChild(0).GetComponent<TileAnimator>().animationClips[0].delay = 0.013f;
        }
    }

    public void LoadIsFast(int numb){
        this.fast = numb;

        if(numb == 1)
            this.moveSpeed = -10;
    }

    public void ChangeDirectionFromOnOff(){
        if (this.onOffState == OnOffState.NONE | this.lastOnOffState == SceneManager.onOffState)
            return;

        this.lastOnOffState = SceneManager.onOffState;
        if (this.onOffState == OnOffState.Blue)
            this.onOffState = OnOffState.Red;
        else
            this.onOffState = OnOffState.Blue;

        if (this.moveSpeed > 0)
            LoadDirection(LiftHelper.Direction.LEFT);
        else
            LoadDirection(LiftHelper.Direction.RIGHT);
        foreach (Transform child in this.transform.GetChild(2)){
            if (child != this.transform.GetChild(2))
                Destroy(child.gameObject);
        }
        LoadLength();
    }

}
