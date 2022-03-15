using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.WDCore
{
    public static class FileHandler
    {
        public static IEnumerator ReadTextFile(string path, Action<string> onResponse)
        {
            path = "file://" + path;
            using var webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            onResponse?.Invoke(webRequest.result == UnityWebRequest.Result.Success
                ? webRequest.downloadHandler.text
                : null);
        }
        
        public static IEnumerator ReadBytesFile(string path, Action<byte[]> onResponse)
        {
            path = "file://" + path;
            using var webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            onResponse?.Invoke(webRequest.result == UnityWebRequest.Result.Success
                ? webRequest.downloadHandler.data
                : null);
        }

        public static void WriteTextFile(string path, string data)
        {
            using var streamWriter = File.CreateText(path);
            streamWriter.Write(data);
        }
        
        public static void ReadBytesFile(string path, out byte[] response, Action onFileNotFound = null)
        {
            if (!File.Exists(path))
            {
                onFileNotFound?.Invoke();
                response = null;
                return;
            }
            
            response = File.ReadAllBytes(path);
        }

        public static void WriteBytesFile(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public static void DeleteFile(string path, Action onFileNotFound = null)
        {
            if (!File.Exists(path))
            {
                onFileNotFound?.Invoke();
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                Debug.Log("Cannot delete a file at path: " + path);
            }
            
        }

        public static string FromBytesToString(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}