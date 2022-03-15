using System;
using System.Collections;
using Modules.Books;
using Modules.SpeechKit;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;
using Cursor = Modules.Starter.Cursor;

namespace Modules.WDCore
{
    public class ApplicationController : MonoBehaviour
    {
        [SerializeField] private bool isShowIntro = true;
        
        private void Awake()
        {
            ProductDetection();
            SettingsSetup();
        }
        
        private IEnumerator Start()
        {
            Cursor.IsBlocked = true;
            GameManager.Instance.onProductChange?.Invoke(GameManager.Instance.defaultProduct);
            GameManager.Instance.starterController.CamInit();

            yield return StartCoroutine(CheckInternetReachability());

            GameManager.Instance.loadingController.Init("");
            
            if(isShowIntro)
                yield return StartCoroutine(GameManager.Instance.introController.Init());
            
            GameManager.Instance.playFabLoginController.Init();
            yield return new WaitUntil(() => GameManager.Instance.playFabLoginController.isLogged);

            yield return StartCoroutine(GameManager.Instance.languageInitController.Init());
            GameManager.Instance.settingsController.PreInit();
            yield return StartCoroutine(GameManager.Instance.appUpdateController.Init());
            
            yield return StartCoroutine(DatabaseUpdate(true));
            GameManager.Instance.clientNetworkManager.Init();
            StartCoroutine(LoadStartScene());
            
            yield return StartCoroutine(BookDatabase.Instance.BookLoading());
            
            GameManager.Instance.GSAuthController.CheckActivation();
            InitalInits();

            StartCoroutine(GameManager.Instance.scenarioSelectorController.Init());
        }

        public IEnumerator DatabaseUpdate(bool isOnStart = false)
        {
            Debug.Log("Database update: Start");
            GameManager.Instance.loadingController.Init(Language.LangNames[Language.Code][10]);
            BookDatabase.Instance.Init();
            
            if(!isOnStart)
                yield return StartCoroutine(BookDatabase.Instance.BookLoading());
            
            Debug.Log("Database update: End");
            var urlInfo = BookDatabase.Instance.URLInfo;
            Debug.Log("Reading Addressable catalogs: Start");
            yield return StartCoroutine(GameManager.Instance.addressablesS3.Init("default",
                urlInfo.AccessKeyS3, urlInfo.SecretKeyS3, urlInfo.ServiceUrlS3, urlInfo.S3CatalogPaths));
            Debug.Log("Reading Addressable catalogs: End");

            if (!isOnStart)
            {
                GameManager.Instance.loadingController.Hide();
                GameManager.Instance.mainMenuController.ShowMenu(true);
            }
            
        }

        private IEnumerator LoadStartScene()
        {
            BookDatabase.Instance.Configurations.TryGetValue("Start_Scene", out var startScene);
            if (!string.IsNullOrEmpty(startScene))
                yield return StartCoroutine(GameManager.Instance.starterController.Init(startScene));
        }

        private void InitalInits()
        {
            GameManager.Instance.settingsController.Init();
            GameManager.Instance.profileController.Init();
            GameManager.Instance.inventoryController.Init();
            GameManager.Instance.statisticsController.Init();
            GameManager.Instance.statisticsManager.SyncStatistics();
            GameManager.Instance.mainMenuController.Init();
            GameManager.Instance.reportErrorController.Init();
            GameManager.Instance.helpController.Init();
            GameManager.Instance.filmController.Init();
            GameManager.Instance.chatController.Init();
            GameManager.Instance.playFabCurrencyController.Init();
            GameManager.Instance.previewDownloader.InitAcademix();
            GameManager.Instance.starterController.ShowCanvas();
            GameManager.Instance.appControls.Init();
            TextToSpeech.Instance.Init();
            StartCoroutine(GameManager.Instance.VSMonitorController.Init());
        }

        private void SettingsSetup()
        {
            DirectoryPath.CheckDirectories();
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(this);
        }

        private void ProductDetection()
        {
            var product = PlayFabSettings.TitleId switch
            {
                "E0400" => GameManager.Product.Vargates,
                "DF7FB" => GameManager.Product.Academix,
                _ => GameManager.Product.Vargates
            };
            GameManager.Instance.defaultProduct = product;
        }

        private IEnumerator CheckInternetReachability()
        {
            Debug.Log("Internet check: Start");
            var isInternetReachable = false;
            while (!isInternetReachable)
            {
                UnityWebRequest www = UnityWebRequest.Get("http://google.com");
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    GameManager.Instance.warningController.ShowExitWarning("No internet connection.\nClose the application?", Application.Quit);
                    GameManager.Instance.warningController.SetOfflineButtonsText();
                    yield return new WaitForSeconds(5.0f);
                }
                else
                {
                    isInternetReachable = true;
                    GameManager.Instance.warningController.HideWarningView();
                }
            }
            Debug.Log("Internet check: End");
        }
    }
}
