using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorBillBlaster : MonoBehaviour{

    public int length = 0;

    public void LoadLength(){
        foreach (Transform child in this.transform.GetChild(0))
            Destroy(child.gameObject);

        for (int i = 0; i < this.length; i++){
            GameObject clon = new GameObject("Clon");
            clon.transform.SetParent(this.transform.GetChild(0));
            clon.transform.localPosition = new Vector3(0, -1 - i - 0.5f, 0);
            clon.AddComponent<SpriteRenderer>();
            clon.GetComponent<SpriteRenderer>().material = SettingsManager.instance.spriteMaterial;
            clon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            clon.GetComponent<SpriteRenderer>().sprite = TileManager.instance.GetSpriteFromTileset(33, TileManager.TilesetType.EnemyTileset);
            TileManager.loadedTiles.Add(new TileManager.Tile(TileManager.TilesetType.EnemyTileset, 33, clon.GetComponent<SpriteRenderer>()));
        }

        this.transform.GetChild(1).localPosition = new Vector3(0, -0.5f - length - 0.5f, 0);
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
            if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y < lastY){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.length++;
                LoadLength();
            }else if ((int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y > lastY && this.length != 0){
                lastY = (int)GameManager.instance.sceneManager.levelEditorCursor.transform.position.y;
                this.length--;
                LoadLength();
            }

            yield return new WaitForSeconds(0);
        }
        
        GameManager.instance.sceneManager.levelEditorCursor.currentAction = LevelEditorCursor.CursorAction.NOTHING;
        button.GetComponent<SpriteRenderer>().enabled = false;
    }
}
