using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boo : Entity{

    public float moveSpeedY;

    private Transform player;
    private SpriteRenderer playerSP;
    private Transform _transform;

    private void Start(){
        this.player = GameManager.instance.sceneManager.players[0].transform;
        this.playerSP = this.player.GetComponent<SpriteRenderer>();
        this._transform = this.transform;
    }

    private void Update(){
        if (this.isCaptured)
            return;

        if (sp.flipX != playerSP.flipX){
            this.canMove = true;
            this.sp.sprite = TileManager.instance.currentStyle.enemyTileset[26];
        }else{
            this.canMove = false;
            this.sp.sprite = TileManager.instance.currentStyle.enemyTileset[27];
        }

        if (_transform.position.x > player.position.x)
            this.sp.flipX = false;
        else
            this.sp.flipX = true;

        if (canMove){
            this._transform.position = Vector2.MoveTowards(this._transform.position, this.player.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-1f, 1f)), this.moveSpeed * Time.deltaTime);
        }
    }

}
