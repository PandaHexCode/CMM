using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.Unlock;
using System.IO;

[CreateAssetMenu(fileName = "UnlockManager", menuName = "UMM/UnlockManager", order = 2)]
public class UnlockManager : ScriptableObject{

    public static UnlockManager instance;
    public UnlockableThing[] unlockableThings;

    private string savePath;

    private void Awake(){
        UnlockManager.instance = this;
        this.savePath = Application.persistentDataPath + "\\unlock.dat";
        LoadFile();
    }

    public void SaveFile(){
        string content = string.Empty;
        foreach(UnlockableThing unlockableThing in this.unlockableThings){
            content = content + (int)unlockableThing.id + ":" + unlockableThing.isUnlocked + "\n";
        }

        content = GameManager.Encrypt(content, "UMMUNLOCK");
        GameManager.SaveFile(this.savePath, content);
    }

    public void LoadFile(){
        if (!File.Exists(this.savePath)){
            SaveFile();
            return;
        }

        string content = GameManager.GetFileIn(this.savePath);
        content = GameManager.Decrypt(content, "UMMUNLOCK");
        string[] lines = content.Split('\n');
        foreach(string line in lines){
            if (line == string.Empty)
                continue;

            string[] args = line.Split(':');
            int id = GameManager.StringToInt(args[0]);
            if (args[1].Equals("true", System.StringComparison.OrdinalIgnoreCase))
                this.unlockableThings[id].isUnlocked = true;
            else
                this.unlockableThings[id].isUnlocked = false;
        }
    }

}

namespace UMM.Unlock{
    
    public enum UnlockID { NULL = 0, MYSTERY_BOWSER = 1, MYSTERY_QUESTIONBLOCK = 2, MYSTERY_TOAD = 3, MYSTERY_GOLDMARIO = 4, MYSTERY_BUILDERMARIO = 5, MYSTERY_SHYGUY = 6, MYSTERY_KIRBY = 7, MYSTERY_YOSHI = 8};

    [System.Serializable]
    public class UnlockableThing{
        public UnlockID id;
        public bool isUnlocked = false;
        public Sprite previewSprite;

        public void Unlock(){
            this.isUnlocked = true;
            UnlockManager.instance.SaveFile();
        }

        public void Lock(){
            this.isUnlocked = false;
            UnlockManager.instance.SaveFile();
        }
    }

}