using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMM.BlockData;

[ExecuteAlways]
public class BlockDataViewerWindow : EditorWindow{

    private BlockID currentBlockID = BlockID.GROUND;
    private List<Texture2D> textures = new List<Texture2D>();
   
    [MenuItem("UMM/BlockDataViewer")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(BlockDataViewerWindow));
    }


    private void OnGUI(){
        GUILayout.Label("BlockData Viewer\n");
        foreach(Texture2D texture2D in textures){
            GUILayout.Box(texture2D);
        }

        this.currentBlockID = (BlockID)EditorGUILayout.EnumPopup("BlockID", this.currentBlockID);
        if (GUILayout.Button("Render")){
            textures.Clear();
            BlockData blockData = Camera.main.GetComponentInChildren<GameManager>().blockDataManager.blockDatas[(int)this.currentBlockID];
 
            foreach(TileManager.StyleID style in System.Enum.GetValues(typeof(TileManager.StyleID))){
                foreach (TileManager.TilesetID id in System.Enum.GetValues(typeof(TileManager.TilesetID))){
                    LoadSprite(blockData.spriteId, blockData.tilesetType, style, id);
                }
            }
        }
    }

    private void LoadSprite(int spriteID, TileManager.TilesetType type, TileManager.StyleID styleID, TileManager.TilesetID tilesetID){
        TileManager.Tileset refTileset;
        System.Object res = Resources.Load("Styles\\" + styleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + tilesetID.ToString().Replace("TilesetID", "").Replace(".", "") + "Tileset");
        GameObject obj = Instantiate((GameObject)res);
        refTileset = obj.GetComponent<MemoryTileset>().tileset;
        DestroyImmediate(obj);
        Sprite sprite = null;
        switch (type){
            case TileManager.TilesetType.MainTileset:
                sprite = refTileset.mainTileset[spriteID];
                break;
            case TileManager.TilesetType.ItemTileset:
                sprite = refTileset.itemTileset[spriteID];
                break;
            case TileManager.TilesetType.EnemyTileset:
              //  sprite = refTileset.enemyTileset[spriteID];
                break;
        }
        if (!sprite.texture.isReadable){
            Debug.LogError("Tileset is not readable!");
            return;
        }
        var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                (int)sprite.textureRect.y,
                                                (int)sprite.textureRect.width,
                                                (int)sprite.textureRect.height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();
        textures.Add(croppedTexture);
    }

}
