using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEditorBlocksSlider : MonoBehaviour{

    public int value = 0;
    public int maxValue = 23;
    public float startPosX = 2.24f;
    public float endPosX = -2.24f ;

    private bool canPress;

    private void Update(){
        if(this.canPress && Input.GetMouseButtonDown(0)){
            StartSliderMove();
        }
    }

    public void StartSliderMove(){
        StartCoroutine(SliderMovingIE());
    }

    private IEnumerator SliderMovingIE(){
        Transform cursor = GameManager.instance.sceneManager.levelEditorCursor.transform;
        cursor.GetComponent<LevelEditorCursor>().SetCanBuild(false);

        while (!Input.GetMouseButtonUp(0) && (cursor.localPosition.x > -2f && cursor.localPosition.x < 2f)){
            if (this.transform.position.x > cursor.position.x - 0.2f){
                float steps = Vector2.Distance(new Vector2(this.transform.position.x, 0), new Vector2(cursor.position.x - 1, 0));
                SetCurrentValue(this.value - (int)steps);
            }else{
                float steps = Vector2.Distance(new Vector2(this.transform.position.x, 0), new Vector2(cursor.position.x + 1, 0));
                SetCurrentValue(this.value + (int)steps);
            }
            yield return new WaitForSeconds(0);
        }
    }

    public void SetCurrentValue(int newValue){
        if (newValue < 0 | newValue > this.maxValue)
            return;

        this.value = newValue;

        float steps = Vector2.Distance(new Vector2(this.startPosX, 0), new Vector2(this.endPosX, 0)) / this.maxValue;
        this.transform.localPosition = new Vector3(this.startPosX - (this.value * steps), this.transform.localPosition.y);
        LevelEditorManager.instance.blockButtonsParent.transform.localPosition = new Vector3(-this.value * 1.4324324324324324324324324324324f, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 13)
            this.canPress = true;
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 13)
            this.canPress = false;
    }

}
