using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMM.BlockData;
using System;

[ExecuteAlways]
public class ThemeEditor : EditorWindow{

    public TileManager.StyleID styleID = TileManager.StyleID.SMB1;

    [MenuItem("UMM/ThemeEditor")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(ThemeEditor));
    }

    private void CheckPlayerPowerupMissingSprites(PlayerSpriteManager.PlayerPowerupSprites p, string name){
        string content = string.Empty;
        if (p.stand.Length < 1)
            content = content + " stand missing!\n";
        if (p.walk.Length < 1)
            content = content + " walk missing!\n";
        if (p.run.Length < 1)
            content = content + " run missing!\n";
        if (p.swim.Length < 1)
            content = content + " swim missing!\n";
        if (p.climb.Length < 1)
            content = content + " climb missing!\n";
        if (p.grabStand.Length < 1)
            content = content + " grab stand missing!\n";
        if (p.grabWalk.Length < 1)
            content = content + " grab walk missing!\n";
        if (p.grabSwim.Length < 1)
            content = content + " grab swim missing!\n";

        if (p.jump == null)
            content = content + " jump missing!\n";
        if (p.fall == null)
            content = content + " fall missing!\n";
        if (p.runReturn == null)
            content = content + " run return missing!\n";
        if (p.duck == null)
            content = content + " duck missing!\n";
        if (p.death == null)
            content = content + " death missing!\n";
        if (p.kick == null)
            content = content + " kick missing!\n";
        if (p.grabDuck == null)
            content = content + " grab duck missing!\n";

        if(content != string.Empty)
            Debug.Log(name + "\n" + content);
    }

    private void OnGUI(){
        GUILayout.Label("Theme Editor\n");
        this.styleID = (TileManager.StyleID)EditorGUILayout.EnumPopup("Style", this.styleID);

        GUILayout.Label("\nAdd PlayerSprites inspired from other Array\n");

        GameObject res = (GameObject)Resources.Load("Styles\\" + this.styleID.ToString().Replace("StyleID", "").Replace(".", "") + "\\Style");
        MemoryStyle style = res.GetComponent<MemoryStyle>();

        if(GUILayout.Button("Check for missing Sprites")){
            CheckPlayerPowerupMissingSprites(style.style.playerSprites.smallMario, "smallMario");
            CheckPlayerPowerupMissingSprites(style.style.playerSprites.bigMario, "bigMario");
            CheckPlayerPowerupMissingSprites(style.style.playerSprites.fireMario, "firelMario");
            CheckPlayerPowerupMissingSprites(style.style.playerSprites.probellerMario, "probellerMario");
            CheckPlayerPowerupMissingSprites(style.style.playerSprites.iceMario, "iceMario");

            CheckPlayerPowerupMissingSprites(style.style.playerSpritesLuigi.smallMario, "smallLuigi");
            CheckPlayerPowerupMissingSprites(style.style.playerSpritesLuigi.bigMario, "bigLuigi");
            CheckPlayerPowerupMissingSprites(style.style.playerSpritesLuigi.fireMario, "firelLuigi");
            CheckPlayerPowerupMissingSprites(style.style.playerSpritesLuigi.probellerMario, "probellerLuigi");
            CheckPlayerPowerupMissingSprites(style.style.playerSpritesLuigi.iceMario, "iceLuigi");
        }

        if (GUILayout.Button("Test Move Blocks Order from temp"))
        {
            GameManager.instance.blockDataManager.levelEditorBlocksOrder = GameManager.instance.blockDataManager.temp;
        }
        if(GUILayout.Button("Test Move Blocks Order")){
            GameManager.instance.blockDataManager.temp = GameManager.instance.blockDataManager.levelEditorBlocksOrder;
            int begin = 4;
            BlockID[] blockIDs = new BlockID[GameManager.instance.blockDataManager.levelEditorBlocksOrder.Length + 5];
            for (int i = 0; i < begin; i++){
                blockIDs[i] = GameManager.instance.blockDataManager.levelEditorBlocksOrder[i];
            }
            for (int i = begin; i < blockIDs.Length; i++){
                try
                {
                    blockIDs[i + 1] = GameManager.instance.blockDataManager.levelEditorBlocksOrder[i];
                }catch (Exception e)
                {

                }
            }

            GameManager.instance.blockDataManager.levelEditorBlocksOrder = blockIDs;
        }

        if (GUILayout.Button("BlocksOrder")){
            List<BlockID> ids = new List<BlockID>();
            foreach (BlockData blockData in GameManager.instance.blockDataManager.blockDatas){
                if (blockData.sortType == SortType.BLOCK && blockData.id != BlockID.CASTLESPECIALGROUND && blockData.id != BlockID.PSWITCHBLOCK && blockData.id != BlockID.PSWITCHBLOCK1 && blockData.id != BlockID.ICE_BLOCK && blockData.id != BlockID.SMALLBLOCK && blockData.id != BlockID.MINI_PIPE && blockData.id != BlockID.FIRE_BAR && blockData.id != BlockID.WATER && blockData.id != BlockID.POISON && blockData.id != BlockID.KEY_DOOR && blockData.id != BlockID.PSWITCH_DOOR && blockData.id != BlockID.LONGQUESTIONBLOCK)
                    ids.Add(blockData.id);

                if (blockData.id == BlockID.LAVA){
                    ids.Add(BlockID.POISON);
                    ids.Add(BlockID.WATER);
                }else if(blockData.id == BlockID.DOOR){
                    ids.Add(BlockID.PSWITCH_DOOR);
                    ids.Add(BlockID.KEY_DOOR);
                }else if(blockData.id == BlockID.QUESTION_BLOCK)
                    ids.Add(BlockID.LONGQUESTIONBLOCK);
                else if(blockData.id == BlockID.DISABLEDBURNER)
                    ids.Add(BlockID.FIRE_BAR);
                else if(blockData.id == BlockID.NORMAL_BLOCK){
                    ids.Add(BlockID.ICE_BLOCK);
                    ids.Add(BlockID.SMALLBLOCK);
                }
                else if(blockData.id == BlockID.PIPE)
                    ids.Add(BlockID.MINI_PIPE);
                else if(blockData.id == BlockID.DONUTBLOCK)
                    ids.Add(BlockID.CASTLESPECIALGROUND);
                else if(blockData.id == BlockID.ONOFF_BLUE_BLOCK){
                    ids.Add(BlockID.PSWITCHBLOCK);
                    ids.Add(BlockID.PSWITCHBLOCK1);
                }
            }
            GameManager.instance.blockDataManager.levelEditorBlocksOrder = ids.ToArray();
        }

        if(GUILayout.Button("EnemiesOrder")){
            List<BlockID> ids = new List<BlockID>();
            foreach(BlockData blockData in GameManager.instance.blockDataManager.blockDatas){
                if (blockData.sortType == SortType.ENEMY && blockData.id != BlockID.GOOMBRAT && blockData.id != BlockID.BOWSER && blockData.id != BlockID.BOORING && blockData.id != BlockID.PUMKINGOOMBA && blockData.id != BlockID.GROUNDBOO && blockData.id != BlockID.RED_KOOOPATROOPA)
                    ids.Add(blockData.id);
                if (blockData.id == BlockID.BOO){
                    ids.Add(BlockID.GROUNDBOO);
                    ids.Add(BlockID.BOORING);
                }
                if (blockData.id == BlockID.KOOOPATROOPA)
                    ids.Add(BlockID.RED_KOOOPATROOPA);
                if (blockData.id == BlockID.GOOMBA){
                    ids.Add(BlockID.GOOMBRAT);
                    ids.Add(BlockID.PUMKINGOOMBA);
                }
            }

            ids.Add(BlockID.BOWSER);

            GameManager.instance.blockDataManager.levelEditorEnemiesOrder = ids.ToArray();
        }

        if (GUILayout.Button("ItemsOrder")){
            List<BlockID> ids = new List<BlockID>();
            foreach (BlockData blockData in GameManager.instance.blockDataManager.blockDatas){
                if (blockData.sortType == SortType.ITEM && blockData.id != BlockID.MKITEMSLOT_FEATHER && blockData.id != BlockID.MKITEMSLOT_ONOFFSWITCHER && blockData.id != BlockID.MINI_MUSHROOM && blockData.id != BlockID.PROBELLER && blockData.id != BlockID.KEY && blockData.id != BlockID.MYSTERY_MUSHROM && blockData.id != BlockID.ICEFLOWER && blockData.id != BlockID.FIREFLOWER && blockData.id != BlockID.SMASHFLOWER && blockData.id != BlockID.ONEUP)
                    ids.Add(blockData.id);

                if (blockData.id == BlockID.MUSHROOM){
                    ids.Add(BlockID.ONEUP);
                    ids.Add(BlockID.PROBELLER);
                    ids.Add(BlockID.MYSTERY_MUSHROM);
                    ids.Add(BlockID.MINI_MUSHROOM);
                    ids.Add(BlockID.FIREFLOWER);
                    ids.Add(BlockID.ICEFLOWER);
                    ids.Add(BlockID.SMASHFLOWER);
                }
            }
            ids.Add(BlockID.MKITEMSLOT_FEATHER);
            ids.Add(BlockID.MKITEMSLOT_ONOFFSWITCHER);
            ids.Add(BlockID.KEY);

            GameManager.instance.blockDataManager.levelEditorItemsOrder = ids.ToArray();
        }

        if (GUILayout.Button("SmallLuigi from SmallMario"))
            style.style.playerSpritesLuigi.smallMario = InspireArray(style.style.playerSprites.smallMario);
        if (GUILayout.Button("BigLuigi from BigMario"))
            style.style.playerSpritesLuigi.bigMario = InspireArray(style.style.playerSprites.bigMario);
        if (GUILayout.Button("FireLuigi from FireMario"))
            style.style.playerSpritesLuigi.fireMario = InspireArray(style.style.playerSprites.fireMario);
        if (GUILayout.Button("ProbellerLuigi from ProbellerMario"))
            style.style.playerSpritesLuigi.probellerMario = InspireArray(style.style.playerSprites.probellerMario);
        if (GUILayout.Button("AcornLuigi from AcornMario"))
            style.style.playerSpritesLuigi.acornMario = InspireArray(style.style.playerSprites.acornMario);
        if (GUILayout.Button("IceLuigi from IceMario"))
            style.style.playerSpritesLuigi.iceMario = InspireArray(style.style.playerSprites.iceMario);
        if (GUILayout.Button("SmashLuigi from SmashMario"))
            style.style.playerSpritesLuigi.smashMario = InspireArray(style.style.playerSprites.smashMario);
        if (GUILayout.Button("SmoLuigi from SmoMario"))
            style.style.playerSpritesLuigi.smoMario = InspireArray(style.style.playerSprites.smoMario);
        if (GUILayout.Button("SmoCaplessLuigi from SmoCaplessMario"))
            style.style.playerSpritesLuigi.smoCaplessMario = InspireArray(style.style.playerSprites.smoCaplessMario);
        if (GUILayout.Button("IceLuigi from FireLuigi"))
            style.style.playerSpritesLuigi.iceMario = InspireArray(style.style.playerSpritesLuigi.fireMario);
        if (GUILayout.Button("ProbellerLuigi from BigLuigi"))
            style.style.playerSpritesLuigi.probellerMario = InspireArray(style.style.playerSpritesLuigi.bigMario);
        if (GUILayout.Button("ttttt")){
            GameObject res2 = (GameObject)Resources.Load("Styles\\" + TileManager.StyleID.SMAS1.ToString().Replace("StyleID", "").Replace(".", "") + "\\Style");
            MemoryStyle style2 = res2.GetComponent<MemoryStyle>();
            style.style.playerSpritesToad = style2.style.playerSpritesToad;
        }
        if (GUILayout.Button("ttttt2"))
        {
            style.style.playerSpritesToad.smoCaplessMario = InspireArray(style.style.playerSpritesToad.bigMario);

        }
    }

    public PlayerSpriteManager.PlayerPowerupSprites InspireArray(PlayerSpriteManager.PlayerPowerupSprites inspiration){
        Sprite[] inputSprites = new Sprite[Selection.objects.Length];
        for (int i = 0; i < Selection.objects.Length; i++){
            try{
                inputSprites[i] = (Sprite)Selection.objects[i];
            }
            catch (Exception ex){
            }
        }

        PlayerSpriteManager.PlayerPowerupSprites output = new PlayerSpriteManager.PlayerPowerupSprites();

        output.stand = TryToCathSpriteArray(inspiration.stand, inputSprites);
        output.walk = TryToCathSpriteArray(inspiration.walk, inputSprites);
        output.run = TryToCathSpriteArray(inspiration.run, inputSprites);
        output.swim = TryToCathSpriteArray(inspiration.swim, inputSprites);
        output.jump = TryToCathSprite(inspiration.jump, inputSprites);
        output.fall = TryToCathSprite(inspiration.fall, inputSprites);
        output.runReturn = TryToCathSprite(inspiration.runReturn, inputSprites);
        output.duck = TryToCathSprite(inspiration.duck, inputSprites);
        output.death = TryToCathSpriteArray(inspiration.death, inputSprites);
        output.throwFireBall = TryToCathSprite(inspiration.throwFireBall, inputSprites);
        output.acornFly = TryToCathSprite(inspiration.acornFly, inputSprites);
        output.probellerFly = TryToCathSpriteArray(inspiration.probellerFly, inputSprites);
        output.spin = TryToCathSpriteArray(inspiration.spin, inputSprites);
        output.smashFlowerSmash = TryToCathSpriteArray(inspiration.smashFlowerSmash, inputSprites);
        output.climb = TryToCathSpriteArray(inspiration.climb, inputSprites);
        output.grabStand = TryToCathSpriteArray(inspiration.grabStand, inputSprites);
        output.grabWalk = TryToCathSpriteArray(inspiration.grabWalk, inputSprites);
        output.grabSwim = TryToCathSpriteArray(inspiration.grabSwim, inputSprites);
        output.grabDuck = TryToCathSprite(inspiration.grabDuck, inputSprites);
        Debug.Log("Suscesfully!");
        return output;
    }

    private Sprite TryToCathSprite(Sprite inspirationSprite, Sprite[] inputSprites){
        try{
            int numb = 0;
            int.TryParse(inspirationSprite.name.Split('_')[1], out numb);

            return inputSprites[numb];
        }catch(Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
            return null;
        }
    }

    private Sprite[] TryToCathSpriteArray(Sprite[] inspirationSprites, Sprite[] inputSprites){
        try{
            Sprite[] output = new Sprite[inspirationSprites.Length];
            for (int i = 0; i < inspirationSprites.Length; i++){
                output[i] = TryToCathSprite(inspirationSprites[i], inputSprites);
            }

            return output;
        }catch (Exception e){
            Debug.Log(e.Message + "|" + e.StackTrace);
            return null;
        }
    }
}
