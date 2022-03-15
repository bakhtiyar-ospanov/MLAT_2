using System;
using System.Collections;
using Modules.Books;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.Starter
{
    public class StarterController : MonoBehaviour
    {
        public bool isFreeMode;
        public bool isBlockReticule;
        public Action<Camera> CameraInit;
        public AsyncOperationHandle<SceneInstance> SceneHandle { get; private set; }
        
        [SerializeField] private FPCDesktop fpcDesktop;
        [SerializeField] private FPCMobile fpcMobile;
        [SerializeField] private FPCVR fpcVR;
        [SerializeField] private FPCAcademix fpcAcademix;
        
        private FPC _currentFPC;
        private Transform _lastTarget;

        private void Awake()
        {
            if(fpcDesktop != null)
                fpcDesktop.gameObject.SetActive(false);
            if(fpcMobile != null)
                fpcMobile.gameObject.SetActive(false);
            if (fpcAcademix != null)
            {
                fpcAcademix.gameObject.SetActive(false);
                fpcAcademix.onMovingChanged += val => onMovingChanged?.Invoke(val);
            }
        }

        public void CamInit()
        {
            if (GameManager.Instance.defaultProduct == GameManager.Product.Academix)
                _currentFPC = fpcAcademix;
            else if (XRSettings.enabled)
                _currentFPC = fpcVR;
            else if (Input.touchSupported)
                _currentFPC = fpcMobile;
            else
            {
                _currentFPC = fpcDesktop;
                Cursor.IsBlocked = false;
                isFreeMode = true;
            }
                

            _currentFPC.gameObject.SetActive(true);
            _currentFPC.SetKinematic(true);
            _currentFPC.SetActivePanel(false);
            CameraInit?.Invoke(_currentFPC.GetCamera());
        }

        public void ActivateAnotherFPC(int type)
        {
            _currentFPC.gameObject.SetActive(false);

            switch (type)
            {
                case 0:
                    fpcAcademix.SelectReticule(0);
                    if (Input.touchSupported)
                        _currentFPC = fpcMobile;
                    else
                        _currentFPC = fpcDesktop;
                    _currentFPC.gameObject.SetActive(true);
                    _currentFPC.SetActivePanel(true);
                    Cursor.IsBlocked = false;
                    Cursor.ActivateCursor(false);
                    break;
                case 1:
                    Cursor.ActivateCursor(true);
                    Cursor.IsBlocked = true;
                    _currentFPC = fpcAcademix;
                    _currentFPC.gameObject.SetActive(true);
                    _currentFPC.SetActivePanel(true);
                    break;
            }
            
            CameraInit?.Invoke(_currentFPC.GetCamera());
        }

        public void InitNoWait(string sceneId, string world = null)
        {
            StopAllCoroutines();
            StartCoroutine(Init(sceneId, true, null, world));
        }
        
        public IEnumerator Init(string sceneId, bool isToHideLoading = true, Action<bool> completedCallback = null, string world = null)
        {
            Debug.Log($"Scene load {sceneId}: Start");
            yield return StartCoroutine(DimedusCheck());

            if (!string.IsNullOrEmpty(world))
                yield return StartCoroutine(GameManager.Instance.addressablesS3.LoadAdditionalWorld(world));

            var check = Addressables.LoadResourceLocationsAsync(sceneId);
            yield return check;
            var count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No scene with this id is in Addressables
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {sceneId})");
                yield break;
            }
            
            var getDownloadSize = Addressables.GetDownloadSizeAsync(sceneId);
            yield return getDownloadSize;
            var size= getDownloadSize.Result;
            Addressables.Release(getDownloadSize);

            GameManager.Instance.clientNetworkManager.DisconnectMultiplayer();

            SceneHandle = Addressables.LoadSceneAsync(sceneId);
            var loading = GameManager.Instance.loadingController;
            loading.Init(TextData.Get(4));
            while (!SceneHandle.IsDone)
            {
                loading.SetProgress(SceneHandle.PercentComplete, size);
                yield return null;
            }
            
            if (!string.IsNullOrEmpty(world))
                GameManager.Instance.addressablesS3.RestoreDefaultWorld();

            if (SceneHandle.Status != AsyncOperationStatus.Succeeded)
            {
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {sceneId})");
                loading.Hide();
                completedCallback?.Invoke(false);
                yield break;
            }
            TempClearing();
            
            
            if (isToHideLoading)
            {
                yield return StartCoroutine(GameManager.Instance.blackout.Show());
                loading.Hide();
            }
                
            Debug.Log($"Scene load {sceneId}: End");
            
            yield return StartCoroutine(GameManager.Instance.clientNetworkManager.MultiplayerCheck());

            if(!XRSettings.enabled)
                _currentFPC.Init(GameObject.Find("PlayerStart"));
            
            StartCoroutine(GameManager.Instance.assetController.SearchAssets());
            GameManager.Instance.postProcessController.Init();

            completedCallback?.Invoke(true);
            if (isToHideLoading)
                yield return StartCoroutine(GameManager.Instance.blackout.Hide());
        }

        // public void InitFPC()
        // {
        //     _currentFPC.Init(GameObject.Find("PlayerStart"));
        // }

        public void LookAt(Transform target)
        {
            _lastTarget = target;
            if(target == null) return;
            _currentFPC.LookAt(target);
        }

        public void SetActiveFPC(bool val)
        {
            _currentFPC.gameObject.SetActive(val);
        }

        public void SetKinematic(bool val)
        {
            _currentFPC.SetKinematic(val);
        }
        
        public Camera GetCamera()
        {
            return _currentFPC.GetCamera();
        }

        public Transform GetLookTarget()
        {
            return _currentFPC.GetLookTarget();
        }

        #region DESKTOP SPECIFIC METHODS

        public void SelectReticule(int type)
        {
            if (Input.touchSupported || isBlockReticule) return;

            if(_currentFPC.GetType() == typeof(FPCDesktop))
                fpcDesktop.SelectReticule(type);
            else if(fpcAcademix.GetType() == typeof(FPCAcademix))
                fpcAcademix.SelectReticule(type);
        }

        #endregion

        #region MOBILE SPECIFIC METHODS

        public bool IsSwiping()
        {
            if (XRSettings.enabled || !Input.touchSupported) return false;
            return fpcMobile.IsSwiping();
        }
        public bool IsSwipingTouch()
        {
            if (XRSettings.enabled || !Input.touchSupported) return false;
            return fpcMobile.IsSwipingTouch();
        }

        public FixedJoystick GetJoystick()
        {
            if (XRSettings.enabled || !Input.touchSupported) return null;
            return fpcMobile.GetJoystick();
        }

        #endregion

        #region VR SPECIFIC METHODS

        public FPCVR GetFPСVR()
        {
            return fpcVR;
        }

        #endregion

        #region ACADEMIX SPECIFIC METHODS

        public Action<bool> onMovingChanged;

        public void EnableOrbitCamera(bool val)
        {
            if(fpcAcademix == null) return;
            fpcAcademix.EnableOrbitCamera(val);
        }
        

        #endregion
        
        private void TempClearing()
        {
            var cams = FindObjectsOfType<Camera>();
            
            foreach (var cam in cams)
            {
                var obj = cam.transform.root.gameObject;
                if(obj.scene.name == "DontDestroyOnLoad") continue;
                DestroyImmediate(obj);
            }
            
            var events = FindObjectsOfType<EventSystem>();
            
            foreach (var e in events)
            {
                var obj = e.transform.root.gameObject;
                if(obj.scene.name == "DontDestroyOnLoad") continue;
                DestroyImmediate(obj);
            }
        }

        private IEnumerator DimedusCheck()
        {
            if (GameManager.Instance.scenarioController.IsLaunched)
            {
                if (!PlayerPrefs.HasKey("PROF_PIN"))
                    GameManager.Instance.loadingController.Init(TextData.Get(210));
                yield return StartCoroutine(GameManager.Instance.scenarioController.Unload(false));
                yield return new WaitUntil(() => !GameManager.Instance.scenarioController.IsLaunched);
                GameManager.Instance.mainMenuController.ShowMenu(false);
            }
        }

        private void Update()
        {
            if(InputCheck()) return;

            if (Input.GetKeyDown(KeyCode.R))
                ActivateFreeMode();
            if (Input.GetKeyDown(KeyCode.T))
                LookAt(_lastTarget);
        }

        public void ActivateFreeMode()
        {
            isFreeMode = !isFreeMode;
            // if (isFreeMode && GameManager.Instance.defaultProduct == GameManager.Product.Academix)
            //     GameManager.Instance.warningController.ShowWarning(TextData.Get(334));
            
            ActivateAnotherFPC(isFreeMode ? 0:1);
            StartCoroutine(GameManager.Instance.clientNetworkManager.MultiplayerCheck());
        }

        private bool InputCheck()
        {
            var selectedObj = EventSystem.current.currentSelectedGameObject;
            return selectedObj != null && 
                   (selectedObj.TryGetComponent(out TMP_InputField _) || selectedObj.TryGetComponent(out InputField _));
        }

        public void ReplaceMPlayer(FPCDesktop.PlayerComponents playerComponents)
        {
            fpcDesktop.ReplaceMPlayer(playerComponents);
            CameraInit?.Invoke(_currentFPC.GetCamera());
        }

        public void RestoreLocalPlayer()
        {
            fpcDesktop.RestoreLocalPlayerComponent();
            CameraInit?.Invoke(_currentFPC.GetCamera());
        }

        public void ShowCanvas()
        {
            _currentFPC.SetActivePanel(true);
        }
    }
}