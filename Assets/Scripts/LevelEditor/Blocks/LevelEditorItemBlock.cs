using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockData;

public class LevelEditorItemBlock : MonoBehaviour{

    public bool isBreakBlock = false;
    [System.NonSerialized]public int extraNumber = 0;
    private BlockID contentBlock = BlockID.COIN;

    private void Awake(){
        if (this.isBreakBlock)
            this.contentBlock = BlockID.ERASER;
    }

    public void SetContentBlock(BlockID id, GameObject from = null){
        this.contentBlock = id;
        if (!this.isBreakBlock && id == BlockID.COIN)
            this.transform.GetChild(0).gameObject.SetActive(false);
        else if(id == BlockID.ERASER)
            this.transform.GetChild(0).gameObject.SetActive(false);
        else
            this.transform.GetChild(0).gameObject.SetActive(true);

        if (id == BlockID.MYSTERY_MUSHROM && from != null)
            this.extraNumber = from.GetComponent<LevelEditorMysteryMushrom>().costumeNumber;
    }

    public BlockID GetContentBlock(){
        return this.contentBlock;
    }

}
