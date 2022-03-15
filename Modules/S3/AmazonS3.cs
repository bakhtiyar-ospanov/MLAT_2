using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Modules.WDCore;
using UnityEngine;

namespace Modules.S3
{
    public class AmazonS3 : Singleton<AmazonS3>
    {
        private AmazonS3Client _client;
        private TransferUtility _fileTransferUtility;
        private float _progress;
        private Dictionary<string, CancellationTokenSource> _cancelSources;

        public void Init(string accessKey, string secretKey, string serviceUrl)
        {
            _client = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config {ServiceURL = "https://" + serviceUrl});
            _fileTransferUtility = new TransferUtility(_client);
            _cancelSources = new Dictionary<string, CancellationTokenSource>();
        }

        public IEnumerator DownloadFile(string bucketName, string pathIn, string pathOut, AmazonS3Client client = null)
        {
            Debug.Log($"Загрузка файла: {bucketName}/{pathIn}");
            var currentTransferUtility = client != null ? new TransferUtility(client) : _fileTransferUtility;
            var cancelSource = new CancellationTokenSource();
            var downloadRequest = new TransferUtilityDownloadRequest()
            {
                Key = pathIn,
                FilePath = pathOut,
                BucketName = bucketName,
            };
        
            //downloadRequest.WriteObjectProgressEvent += OnProgressChanged;
        
            var download = currentTransferUtility.DownloadAsync(downloadRequest, cancelSource.Token);
            _cancelSources.Add(pathIn, cancelSource);
            while (!download.IsCompleted && !download.IsCanceled && !download.IsFaulted)
            { yield return null; }

            _cancelSources.Remove(pathIn);

            if (download.IsCanceled || download.IsFaulted)
            {
                FileHandler.DeleteFile(pathOut);
                download.Dispose();
                // yield return new WaitForSeconds(2.0f);
                // yield return StartCoroutine(DownloadFile(bucketName, pathIn, pathOut));
                yield break;
            }

            download.Dispose();
        }
        
        public IEnumerator DownloadFileEncryption(string bucketName, string pathIn, string pathOut, AmazonS3Client client = null)
        {
            var currentClient = client ?? _client;
            var cancelSource = new CancellationTokenSource();
            var downloadRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = pathIn,
            };

            var download = currentClient.GetObjectAsync(downloadRequest, cancelSource.Token);
            _cancelSources.Add(pathIn, cancelSource);
            while (!download.IsCompleted && !download.IsCanceled && !download.IsFaulted)
            { yield return null; }

            _cancelSources.Remove(pathIn);

            if (download.IsCanceled || download.IsFaulted)
            {
                FileHandler.DeleteFile(pathOut);
                download.Dispose();
                // yield return new WaitForSeconds(2.0f);
                // yield return StartCoroutine(DownloadFileEncryption(bucketName, pathIn, pathOut));
                yield break;
            }
            
            using (var reader = new StreamReader(download.Result.ResponseStream))
            {
                var readTask =  reader.ReadToEndAsync();
                while (!readTask.IsCompleted && !readTask.IsCanceled && !readTask.IsFaulted)
                { yield return null; }
                
                var encrypted = Decipher.EncryptStringToBytes_Des(readTask.Result);
                FileHandler.WriteBytesFile(pathOut, encrypted);
            }
            
            download.Dispose();
        }

        public IEnumerator GetLastModifiedDate(string bucketName, string pathIn, Action<DateTime> resultCallback, AmazonS3Client client = null)
        {
            var getObjectMetadataRequest = new GetObjectMetadataRequest() { BucketName = bucketName, Key = pathIn };
            var currentClient = client ?? _client;
            var meta = currentClient.GetObjectMetadataAsync(getObjectMetadataRequest);
            while (!meta.IsCompleted && !meta.IsCanceled && !meta.IsFaulted) { yield return null; }
        
            if (meta.IsCanceled || meta.IsFaulted)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(GetLastModifiedDate(bucketName, pathIn, resultCallback, client));
                yield break;
            }

            resultCallback?.Invoke(meta.Result.LastModified);
        }

        public IEnumerator GetLastModifiedDates(string pathIn, Action<Dictionary<string, DateTime>> callback, AmazonS3Client client = null)
        {
            var task = ListingObjectsAsync(pathIn);
            
            while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted) { yield return null; }
        
            if (task.IsCanceled || task.IsFaulted)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(GetLastModifiedDates(pathIn, callback));
                yield break;
            }
        
            callback?.Invoke(task.Result);
        }
        
        private async Task<Dictionary<string, DateTime>> ListingObjectsAsync(string catalogLocation)
        {
            var objects = new Dictionary<string, DateTime>();
            var split = catalogLocation.Split('/').ToList();
            var bucket = split[0];
            split.RemoveAt(0);
            var prefix = string.Join("/", split);
            var request = new ListObjectsV2Request {BucketName = bucket, Prefix = prefix};
            ListObjectsV2Response response;
            
            do
            {
                response = await _client.ListObjectsV2Async(request);
                foreach (var s3Object in response.S3Objects)
                {
                    var key = Path.GetFileNameWithoutExtension(s3Object.Key);
                    if(!string.IsNullOrEmpty(key))
                        objects.Add(key, s3Object.LastModified);
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);
            return objects;
        }
        
        public IEnumerator ListObjects(string bucket, string folder, Action<List<string>> callback)
        {
            var task = ListingObjectsAsync(bucket, folder);
            
            while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted) { yield return null; }
        
            if (task.IsCanceled || task.IsFaulted)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ListObjects(bucket, folder, callback));
                yield break;
            }
        
            callback?.Invoke(task.Result);
        }
        
        private async Task<List<string>> ListingObjectsAsync(string bucket, string folder)
        {
            var objects = new List<string>();
            var request = new ListObjectsV2Request {BucketName = bucket, Prefix = folder};
            ListObjectsV2Response response;
            
            do
            {
                response = await _client.ListObjectsV2Async(request);
                objects.AddRange(response.S3Objects.Select(x => Path.GetFileName(x.Key)));
                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);
            return objects;
        }

        public void CancelAll()
        {
            foreach (var cancelSource in _cancelSources)
                cancelSource.Value.Cancel();
        }

        public IEnumerator UploadFile(string filePath, string bucket, string key)
        {
            Debug.Log($"Upload a file to AWS: {bucket}/{key}");
            var task = _fileTransferUtility.UploadAsync(filePath, bucket, key);
            
            while (!task.IsCompleted && !task.IsCanceled && !task.IsFaulted) { yield return null; }
        
            if (task.IsCanceled || task.IsFaulted)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(UploadFile(bucket, filePath, key));
            }
        }

        public bool CheckFileExists(string bucket, string filename)
        {
            var s3FileInfo = new Amazon.S3.IO.S3FileInfo(_client, bucket, filename);
            Debug.Log($"Check cache on AWS of {filename}: " + s3FileInfo.Exists);
            return s3FileInfo.Exists;
        }

    }
}
