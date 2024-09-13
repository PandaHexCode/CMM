using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;

public class LevelEditorConnector : MonoBehaviour{/*for WarpBoxes, Doors etc (But not for pipes)*/

    public bool isMainConnector = false;
    public GameObject connectedBlock;

    public void ConnectBlock(bool isMainConnector, GameObject connectedBlock){
        this.connectedBlock = connectedBlock;
        this.isMainConnector = isMainConnector;
    }

    private void OnDestroy(){
        if (LevelEditorManager.instance != null && this.connectedBlock != null){
            BlockField blockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldDirectFromCurrentBlock(this.connectedBlock);
            if (blockField != null)
                LevelEditorManager.instance.blockFieldManager.CheckBlockField(blockField);
        }
    }

    public void GoToConnection(){
        GameManager.instance.sceneManager.players[0].transform.position = this.connectedBlock.transform.position;
    }

}
