using UnityEngine;
using UMM.Mystery;
using UMM.Unlock;

[CreateAssetMenu(fileName = "MysteryCostumesManager", menuName = "UMM/MysteryCostumesManager", order = 2)]
public class MysteryCostumesManager : ScriptableObject{
    public MysteryCostume[] mysteryCostumes;
    public Sprite[] buttonSprites;
}

namespace UMM.Mystery{
    [System.Serializable]
    public class MysteryCostume{
        public string name = string.Empty;
        public bool isUnlockable = false;
        public UnlockID unlockID;
        public bool isUnused = false;
        public bool isSpecial = false;public int specialID = 0;
        public bool isPatreon = false;
        public PlayerSpriteManager.PlayerPowerupSprites sprites;
    }
}