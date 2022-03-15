using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.SpeechKit;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace Modules.MainMenu
{
    public class SettingsController : MonoBehaviour
    {
        private SettingsView _view;

        private Canvas[] _canvases;
        private string[] _langCodes;
        public Action<bool> onBetaModeChange;
        public Action<bool> onTeacherModeChange;
        public Action<int> onQualityChange;
        public List<(int, int)> screenResolutions;
        private List<float> _zoomOptions;
        private bool _isAllCanvasesHidden;
        private Dictionary<Canvas, bool> _canvasStates;

        private void Awake()
        {
            _view = GetComponent<SettingsView>();

            //FPS
            if(!PlayerPrefs.HasKey("SHOW_FPS"))
            {
                PlayerPrefs.SetInt("SHOW_FPS", 0);
                PlayerPrefs.Save();
            }
            
            // Zoom setup
            _canvases = FindObjectsOfType<Canvas>(true);
            if (!PlayerPrefs.HasKey("ZOOM"))
            {
                var defaultZoom = 1.0f;
                PlayerPrefs.SetFloat("ZOOM", defaultZoom);
                PlayerPrefs.Save();
            }

            var zoomScale = PlayerPrefs.GetFloat("ZOOM");
            ChangeZoom(zoomScale);
            
            //Post FX
            if(!PlayerPrefs.HasKey("POST_FX"))
            {
                PlayerPrefs.SetInt("POST_FX", 1);
                PlayerPrefs.Save();
            }
                     
            //VSync
            if(!PlayerPrefs.HasKey("VSYNC"))
            {
                PlayerPrefs.SetInt("VSYNC", 1);
                PlayerPrefs.Save();
            }
            
            //Time Limit
            if(!PlayerPrefs.HasKey("TIME_LIMIT"))
            {
                PlayerPrefs.SetInt("TIME_LIMIT", 4);
                PlayerPrefs.Save();
            }

            var enableVSync = PlayerPrefs.GetInt("VSYNC");
            EnableVSync(enableVSync);

            foreach (var developerItem in _view.developerItems)
                developerItem.SetActive(false);
            
            _view.betaTestingTgl.onValueChanged.AddListener(val =>
            {
                if (GameManager.Instance.defaultProduct == GameManager.Product.Vargates)
                {
                    _view.betaTestingTgl.SetIsOnWithoutNotify(true);
                    return;
                }
                SetDevMode(val);
                GameManager.Instance.playFabLoginController.SetUserData("BETA_TESTING", val.ToString());
            });
            
            
            _view.fpsTgl.onValueChanged.AddListener(GameManager.Instance.FPSController.SetFPS);
            _view.fpsTgl.isOn = PlayerPrefs.GetInt("SHOW_FPS") == 1;
            
            _view.profModeTgl.isOn = PlayerPrefs.GetInt("TEACHER_MODE") == 1;
            _view.profModeTgl.onValueChanged.AddListener(SetTeacherMode);

            if (_view.pintModalRoot != null)
            {
                _view.pintModalRoot.SetActive(false);
                _view.wrongPinError.SetActive(false);
                _view.pinField.onValueChanged.AddListener(val => _view.wrongPinError.SetActive(false));
            }

            _view.versionTxt.text = Application.version;
            _view.dbUpdateButton.onClick.AddListener(() => 
                StartCoroutine(GameManager.Instance.applicationController.DatabaseUpdate()));
        

            _view.downloadAllCasesButton.onClick.AddListener(() =>
                StartCoroutine(GameManager.Instance.addressablesS3.DownloadAllCases()));
            
            GameManager.Instance.playFabLoginController.OnUserLogin += val =>
            {
                var betaRaw = GameManager.Instance.playFabLoginController.GetUserData("BETA_TESTING");
                if (string.IsNullOrEmpty(betaRaw))
                    betaRaw = "False";
                
                var isBetaOn = bool.Parse(betaRaw);
                if (GameManager.Instance.defaultProduct == GameManager.Product.Vargates) isBetaOn = true;

                _view.betaTestingTgl.SetIsOnWithoutNotify(isBetaOn);
                SetDevMode(val);
            };
            
            _view.zoomDropdown.onValueChanged.AddListener(ChangeZoom);
            
            _view.postFxTgl.onValueChanged.AddListener(GameManager.Instance.postProcessController.EnablePostFX);
            _view.postFxTgl.isOn = PlayerPrefs.GetInt("POST_FX") == 1;
            
            _view.vSyncTgl.onValueChanged.AddListener(EnableVSync);
            _view.vSyncTgl.isOn = PlayerPrefs.GetInt("VSYNC") == 1;
            
            _view.btnClearCache.onClick.AddListener(() => StartCoroutine(ClearCache()));
            _view.btnOpenLogs.onClick.AddListener(() => Application.OpenURL(Application.persistentDataPath + "/Logs"));
            
            _view.timeLimitDropdown.onValueChanged.AddListener(ChangeTimeLimit);
            
#if UNITY_XR
            view.langDropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
            view.qualityDropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
            view.resolutionDropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
            view.doctorVoiceDropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;

            if (view.pintModalRoot != null)
                view.pinField.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.pinField));
#endif

            if(Application.platform == RuntimePlatform.IPhonePlayer)
                SetSafeAreaIOS();
        }

        private void Start()
        {
            if (XRSettings.enabled)
                SetupVRCameras();
        }

        private void SetSafeAreaIOS()
        {
            foreach (var _canvas in _canvases)
            {
                var _safeAreaObj = new GameObject("Safe Area", typeof(RectTransform));
                _safeAreaObj.transform.SetParent(_canvas.transform);
                
                var safeAreaRect = _safeAreaObj.GetComponent<RectTransform>();

                safeAreaRect.pivot = new Vector2(0.5f, 0.5f);
                safeAreaRect.anchorMin = Vector3.zero;
                safeAreaRect.anchorMax = Vector3.one;
                safeAreaRect.anchoredPosition = Vector3.zero;
                safeAreaRect.sizeDelta = Vector2.zero;
                safeAreaRect.localScale = Vector3.one;

                var safeArea = Screen.safeArea;
                var minAnchor = safeArea.position;
                var maxAnchor = minAnchor + safeArea.size;
                
                minAnchor.x /= Screen.width;
                minAnchor.y /= Screen.height;
                maxAnchor.x /= Screen.width;
                maxAnchor.y /= Screen.height;
                
                safeAreaRect.anchorMin = minAnchor;
                safeAreaRect.anchorMax = maxAnchor;

                for (int i = _canvas.transform.childCount - 1; i >= 0; --i)
                {
                    Transform child = _canvas.transform.GetChild(i);
                    child.SetParent(_safeAreaObj.transform,false);
                }
            }
        }

        public void PreInit()
        {
            FillUpResolutionOptions();
            SetUpZoomLimits(Screen.currentResolution.width, Screen.currentResolution.height);
#if !UNITY_XR
            GameManager.Instance.appControls.Show();
#endif
        }

        public void Init()
        {
            FillUpQualityOptions();
            FillUpLanguagesOptions();
            FillUpDoctorGender();
            FillUpICD();
            FillUpTimeLimits();
            GameManager.Instance.mainMenuController.AddModule("Settings", "", SetActivePanel, new []{_view.root.transform});
        }

        private void SetActivePanel(bool val)
        {
            _view.canvas.SetActive(val);
        }

        private IEnumerator ChangeLanguage(int index)
        {
            Language.SetLanguage(_langCodes[index]);
            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(
                GameManager.Instance.playFabLoginController.ChangeProfileLanguage(_langCodes[index]));
            DestroyImmediate(GameManager.Instance.clientNetworkManager.gameObject);
            DestroyImmediate(GameManager.Instance.gameObject);
            SceneManager.LoadSceneAsync(XRSettings.enabled ? "_VR_MainScene" : "_MainScene");
        }
        
        
        private void ChangeDoctorGender(int index)
        {
            PlayerPrefs.SetInt("DOCTOR_GENDER", index);
            PlayerPrefs.Save();
            TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Doctor, index);
        }
        
        private void ChangeICDVersion(int index)
        {
            PlayerPrefs.SetInt("ICD_VERSION", index);
            PlayerPrefs.Save();
        }
        
        private void FillUpResolutionOptions()
        {
            screenResolutions = new List<(int, int)>();

            foreach (var resolution in Screen.resolutions)
            {
                if(resolution.width < 1000 || resolution.height < 700) continue;
                var res = (resolution.width, resolution.height);
                if(!screenResolutions.Contains(res))
                    screenResolutions.Add(res);
            }
            
            var currentResIndex = 0;

            _view.resolutionDropdown.options = new List<TMP_Dropdown.OptionData>();
            for(var i = 0; i < screenResolutions.Count; ++i)
            {
                _view.resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(
                    $"{screenResolutions[i].Item1}x{screenResolutions[i].Item2}"));

                if (screenResolutions[i].Item1 == Screen.currentResolution.width &&
                    screenResolutions[i].Item2 == Screen.currentResolution.height)
                    currentResIndex = i;
            }

            _view.resolutionDropdown.onValueChanged.RemoveAllListeners();
            _view.resolutionDropdown.value = currentResIndex;
            _view.resolutionDropdown.onValueChanged.AddListener(index =>
            {
                Screen.SetResolution(screenResolutions[index].Item1, screenResolutions[index].Item2, Screen.fullScreen);
                SetUpZoomLimits(screenResolutions[index].Item1, screenResolutions[index].Item2, true);
            });
        }
        
        private void FillUpQualityOptions()
        {
            _view.qualityDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(TextData.Get(54)),
                new TMP_Dropdown.OptionData(TextData.Get(53)),
                new TMP_Dropdown.OptionData(TextData.Get(52))
            };
            _view.qualityDropdown.onValueChanged.RemoveAllListeners();
            _view.qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
            _view.qualityDropdown.onValueChanged.AddListener(QualitySettings.SetQualityLevel);
            _view.qualityDropdown.onValueChanged.AddListener(val => onQualityChange?.Invoke(val));
            _view.qualityDropdown.onValueChanged.AddListener(UpdateVSync);
        }

        private void FillUpLanguagesOptions()
        {
            _langCodes = Language.LangNames.Keys.ToArray();
            if(_langCodes == null) return;
            
            _view.langDropdown.options = new List<TMP_Dropdown.OptionData>();
            foreach (var langCode in Language.LangNames)
                _view.langDropdown.options.Add(new TMP_Dropdown.OptionData(langCode.Value[0]));

            _view.langDropdown.onValueChanged.RemoveAllListeners();
            _view.langDropdown.SetValueWithoutNotify(Array.IndexOf(_langCodes, Language.Code));
            _view.langDropdown.onValueChanged.AddListener(val => StartCoroutine(ChangeLanguage(val)));
        }

        private void FillUpDoctorGender()
        {
            if (!PlayerPrefs.HasKey("DOCTOR_GENDER"))
            {
                PlayerPrefs.SetInt("DOCTOR_GENDER", 0);
                PlayerPrefs.Save();
            }

            var doctorGender = PlayerPrefs.GetInt("DOCTOR_GENDER");
            TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Doctor, doctorGender);

            _view.doctorVoiceDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(TextData.Get(201)),
                new TMP_Dropdown.OptionData(TextData.Get(200))
            };
            
            _view.doctorVoiceDropdown.onValueChanged.RemoveAllListeners();
            _view.doctorVoiceDropdown.SetValueWithoutNotify(doctorGender);
            _view.doctorVoiceDropdown.onValueChanged.AddListener(ChangeDoctorGender);
        }
        
        private void FillUpICD()
        {
            if (!PlayerPrefs.HasKey("ICD_VERSION"))
            {
                PlayerPrefs.SetInt("ICD_VERSION", 0);
                PlayerPrefs.Save();
            }

            var icdVersion = PlayerPrefs.GetInt("ICD_VERSION");
            
            _view.icdDropdown.options = new List<TMP_Dropdown.OptionData>
                {
                    new(TextData.Get(285)),
                    new(TextData.Get(317))
                };
                
            _view.icdDropdown.onValueChanged.RemoveAllListeners();
            _view.icdDropdown.SetValueWithoutNotify(icdVersion);
            _view.icdDropdown.onValueChanged.AddListener(ChangeICDVersion);
        }

        private void FillUpTimeLimits()
        {
            _view.timeLimitDropdown.options = new List<TMP_Dropdown.OptionData>
            {
                new(TextData.Get(87)), 
                new($"25 {TextData.Get(86)}"), new($"30 {TextData.Get(86)}"), 
                new($"35 {TextData.Get(86)}"), new($"40 {TextData.Get(86)}"),
                new($"45 {TextData.Get(86)}"), new($"50 {TextData.Get(86)}"),
                new($"55 {TextData.Get(86)}"), new($"60 {TextData.Get(86)}"),
            };
            
            _view.timeLimitDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("TIME_LIMIT"));
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F5))
                _view.dbUpdateButton.onClick?.Invoke();
            if (Input.GetKeyDown(KeyCode.F12))
                HideAllCanvases();
        }

        private void ChangeZoom(float val)
        {
            foreach (var canvas in _canvases)
                canvas.scaleFactor = val;
            
            PlayerPrefs.SetFloat("ZOOM", val);
            PlayerPrefs.Save();
        }
        
        private void ChangeZoom(int val)
        {
            if(_zoomOptions.Count > val)
                ChangeZoom(_zoomOptions[val]);
        }

        private void SetUpZoomLimits(int width, int height, bool isForceMax = false)
        {
            var maxZoom = Math.Min(width / 1920.0f, height / 1080.0f);
            var minZoom = maxZoom - 0.5f * maxZoom;
            minZoom = minZoom < 0.3f ? 0.3f : minZoom;

            _zoomOptions = new List<float>();
            var options = new List<TMP_Dropdown.OptionData>();
            var zoomScale = PlayerPrefs.GetFloat("ZOOM");
            var currentIndex = 0;
            var lastZoom = maxZoom;
            
            while (lastZoom > minZoom)
            {
                options.Add(new TMP_Dropdown.OptionData(lastZoom.ToString("N1")));
                _zoomOptions.Add(lastZoom);

                if (zoomScale < lastZoom)
                    currentIndex++;
                
                lastZoom -= 0.1f;
            }

            if (_zoomOptions.Count == 0)
            {
                options.Add(new TMP_Dropdown.OptionData("1.0"));
                _zoomOptions.Add(1.0f);
            }
            
            _view.zoomDropdown.options = options;

            if (isForceMax || zoomScale > maxZoom || zoomScale < minZoom)
            {
                _view.zoomDropdown.SetValueWithoutNotify(0);
                ChangeZoom(0);
            }
            else
            {
                _view.zoomDropdown.SetValueWithoutNotify(currentIndex);
                ChangeZoom(currentIndex);
            }
                
        }

        
        private void SetupVRCameras()
        {
            #if UNITY_XR
            
            var fpcVR = GameManager.Instance.starterController.GetFPСVR();
            var cam = fpcVR.GetCamera();
            var pointer = fpcVR.GetPointer();
            
            foreach (var canvas in _canvases)
            {
                if(canvas.renderMode != RenderMode.WorldSpace) continue;
                canvas.worldCamera = cam;
                
                var ovr = canvas.GetComponent<OVRRaycaster>();
                if(ovr == null) continue;
                ovr.pointer = pointer;
            }
            
            #endif
        }

        private IEnumerator ClearCache()
        {
            Debug.Log("Clear Cache");
            var scene = SceneManager.CreateScene("Idle");
            SceneManager.SetActiveScene(scene);
            
            var oldScene = GameManager.Instance.starterController.SceneHandle;
            var oldSceneName = "";
            if (oldScene.IsValid() && oldScene.Status == AsyncOperationStatus.Succeeded)
            {
                oldSceneName = oldScene.Result.Scene.name;
                yield return Addressables.UnloadSceneAsync(oldScene);
            }
            
            try
            {
                Addressables.ClearDependencyCacheAsync("default");
                Addressables.ClearResourceLocators();
                Caching.ClearCache();
                Resources.UnloadUnusedAssets();
            }
            finally 
            { }

            yield return StartCoroutine(GameManager.Instance.applicationController.DatabaseUpdate());
            
            StartCoroutine(GameManager.Instance.starterController.Init(oldSceneName));
        }

        public void SelectResolutionInDropdown(int index)
        {
            _view.resolutionDropdown.value = index;
        }

        public int GetResolutionIndex()
        {
            return _view.resolutionDropdown.value;
        }

        private void SetTeacherMode(bool val)
        {
            PlayerPrefs.SetInt("TEACHER_MODE", val ? 1 : 0);
            PlayerPrefs.Save();
            
            onTeacherModeChange?.Invoke(val);
            
            // if (val)
            // {
            //     void ApplyCall()
            //     {
            //         var pin = _view.pinField.text;
            //         if (string.IsNullOrEmpty(pin)) return;
            //         PlayerPrefs.SetString("PROF_PIN", pin);
            //         PlayerPrefs.Save();
            //         _view.pintModalRoot.SetActive(false);
            //     }
            //
            //     _view.title.text = TextData.Get(272);
            //     _view.pinField.text = "";
            //     _view.applyPinButton.onClick.RemoveAllListeners();
            //     _view.applyPinButton.onClick.AddListener(ApplyCall);
            //     _view.pinField.onSubmit.RemoveAllListeners();
            //     _view.pinField.onSubmit.AddListener(x => ApplyCall());
            //     _view.pintModalRoot.SetActive(true);
            // }
            // else
            // {
            //     PlayerPrefs.DeleteKey("PROF_PIN");
            // }
        }

        public IEnumerator ShowProfPinModal()
        {
            void ApplyCall() {
                var pin = _view.pinField.text;
                if (string.IsNullOrEmpty(pin)) return;
                var correctPin = PlayerPrefs.GetString("PROF_PIN");
                if (correctPin == pin)
                {
                    _view.pintModalRoot.SetActive(false);
                    _view.wrongPinError.SetActive(false);
                    _view.root.SetActive(true);
                    SetActivePanel(false);
                    GameManager.Instance.mainMenuController.isBlocked = false;
                }
                else
                {
                    _view.wrongPinError.SetActive(true);
                }
                    
            }
            _view.title.text = TextData.Get(274);
            _view.pinField.text = "";
            _view.applyPinButton.onClick.RemoveAllListeners();
            _view.applyPinButton.onClick.AddListener(ApplyCall);
            _view.pinField.onSubmit.RemoveAllListeners();
            _view.pinField.onSubmit.AddListener(x => ApplyCall());
            _view.pintModalRoot.SetActive(true);
            _view.root.SetActive(false);
            SetActivePanel(true);
            GameManager.Instance.mainMenuController.isBlocked = true;
            Starter.Cursor.ActivateCursor(true);

            yield return new WaitUntil(() => !_view.pintModalRoot.activeSelf);
        }

        private void EnableVSync(bool val)
        {
            int valInt = val ? 1 : 0;
            EnableVSync(valInt);
        }

        private void EnableVSync(int val)
        {
            if (val > 1) val = 1; 
            else if (val < 0) val = 0;

            PlayerPrefs.SetInt("VSYNC", val);
            PlayerPrefs.Save();
            QualitySettings.vSyncCount = val;
        }

        private void UpdateVSync(int qualityLevel)
        {
            int currentVSync = PlayerPrefs.GetInt("VSYNC");
            if (currentVSync != QualitySettings.vSyncCount)
                EnableVSync(currentVSync);
        }

        private void HideAllCanvases()
        {
            if(!GameManager.Instance.isBetaTest) return;

            _isAllCanvasesHidden = !_isAllCanvasesHidden;

            if (_isAllCanvasesHidden)
                _canvasStates = new Dictionary<Canvas, bool>();

            foreach (var canvas in _canvases)
            {
                if (_isAllCanvasesHidden)
                {
                    _canvasStates.Add(canvas, canvas.gameObject.activeSelf);
                    canvas.gameObject.SetActive(false);
                }
                else
                {
                    canvas.gameObject.SetActive(_canvasStates[canvas]);
                }
                
            }
            var patient = GameManager.Instance.assetController.patientAsset;
            if(patient != null)
                patient.ShowExternalMenuButton(!_isAllCanvasesHidden);
        }
        
        public float GetCanvasScaleFactor()
        {
            return _zoomOptions[_view.zoomDropdown.value];
        }

        private void ChangeTimeLimit(int val)
        {
            var limit = new[] {0, 25, 30, 35, 40, 45, 50, 55, 60};
            PlayerPrefs.SetInt("TIME_LIMIT", val);
            PlayerPrefs.Save();
        }

        private void SetDevMode(bool val)
        {
            GameManager.Instance.isBetaTest = val;
            
            foreach (var developerItem in _view.developerItems)
                developerItem.SetActive(val);
            onBetaModeChange?.Invoke(val);
        }
    }
}
