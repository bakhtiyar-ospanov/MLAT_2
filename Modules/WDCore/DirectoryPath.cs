using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Modules.WDCore
{
    public static class DirectoryPath
    {
#if UNITY_IOS
        [DllImport ("__Internal")]
        internal static extern bool OpenDocumentOnIOS (string path);
#endif
        private static string _root;
        private static string _streaming;
        
        public static string Speech;
        public static string Books;
        public static string Statistics;
        public static string Statuses;
        public static string CheckTables;
        public static string CheckUps;
        public static string Points;
        public static string Previews;
        public static string ExternalCourses;
        public static string Screenshots;
        public static string UpdateFile;
        public static string UpdateDir;
        public static string VitalSignMonitors;
        
        public static void CheckDirectories()
        {
            _root = Application.persistentDataPath;
            _streaming = Application.streamingAssetsPath;

            Speech = _root + "/Speech/";
            Books = _root + "/Books/";
            Statistics = _root + "/Statistics/";
            Statuses = _root + "/Statuses/";
            CheckTables = _root + "/CheckTables/";
            CheckUps = _root + "/CheckUps/";
            Points = _root + "/Points/";
            Previews = _root + "/Previews/";
            VitalSignMonitors = _root + "/VitalSignMonitors/";
            Screenshots = _root + "/Screenshots/";
            ExternalCourses = _root + "/ExternalCourses/";
            UpdateDir = Application.platform == RuntimePlatform.Android ? _root + "/VargatesUpdate/" : "C:/VargatesUpdate/";
            UpdateFile = Application.platform == RuntimePlatform.Android ? UpdateDir + "Update.apk" : UpdateDir + "Update.exe";
            
            CheckDirectory(Speech);
            CheckDirectory(Books);
            CheckDirectory(Statuses);
            CheckDirectory(CheckTables);
            CheckDirectory(CheckUps);
            CheckDirectory(Points);
            CheckDirectory(Statistics);
            CheckDirectory(ExternalCourses);
            #if !(UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX)
                CheckDirectory(UpdateDir);
            #endif
            CheckDirectory(Previews);
            CheckDirectory(Screenshots);
            CheckDirectory(VitalSignMonitors);
        }
        
        public static void OpenPDF(string path)
        {
            Debug.Log("path = " + path);
#if UNITY_ANDROID
            AndroidContentOpenerWrapper.OpenContent(path);
#elif UNITY_IOS
            OpenDocumentOnIOS(path);
            
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Application.OpenURL("file:///" + path.Replace(" ", "%20"));
#else
            Application.OpenURL("file:///" + path);
#endif
        }

        public static void CheckDirectory(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception e)
            {
                Debug.Log("error creating directory: " + dir + " \nerror: " + e);
            }
        }
        
        public static void DeleteDirectory(string dir)
        {
            try
            {
                if(Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
                Debug.Log("error deleting directory: " + dir + " \nerror: " + e);
                ExtensionCoroutine.Instance.StartExtendedCoroutine(DeleteDirectoryWithDelay(dir));
            }
        }
        
        public static IEnumerator DeleteDirectoryWithDelay(string dir)
        {
            yield return new WaitForSeconds(0.5f);
            try
            {
                if(Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
                Debug.Log("error deleting directory: " + dir + " \nerror: " + e);
                ExtensionCoroutine.Instance.StartExtendedCoroutine(DeleteDirectoryWithDelay(dir));
            }
        }
        
    }
}
