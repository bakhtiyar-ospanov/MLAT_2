using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Books;
using Modules.S3;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Scenario
{
    public class PreviewDownloader : MonoBehaviour
    {
        public class PreviewInfo
        {
            public Dictionary<string, DateTime> localDateById;
            public Dictionary<string, DateTime> serverDateById;
            public string pathOut;
            public string infoPath;
            public string previewPath;
        }

        private Dictionary<string, PreviewInfo> _infos = new();

        public void InitAcademix()
        {
            BookDatabase.Instance.URLInfo.S3BookPaths.TryGetValue("ScenarioPreview", out var scenarioPreview);

            var info = new PreviewInfo
            {
                pathOut = DirectoryPath.Previews,
                infoPath = Path.Combine(DirectoryPath.Previews, "Info.json"),
                serverDateById = new Dictionary<string, DateTime>(),
                previewPath = scenarioPreview
            };

            StartCoroutine(Init("academix", info));
        }


        public IEnumerator Init(string uniqueId, PreviewInfo info)
        {
            if(_infos.ContainsKey(uniqueId)) yield break;
            
            _infos.Add(uniqueId, info);
            yield return StartCoroutine(AmazonS3.Instance.GetLastModifiedDates(info.previewPath,
                val => info.serverDateById = info.serverDateById.Union(val).
                    ToDictionary(t => t.Key, t => t.Value)));
            

            if (File.Exists(info.infoPath))
            {
                yield return StartCoroutine(FileHandler.ReadTextFile(info.infoPath,
                    val => info.localDateById = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(val)));
            }
            else
            {
                info.localDateById = new Dictionary<string, DateTime>();
            }
        }
        public IEnumerator DownloadPreview(string uniqueId, string id, Action<Texture2D> callback)
        {
            _infos.TryGetValue(uniqueId, out var info);
            
            if(info == null) yield break;
            
            yield return new WaitUntil(() => info.localDateById != null && info.serverDateById != null);
            
            var fileName = id + ".jpg";
            var pathOut = Path.Combine(info.pathOut, fileName);
            var split = info.previewPath.Split('/').ToList();
            var bucket = split[0];
            split.RemoveAt(0);
            var awsPath = string.Join("/", split);

            awsPath += "/" + fileName;

            info.localDateById.TryGetValue(id, out var localTime);
            info.serverDateById.TryGetValue(id, out var serverTime);

            if ((serverTime != localTime || !File.Exists(pathOut)) && serverTime != default)
            {
                yield return StartCoroutine(AmazonS3.Instance.DownloadFile(bucket, awsPath, pathOut));

                if (localTime == default)
                    info.localDateById.Add(id, serverTime);
                else
                    info.localDateById[id] = serverTime;
                
                FileHandler.WriteTextFile(info.infoPath, JsonConvert.SerializeObject(info.localDateById));
            }
            
            if(File.Exists(pathOut))
                yield return StartCoroutine(WebRequestHandler.Instance.TextureRequest(pathOut, callback));
            else
                callback?.Invoke(null);
        }
    }
}
