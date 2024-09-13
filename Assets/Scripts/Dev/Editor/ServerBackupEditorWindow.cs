using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UMM.FTP;
using System.Net;

[ExecuteAlways]
public class ServerBackupEditorWindow : EditorWindow{

    private string backupPath;

    private void Awake(){
        this.backupPath = @"D:\Unity_MyGamesOnD\!builds\UMMBuilds\NewProject\ServerBackups\Backup\";
    }

    [MenuItem("UMM/ServerBackup")]
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(ServerBackupEditorWindow));
    }

    private void OnGUI(){
        if (Application.isPlaying){
            GUILayout.Label("Please enter EditMode!");
            return;
        }

        GUILayout.Label("FTP Server Backup\n");

        this.backupPath = EditorGUILayout.TextField("Path", this.backupPath);

        if (GUILayout.Button("Backup")){
            System.IO.DirectoryInfo di = new DirectoryInfo(this.backupPath);

            foreach (FileInfo file in di.GetFiles()){
                file.Delete();
            }foreach (DirectoryInfo dir in di.GetDirectories()){
                dir.Delete(true);
            }

          //  FTPManager.DownloadFtpDirectory(FTPManager.hostUrl + "/htdocs/", new NetworkCredential(FTPManager.hostName, FTPManager.hostPassword), this.backupPath);
        }

        if (GUILayout.Button("GetServerVersion")){
            string ver = FTPManager.ReadFile("htdocs/currentVersion.txt");
            Debug.Log("ServerVersion: " + ver);
        }
    }
}
