using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarItem : TileAnimator{

    [Header("StarItem")]
    public float velocityX;
    public Vector2 velocity;

    private Rigidbody2D rb;
    private GameObject effect;
    private LayerMask layerMask;
    private Transform _transform;

    private void Start(){
        this._transform = this.transform;
        this.rb = GetComponent<Rigidbody2D>();
        this.effect = GameManager.instance.sceneManager.coinEffect;
        this.layerMask = GameManager.instance.entityWandMask;
        StartCoroutine(EffectLoop());
    }

    private void Update(){
        CheckWand();
        if (Time.timeScale > 0){
            _transform.Translate(velocityX * Time.deltaTime, 0, 0);
            if (rb.velocity.y < velocity.y)
                rb.velocity = velocity;
        }
    }

    public void CheckWand(){
        if (velocityX < 0){
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.left, 0.5f, layerMask);
            if (raycastHit2D)
                velocityX = -velocityX;
        }else{
            RaycastHit2D raycastHit2D = Physics2D.Raycast(_transform.position, Vector2.right, 0.5f, layerMask);
            if (raycastHit2D)
                velocityX = -velocityX;
        }
    }

    private IEnumerator EffectLoop(){
        GameObject eff = Instantiate(this.effect);
        eff.transform.position = this._transform.position + new Vector3(UnityEngine.Random.Range(-1, 3), UnityEngine.Random.Range(-1, 3), UnityEngine.Random.Range(1, 3));
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(EffectLoop());
    }

    private void OnCollisionEnter2D(Collision2D collision){
        rb.velocity = new Vector2(velocity.x, -velocity.y);

        if (collision.gameObject.layer == 9){
            collision.gameObject.GetComponent<PlayerController>().GetStar();
            GameObject effect = Instantiate(GameManager.instance.sceneManager.coinEffect);
            effect.transform.position = this._transform.position;
            SceneManager.respawnableEntities.Remove(SceneManager.GetRespawnableEntityFromEntity(this.gameObject));
            Destroy(this.gameObject);
        }
    }

}
