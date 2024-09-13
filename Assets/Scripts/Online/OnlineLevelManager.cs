using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMM.FTP;
using System.IO;

public class OnlineLevelManager : MonoBehaviour{
    
    public void UploadLevel(string path){
        //FTPManager.LL();
        string folderName = string.Empty;

        string name = path;
        name = name.Replace(GameManager.LEVEL_PATH, "");
        name = name.Replace(".umm", "");

        folderName = name;
        folderName = name + "_" + GameManager.instance.buildData.USERNAME;

        string[] line = GameManager.GetFileIn(path).Split('\n')[0].Split(':');
        folderName = folderName + "_" + line[0];
        string[] line2 = GameManager.GetFileIn(path).Split('\n')[1].Split(':');
        folderName = folderName + "_" + line2[0] + "_";

        string[] line3 = GameManager.GetFileIn(path).Split('\n')[2].Split(':');
        string tags = string.Empty;
        foreach (string arg in line3){
            if (string.IsNullOrEmpty(arg))
                continue;
            int i = -1;
            i = GameManager.StringToInt(arg);
            if (i != -1)
                tags = tags + i + "_";
        }

        folderName = folderName + tags;

        folderName = folderName.Remove(folderName.Length - 1);

        Debug.Log(folderName);

        List<string> allLevels = FTPManager.GetFtpDirectoryContents("htdocs/Levels/");
        foreach(string level in allLevels){
            string[] args = level.Split('_');
            if(args[0].Equals(name, System.StringComparison.OrdinalIgnoreCase) && args[1].Equals(GameManager.instance.buildData.USERNAME, System.StringComparison.OrdinalIgnoreCase)){
                Debug.LogError("Level already uploaded!");
                return;
            }
        }

        FTPManager.CreateDir("htdocs/Levels/" + folderName);
        FTPManager.Upload(path, "htdocs/Levels/" + folderName + "/" + "level.umm");
        FTPManager.CreateDir("htdocs/Levels/" + folderName + "/Comments");
    }

    public void PlayLevel(string levelName, string userName){
        List<string> allLevels = FTPManager.GetFtpDirectoryContents("htdocs/Levels/");
        foreach (string level in allLevels){
            string[] args = level.Split('_');
            if (args[0].Equals(levelName, System.StringComparison.OrdinalIgnoreCase) && args[1].Equals(userName, System.StringComparison.OrdinalIgnoreCase)){
                if (File.Exists(Application.persistentDataPath + "\\onlineTempLevel.lvl"))
                    File.Delete(Application.persistentDataPath + "\\onlineTempLevel.lvl");

                Debug.Log("htdocs/Levels/" + level + "/level.umm");
                Debug.Log(FTPManager.CheckIfFileExists("htdocs/Levels/" + level + "/level.umm"));
                GameManager.SaveFile(Application.persistentDataPath + "\\onlineTempLevel.lvl", FTPManager.ReadFile("htdocs/Levels/" + level + "/level.umm"));
                Debug.Log("Tt");
                GameManager.instance.sceneManager.StartOnlyPlayModeLevel(Application.persistentDataPath + "\\onlineTempLevel.lvl", true, args[0]);
                return;
            }
        }
    }

}
