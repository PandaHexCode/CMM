using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteManager : MonoBehaviour{

    public static PlayerSpriteManager instance;

    public PlayerSprites currentPlayerSprites;
    public PlayerSprites currentPlayerSpritesLuigi;
    public PlayerSprites currentPlayerSpritesToad;

    [System.Serializable]
    public class PlayerSprites{
        public PlayerPowerupSprites smallMario;
        public PlayerPowerupSprites bigMario;
        public PlayerPowerupSprites fireMario;
        public PlayerPowerupSprites probellerMario;
        public PlayerPowerupSprites acornMario;
        public PlayerPowerupSprites iceMario;
        public PlayerPowerupSprites smashMario;
        public PlayerPowerupSprites smoMario;
        public PlayerPowerupSprites smoCaplessMario;
    }

    [System.Serializable]
    public class PlayerPowerupSprites {
        public Sprite[] stand;
        public Sprite[] walk;
        public Sprite[] run;
        public Sprite[] swim;
        public Sprite[] climb;
        public Sprite jump;
        public Sprite fall;
        public Sprite runJump;
        public Sprite runReturn;
        public Sprite duck;
        public Sprite[] death;
        public Sprite kick;
        public Sprite throwFireBall;
        public Sprite acornFly;
        public Sprite mysteryExtra;
        public Sprite[] probellerFly;
        public Sprite[] smashFlowerSmash;
        public Sprite[] spin;
        [Header("Grab")]
        public Sprite[] grabStand;
        public Sprite[] grabWalk;
        public Sprite[] grabSwim;
        public Sprite grabDuck;
    }

    [System.Serializable]
    public struct ToadColors{
        public Color standartCol1;
        public Color standartCol2;
        public Color standartCol3;
        public Color standartCol4;
    }

    private void Awake(){
        instance = this;
    }

}
