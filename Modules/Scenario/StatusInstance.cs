using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.S3;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Modules.Scenario
{
    public class StatusInstance
    {
        [JsonProperty(PropertyName = "case")] public Status FullStatus;
        
        public class Status
        {
            [JsonProperty(PropertyName = "checkUps")] public List<CheckUp> checkUps;

            public class CheckUp
            {
                [JsonProperty(PropertyName = "id")] public string id;
                [JsonProperty(PropertyName = "children")] public List<CheckUp> children;

                // type=0 name, type=1 details, type=2 level, type=3 pointId
                public FullCheckUp GetInfo()
                {
                    var allCheckUps = BookDatabase.Instance.allCheckUps;
                    return allCheckUps.Select(GetInfoHelper)
                        .FirstOrDefault(val => val != null);
                }

                public PhysicalPoint GetPointInfo()
                {
                    var pointId = GetInfo().pointId;
                    if (pointId == null) return null;
                    BookDatabase.Instance.physicalPointById.TryGetValue(pointId, out var physicalPoint);
                    return physicalPoint;
                }

                private FullCheckUp GetInfoHelper(FullCheckUp fullCheckUp)
                {
                    if (fullCheckUp == null)
                        return null;
                    return fullCheckUp.id == id ? fullCheckUp : 
                        fullCheckUp.children.Select(GetInfoHelper)
                            .FirstOrDefault(val => val != null);
                }

                public IEnumerator GetMedia(Action<Dictionary<string, Object>> callback)
                {
                    var folder = $"{id.PadLeft(12, '0')}/media/";
                    var files = GetInfo().files;
                    var localFiles = new List<string>();

                    if(Directory.Exists($"{DirectoryPath.CheckUps}{folder}"))
                    {
                        localFiles = Directory.GetFiles($"{DirectoryPath.CheckUps}{folder}").Select(Path.GetFileName).ToList();
                        var fileNames = files.Select(x => x.name).ToList();

                        foreach (var localFile in localFiles)
                        {
                            if(fileNames.Contains(localFile)) continue;
                            FileHandler.DeleteFile($"{DirectoryPath.CheckUps}{folder}{localFile}");
                        }
                    }
                    
                    var checkupMedia = BookDatabase.Instance.URLInfo.S3BookPaths["CheckupMedia"];
                    var split = checkupMedia.Split('/').ToList();
                    var bucket = split[0];
                    split.RemoveAt(0);
                    var path = string.Join("/", split);
                    
                    foreach (var file in files)
                    {
                        if(localFiles.Contains(file.name)) continue;
                        var filePath = $"{folder}{file.name}";
                        yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                            AmazonS3.Instance.DownloadFile(bucket,
                            $"{path}/{filePath}", $"{DirectoryPath.CheckUps}{filePath}"));
                    }
                    
                    var outObjects = new Dictionary<string, Object>();
                    foreach (var file in files)
                    {
                        var filePath = $"{DirectoryPath.CheckUps}{folder}{file.name}";
                        if(!File.Exists(filePath)) continue;
                        
                        if (file.mime.Contains("audio"))
                        {
                            ExtensionCoroutine.Instance.StartExtendedCoroutineNoWait(WebRequestHandler.Instance.AudioRequest(filePath, audioClip =>
                            {
                                outObjects.Add(file.name, audioClip);
                            }));
                            
                        } else if (file.mime.Contains("image"))
                        {
                            yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                                WebRequestHandler.Instance.TextureRequest(filePath, texture => {outObjects.Add(file.name, texture);}));
                        }
                    }
                    yield return new WaitUntil(() => outObjects.Count == files.Count);
                    callback?.Invoke(outObjects);
                }
            }
        }
    }
}
