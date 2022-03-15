using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.WDCore;
using Modules.S3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.Books
{
    public class BookDatabase : Singleton<BookDatabase>
    {
        public UrlInfo URLInfo;
        public Dictionary<string, string> Configurations;
        public MedicalBase MedicalBook;
        public VargatesBase VargatesBook;
        public Dictionary<string, FullCheckUp> fullCheckUpById;
        public Dictionary<string, PhysicalPoint> physicalPointById;
        public List<FullCheckUp> allCheckUps;
        public List<WDPhysicalPoint> allPhysicalPoints;
        public bool isDone;

        public void Init()
        {
            URLInfo = JsonConvert.DeserializeObject<UrlInfo>(Configurations["PathConfig"]);
            URLInfo.InitVersion();
            
            AmazonS3.Instance.Init(URLInfo.AccessKeyS3, URLInfo.SecretKeyS3, URLInfo.ServiceUrlS3);
        }

        public IEnumerator BookLoading()
        {
            isDone = false;

            var bookLoaders = new List<BookLoaderS3>();
            ATCCS atccs = null;
            ICD10 icd10 = null;
            ICD11 icd11 = null;
            
            foreach (var s3Path in URLInfo.S3BookPaths)
            {
                var split = s3Path.Value.Split('/').ToList();
                var bucket = split[0];
                split.RemoveAt(0);
                var path = string.Join("/", split);
                
                switch (s3Path.Key)
                {
                    case "AcademixBase":
                        bookLoaders.Add(new BookLoaderS3(bucket, path, $"{DirectoryPath.Books}/AcademixBase/",
                            s => MedicalBook = JsonConvert.DeserializeObject<MedicalBase>(s), true, true));
                        break;
                    case "ATCCS":
                        bookLoaders.Add(new BookLoaderS3(bucket, path, $"{DirectoryPath.Books}/ATCCS/",
                            s => atccs = JsonConvert.DeserializeObject<ATCCS>(s), false, true));
                        break;
                    case "ICD-10":
                        bookLoaders.Add(new BookLoaderS3(bucket, path, $"{DirectoryPath.Books}/ICD-10/",
                            s => icd10 = JsonConvert.DeserializeObject<ICD10>(s), false, true));
                        break;
                    case "ICD-11":
                        bookLoaders.Add(new BookLoaderS3(bucket, path, $"{DirectoryPath.Books}/ICD-11/",
                            s => icd11 = JsonConvert.DeserializeObject<ICD11>(s), false, true));
                        break;
                }
            }
            yield return new WaitUntil(() => bookLoaders.All(bookLoader => bookLoader.IsDone));

            MedicalBook.ATCs = atccs?.atccs;
            MedicalBook.ICD10s = icd10?.icd10s;
            MedicalBook.ICD11s = icd11?.icd11s;
            
#if UNITY_ANDROID
            yield return StartCoroutine(CopyStreamingAssets());
#endif
            CreateDictionaries();
            isDone = true;
        }

        private void CreateDictionaries()
        {
            MedicalBook?.CreateDictionaries();

            // WD generated books
            if(allCheckUps != null)
                fullCheckUpById = allCheckUps
                    .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);

            if (allPhysicalPoints != null)
            {
                physicalPointById = new Dictionary<string, PhysicalPoint>();
                foreach (var wdPhysicalPoint in allPhysicalPoints)
                {
                    foreach (var item in wdPhysicalPoint.items)
                    {
                        item.values = wdPhysicalPoint.values;
                        physicalPointById.Add(item.id, item);
                    }
                }
            }
        }
        private IEnumerator CopyStreamingAssets()
        {
            if (File.Exists(Application.persistentDataPath + "/segoeui.ttf")) yield break;
            
            using var webRequest = UnityWebRequest.Get(Application.streamingAssetsPath + "/Fonts/segoeui.ttf");
            yield return webRequest.SendWebRequest();
            
            FileHandler.WriteBytesFile(Application.persistentDataPath + "/segoeui.ttf", webRequest.downloadHandler.data);
        }
    }
}
