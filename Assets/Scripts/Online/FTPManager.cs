using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace UMM.FTP{

    public class FTPManager{

        public static string ReadFile(string fileName){
            WebClient request = new WebClient();
            string url = BuildData.hU + fileName;
            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);

            byte[] newFileData = request.DownloadData(url);
            string fileString = System.Text.Encoding.UTF8.GetString(newFileData);
            return fileString;
        }

        public static bool CreateDir(string dirName){
            var request = (FtpWebRequest)WebRequest.Create(BuildData.hU + dirName);
            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;

            try
            {
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
            }

            return false;
        }

        public static bool CheckIfFileExists(string fileName){
            var request = (FtpWebRequest)WebRequest.Create(BuildData.hU + fileName);
            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            try{
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                return true;
            }catch (WebException ex){
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
            }

            return false;
        }

        public static void Upload(string filename, string initialPath){
            var file = new FileInfo(filename);
            var address = new Uri(BuildData.hU + initialPath);
            var request = FtpWebRequest.Create(address) as FtpWebRequest;

            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);
            request.KeepAlive = false;
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.ContentLength = file.Length;

            // Set buffer size to 2KB.
            var bufferLength = 2048;
            var buffer = new byte[bufferLength];
            var contentLength = 0;

            var fs = file.OpenRead();

                // Stream to which file to be uploaded is written.
                var stream = request.GetRequestStream();

                // Read from file stream 2KB at a time.
                contentLength = fs.Read(buffer, 0, bufferLength);

                // Loop until stream content ends.
                while (contentLength != 0){
                    stream.Write(buffer, 0, contentLength);
                    contentLength = fs.Read(buffer, 0, bufferLength);
                }

                // Close file and request streams
                stream.Close();
                fs.Close();
            
        }

        public static void LL(){
            BuildData.hN = "if0_37256819";
            BuildData.hP = "E3jq1MQE6y8";
            BuildData.hU = "ftp://ftpupload.net:21/";

        }

        public static void BB(){
            
        }

        public static FileStream DownloadWithFTP(string fileName, string savePath = ""){
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(BuildData.hU + fileName));
            //request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = true;
  
            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);
  
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            //If savePath is NOT null, we want to save the file to path
            //If path is null, we just want to return the file as array
            if (!string.IsNullOrEmpty(savePath)){


                if (File.Exists(savePath))
                    File.Delete(savePath);
                Stream reader = request.GetResponse().GetResponseStream();
                //Create Directory if it does not exist
                if (!Directory.Exists(Path.GetDirectoryName(savePath))){
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                }

                FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate);

                int bytesRead = 0;
                byte[] buffer = new byte[2048];
     
                while (true){
                    bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    fileStream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
                return fileStream;
            }
            else {
                return null;
            }
        }

        public static List<string> GetFtpDirectoryContents(string path){
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(BuildData.hU + path);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            request.Credentials = new NetworkCredential(BuildData.hN, BuildData.hP);

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);

            List<string> fileNames = new List<string>();
            fileNames = reader.ReadToEnd().Split('\n').ToList();

            List<string> deleteNames = new List<string>();
            for (int i = 0; i < fileNames.Count; i++){
                if (fileNames[i].StartsWith(".") | fileNames[i].StartsWith("..") | fileNames[i].StartsWith("...") | string.IsNullOrEmpty(fileNames[i])){
                    deleteNames.Add(fileNames[i]);
                }
            }

            for (int i = 0; i < deleteNames.Count; i++){
                fileNames.Remove(deleteNames[i]);
            }

            reader.Close();
            response.Close();

            deleteNames.Clear();
            return fileNames;
        }

        public static void DownloadFtpDirectory(string url, NetworkCredential credentials, string localPath){
            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream)){
                while (!listReader.EndOfStream){
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines){
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                string localFilePath = Path.Combine(localPath, name);
                string fileUrl = url + name;

                if (permissions[0] == 'd'){
                    if (!Directory.Exists(localFilePath)){
                        Directory.CreateDirectory(localFilePath);
                    }

                    DownloadFtpDirectory(fileUrl + "/", credentials, localFilePath);
                }else{
                    FtpWebRequest downloadRequest =
                        (FtpWebRequest)WebRequest.Create(fileUrl);
                    downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    downloadRequest.Credentials = credentials;

                    using (FtpWebResponse downloadResponse =
                              (FtpWebResponse)downloadRequest.GetResponse())
                    using (Stream sourceStream = downloadResponse.GetResponseStream())
                    using (Stream targetStream = File.Create(localFilePath)){
                        byte[] buffer = new byte[10240];
                        int read;
                        while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0){
                            targetStream.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }
    }

}
