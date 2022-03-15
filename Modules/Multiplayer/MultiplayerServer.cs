using System.Collections;
using System.Collections.Generic;
using Mirror;
using Modules.Books;
using Modules.S3;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Modules.Multiplayer
{
    public class MultiplayerServer : MonoBehaviour
    {
        public class MultiplayerInfo
        {
            public string ServerScene;
            public string IPAddress;
            public string Port;
        }
        
        [SerializeField] private AddressablesS3 addressablesS3;
        [SerializeField] private NetworkManager networkManager;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            networkManager.StartServer();
            PlayfabLogin();
        }

        private void PlayfabLogin()
        {
            Debug.Log("PlayfabLogin: Start");
            var request = new LoginWithCustomIDRequest { 
                CustomId = SystemInfo.deviceUniqueIdentifier, 
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserAccountInfo = true,
                    GetTitleData = true
                }
            };
            
            PlayFabClientAPI.LoginWithCustomID(request, OnCustomIDLoginSuccess, Debug.Log);
        }

        private void OnCustomIDLoginSuccess(LoginResult result)
        {
            StartCoroutine(LoadScene(result.InfoResultPayload.TitleData));
        }

        private IEnumerator LoadScene(Dictionary<string, string> titleData)
        {
            var info = JsonConvert.DeserializeObject<MultiplayerInfo>(titleData["Multiplayer"]);
            var startScene = info.ServerScene;

            var urlInfo = JsonConvert.DeserializeObject<UrlInfo>(titleData["PathConfig"]);
            urlInfo.InitVersion();

            yield return StartCoroutine(addressablesS3.Init("default",
                urlInfo.AccessKeyS3, urlInfo.SecretKeyS3, urlInfo.ServiceUrlS3, urlInfo.S3CatalogPaths));
            
            var check = Addressables.LoadResourceLocationsAsync(startScene);
            yield return check;
            var count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No scene with this id is in Addressables
                Debug.Log("NO START SCENE " + startScene);
                yield break;
            }
            
            Debug.Log("YES START SCENE " + startScene);
            
            var sceneHandle =  Addressables.LoadSceneAsync(startScene);
            
            while (!sceneHandle.IsDone)
            {
                Debug.Log("Loading scene: " + sceneHandle.PercentComplete);
                yield return null;
            }
            
            Debug.Log("ACTIVE SCENE: " + SceneManager.GetActiveScene().name);
            
            var cams = FindObjectsOfType<Camera>();
            foreach (var cam in cams)
            {
                var obj = cam.transform.root.gameObject;
                if(obj.scene.name == "DontDestroyOnLoad") continue;
                DestroyImmediate(obj);
            }
        }
    }
}
