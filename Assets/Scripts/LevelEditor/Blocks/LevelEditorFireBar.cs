using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorFireBar : MonoBehaviour{

    public int lengthY = 2;
    public int direction = 0;

    public void LoadLength(){
        Transform parent = this.transform.GetChild(0).transform;
        float z = parent.rotation.z;
        this.transform.GetChild(1).rotation = Quaternion.Euler(0,0,0);
        parent.rotation = Quaternion.Euler(0, 0, 0);
        this.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = true;

        foreach (Transform child in parent){
            if (child != this.transform.GetChild(0).transform.GetChild(0))
                Destroy(child.gameObject);
            else
                child.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }

        for (int i = 0; i < this.lengthY; i++){
            GameObject clon = Instantiate(this.transform.GetChild(1).gameObject, parent);
            clon.transform.position = this.transform.GetChild(1).position + new Vector3(0, (i * 0.45f), 0);
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(175, TileManager.TilesetType.ObjectsTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.ObjectsTileset, 175, clon.GetComponent<SpriteRenderer>()));
        }

        this.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
        this.transform.GetChild(0).transform.GetChild(0).localPosition = new Vector3(0, this.lengthY * 0.5f - 0.3f, 0);
        parent.rotation = Quaternion.Euler(0, 0, (int)z);
    }

    public void ChangeDirection(){
        if (this.direction == 0){
            this.transform.GetChild(2).GetComponent<SpriteRenderer>().flipY = true;
            this.direction = 1;
        }else{
            this.transform.GetChild(2).GetComponent<SpriteRenderer>().flipY = false;
            this.direction = 0;
        }
    }

     public void StartMovingLength(GameObject button){
        if (GameManager.instance.sceneManager.levelEditorCursor.currentAction == LevelEditorCursor.CursorAction.MOVE)
            return;

        button.GetComponent<SpriteRenderer>().enabled = true;
        GameManager.instance.sceneManager.levelEditorCursor.StopAllCoroutines();
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
        StartCoroutine(MovingLengthIE(button));
    }

    private IEnumerator MovingLengthIE(GameObject button){
        int orgY = 0;
        if(this.transform.eulerAngles.z == 0)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
        else if (this.transform.eulerAngles.z == -90 | this.transform.eulerAngles.z == 270 | this.transform.eulerAngles.z == 90)
            orgY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.x;

        int lastY = orgY;
        while (!Input.GetMouseButtonUp(0)){
            GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.CHANGE_BLOCK_ACTION;
            float distance = Vector3.Distance(GameManager.instance.sceneManager.levelEditorCursor.transform.position, this.transform.position);
            if((int)distance > 1)
                this.lengthY = (int)distance;
            LoadLength();
            Vector3 mouse_pos = Input.mousePosition;
            mouse_pos.z = 5.23f; //The distance between the camera and object
            Vector3 object_pos = Camera.main.WorldToScreenPoint(this.transform.GetChild(0).transform.position);
            mouse_pos.x = mouse_pos.x - object_pos.x;
            mouse_pos.y = mouse_pos.y - object_pos.y;
            float angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
            transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, 0, (int)angle - 80));
            yield return new WaitForSeconds(0);
        }
        
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }
}
