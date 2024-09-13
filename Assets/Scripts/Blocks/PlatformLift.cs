using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformLift : MonoBehaviour{

    public LiftHelper.Direction direction;
    public int length;

    private float speed = 3f;
    private bool isInDirectionChange = false;
    private bool isInFall = false;

    private float up_goal = 0;
    private float down_goal = 0;
    private float left_goal = 0;
    private float right_goal = 0;

    public LayerMask standMask;

    private Transform _transform;
    private LiftHelper liftHelper;

    public void Register(){
        this._transform = this.transform;
        this.liftHelper = GetComponent<LiftHelper>();
        this.up_goal = this._transform.localPosition.y + 0.8f;
        this.down_goal = this._transform.localPosition.y - 0.8f;
        this.right_goal = this._transform.localPosition.x + 0.8f;
        this.left_goal = this._transform.localPosition.x - 0.8f;
    }

    public void LoadLength() {
        int middleSpriteID = 50;
        int endSpriteID = 51;
        if (this.direction == LiftHelper.Direction.BLUE) {
            middleSpriteID = 53;
            endSpriteID = 54;
        }

        GameObject clonReference = this.transform.GetChild(1).gameObject;
        Transform parent = this.transform.GetChild(0);
        clonReference.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(middleSpriteID, TileManager.TilesetType.ObjectsTileset);
        GameObject endRef = this.transform.GetChild(2).gameObject;
        endRef.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(endSpriteID, TileManager.TilesetType.ObjectsTileset);
        endRef.transform.localPosition = new Vector3(2, 0, 0);

        foreach (Transform child in parent) {
            if (child != clonReference.transform)
                Destroy(child.gameObject);
        }

        for (int i = 1; i <= this.length; i++) {
            GameObject clon = Instantiate(clonReference, parent);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(middleSpriteID, TileManager.TilesetType.ObjectsTileset);
            clon.transform.position = clonReference.transform.position + new Vector3(i, 0);
            endRef.transform.position = clon.transform.position + new Vector3(1, 0);
        }

        BoxCollider2D bx = GetComponent<BoxCollider2D>();
        bx.size = new Vector2(3 + this.length, 0.625f);
        bx.offset = new Vector2(1 + (0.5f * this.length), 0);

        if (this.direction == LiftHelper.Direction.BLUE){
            BoxCollider2D bx2 = GetComponents<BoxCollider2D>()[1];
            bx2.size = new Vector2(3 + this.length, 0.04268283f);
            bx2.offset = new Vector2(1 + (0.5f * this.length), 0.3227662f);
        }
    }

    private void Update(){
        if (this.direction == LiftHelper.Direction.BLUE | this.direction == LiftHelper.Direction.NONE)
            return;

        switch (this.direction){
            case LiftHelper.Direction.UP:
                if (this._transform.localPosition.y < this.up_goal | this.isInDirectionChange)
                    this.liftHelper.MoveLift(0, this.speed * Time.deltaTime, 0, direction);
                else
                    StartCoroutine(ChangeDirection(LiftHelper.Direction.DOWN));
                break;
            case LiftHelper.Direction.DOWN:
                if (this._transform.localPosition.y > this.down_goal | this.isInDirectionChange)
                    this.liftHelper.MoveLift(0, -this.speed * Time.deltaTime, 0, direction);
                else
                    StartCoroutine(ChangeDirection(LiftHelper.Direction.UP));
                break;
            case LiftHelper.Direction.RIGHT:
                if (this._transform.localPosition.x < this.right_goal | this.isInDirectionChange)
                    this.liftHelper.MoveLift(this.speed * Time.deltaTime, 0, 0, direction);
                else
                    StartCoroutine(ChangeDirection(LiftHelper.Direction.LEFT));
                break;
            case LiftHelper.Direction.LEFT:
                if (this._transform.localPosition.x > this.left_goal | this.isInDirectionChange)
                    this.liftHelper.MoveLift(-this.speed * Time.deltaTime, 0, 0, direction);
                else
                    StartCoroutine(ChangeDirection(LiftHelper.Direction.RIGHT));
                break;
        }
    }

    private IEnumerator ChangeDirection(LiftHelper.Direction toDirection){
        this.isInDirectionChange = true;
        float orgSpeed = this.speed;
        while(this.speed > 0){
            this.speed = this.speed - 2 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        this.direction = toDirection;

        while (this.speed < orgSpeed){
            this.speed = this.speed + 2 * Time.deltaTime;
            yield return new WaitForSeconds(0);
        }

        this.speed = orgSpeed;
        this.isInDirectionChange = false;
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(this.transform);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.isInFall && this.direction == LiftHelper.Direction.BLUE){
            if (collision.gameObject.transform.position.y < (this.transform.position.y + 0.5f))
                return;
            StartCoroutine(Falling());
        }
    }

    private void OnCollisionExit2D(Collision2D collision){
        if (collision.gameObject.layer == 9)
            collision.gameObject.transform.SetParent(null);
    }

    private IEnumerator Falling(){
        this.gameObject.AddComponent<Rigidbody2D>().freezeRotation = true;
        this.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0.2f;
        this.gameObject.GetComponent<Rigidbody2D>().mass = 4f;
        this._transform = this.transform;
        this.isInFall = true;
        float fallSpeed = 0;

        while(this._transform.position.y > 0){
            yield return new WaitForSeconds(0);
        }

        Destroy(this.gameObject);
    }

    private void OnDestroy(){
        foreach(PlayerController player in GameManager.instance.sceneManager.players){
            if (player.transform.parent == this.gameObject)
                player.transform.SetParent(null);
        }
    }
}
