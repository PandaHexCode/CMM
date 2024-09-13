using UnityEngine;
using UMM.BlockData;

[CreateAssetMenu(fileName = "BlockDataManager", menuName = "UMM/BlockDataManager", order = 1)]
public class BlockDataManager : ScriptableObject{
    public GameObject editorTempBlockPrefarb;
    public BlockData[] blockDatas;
    [Header("LevelEditorOrder")]
    public BlockID[] levelEditorBlocksOrder;
    public BlockID[] levelEditorItemsOrder;
    public BlockID[] levelEditorEnemiesOrder;
    public BlockID[] levelEditorDecoOrder;
    public BlockID[] levelEditorSpecialOrder;
    public BlockID[] temp;
}

namespace UMM.BlockData{
    public enum BlockID{
        ERASER = -2,
        CONNECTED_BLOCK = -1,
        GROUND = 0,
        NORMAL_BLOCK = 1,
        GOOMBA = 2,
        COIN = 3,
        QUESTION_BLOCK = 4,
        BREAK_BLOCK = 5,
        MUSHROOM = 6,
        FIREFLOWER = 7,
        PROBELLER = 8,
        PIPE = 9,
        INVISBLE_QUESTION_BLOCK = 10,
        STARTPOINT = 11,
        ONOFF_SWITCHER = 12,
        ONOFF_RED_BLOCK = 13,
        ONOFF_BLUE_BLOCK = 14,
        KOOOPATROOPA = 15,
        LAVA = 16,
        LAVA_BLOCK = 17,
        MKITEMSLOT_FEATHER = 18,
        MKITEMSLOT_ONOFFSWITCHER = 19,
        WARPBOX = 20,
        CLOUD = 21,
        DONUTBLOCK = 22,
        ONEUP = 23,
        POWERSTAR = 24,
        SPIKE_BLOCK = 25,
        RED_KOOOPATROOPA = 26,
        MESSAGEBLOCK = 27,
        MUNCHER = 28,
        PIRANHA = 29,
        FIRE_PIRANHA = 30,
        BOO = 31,
        FIRE_ENEMY = 32,
        BOMB_ENEMY = 33,
        DECO1 = 34,
        DECOBUSH = 35,
        DECO2 = 36,
        DECO3 = 37,
        DECOBIGARROW = 38,
        DOOR = 39,
        ACORN = 40,
        CHECKPOINT = 41,
        GOAL1 = 42,
        GOAL2 = 43,
        GOAL3 = 44,
        ONEWAYWALL = 45,
        ICEFLOWER = 46,
        SMASHFLOWER = 47,
        PSWITCH = 48,
        DECOARROW = 49,
        GRINDER = 50,
        DRYBONE = 51,
        BILLENEMY = 52,
        BILLBLASTER = 53,
        REDBILLENEMY = 54,
        REDBILLBLASTER = 55,
        BIGBILLENEMY = 56,
        REDBIGBILLENEMY = 57,
        THWOMP = 58,
        GOOMBRAT = 59,
        GROUNDBOO = 60,
        WIGGLER = 61,
        BURNER = 62,
        DISABLEDBURNER = 63,
        Coin10 = 64,
        Coin30 = 65,
        Coin50 = 66,
        DECOCASTLE = 67,
        CAPPYITEM = 68,
        MYSTERY_MUSHROM = 69,
        ICE_BLOCK = 70,
        PSWITCH_DOOR = 71,
        PLATFORM = 72,
        BLUEPLATFORM = 73,
        SPEEDMOVEBLOCK = 74,
        DECOPLATFORM = 75,
        LONGQUESTIONBLOCK = 76,
        FAKEWALL = 77,
        VINE = 78,
        DECOMUSHROOM_PLATFORM = 79,
        FIRE_BAR = 80,
        CAM_STOPPER = 81, 
        INVISBLE_BLOCK = 82,
        PLATFORM_SPAWNER = 83,
        HAMMERBRO = 84,
        BIGHAMMERBRO = 85,
        SPIKETOP = 86,
        FLYSHELL = 87,
        GREEN_FLYSHELL = 88,
        BUZZYBEETLE = 89,
        SPINNY = 90,
        BOWSER = 91,
        CASTLESPECIALGROUND = 92,
        KEY = 93,
        KEY_DOOR = 94,
        MINI_MUSHROOM = 95,
        SMALLBLOCK = 96,
        MINI_PIPE = 97,
        POISON = 98,
        POISON_BLOCK = 99,
        WATER = 100,
        WATER_BLOCK = 101,
        REDSKYROTATEBLOCK = 102,
        GREENSKYROTATEBLOCK = 103,
        BONEPLATFORM = 104,
        BLUEBONEPLATFORM = 105,
        AIRBUBBLES = 106,
        GREENCHEEPCHEEP = 107,
        REDCHEEPCHEEP = 108,
        TRAMPOLINE = 109,
        BOORING = 110,
        FAKEDOOR = 111,
        LIGHTBLOCK = 112,
        FAKEQUESTIONBLOCK = 113,
        PSWITCHBLOCK = 114,
        PSWITCHBLOCK1 = 115,
        PUMKINGOOMBA = 116,
        ICE = 117,
        LIGHTBLOCK_SMALL = 118,
        BULLY = 119,
    }

    public enum SortType{
        NULL = -1,
        BLOCK = 0,
        ITEM = 1,
        ENEMY = 2,
        DECO = 3,
        SPECIAL = 4,
    }

    [System.Serializable]
    public class BlockData{
        public BlockID id;
        public GameObject prefarb;
        public int spriteId;
        public TileManager.TilesetType tilesetType;
        public bool keepOriginalSprite = false;
        public bool canRespawn = false;
        public bool canPutInItemBlock = false;
        public bool canPutInPipe = false;
        public int maxPipeSpawnSize = 3;
        public bool canSpawnInRandomizer = true;
        public bool glowInDark = false;
        public Vector3 darkGlowMaskSize;
        [Header("ExtraEditorSettings")]
        public SortType sortType = SortType.BLOCK;
        public GameObject customEditorPrefarb;
        public Vector2 needSize = Vector2.zero;
        public Vector3 placeOffset = Vector2.zero;
        public bool canSpam = true;
        public bool smallIcon = false;
        public bool onlyForSMB1 = false;
        public bool dontDisplayButtonInEditor = false;
        public bool dontRenderInPlayMode = false;
        public bool cantDestroy = false;
        public Vector3 blockGUICustomSize = Vector3.zero;
    }
}

namespace UMM.LevalDatas{

    public enum Tag{
        NULL = -1,
        Standart = 0,
        Puzzle = 1,
        Speedrun = 2,
        Automatic = 3,
        Short = 4,
        Multiplayer = 5,
        Themed = 6,
        Music = 7,
        Art = 8,
        Technical = 9,
        Boss_battle = 10,
        Troll = 11,
        Deleted = 12,/*Unused*/
        Kaizo = 13,
        Remake = 14,
        Cute = 15,
        Modded = 16,
    }

    public static class LevelDataConverter{

        public static Tag StringToTag(string str){
            string[] names = System.Enum.GetNames(typeof(Tag));

            for (int i = 0; i < names.Length; i++){
                if (names[i].Replace("_", " ").Equals(str))
                    return (Tag)i;
            }

            return Tag.NULL;
        }

    }

}