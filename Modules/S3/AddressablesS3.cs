using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Modules.S3
{
    public class AddressablesS3 : MonoBehaviour
    {
        private class Client
        {
            public AmazonS3Client client;
            public string serviceUrl;
        }

        public class WorldInfo
        {
            public string id;
            public string accessKey;
            public string secretKey;
            public string serverUrl;
            public string catalogPath;
        }

        private string _currentClientId;
        private Dictionary<string, Client> _clients = new();
        private const float SignedUrlExp = 0.5f;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            Addressables.ResourceManager.InternalIdTransformFunc =
                MyTransformAddressableIdFunc;
        }

        public IEnumerator Init(string id, string accessKey, string secretKey, string serviceUrl, List<string> catalogLocations)
        {
            _currentClientId = id;
            
            if (!_clients.ContainsKey(id))
            {
                _clients.Add(id, new Client
                {
                    serviceUrl = serviceUrl,
                    client = new AmazonS3Client(accessKey, secretKey,
                        new AmazonS3Config {ServiceURL = "https://" + serviceUrl})
                });
            }

            yield return StartCoroutine(GetLatestCatalog(catalogLocations));
        }

        private IEnumerator CatalogLoader(string url)
        {
            Debug.Log("Reading Addressable catalogs: " + url);
            var handle = Addressables.LoadContentCatalogAsync(url);
            yield return handle;
            Addressables.Release(handle);
        }

        private string MyTransformAddressableIdFunc(IResourceLocation location)
        {
            if (location.InternalId.StartsWith("http") &&
                !location.InternalId.Contains("localhost"))
            {
                return GetSignedUrl(location.InternalId);
            }

            return location.InternalId;
        }
        
        public string GetSignedUrlVideo(string bucketName, string objectKey, float duration)
        {
            var request1 = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(duration)
            };

            var signedUrl = _clients[_currentClientId].client.GetPreSignedURL(request1);

            return signedUrl;
        }

        private string GetSignedUrl(string publicUrl)
        {
            var unsigned = publicUrl;
            if (unsigned.Contains(".json%"))
                unsigned = publicUrl.Split('%')[0];
            else if (unsigned.Contains(".json?"))
                unsigned = publicUrl.Split('?')[0];

            var pattern = "https://(.*)." + _clients[_currentClientId].serviceUrl + "/(.*)";
            var match = new Regex(@pattern).Match(unsigned);
            var bucketName = match.Groups[1].Value;
            var objectKey = match.Groups[2].Value;

            var request1 = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.UtcNow.AddHours(SignedUrlExp)
            };

            var signedUrl = _clients[_currentClientId].client.GetPreSignedURL(request1);

            return signedUrl;
        }

        private IEnumerator GetLatestCatalog(List<string> catalogLocations)
        {
            var objects = new List<(string, S3Object)>();
            foreach (var task in catalogLocations.Select(ListingObjectsAsync))
            {
                yield return new WaitUntil(() => task.IsCompleted);
                objects.AddRange(task.Result);
            }

            var catalogs = new Dictionary<string, List<(string, S3Object)>>();
            foreach (var s3Object in objects.Where(s3Object => s3Object.Item2.Key.Contains(".json")))
            {
                var catalogPath = s3Object.Item2.Key;
                if (catalogPath.Contains("_gsdata_")) continue;

                var split = catalogPath.Split('/').ToList();
                var platform = split[split.Count - 2];
                split.RemoveAt(split.Count-1);
                split.RemoveAt(split.Count-1);
                var type = string.Join("/", split);
                var currentPlatform = GetPlatform();

                if (platform != currentPlatform) continue;
                if (!catalogs.ContainsKey(type))
                    catalogs.Add(type, new List<(string, S3Object)>());
                catalogs[type].Add(s3Object);
            }

            foreach (var (bucket, s3Object) in catalogs.Select(catalog =>
                catalog.Value.OrderByDescending(x => x.Item2.LastModified).ToList()[0]))
                yield return StartCoroutine(CatalogLoader($"https://{bucket}.{_clients[_currentClientId].serviceUrl}/{s3Object.Key}"));
        }

        private static string GetPlatform()
        {
            return Application.platform switch
            {
                RuntimePlatform.Android => "Android",
                RuntimePlatform.WindowsEditor => "StandaloneWindows64",
                RuntimePlatform.WindowsPlayer => "StandaloneWindows64",
                RuntimePlatform.WindowsServer => "StandaloneWindows64",
                RuntimePlatform.OSXPlayer => "StandaloneOSX",
                RuntimePlatform.OSXEditor => "StandaloneOSX",
                RuntimePlatform.IPhonePlayer => "iOS",
                RuntimePlatform.LinuxEditor => "StandaloneLinux64",
                RuntimePlatform.LinuxPlayer => "StandaloneLinux64",
                RuntimePlatform.LinuxServer => "StandaloneLinux64",
                _ => null
            };
        }

        private async Task<List<(string, S3Object)>> ListingObjectsAsync(string catalogLocation)
        {
            var objects = new List<(string, S3Object)>();
            var split = catalogLocation.Split('/').ToList();
            var bucket = split[0];
            split.RemoveAt(0);
            var prefix = string.Join("/", split);
            var request = new ListObjectsV2Request {BucketName = bucket, Prefix = prefix};
            ListObjectsV2Response response;
            
            do
            {
                response = await _clients[_currentClientId].client.ListObjectsV2Async(request);
                objects.AddRange(response.S3Objects.Select(s3Object => (bucket, s3Object)));
                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);
            return objects;
        }
        
        private IEnumerator UpdateCatalogs()
        {
            var catalogsToUpdate = new List<string>();
            var checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
            checkForUpdateHandle.Completed += op =>
            {
                if(op.Status == AsyncOperationStatus.Succeeded)
                    catalogsToUpdate.AddRange(op.Result);
            };
            yield return checkForUpdateHandle;
            if (catalogsToUpdate.Count <= 0) yield break;
            var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);
            yield return updateHandle;
        }

        public IEnumerator DownloadAllBundles()
        {
            Debug.Log("DownloadAllBundles");
            var loading = GameManager.Instance.loadingController;
            loading.Init(TextData.Get(139));
            yield return StartCoroutine(UpdateCatalogs());
            
            var loc = Addressables.ResourceLocators;
            var keys = new Dictionary<string, long>();
            foreach (var locator in loc)
            {
                foreach (var ll in locator.Keys)
                {
                    locator.Locate(ll, null, out var resLoc);
                    foreach (var res in resLoc)
                    {
                        if(res.PrimaryKey.Contains(".bundle") || res.PrimaryKey.Contains("Assets")) continue;
                        var key = res.PrimaryKey.Split('/');
                        if(!keys.ContainsKey(key[0]))
                            keys.Add(key[0], 0);
                    }
                }
            }
            
            long totalSize = 0;
            var updKeys = new Dictionary<string, long>();
            foreach (var key in keys)
            {
                var handle = Addressables.LoadResourceLocationsAsync(key.Key);
                yield return handle;
                var count = handle.Result.Count;
                Addressables.Release(handle);
                if(count == 0) continue;
                var getDownloadSize = Addressables.GetDownloadSizeAsync(key.Key);
                yield return getDownloadSize;
                updKeys.Add(key.Key, getDownloadSize.Result);
                totalSize += getDownloadSize.Result;
                Addressables.Release(getDownloadSize);
            }
            
            loading.Init(TextData.Get(8));

            var downloadedSize = 0.0f;
            foreach (var key in updKeys)
            {
                if(key.Value == 0) continue;
                var handle = Addressables.DownloadDependenciesAsync(key.Key);
                while (!handle.IsDone)
                {
                    var percent = (downloadedSize + handle.PercentComplete * key.Value) / totalSize;
                    loading.SetProgress(percent, totalSize);
                    yield return null;
                }

                downloadedSize += key.Value;
                Addressables.Release(handle);
            }
            loading.Hide();
        }

        public IEnumerator DownloadAllCases()
        {
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            
            var loading = GameManager.Instance.loadingController;
            loading.Init(TextData.Get(139), true);
            var objectsCount = 0;

            var patientPath = "AS";
            var patientsId = BookDatabase.Instance.MedicalBook.scenarios.Select(x => x.patientId).Distinct().ToList();
            var statusesId = BookDatabase.Instance.MedicalBook.scenarios.Select(x => x.statusId).Distinct().ToList();
            var checkTablesId = BookDatabase.Instance.MedicalBook.scenarios.Select(x => x.checkTableId).Distinct().ToList();
            var cabinetsId = BookDatabase.Instance.MedicalBook.scenarios.Select(x => x.cabinetId).Distinct().ToList();
            var categories = new List<IEnumerable<string>>() { patientsId, cabinetsId };

            loading.Init(TextData.Get(8), true);

            var checkedPatientsId = new List<string>();
            var checkedCabinetsId = new List<string>();

            var checkedCategories = new List<List<string>>() { checkedPatientsId, checkedCabinetsId };

            foreach (var category in categories)
            {
                foreach (var item in category)
                {
                    var key = "";
                    var categoryIndex = -1;

                    switch (categories.IndexOf(category))
                    {
                        case 0:
                            key = patientPath + item;
                            categoryIndex = 0;
                            break;
                        case 1:
                            key = item;
                            categoryIndex = 1;
                            break;
                    }

                    if (String.IsNullOrEmpty(key) || categoryIndex == -1) continue;

                    var check = Addressables.LoadResourceLocationsAsync(key);
                    yield return check;
                    var count = check.Result.Count;
                    Addressables.Release(check);
                    if (count > 0)
                        checkedCategories[categoryIndex].Add(key);
                    else
                    {
                        Debug.LogWarning("No item with ID " + key + " in addressables");
                    }
                }
            }

            var totalObjectsCount = checkedPatientsId.Count;

            foreach (var category in checkedCategories)
            {
                switch (checkedCategories.IndexOf(category))
                {
                    case 0:
                        loading.Init(TextData.Get(267), true);
                        break;
                    case 1:
                        loading.Init(TextData.Get(268), true);
                        break;
                }

                foreach (var item in category)
                {
                    if (ct.IsCancellationRequested)
                    {
                        yield break;
                    }

                    var handle = Addressables.DownloadDependenciesAsync(item);

                    while (!handle.IsDone)
                    {
                        loading.SetProgressCount(objectsCount, totalObjectsCount);
                        yield return null;
                    }
                    objectsCount++;
                    yield return handle;

                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {item})");
                        loading.Hide();
                        StopAllCoroutines();
                        yield break;
                    }
                }
            }

            loading.Hide();
            yield return new WaitForEndOfFrame();
            GameManager.Instance.mainMenuController.ShowMenu(true);
        }

        public IEnumerator CancelDownload()
        {
            _cts.Cancel();
            GameManager.Instance.loadingController.Hide();
            yield return new WaitForEndOfFrame();
            GameManager.Instance.mainMenuController.ShowMenu(true);
            yield return null;
            StopAllCoroutines();
        }

        public IEnumerator LoadAdditionalWorld(string id)
        {
            Debug.Log("LoadAdditionalCatalog: " + id);
            WorldInfo info = null;
            yield return StartCoroutine(GetWorldInfo(id, s => info = s));
            
            if(info == null) yield break;
            
            yield return StartCoroutine(Init(info.id, info.accessKey, info.secretKey, info.serverUrl,
                new List<string> {info.catalogPath}));

        }

        public IEnumerator GetWorldInfo(string id, Action<WorldInfo> worldInfo)
        {
            var urlInfo = BookDatabase.Instance.URLInfo;
            var form = new Dictionary<string, string> {{"pass", urlInfo.WorldAccess.Password},{"id", id}};
            var www = UnityWebRequest.Post(urlInfo.WorldAccess.WebServiceUrl, form);
            www.timeout = 10;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                yield return new WaitForSeconds(5.0f);
                yield return StartCoroutine(LoadAdditionalWorld(id));
            }
            else
            {
                var response = www.downloadHandler.text;
                var parsed = JObject.Parse(response);
                var status = parsed["status"]?.ToString();

                if (status == "SUCCESS")
                {
                    var info = JsonConvert.DeserializeObject<WorldInfo>(parsed["info"]?.ToString() ?? string.Empty);
                    worldInfo?.Invoke(info);
                }
            }
        }

        public void RestoreDefaultWorld()
        {
            _currentClientId = "default";
        }
    }
}
