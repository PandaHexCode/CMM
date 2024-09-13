using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class TileAnimatorTestDevWindow : EditorWindow{

    private TileManager.StyleID styleID = TileManager.StyleID.SMB1;
    private TileManager.TilesetID tilesetID = TileManager.TilesetID.Plain;
    private GameObject gameObject;
    private bool isInAnimation = false;private Sprite originalSprite;

    [MenuItem("UMM/TestTileAnimator")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(TileAnimatorTestDevWindow));
    }

    private void OnGUI(){
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("GameObject");
        GameObject oldObject = this.gameObject;
        this.gameObject = EditorGUILayout.ObjectField(this.gameObject, typeof(GameObject), true) as GameObject;
        if(oldObject != this.gameObject){
            oldObject.GetComponent<TileAnimator>().StopAllCoroutines();
            oldObject.GetComponent<SpriteRenderer>().sprite = this.originalSprite;
            this.isInAnimation = false;
        }

        EditorGUILayout.BeginHorizontal();
        this.styleID = (TileManager.StyleID)EditorGUILayout.EnumPopup("Style", this.styleID);
        this.tilesetID = (TileManager.TilesetID)EditorGUILayout.EnumPopup("Tileset", this.tilesetID);
        EditorGUILayout.EndHorizontal();

        if (this.gameObject != null && this.gameObject.GetComponent<TileAnimator>() == null){
            GUILayout.Label("No TileAnimatior script was found on " + this.gameObject.name + " !");
            return;
        }

        TileAnimator animator = this.gameObject.GetComponent<TileAnimator>();
        animator.sp = animator.GetComponent<SpriteRenderer>();

        EditorGUILayout.BeginHorizontal();
        if (this.isInAnimation){
            if (GUILayout.Button("Stop Animation")){
                animator.StopAllCoroutines();
                animator.sp.sprite = this.originalSprite;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        foreach(TileAnimator.AnimationClip clip in animator.animationClips){
            if (this.isInAnimation){
                animator.StopAllCoroutines();
                animator.sp.sprite = this.originalSprite;
            }

            this.isInAnimation = true;
            this.originalSprite = animator.sp.sprite;
            GUILayout.Label("\nClipId: " + clip.id + 
                "\nDelay: " + clip.delay +
                "\nRepeat: " + clip.repeat +
                "\nWaitBeforeRepeat: " + clip.waitBeforeRepeat +
                "\nBackwards: " + clip.backwards);

            GUILayout.Label("TileIds: ");
            foreach (int tileId in clip.tileNumbers)
                GUILayout.Label("-" + tileId);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Play")){
                animator.StartCoroutine(PlayAnimationClip(clip, animator.sp));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private IEnumerator PlayAnimationClip(TileAnimator.AnimationClip animationClip, SpriteRenderer sp){
        GameObject res = (GameObject)Resources.Load("Styles\\" + this.styleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Style");
        MemoryStyle style = res.GetComponent<MemoryStyle>();
        GameObject res2 = (GameObject)Resources.Load("Styles\\" + this.styleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + this.tilesetID.ToString().Replace("TilesetID", "") + "Tileset");
        MemoryTileset tileset = res2.GetComponent<MemoryTileset>();

        if (!animationClip.backwards){
            foreach (int tile in animationClip.tileNumbers){
                Debug.Log(tile);
                sp.sprite = GetSpriteFromTileset(tile, animationClip.tilesetType, style, tileset);
                yield return new WaitForSecondsRealtime(animationClip.delay);
            }
        }else{
            for (int i = animationClip.tileNumbers.Length - 1; i >= 0; i--){
                sp.sprite = GetSpriteFromTileset(animationClip.tileNumbers[i], animationClip.tilesetType, style, tileset);
                yield return new WaitForSecondsRealtime(animationClip.delay);
            }
        }

        if (animationClip.repeat){
            if (animationClip.waitBeforeRepeat)
                yield return new WaitForSecondsRealtime(0.5f);
            sp.GetComponent<TileAnimator>().StartCoroutine(PlayAnimationClip(animationClip, sp));
        }else 
            this.isInAnimation = false;
    }

    public Sprite GetSpriteFromTileset(int id, TileManager.TilesetType type, MemoryStyle style, MemoryTileset tileset){
        switch (type){
            case TileManager.TilesetType.MainTileset:
                return tileset.tileset.mainTileset[id];
            case TileManager.TilesetType.EnemyTileset:
                return style.style.enemyTileset[id];
            case TileManager.TilesetType.EffectTileset:
                return style.style.effectTileset[id];
            case TileManager.TilesetType.BulletTileset:
                return style.style.bulletTileset[id];
            case TileManager.TilesetType.ItemTileset:
                return tileset.tileset.itemTileset[id];
            case TileManager.TilesetType.ObjectsTileset:
                return style.style.objectsTileset[id];
            case TileManager.TilesetType.ExtraTileset:
                return tileset.tileset.extraTilesetAnimations[id];
            default:
                Debug.LogError("TilesetType was invailed!");
                return GameManager.instance.sceneManager.missingSpriteSprite;
        }
    }

}
