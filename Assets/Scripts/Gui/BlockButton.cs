using UnityEngine;
using UMM.BlockData;

public class BlockButton : MonoBehaviour{

    public BlockID blockID;
    private bool canPress = false;
    private static SpriteRenderer lastBlockButton = null;

    private void Update(){
        if (this.canPress && Input.GetMouseButtonDown(0))
            Choose();
    }

    public void Choose(){
        this.canPress = false;
        GameManager.instance.sceneManager.levelEditorCursor.SetCurrentBlock(this.blockID);
        if (lastBlockButton != null){
            lastBlockButton.color = Color.white;
            lastBlockButton.GetComponent<Animator>().enabled = false;
            lastBlockButton.transform.GetChild(0).localPosition = Vector3.zero;
        }
        GetComponent<SpriteRenderer>().color = new Color(1, 0.7960784f, 0, 1);
        GetComponent<Animator>().enabled = true;
        GetComponent<Animator>().Play(0);
        lastBlockButton = GetComponent<SpriteRenderer>();
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
