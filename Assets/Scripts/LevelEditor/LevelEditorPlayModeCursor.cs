using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorPlayModeCursor : MonoBehaviour{

    private GameObject targetPlayer = null;
    private bool isInMove = false;
    private bool savedBoolean = false;

    private Transform _transform;
    private LevelEditorManager levelEditorManager;
    private Camera cam;

    private void Awake(){
        this._transform = this.transform;
        this.levelEditorManager = LevelEditorManager.instance;
        this.cam = Camera.main;
    }

    private void OnEnable(){
        this.targetPlayer = null;
    }

    private void OnDisable(){
        this.targetPlayer = null;
    }

    private void Update(){
        Vector3 pz = cam.ScreenToWorldPoint(Input.mousePosition);
        pz.z = 0;
        this._transform.position = pz;

        if(Input.GetMouseButton(0) && this.targetPlayer != null && Time.timeScale != 0){
            this.isInMove = true;
            GameManager.instance.sceneManager.playerCamera.FreezeCamera();
            this.targetPlayer.GetComponent<Rigidbody2D>().isKinematic = true;
            this.targetPlayer.GetComponent<Rigidbody2D>().simulated = false;
            this.targetPlayer.transform.position = pz;
        }

        if(Input.GetMouseButtonUp(0) && this.targetPlayer != null && Time.timeScale != 0){
            this.isInMove = false;
            if (this.savedBoolean)
                GameManager.instance.sceneManager.playerCamera.FreezeCamera();
            else
                GameManager.instance.sceneManager.playerCamera.UnfreezeCamera();
            this.targetPlayer.GetComponent<Rigidbody2D>().isKinematic = false;
            this.targetPlayer.GetComponent<Rigidbody2D>().simulated = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.isInMove){
            this.targetPlayer = collision.gameObject;
            this.savedBoolean = GameManager.instance.sceneManager.playerCamera.IsFreeze();
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9 && !this.isInMove)
            this.targetPlayer = null;
    }

}