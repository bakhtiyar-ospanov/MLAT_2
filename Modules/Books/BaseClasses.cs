using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modules.Books
{
    public class UrlInfo
    {
        public class WA
        {
            [JsonProperty(PropertyName = "WebServiceUrl")] public string WebServiceUrl;
            [JsonProperty(PropertyName = "Password")] public string Password;
        }
        [JsonProperty(PropertyName = "VargatesAccessKeyS3")] public string AccessKeyS3;
        [JsonProperty(PropertyName = "VargatesSecretKeyS3")] public string SecretKeyS3;
        [JsonProperty(PropertyName = "ServiceUrlS3")] public string ServiceUrlS3;
        [JsonProperty(PropertyName = "FilmsPath")] public string FilmsPath;
        [JsonProperty(PropertyName = "SpeechPath")] public string SpeechPath;
        [JsonProperty(PropertyName = "YandexTTS")] public string YandexTTS;
        [JsonProperty(PropertyName = "S3BookPaths_v3")] private Dictionary<string,string> S3BookPaths_v3;
        [JsonProperty(PropertyName = "S3CatalogPaths_v1")] private List<string> S3CatalogPaths_v1;
        [JsonProperty(PropertyName = "WorldAccess")] public WA WorldAccess;
        [JsonIgnore] public Dictionary<string,string> S3BookPaths;
        [JsonIgnore] public List<string> S3CatalogPaths;

        public void InitVersion()
        {
            S3BookPaths = S3BookPaths_v3;
            S3CatalogPaths = S3CatalogPaths_v1;
        }
    }
}
