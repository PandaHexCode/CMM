using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBlock : MonoBehaviour{

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && collision.GetComponent<Rigidbody2D>().velocity.y > -5){
            StartCoroutine(RotateIE());
        }
    }

    private IEnumerator RotateIE(){
        //GameManager.StopTimeScale();
        Transform par = GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea).transform;
        while(GameManager.instance.sceneManager.GetAreaParent(GameManager.instance.sceneManager.currentArea).transform.rotation.z < 90){
            if (Input.GetKey(KeyCode.UpArrow))
                par.transform.Rotate(0, 0, 10 * Time.deltaTime);
            else if (Input.GetKey(KeyCode.DownArrow))
                par.transform.Rotate(0, 0, -10 * Time.deltaTime);
            par.transform.Translate(10 * Time.deltaTime, 0, 0);
            yield return new WaitForSecondsRealtime(0);
        }
        GameManager.ResumeTimeScale();
    }

}
