using System;
using System.Collections;
using System.Globalization;
using System.IO;
using Amazon.S3;
using Modules.WDCore;
using Modules.S3;
using UnityEngine;

namespace Modules.Books
{
    public class BookLoaderS3
    {
        private string _s3Path;
        private string _filename;
        private string _localPath;
        private string _bucketName;
        private bool _isToEncrypt;
        private AmazonS3Client _client;
        private event Action<string> OnFinishedText;

        public bool IsDone;
        
        public BookLoaderS3(string bucketName, string s3Path, string outFolder, Action<string> onFinishedText = null, bool isToEncrypt = false, bool isLangSplit = false, AmazonS3Client client = null)
        {
            if (isLangSplit)
                s3Path += Language.Code + ".json";
            
            _isToEncrypt = isToEncrypt;
            _s3Path = s3Path;
            _filename = Path.GetFileName(s3Path);
            _localPath = outFolder + _filename;
            _bucketName = bucketName;
            _client = client;
            IsDone = false;
            OnFinishedText = onFinishedText;
            DirectoryPath.CheckDirectory(outFolder);

            ExtensionCoroutine.Instance.StartExtendedCoroutineNoWait(
                Application.internetReachability != NetworkReachability.NotReachable ? CheckChange() : ReadLocalJson());
        }

        private IEnumerator CheckChange()
        {
            string serverDate = null;
            var localDate = PlayerPrefs.GetString(_s3Path);
            
            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                AmazonS3.Instance.GetLastModifiedDate(_bucketName, _s3Path, 
                val => serverDate = val.ToString(CultureInfo.InvariantCulture), _client));
            
            Debug.Log($"Database update: {_bucketName}/{_s3Path}, Server date: {serverDate}");
            
            if (localDate == serverDate && File.Exists(_localPath) && serverDate != null)
                yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(ReadLocalJson());
            else
                yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(DownloadServerJson(serverDate));
        }

        private IEnumerator DownloadServerJson(string serverDate)
        {
            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(_isToEncrypt ? 
                AmazonS3.Instance.DownloadFileEncryption(_bucketName, _s3Path, _localPath, _client):
                AmazonS3.Instance.DownloadFile(_bucketName, _s3Path, _localPath, _client));

            PlayerPrefs.SetString(_s3Path, serverDate);
            PlayerPrefs.Save();
            Debug.Log($"Database update: {_bucketName}/{_s3Path} - Download end");
            
            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(ReadLocalJson());
        }

        private IEnumerator ReadLocalJson()
        {
            if (OnFinishedText == null)
            {
                IsDone = true;
                yield break;
            }
            
            var response = "";
            byte[] byteResponse = null;
            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(_isToEncrypt ? 
                FileHandler.ReadBytesFile(_localPath, val => byteResponse = val) :
                FileHandler.ReadTextFile(_localPath, val => response = val));

            if (_isToEncrypt)
                response = Decipher.DecryptStringFromBytes_Des(byteResponse);
                    
            OnFinishedText?.Invoke(response);
            Debug.Log($"Database update: {_bucketName}/{_s3Path} - Reading end");
            IsDone = true;
            OnFinishedText = null;
        }
    }
}
