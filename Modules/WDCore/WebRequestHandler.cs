using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.WDCore
{
    public class WebRequestHandler : Singleton<WebRequestHandler>
    {
        private static readonly float repeatRequestTime = 2.0f;

        public IEnumerator GetRequest(string url, Action<string> response, (string, string) header = default, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            //Debug.Log("GET: " + url);

            using var webRequest = UnityWebRequest.Get(url);
            if(header != default) webRequest.SetRequestHeader(header.Item1, header.Item2);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {webRequest.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(GetRequest(url, response, header, onNetworkError));
            }
            else
            {
                response?.Invoke(webRequest.downloadHandler.text);
            }
        }
        
        public IEnumerator PostRequest(string url, WWWForm form, Action<string> response, (string, string) header = default, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            //Debug.Log("POST: " + url);

            using var webRequest = UnityWebRequest.Post(url, form);
            if(header != default) webRequest.SetRequestHeader(header.Item1, header.Item2);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {webRequest.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(PostRequest(url, form, response, header, onNetworkError));
            }
            else
            {
                response?.Invoke(webRequest.downloadHandler.text);
            }
        }
        
        public IEnumerator PostRequestJson(string url, string body, Action<string> response, (string, string) header = default, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            //Debug.Log("POST: " + url);

            using var webRequest = new UnityWebRequest(url, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(body);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
                
            if(header != default) webRequest.SetRequestHeader(header.Item1, header.Item2);
                
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {webRequest.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(PostRequestJson(url, body, response, header, onNetworkError));
            }
            else
            {
                response?.Invoke(webRequest.downloadHandler.text);
            }
        }
        
        public IEnumerator TextureRequest(string url, Action<Texture2D> response, (string, string) header = default, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            //Debug.Log("GET TEXTURE: " + "file://" + url);

#if UNITY_ANDROID            
            using var webRequest = UnityWebRequestTexture.GetTexture("jar:file://" + url);
#else
            using var webRequest = UnityWebRequestTexture.GetTexture("file://" + url);
#endif
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {webRequest.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(TextureRequest(url, response, header, onNetworkError));
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(webRequest);
                response?.Invoke(texture);
            }
        }
        
        public IEnumerator AudioRequest(string url, Action<AudioClip> response, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            Debug.Log("GET AUDIO: " + "file://" + url);
            var type = url.EndsWith(".mp3") ? AudioType.MPEG : url.EndsWith(".ogg") ? AudioType.OGGVORBIS : AudioType.WAV;
            using var www = UnityWebRequestMultimedia.GetAudioClip("file://" + url, type);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {www.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(AudioRequest(url, response, onNetworkError));
            }
            else
            {
                response?.Invoke(DownloadHandlerAudioClip.GetContent(www));
            }
        }

        public IEnumerator DownloadRequest(string url, string pathOut, Action<float, float, string, bool> response, Action<UnityWebRequest> cancelCallback, (string, string) header = default, Action<string> onNetworkError = null, bool isPersistent = true)
        {
            //Debug.Log("DOWNLOAD: " + url);

            var webRequest = new UnityWebRequest(url) {method = UnityWebRequest.kHttpVerbGET};
            if(header != default) webRequest.SetRequestHeader(header.Item1, header.Item2);

            var dh = new DownloadHandlerFile(pathOut) {removeFileOnAbort = true};
            webRequest.downloadHandler = dh;
            webRequest.SendWebRequest();
            cancelCallback?.Invoke(webRequest);
            var size = webRequest.GetResponseHeader("Content-Length");

            while (!dh.isDone)
            {
                if(string.IsNullOrEmpty(size))
                    size = webRequest.GetResponseHeader("Content-Length");
                response?.Invoke(webRequest.downloadProgress, webRequest.downloadedBytes, size, false);
                yield return null;
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                onNetworkError?.Invoke($"URL: {url}. Error: {webRequest.error}");
                if (!isPersistent /*|| !UrlData.isConnected*/) yield break;
                yield return new WaitForSeconds(repeatRequestTime);
                yield return StartCoroutine(DownloadRequest(url, pathOut, response, cancelCallback, header, onNetworkError));
            }
            else
            {
                response?.Invoke(webRequest.downloadProgress, webRequest.downloadedBytes, size, true);
            }
        }
    }
}
