using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryModeNPC : SpriteAnimator{

    public StoryModeManager.DialogText[] dialog;

    private bool canStartDialog = false;
    private GameObject currentPopupInfo = null;

    private void Update(){
        if (this.canStartDialog && GameManager.instance.sceneManager.players[0].input.UP){
            StoryModeManager.instance.StartDialogBox(this.dialog);
            StopCoroutine("DestroyPopupIE");
            this.canStartDialog = false;
            StartCoroutine(DestroyPopupIE());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            StopCoroutine("DestroyPopupIE");
            this.canStartDialog = true;
            if (this.currentPopupInfo != null)
                Destroy(this.currentPopupInfo);
            this.currentPopupInfo = Instantiate(StoryModeManager.instance.dialogPopupInfo, this.transform);
            this.currentPopupInfo.transform.position = this.transform.position + new Vector3(-0.4f, 0.8f, 0);
        }
    }

    private void OnTriggerExit2D(Collider2D collision){
        if (collision.gameObject.layer == 9){
            StopCoroutine("DestroyPopupIE");
            this.canStartDialog = false;
            StartCoroutine(DestroyPopupIE());
        }
    }

    private IEnumerator DestroyPopupIE(){
        this.currentPopupInfo.GetComponent<Animator>().Play("MessageInfoPopup_Disable");
        yield return new WaitForSeconds(0.2f);
        if (this.currentPopupInfo != null)
                Destroy(this.currentPopupInfo);
    }

}
