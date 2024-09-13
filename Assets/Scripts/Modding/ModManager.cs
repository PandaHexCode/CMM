using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "ModManager", menuName = "UMM/ModManager", order = 5)]
public class ModManager : ScriptableObject{
    
    public static List<Mod> loadedMods = new List<Mod>();

    public static List<TileManager.Tileset> injectedCustomThemes = new List<TileManager.Tileset>();

    public class Mod{
        public string modName;
        public string modDescription;
        public string modAuthor;
        public float modVersion;
        public int modImportance;
        public float targetVersion;
        public string folderPath;

        public void InjectMod(){
            if (Directory.Exists(folderPath + "\\CustomThemes")){
                List<string> files = GameManager.GetDirectories(folderPath + "\\CustomThemes", "*");
                foreach (string folderPath in files){
                    InjectCustomTheme(folderPath);
                }
            }
        }
    }

    public static void LoadMod(string folderPath){
        Mod mod = new Mod();

        try{
            string fileContent = GameManager.GetFileIn(folderPath + "\\mod-info.cmm");
            if (1.4f.ToString().Contains(","))
                fileContent = fileContent.Replace(".", ",");
            else
                fileContent = fileContent.Replace(",", ".");
            string[] modInfo = fileContent.Split('\n');
            mod.modName = modInfo[1].Split('=')[1];
            mod.modDescription = modInfo[2].Split('=')[1];
            mod.modAuthor = modInfo[3].Split('=')[1];
            mod.modVersion = GameManager.StringToFloat(modInfo[4].Split('=')[1]);
            mod.modImportance = GameManager.StringToInt(modInfo[5].Split('=')[1]);
            mod.folderPath = folderPath;
            mod.InjectMod();
        }catch(Exception e){
            Debug.LogError("ERROR loading Mod: " + folderPath + "\nMessage:" + e.Message + "\nStackTrace" + e.StackTrace);
            return;
        }

       // if (GameManager.VERSION_FLOAT > mod.targetVersion)
           // Debug.Log("Mod was made for an older CMM version(" + mod.targetVersion +")!");

        ModManager.loadedMods.Add(mod);
        Debug.Log("Mod " + mod.modName + " loaded successfully!");
    }

    public static void InjectCustomTheme(string folderPath){
        if(!File.Exists(folderPath + "\\Theme-info.cmm")){
            Debug.LogError(folderPath + " Theme-info.cmm not found");
            return;
        }

        Debug.Log(folderPath);

        string[] infos = GameManager.GetFileIn(folderPath + "\\Theme-info.cmm").Split('\n');
        TileManager.StyleID style = (TileManager.StyleID)GameManager.StringToInt(infos[0].Split('=')[1]);

        System.Object res = Resources.Load("Styles\\" + style.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + TileManager.TilesetID.Plain.ToString().Replace("TilesetID", "").Replace(".", "") + "Tileset");
        if (res == null){
            Debug.Log("NULL");
            return;
        }

        GameObject obj = Instantiate((GameObject)res);
        TileManager.Tileset refTileset = null;

        try{
            refTileset = obj.GetComponent<MemoryTileset>().tileset;
            Destroy(obj);
        }catch (Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
            Destroy(obj);
        }

        System.Object res2 = Resources.Load("Styles\\" + style.ToString().Replace("StyleID", "").Replace(".", "") + "\\Style");
        GameObject obj2 = Instantiate((GameObject)res2);
        TileManager.Style refStyle = obj2.GetComponent<MemoryStyle>().style;
        Destroy(obj2);

        TileManager.Tileset tileset = new TileManager.Tileset(null, null, null);
        tileset.id = TileManager.TilesetID.Custom;
        tileset.customId = infos[1].Split('=')[1];

        tileset.mainTileset = FileToSpriteArray(folderPath + "\\Main.png", refTileset.mainTileset);
        tileset.itemTileset = FileToSpriteArray(folderPath + "\\Items.png", refTileset.itemTileset);
        tileset.replacedEnemyTiles = FileToSpriteArray(folderPath + "\\Enemies.png", refStyle.enemyTileset);
        tileset.replacedObjectTiles = FileToSpriteArray(folderPath + "\\Objects.png", refStyle.objectsTileset);
        tileset.backgroundSprite = LoadSprite(folderPath + "\\Background.png", TextureFormat.R16);
      //  tileset.playModeBGM

        if (File.Exists(folderPath + "\\Button.png"))
            tileset.tilesetButtonSprite = LoadSprite(folderPath + "\\Button.png", TextureFormat.R16);
        else
            tileset.tilesetButtonSprite = refTileset.tilesetButtonSprite;
        ModManager.injectedCustomThemes.Add(tileset);
        Debug.Log("InjectedCustomTheme: " + tileset.customId);
    }

    public static Sprite[] FileToSpriteArray(string path, Sprite[] orginal){
        if (!File.Exists(path)){
            return orginal;
        }

        List<Sprite> array = new List<Sprite>();
        Sprite main = LoadSprite(path, orginal[0].texture.format);

        foreach(Sprite sprite in orginal){
            array.Add(Sprite.Create(main.texture, sprite.rect, new Vector2(0.5f, 0.5f), sprite.pixelsPerUnit));
        }

        return array.ToArray();
    }

    private static Sprite LoadSprite(string path, TextureFormat format){
        if (string.IsNullOrEmpty(path)) return null;
        if (System.IO.File.Exists(path)){
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.filterMode = FilterMode.Point;
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16);
            return sprite;
        }
        return null;
    }

    public static TileManager.Tileset TryToPatchTileset(TileManager.Tileset tileset){
        foreach(Mod mod in ModManager.loadedMods){
            string tilesetPath = mod.folderPath + "\\Textures\\Styles\\" + TileManager.instance.currentStyleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Tilesets\\" + tileset.id.ToString().Replace("TilesetID", "").Replace(".", "");
            if (Directory.Exists(tilesetPath)){
                if (File.Exists(tilesetPath + "\\Main.png"))
                    tileset.mainTileset = FileToSpriteArray(tilesetPath + "\\Main.png", tileset.mainTileset);
                if (File.Exists(tilesetPath + "\\Items.png"))
                    tileset.itemTileset = FileToSpriteArray(tilesetPath + "\\Items.png", tileset.itemTileset);
                if (File.Exists(tilesetPath + "\\ReplacedEnemies.png"))
                    tileset.replacedEnemyTiles = FileToSpriteArray(tilesetPath + "\\ReplacedEnemies.png", TileManager.instance.currentStyle.enemyTileset);
                if (File.Exists(tilesetPath + "\\ReplacedObjects.png"))
                    tileset.replacedObjectTiles = FileToSpriteArray(tilesetPath + "\\ReplacedObjects.png", TileManager.instance.currentStyle.objectsTileset);
                if (File.Exists(tilesetPath + "\\Background.png"))
                    tileset.backgroundSprite = LoadSprite(tilesetPath + "\\Background.png", TextureFormat.R16);
            }
        }

        return tileset;
    }

    public static TileManager.Style TryToPatchStyle(TileManager.Style style){
        foreach (Mod mod in ModManager.loadedMods){
            string stylePath = mod.folderPath + "\\Textures\\Styles\\" + style.id.ToString().Replace("StyleID", "").Replace(".", "") + "\\Static";
            if (Directory.Exists(stylePath)){
                if (File.Exists(stylePath + "\\Enemies.png"))
                    style.enemyTileset = FileToSpriteArray(stylePath + "\\Enemies.png", style.enemyTileset);
                if (File.Exists(stylePath + "\\Objects.png"))
                    style.objectsTileset = FileToSpriteArray(stylePath + "\\Objects.png", style.objectsTileset);
                if (File.Exists(stylePath + "\\Bullet.png"))
                    style.bulletTileset = FileToSpriteArray(stylePath + "\\Bullet.png", style.bulletTileset);
            }
        }

        return style;
    }

    public static TileManager.Tileset GetTilesetFromCustomId(string customId){
        foreach(TileManager.Tileset tileset in ModManager.injectedCustomThemes){
            if (tileset.customId.Equals(customId)){
                return tileset;
            }
        }
        return null;
    }

}
