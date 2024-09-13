using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakBlockEffect : MonoBehaviour{

    public bool normalEffect = false;
    public bool noSprites = false;

    private void Awake() {
        TileManager tileManager = TileManager.instance;
        if (this.normalEffect && !this.noSprites){
            GetComponentsInChildren<ParticleSystem>()[0].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(83, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[1].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(83, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[2].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(83, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[3].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(83, TileManager.TilesetType.MainTileset));
        }
        else if(!this.noSprites){
            GetComponentsInChildren<ParticleSystem>()[0].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(80, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[1].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(81, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[2].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(83, TileManager.TilesetType.MainTileset));
            GetComponentsInChildren<ParticleSystem>()[3].textureSheetAnimation.SetSprite(0, tileManager.GetSpriteFromTileset(82, TileManager.TilesetType.MainTileset));
        }
        StartCoroutine(DestroyIE());
    }

    private IEnumerator DestroyIE(){
        yield return new WaitForSecondsRealtime(2);
        Destroy(this.gameObject);
    }

}
