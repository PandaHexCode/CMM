using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashFire : ExplodeEffect{

    public float speed = 5f;

    private void Awake(){
        SceneManager.destroyAfterNewLoad.Add(this.gameObject);
    }

    private void Update(){
        this.transform.Translate(this.speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 11)
            Destroy(this.gameObject);
    }

}
