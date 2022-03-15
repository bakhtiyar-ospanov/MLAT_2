using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modules.WDCore;
using PlayFab;
using PlayFab.Internal;
using UnityEngine;

namespace Modules.Playfab
{
    public class PlayFabFileController : MonoBehaviour
    {
        private string entityId;
        private string entityType;
        private string uploadFileName;
        private string uploadFilePath;
        private Action<DateTime> onFileUploadSuccess;
        private bool _isEncrypted;

        private void Awake()
        {
            GameManager.Instance.playFabLoginController.OnUserLogin += b =>
            {
                entityId = GameManager.Instance.playFabLoginController.entityId;
                entityType = GameManager.Instance.playFabLoginController.entityType;
            };
        }

        public void GetFileMeta(string filename, Action<DateTime, string> response)
        {
            var request = new PlayFab.DataModels.GetFilesRequest { Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType } };
            PlayFabDataAPI.GetFiles(request, result => OnGetFileMeta(result, filename, response), Debug.Log);
        }
        
        private void OnGetFileMeta(PlayFab.DataModels.GetFilesResponse result, string filename, Action<DateTime, string> response)
        {
            var requiredFile = result.Metadata.FirstOrDefault(x => x.Key == filename);
            if(requiredFile.Key != null)
                response?.Invoke(requiredFile.Value.LastModified, requiredFile.Value.DownloadUrl);
            else
                response?.Invoke(DateTime.Now, null);
        }
        
        public void UploadFile(string filename, string filepath, Action<DateTime> onFileUpload, bool isEncrypted)
        {
            uploadFileName = filename;
            uploadFilePath = filepath;
            onFileUploadSuccess = onFileUpload;
            _isEncrypted = isEncrypted;

            var request = new PlayFab.DataModels.InitiateFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType },
                FileNames = new List<string> { uploadFileName },
            };
            PlayFabDataAPI.InitiateFileUploads(request, OnInitFileUpload, Debug.Log);
        }
        
        private void OnInitFileUpload(PlayFab.DataModels.InitiateFileUploadsResponse response)
        {
            FileHandler.ReadBytesFile(uploadFilePath, out var payload);

            if (_isEncrypted)
            {
                var decryptedStr = Decipher.DecryptStringFromBytes_Des(payload);
                payload = Encoding.ASCII.GetBytes(decryptedStr);
            }
            
            PlayFabHttp.SimplePutCall(response.UploadDetails[0].UploadUrl, payload, FinalizeUpload, Debug.Log);
        }

        private void FinalizeUpload(byte[] data)
        {
            var request = new PlayFab.DataModels.FinalizeFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType },
                FileNames = new List<string> { uploadFileName },
            };
            PlayFabDataAPI.FinalizeFileUploads(request, OnUploadSuccess, Debug.Log);
        }
        private void OnUploadSuccess(PlayFab.DataModels.FinalizeFileUploadsResponse result)
        {
            Debug.Log("File upload success: " + uploadFileName);
            
            var requiredFile = result.Metadata.FirstOrDefault(
                x => x.Key == uploadFileName);

            if(requiredFile.Key == null) return;
            
            onFileUploadSuccess?.Invoke(requiredFile.Value.LastModified);
            onFileUploadSuccess = null;
            StartCoroutine(GameManager.Instance.playFabLoginController.SyncPortal(false));
        }
        
        public void GetActualFile(string downloadUrl, string filepath, bool isEncrypted, Action onFileDownload = null)
        {
            _isEncrypted = isEncrypted;
            PlayFabHttp.SimpleGetCall(downloadUrl, file => OnFileAcquired(file, filepath, onFileDownload), Debug.Log);
        }

        private void OnFileAcquired(byte[] result, string filepath, Action onFileDownload)
        {
            if (_isEncrypted)
            {
                var str = Encoding.UTF8.GetString(result);
                var encrypted = Decipher.EncryptStringToBytes_Des(str);
                FileHandler.WriteBytesFile(filepath, encrypted); 
            }
            else
            {
                FileHandler.WriteBytesFile(filepath, result); 
            }
            
            onFileDownload?.Invoke();
        }
    }
}