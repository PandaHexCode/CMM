using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : TileAnimator{

    public float VelocityX;
    public Vector2 Velocity;
    public bool isIceBall = false, iceBallGroundHited = false;

    private Rigidbody2D rb;
    public GameObject Effect;
    private GameObject mySpawner;

    public LayerMask layerMask;
    private void Start(){
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.up * Velocity.y;
        Velocity = rb.velocity;
        StartAnimationClip(this.animationClips[0]);
    }

    private void Update(){
        transform.Translate(VelocityX * Time.deltaTime, 0, 0);
        if (rb.velocity.y < Velocity.y)
            rb.velocity = Velocity;
    }

    public void SetSpawner(GameObject Player){
        this.mySpawner = Player;
    }

    public GameObject GetSpawner(){
        return mySpawner;
    }

    private void OnCollisionEnter2D(Collision2D collision){
        rb.velocity = new Vector2(Velocity.x, -Velocity.y);
        if (this.isIceBall){
            if (this.iceBallGroundHited)
                Explode();
            else
                StartCoroutine(SetIceBallGroundHitedIE());
        }
    }

    private IEnumerator SetIceBallGroundHitedIE(){
        yield return new WaitForSeconds(0.1f);
        this.iceBallGroundHited = true;
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (GameManager.IsInLayerMask(collision.gameObject, layerMask) && !collision.isTrigger){
            SoundManager.PlayAudioClip(SoundManager.currentSoundEffects.hitBlock);
            Explode();
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11)
            Explode();
    }

    public void Explode(){
        GameObject eff = Instantiate(Effect);
        eff.transform.position = transform.position;
        if (mySpawner != null)
            mySpawner.GetComponent<PlayerController>().RemoveFireBall(this.gameObject);
        Destroy(gameObject);
    }
}
