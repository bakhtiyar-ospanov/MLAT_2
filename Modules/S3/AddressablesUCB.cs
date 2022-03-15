#if UNITY_CLOUD_BUILD || UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using UnityEngine;

public class AddressablesUCB : MonoBehaviour
{
    private static AmazonS3Client _client;
    private static TransferUtility _fileTransferUtility;

    public static void PostExport(string exportPath)
    { 
        Debug.Log("POST EXPORT STARTS !!!!!!!!!!");
        var dirPath = Path.GetDirectoryName(exportPath);
        var parentDir = Directory.GetParent(dirPath);
        var buildTarget = Environment.GetEnvironmentVariable("BUILD_TARGET");
        var lastBuildDir = Directory.GetDirectories(parentDir.FullName + "/.build/last");
        var path = lastBuildDir[0] + "/extra_data/addrs/ServerData/" + buildTarget;
        
        Debug.Log("Reading Addressable at path: " + path);
        
        PreCleaning(path);
        UploadToServer(path, buildTarget);

        Debug.Log("POST EXPORT ENDS !!!!!!!!!!");
    }

    private static void UploadToServer(string path, string buildTarget)
    {
        var bucket = Environment.GetEnvironmentVariable("S3_BUCKET");
        var s3CatalogFolder = Environment.GetEnvironmentVariable("S3_CATALOG_FOLDER");
        var s3BundleFolder = Environment.GetEnvironmentVariable("S3_BUNDLE_FOLDER");
        var remoteFolder = $"{s3BundleFolder}/{buildTarget}";
        
        UpdateClient();
        
        var objs = new List<string>();
        var uploadedFiles = new List<string>();
        var request = new ListObjectsV2Request {BucketName = bucket, Prefix = remoteFolder};
        ListObjectsV2Response response;
            
        do
        {
            response = ListObjects(request);
            objs.AddRange(response.S3Objects.Select(x => Path.GetFileName(x.Key)));
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated);
        
        var files = Directory.GetFiles(path);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            uploadedFiles.Add(fileName);

            if (objs.Contains(fileName) && fileName.EndsWith(".bundle"))
            {
                Debug.Log($"ALREADY ON SERVER: {fileName}");
                continue;
            }
            
            Debug.Log($"UPLOADING: {fileName}");
            UploadFile(file, bucket, $"{remoteFolder}/{fileName}");
        }

        var catalogs = files.Where(x => x.EndsWith(".hash") || x.EndsWith(".json")).ToList();

        foreach (var catalog in catalogs)
        {
            var fileName = Path.GetFileName(catalog);
            Debug.Log($"UPLOADING CATALOG: {fileName}");
            UploadFile(catalog, bucket, $"{s3CatalogFolder}/{buildTarget}/{fileName}");
        }
        
        foreach (var toBeDeleted in objs)
        {
            if(uploadedFiles.Contains(toBeDeleted)) continue;
            
            Debug.Log("To be deleted from server: " + toBeDeleted);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = $"{remoteFolder}/{toBeDeleted}"
            };

            DeleteFile(deleteRequest);

            if (toBeDeleted.EndsWith(".json") || toBeDeleted.EndsWith(".hash"))
            {
                deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucket,
                    Key = $"{s3CatalogFolder}/{buildTarget}/{toBeDeleted}"
                };

                DeleteFile(deleteRequest);
            }
        }
    }

    private static void UploadFile(string filePath, string bucket, string key)
    {
        try
        {
            _fileTransferUtility.Upload(filePath, bucket, key);
        }
        catch (Exception)
        {
            Debug.Log("EXCEPTION HAPPENED WHILE UPLOADING :(");
            UpdateClient();
            UploadFile(filePath, bucket, key);
        }
    }

    private static void DeleteFile(DeleteObjectRequest deleteRequest)
    {
        try
        {
            _client.DeleteObject(deleteRequest);
        }
        catch (Exception)
        {
            Debug.Log("EXCEPTION HAPPENED WHILE DELETING:(");
            UpdateClient();
            DeleteFile(deleteRequest);
        }
    }

    private static ListObjectsV2Response ListObjects(ListObjectsV2Request request)
    {
        try
        {
            return _client.ListObjectsV2(request);
        }
        catch (Exception)
        {
            Debug.Log("EXCEPTION HAPPENED WHILE LISTING :(");
            UpdateClient();
            return ListObjects(request);
        }
    }

    private static void UpdateClient()
    {
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        var serviceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL");
        
        _client = new AmazonS3Client(accessKey, secretKey,
            new AmazonS3Config {ServiceURL = "https://" + serviceUrl});
        _fileTransferUtility = new TransferUtility(_client);
    }
    

    private static void PreCleaning(string path)
    {
        try
        {
            var files = Directory.GetFiles(path);
            var modDateByFile = new Dictionary<string, FileInfo>();

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var pureName = "";
                if (fileName.EndsWith(".bundle"))
                {
                    pureName = string.Join("_", fileName.Split('_').SkipLast(1));
                }
                else if(fileName.EndsWith(".hash"))
                {
                    pureName = ".hash";
                } else if (fileName.EndsWith(".json"))
                {
                    pureName = ".json";
                }

                var fileInfo = new FileInfo(file);
            
                if(!modDateByFile.ContainsKey(pureName))
                {
                    modDateByFile.Add(pureName, fileInfo);
                }
                else
                {
                    var prevFile = modDateByFile[pureName];

                    if (prevFile.LastWriteTimeUtc.CompareTo(fileInfo.LastWriteTimeUtc) > 0)
                    {
                        File.Delete(fileInfo.FullName);
                    }
                    else
                    {
                        File.Delete(prevFile.FullName);
                        modDateByFile[pureName] = fileInfo;
                    }
                }
            }
        }
        catch (Exception)
        {
            Debug.Log("EXCEPTION HAPPENED WHILE PRE-CLEANING :(");
            PreCleaning(path);
        }
    }
}

#endif
