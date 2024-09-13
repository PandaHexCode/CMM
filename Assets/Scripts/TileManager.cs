using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Script for manage all Tiles(Sprites)*/
public class TileManager : MonoBehaviour{

    public static TileManager instance;

    public Material spriteMat;
    public Material toadMat;
    [System.NonSerialized]public Tileset[] loadedTilesets = new Tileset[1];
    [System.NonSerialized]public Tileset currentTileset;
    [System.NonSerialized]public Style currentStyle;
    [System.NonSerialized]public StyleID currentStyleID;

    [System.Serializable]public enum TilesetType {MainTileset = 0, EnemyTileset = 1, EffectTileset = 2, BulletTileset = 3, ItemTileset = 4, ObjectsTileset = 5, ExtraTileset = 6};
    [System.Serializable]public enum TilesetID { Plain = 0, Underground = 1, Snow = 2, HauntedHouse = 3, Castle = 4, Desert = 5, Forest = 6, Sky = 7, Airship = 8, HauntedHouse2 = 9, Pumkin = 10,
    /*Special Tilesets (Starts with 20)*/ SMO_CASCADE = 20, SMO_CITY = 21, KIRBY_1 = 22, KIRBY_2 = 23, KIRBY_3 = 24, KIRBY_4 = 25, SMB_EXTRA1 = 26, DEBUG = 27, SMB3_EXTRA1 = 28, Autumn = 29, SMB3_Grass = 30, Volcan = 31, CaveVolcan = 32, Mountain = 33, Cloudy = 34, Water = 35, Custom = 90}
    [System.Serializable]public enum StyleID { SMB1 = 0, SMB3 = 1, SMAS1 = 2, SMAS3 = 3, SMW = 4,}

    [System.NonSerialized]public static List<Tile> loadedTiles = new List<Tile>();

    [System.Serializable]
    public class Style {
        public StyleID id;
        public PlayerSpriteManager.PlayerSprites playerSprites;
        public PlayerSpriteManager.PlayerSprites playerSpritesLuigi;
        public PlayerSpriteManager.PlayerSprites playerSpritesToad;
        public SoundManager.SoundEffects soundEffects;
        public Sprite[] effectTileset;
        public Sprite[] bulletTileset;
        public Sprite[] enemyTileset;
        public Sprite[] objectsTileset;
        public Sprite styleButtonSprite;
        public int editorBackgroundColorId;
        public PlayerSpriteManager.ToadColors[] toadColors;

        [System.NonSerialized] public Sprite[] originalEnemies;
        [System.NonSerialized] public Sprite[] originalObjects;
    }

    [System.Serializable]
    public class Tileset {
        public TilesetID id;public string customId;
        public Sprite[] mainTileset;
        public Sprite[] itemTileset;
        public Sprite[] replacedEnemyTiles;
        public Sprite[] replacedObjectTiles;
        public Sprite[] extraTilesetAnimations;
        public Sprite backgroundSprite;
        public Sprite secondBackgroundSprite;
        public GameObject backgroundExtraPrefarb;
        public AudioClip playModeBGM;
        public AudioClip editModeBGM;
        public Sprite tilesetButtonSprite;
        public Color shadowColor = new Color(0, 0, 0, 0.2078431f);
        public bool autoEnableIsWater = false;

        public Tileset(Sprite[] mainTileset, Sprite[] enemyTileset, Sprite[] itemTileset){
            this.mainTileset = mainTileset;
            this.itemTileset = itemTileset;
        }
    }

    [System.Serializable]
    public class Tile {
        public TilesetType tilesetType;
        public int spriteID;
        public SpriteRenderer renderer;

        public Tile(TilesetType tilesetType, int spriteID, SpriteRenderer renderer){
            this.tilesetType = tilesetType;
            this.spriteID = spriteID;
            this.renderer = renderer;
            TileManager.loadedTiles.Add(this);
        }
    }

    private void Awake(){
        if (instance != null)
            Destroy(this);
        instance = this;
        LoadTilesetWithId(TilesetID.Plain);
        this.loadedTilesets = new Tileset[2];
        LoadTilesetToMemoryWithId(TilesetID.Plain, 0);
        LoadTilesetToMemoryWithId(TilesetID.Underground, 1);
        LoadStyleWithId(StyleID.SMB1);
    }

    public void LoadTilesetWithIntButton(int intID){
        if(LevelEditorManager.instance != null && LevelEditorManager.isLevelEditor && (intID == (int)TilesetID.Castle | intID == (int)TilesetID.Volcan | intID == (int)TilesetID.CaveVolcan)){
            if ((LevelEditorManager.instance.currentArea == 0 && GameManager.instance.sceneManager.fullLevelLiquidIdArea0 == 0) | (LevelEditorManager.instance.currentArea == 1 && GameManager.instance.sceneManager.fullLevelLiquidIdArea1 == 0)){
                LevelEditorManager.instance.ToggleCastleLavaCurrentArea();
            }
        }else{
            if ((LevelEditorManager.instance.currentArea == 0 && GameManager.instance.sceneManager.fullLevelLiquidIdArea0 != 0) | (LevelEditorManager.instance.currentArea == 1 && GameManager.instance.sceneManager.fullLevelLiquidIdArea1 != 0)){
                LevelEditorManager.instance.ToggleCastleLavaCurrentArea();
                if(GameManager.instance.sceneManager.fullLevelLiquidIdArea0 == 2)
                    LevelEditorManager.instance.ToggleCastleLavaCurrentArea();
                if (GameManager.instance.sceneManager.fullLevelLiquidIdArea0 == 3)
                    LevelEditorManager.instance.ToggleCastleLavaCurrentArea();
            }
        }

        LoadTilesetWithId((TilesetID)intID);
        LoadTilesetToMemory(this.currentTileset, LevelEditorManager.instance.currentArea);
    }

    public void LoadTilesetWithId(TilesetID id){
        System.Object res = Resources.Load("Styles\\" + this.currentStyleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + id.ToString().Replace("TilesetID", "").Replace(".", "") + "Tileset");
        if(res == null){
            Debug.Log("NULL");
            LoadTilesetWithId(TilesetID.Plain);
            return;
        }
        GameObject obj = Instantiate((GameObject)res);
        try{
            LoadTileset(obj.GetComponent<MemoryTileset>().tileset);
            Destroy(obj);
        }catch(Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
            Destroy(obj);
            LoadTilesetWithIntButton(0);
        }
    }

    public void LoadTileset(Tileset tileset, bool dontChangeMusic = false){
        try{
            this.spriteMat.SetColor("_ShadowColor", tileset.shadowColor);
            this.toadMat.SetColor("_ShadowColor", tileset.shadowColor);
        }catch(Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
        }
        this.currentTileset = tileset;
        if (this.currentStyle != null){
            this.currentStyle.enemyTileset = new Sprite[this.currentStyle.originalEnemies.Length];
            this.currentStyle.originalEnemies.CopyTo(this.currentStyle.enemyTileset, 0);

            this.currentStyle.objectsTileset = new Sprite[this.currentStyle.originalObjects.Length];
            this.currentStyle.originalObjects.CopyTo(this.currentStyle.objectsTileset, 0);
        }

        if (this.currentStyle != null && tileset.replacedEnemyTiles.Length > 5){
            for (int i = 0; i < tileset.replacedEnemyTiles.Length; i++){
                if (tileset.replacedEnemyTiles[i] != null)
                    this.currentStyle.enemyTileset[i] = tileset.replacedEnemyTiles[i];
            }
        }

        if (this.currentStyle != null && tileset.replacedObjectTiles.Length > 5){
            for (int i = 0; i < tileset.replacedObjectTiles.Length; i++){
                if (tileset.replacedObjectTiles[i] != null)
                    this.currentStyle.objectsTileset[i] = tileset.replacedObjectTiles[i];
            }
        }

        foreach (SpriteRenderer renderer in GameManager.instance.sceneManager.backgroundRenderers){
            renderer.sprite = tileset.backgroundSprite;
        }

        foreach (SpriteRenderer renderer in GameManager.instance.sceneManager.secondBackgroundRenderers){
            renderer.sprite = tileset.secondBackgroundSprite;
        }

        if (tileset.secondBackgroundSprite == null)
            GameManager.instance.sceneManager.secondBackgroundRenderers[0].GetComponent<BackgroundParallax>().enabled = false;
        else
            GameManager.instance.sceneManager.secondBackgroundRenderers[0].GetComponent<BackgroundParallax>().enabled = true;

        if (LevelEditorManager.isLevelEditor && LevelEditorManager.instance != null){
            LevelEditorManager.instance.InitBlockButtons();
            GameManager.instance.sceneManager.backgroundButton.sprite = tileset.tilesetButtonSprite;
        }
        UpdateTiles();
        GroundTileConnector.SetAllWithoutPos();
        if (!dontChangeMusic){
            GameManager.instance.sceneManager.backgroundMusicManager.StopCurrentBackgroundMusic();
            GameManager.instance.sceneManager.backgroundMusicManager.StartPlayingBackgroundMusic();
        }

        GameManager.instance.sceneManager.playerCamera.StopAllCoroutines();
        GameManager.instance.sceneManager.playerCamera.GetComponent<Animator>().Play("Null");
        GameManager.instance.sceneManager.playerCamera.yOffset = 0;

        foreach(SpriteRenderer sp in GameManager.instance.sceneManager.backgroundRenderers){
            foreach (Transform child in sp.transform){
                if (child.GetComponent<SpriteRenderer>() == null)
                    Destroy(child.gameObject);
            }
        }

        if ((LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode) | LevelEditorManager.instance == null){
            if(tileset.backgroundExtraPrefarb != null){
                foreach (SpriteRenderer sp in GameManager.instance.sceneManager.backgroundRenderers){
                    GameObject extra = Instantiate(tileset.backgroundExtraPrefarb, sp.transform);
                }
            }

            if (tileset.id == TilesetID.Sky){
                GameManager.instance.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scrooling = true;
                GameManager.instance.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scroolSpeed = 2;
            }else if (tileset.id == TilesetID.Airship){
                GameManager.instance.sceneManager.playerCamera.minY = 19.1f;
                GameManager.instance.sceneManager.playerCamera.transform.position = new Vector3(GameManager.instance.sceneManager.playerCamera.transform.position.x, 19.1f, GameManager.instance.sceneManager.playerCamera.transform.position.z);
                GameManager.instance.sceneManager.playerCamera.GetComponent<Animator>().Play("CameraAirship");
                StartCoroutine(CamAirShipSafeCheck());
                GameManager.instance.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scrooling = true;
                GameManager.instance.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scroolSpeed = 5;
            }else if (tileset.autoEnableIsWater){
                foreach (PlayerController p in GameManager.instance.sceneManager.players)
                    p.StartCoroutine(p.AutoEnableWaterIE());
            }else{
                GameManager.instance.sceneManager.playerCamera.minY = 19;
                GameManager.instance.sceneManager.playerCamera.transform.position = new Vector3(GameManager.instance.sceneManager.playerCamera.transform.position.x, 19f, GameManager.instance.sceneManager.playerCamera.transform.position.z);
                GameManager.instance.sceneManager.backgroundRenderers[0].GetComponent<BackgroundParallax>().scrooling = false;
                foreach (PlayerController p in GameManager.instance.sceneManager.players)
                    p.SetIsInWater(false);
            }
        }

        if (tileset.id == TilesetID.Snow){
            GameManager.instance.sceneManager.snowEffect.GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, GetSpriteFromTileset(176, TilesetType.ObjectsTileset));
            GameManager.instance.sceneManager.snowEffect.SetActive(true);
            if ((LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode) | LevelEditorManager.instance == null)
                GameManager.instance.sceneManager.snowEffect.GetComponent<ParticleSystem>().Play();
            else
                GameManager.instance.sceneManager.snowEffect.GetComponent<ParticleSystem>().Pause();
        }else
            GameManager.instance.sceneManager.snowEffect.SetActive(false);
    }

    private IEnumerator CamAirShipSafeCheck(){
        yield return new WaitForSeconds(0.2f);
        if (((LevelEditorManager.instance != null && LevelEditorManager.instance.isPlayMode) | LevelEditorManager.instance == null) && this.currentTileset.id == TilesetID.Airship && GameManager.instance.sceneManager.playerCamera.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Null"))
            GameManager.instance.sceneManager.playerCamera.GetComponent<Animator>().Play("CameraAirship");
    }

    public void LoadTilesetFromMemory(int at, bool dontChangeMusic = false){
        LoadTileset(this.loadedTilesets[at], dontChangeMusic);
    }

    public void LoadTilesetToMemoryWithId(TilesetID id, int writeTo){
        System.Object res = Resources.Load("Styles\\" + this.currentStyleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + id.ToString().Replace("TilesetID", "").Replace(".", "") + "Tileset");
        if(res == null){
            LoadTilesetToMemoryWithId(TilesetID.Plain, writeTo);
            return;
        }
        
        GameObject obj = Instantiate((GameObject)res);
        LoadTilesetToMemory(obj.GetComponent<MemoryTileset>().tileset, writeTo);
        Destroy(obj);
    }

    public void LoadTilesetToMemory(Tileset tileset, int writeTo){
        this.loadedTilesets[writeTo] = tileset;
    }

    public void ReloadCurrentMemoryTilesets(){
        for (int i = 0; i < this.loadedTilesets.Length; i++){ 
            if(this.loadedTilesets[i] != null){
                LoadTilesetToMemoryWithId(this.loadedTilesets[i].id, i);
            }
        }
    }

    public void LoadStyleWithInt(int intID){
        LoadStyleWithId((StyleID)intID);
    }

    public void LoadStyleWithId(StyleID id){
        System.Object res = Resources.Load("Styles\\" + id.ToString().Replace("StyleID", "").Replace(".", "") + "\\Style");
        GameObject obj = Instantiate((GameObject)res);
        LoadStyle(obj.GetComponent<MemoryStyle>().style);
        Destroy(obj);
    }

    public void LoadStyle(Style style){
        this.currentStyleID = style.id;
        this.currentStyle = style;

        this.currentStyle.originalEnemies = new Sprite[this.currentStyle.enemyTileset.Length];
        this.currentStyle.enemyTileset.CopyTo(this.currentStyle.originalEnemies, 0);

        this.currentStyle.originalObjects = new Sprite[this.currentStyle.objectsTileset.Length];
        this.currentStyle.objectsTileset.CopyTo(this.currentStyle.originalObjects, 0);

        PlayerSpriteManager.instance.currentPlayerSprites = style.playerSprites;
        PlayerSpriteManager.instance.currentPlayerSpritesLuigi = style.playerSpritesLuigi;
        PlayerSpriteManager.instance.currentPlayerSpritesToad = style.playerSpritesToad;
        foreach (PlayerController p in GameManager.instance.sceneManager.players) {
            if (p.playerType == PlayerController.PlayerType.BLUETOAD)
                p.ChangeToadColor(style.toadColors[0]);
            else if (p.playerType == PlayerController.PlayerType.REDTOAD)
                p.ChangeToadColor(style.toadColors[1]);
            else if (p.playerType == PlayerController.PlayerType.GREENTOAD)
                p.ChangeToadColor(style.toadColors[2]);
        }

        SoundManager.currentSoundEffects = style.soundEffects;
        if (GameManager.instance != null && GameManager.instance.sceneManager != null){
            foreach (PlayerController p in GameManager.instance.sceneManager.players){
                p.SetPowerup(p.GetPowerup(), true);
                p.GetComponent<SpriteRenderer>().sprite = p.currentPlayerSprites.stand[0];
            }
        }
        if (LevelEditorManager.isLevelEditor){
            GameManager.instance.sceneManager.styleButton.sprite = style.styleButtonSprite;
            if(LevelEditorManager.instance != null)
                LevelEditorManager.instance.SetBackgroundColor(LevelEditorManager.instance.editorBackgroundColors[style.editorBackgroundColorId]);
        }
        LoadTilesetWithId(this.currentTileset.id);
        ReloadCurrentMemoryTilesets();
    }

    public void UpdateTiles(){
        List<Tile> remove = new List<Tile>();
        foreach(Tile tile in loadedTiles){
            if (tile.renderer == null)
                remove.Add(tile);
            else
                tile.renderer.sprite = GetSpriteFromTileset(tile.spriteID, tile.tilesetType);
        }

        foreach(Tile tile in remove){
            loadedTiles.Remove(tile);
        }

        remove.Clear();
    }

    public Sprite GetSpriteFromTileset(int number, TilesetType tilesetType){
        try{
            switch (tilesetType){
                case TilesetType.MainTileset:
                    return currentTileset.mainTileset[number];
                case TilesetType.EnemyTileset:
                    return currentStyle.enemyTileset[number];
                case TilesetType.EffectTileset:
                    return currentStyle.effectTileset[number];
                case TilesetType.BulletTileset:
                    return currentStyle.bulletTileset[number];
                case TilesetType.ItemTileset:
                    return currentTileset.itemTileset[number];
                case TilesetType.ObjectsTileset:
                    return currentStyle.objectsTileset[number];
                case TilesetType.ExtraTileset:
                    return currentTileset.extraTilesetAnimations[number];
                default:
                    Debug.LogError("TilesetType was invailed!");
                    return GameManager.instance.sceneManager.missingSpriteSprite;
            }
        }catch(Exception e){
            Debug.LogError("Missing Sprite!"+ number + ":" + tilesetType.ToString());
            return GameManager.instance.sceneManager.missingSpriteSprite;
        }
    }

    public Sprite GetSpriteFromPreLoadedTileset(int at, int number, TilesetType tilesetType){
        try{
            switch (tilesetType){
            case TilesetType.MainTileset:
                return loadedTilesets[at].mainTileset[number];
            case TilesetType.EnemyTileset:
                return currentStyle.enemyTileset[number];
            case TilesetType.EffectTileset:
                return currentStyle.effectTileset[number];
            case TilesetType.BulletTileset:
                return currentStyle.bulletTileset[number];
            case TilesetType.ItemTileset:
                return loadedTilesets[at].itemTileset[number];
            case TilesetType.ObjectsTileset:
                return currentStyle.objectsTileset[number];
            case TilesetType.ExtraTileset:
                return loadedTilesets[at].extraTilesetAnimations[number];
            default:
                Debug.LogError("TilesetType was invailed!");
                return GameManager.instance.sceneManager.missingSpriteSprite;
            }
        }catch (Exception e){
            Debug.LogError("Missing Sprite!" + number + ":" + tilesetType.ToString());
            return GameManager.instance.sceneManager.missingSpriteSprite;
        }
    }

}
