using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;

namespace Modules.AppUpdate
{
    public class AppUpdateController : MonoBehaviour
    {
        private AppUpdateView _view;

        private WebClient _client;
        private string _fileLocation;
        private int _updateDecision;
        private int _secretCounter;
        
        private void Awake()
        {
            _view = GetComponent<AppUpdateView>();
            _view.confirmUpdate.button.onClick.AddListener(() => _updateDecision = 1);
            _view.rejectUpdate.button.onClick.AddListener(() => _updateDecision = -1);
            _view.secretButton.onClick.AddListener(SecretCount);
            _view.cancelDownload.button.onClick.AddListener(CancelDownload);
        }
        
        public IEnumerator Init()
        {
            Debug.Log("App update check: Start");
            
            var config = BookDatabase.Instance.Configurations;
            if(config == null) yield break;
            var isLatestVersion = true;
            var updateUrl = "";
            string betaVersion;
            string betaLink;
            string finalVersion;
            string finalLink;
            
            if(XRSettings.enabled) yield break;

#if UNITY_EDITOR
            yield break;
#endif

#if UNITY_STANDALONE_WIN
            config.TryGetValue("Win_Beta_Version", out betaVersion);
            config.TryGetValue("Win_Beta_Link", out betaLink);
            config.TryGetValue("Win_Final_Version", out finalVersion);
            config.TryGetValue("Win_Final_Link", out finalLink);
#elif UNITY_STANDALONE_OSX
            config.TryGetValue("MacOS_Beta_Version", out betaVersion);
            config.TryGetValue("MacOS_Beta_Link", out betaLink);
            config.TryGetValue("MacOS_Final_Version", out finalVersion);
            config.TryGetValue("MacOS_Final_Link", out finalLink);
#else
            yield break;
#endif
            
            if (finalVersion != null)
            {
                if (GameManager.Instance.isBetaTest && betaVersion != null)
                {
                    var check = ConvertVersion(betaVersion).CompareTo(ConvertVersion(finalVersion));
                    isLatestVersion = ConvertVersion(Application.version).CompareTo(ConvertVersion(check > 0 ? betaVersion : finalVersion)) >= 0;
                    updateUrl = check > 0 ? betaLink : finalLink;
                }
                else
                {
                    if (finalVersion.Contains('f') && (Application.version.Contains('b') || Application.version.Contains('a')))
                        isLatestVersion = false;
                    else
                        isLatestVersion = ConvertVersion(Application.version).CompareTo(ConvertVersion(finalVersion)) >= 0;
                    updateUrl = finalLink;
                }
            }
            
            if(isLatestVersion)
            {
                DirectoryPath.DeleteDirectory(DirectoryPath.UpdateDir);
                Debug.Log("App update check: End");
                yield break;
            }
            
            var langs = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(
                BookDatabase.Instance.Configurations["Langs"]);
            _view.SetTxt(langs[Language.Code]);
            
            // yield return StartCoroutine(ShowPrompt());
            // if (_updateDecision != 1 && _updateDecision != 9)
            // {
            //     Application.Quit();
            //     yield break;
            // }
            // if(_updateDecision != 9)
            yield return StartCoroutine(DownloadUpdate(updateUrl));
        }

        private IEnumerator ShowPrompt()
        {
            _updateDecision = 0;
            _view.ShowPrompt(true);
            yield return new WaitUntil(() => _updateDecision != 0);
            _view.ShowPrompt(false);
        }

        private IEnumerator DownloadUpdate(string url)
        {
            Debug.Log("App update check - Download Update: Start");
            var isDone = false;
            var isCancelled = false;
            _fileLocation = DirectoryPath.UpdateFile;
            _view.ShowProgressPanel(true);
            _client = new WebClient();
            _client.DownloadProgressChanged += (sender, args) => _view.SetProgress(args.BytesReceived, args.TotalBytesToReceive);
            _client.DownloadFileCompleted += (sender, args) =>
            {
                if (args.Cancelled || args.Error != null) isCancelled = true;
                isDone = true;
            };
            _client.DownloadFileAsync(new Uri(url), _fileLocation);
            yield return new WaitUntil(() => isDone);
            
            if(isCancelled) yield break;
            
            Debug.Log("App update check - Download Update: End");

            var startInfo = new ProcessStartInfo(_fileLocation)
            {
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = "/SILENT"
            };

            Process.Start(startInfo);
            yield return new WaitForSeconds(1.0f);
            Application.Quit();
            
            _view.ShowProgressPanel(false);
        }
        

        private void CancelDownload()
        {
            Debug.Log("App update check - Download Update: Cancelled");
            _view.cancelDownload.button.interactable = false;
            _client?.CancelAsync();
            StartCoroutine(DeleteCorruptedWithDelay());
        }
        
        private IEnumerator DeleteCorruptedWithDelay()
        {
            yield return new WaitForSeconds(3.0f);
            FileHandler.DeleteFile(_fileLocation);
            yield return new WaitForSeconds(2.0f);
            Application.Quit();
        }
        
        private static Version ConvertVersion(string version)
        {
            char[] buildTypeChars = { 'a', 'b', 'f' };

            var buildNumberPos = version.LastIndexOfAny(buildTypeChars);
            var multiplier = GetBuildVersionMultiplier(version[buildNumberPos]);
            var buildNumber = int.Parse(version.Substring(buildNumberPos + 1));
            var formattedVersion = version.Substring(0, buildNumberPos) + "." + (multiplier + buildNumber);

            return Version.Parse(formattedVersion);
        }

        private static int GetBuildVersionMultiplier(char type)
        {
            return type switch
            {
                'a' => 10,
                'b' => 100,
                'f' => 1000,
                _ => 1
            };
        }

        private void OnDestroy()
        {
            _client?.CancelAsync();
        }

        private void SecretCount()
        {
            _secretCounter++;

            if (_secretCounter == 10)
            {
                _secretCounter = 0;
                _updateDecision = 9;
            }
        }
    }
}
