using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.BlockField;

public class LevelEditorSkyRotateBlock : MonoBehaviour{

    public int size = 1;
    public Vector3 startPos;
    public BlockField myBlockField;

    public void LoadSize(Vector3 startPos, BlockField blockField = null){
        if (startPos != Vector3.zero)
            this.startPos = startPos;

        if (this.myBlockField != null){
            BlockField targetBlockField = this.myBlockField;
            for (int i = 0; i < (this.size + 1) * 2; i++){
                for (int z = 0; z < (this.size + 1) * 2; z++){
                    BlockField blockField1 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField.blockFieldNumber + z);
                    if (blockField1.mainBlockFieldNumber[0][LevelEditorManager.instance.currentArea] != this.myBlockField.blockFieldNumber | blockField1.currentBlock[0][LevelEditorManager.instance.currentArea] != this.gameObject | this.myBlockField == blockField1)
                        continue;

                    blockField1.currentBlock[0][LevelEditorManager.instance.currentArea] = null;
                    blockField1.mainBlockFieldNumber[0][LevelEditorManager.instance.currentArea] = -1;
                    blockField1.blockId[0][LevelEditorManager.instance.currentArea] = UMM.BlockData.BlockID.GROUND;
                }
                targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField);
            }
        }

        if (blockField != null)
            this.myBlockField = blockField;
       

        this.transform.localScale = new Vector3(this.size, this.size, 1);

        if(this.startPos != Vector3.zero)
            this.transform.position = this.startPos + new Vector3(this.size - 1, this.size - 1, 0);

        if (this.myBlockField != null){
            BlockField targetBlockField = this.myBlockField;
            for (int i = 0; i < this.size * 2; i++){
                for (int z = 0; z < this.size * 2; z++){
                    BlockField blockField1 = LevelEditorManager.instance.blockFieldManager.GetBlockFieldAt(targetBlockField.blockFieldNumber + z);
                    if (blockField1.currentBlock[0][LevelEditorManager.instance.currentArea] == this.gameObject | this.myBlockField == blockField1)
                        continue;

                    LevelEditorManager.instance.blockFieldManager.CheckBlockField(blockField1);
                    blockField1.currentBlock[0][LevelEditorManager.instance.currentArea] = this.gameObject;
                    blockField1.mainBlockFieldNumber[0][LevelEditorManager.instance.currentArea] = this.myBlockField.blockFieldNumber;
                    blockField1.blockId[0][LevelEditorManager.instance.currentArea] = UMM.BlockData.BlockID.CONNECTED_BLOCK;
                }
                targetBlockField = LevelEditorManager.instance.blockFieldManager.GetBlockFieldOverBlockField(targetBlockField);
            }
        }
    }

    public void AddSize(){
        this.size++;
        LoadSize(Vector3.zero);
    }

    public void RemoveSize(){
        if (this.size == 1)
            return;
        this.size--;
        LoadSize(Vector3.zero);
    }

}
