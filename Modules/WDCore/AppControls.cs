using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Modules.WDCore
{
    public class AppControls : MonoBehaviour
    {
        private AppControlsView _view;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
#endif

        private void Awake()
        {
            _view = GetComponent<AppControlsView>();
 
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _view.minimizeApplication.gameObject.SetActive(true);
            _view.minimizeApplication.onClick.AddListener(OnMinimizeButtonClick);
#else
            _view.minimizeApplication.gameObject.SetActive(false);
#endif
            _view.resizeApplication.onClick.AddListener(UpdateScreenMode);
            _view.quitApplication.onClick.AddListener(() =>
                GameManager.Instance.warningController.ShowExitWarning(null, Application.Quit));
            
            _view.newsButton.gameObject.SetActive(false);
            _view.newsButton.onClick.AddListener(GameManager.Instance.newsController.Init);
            
        }

        public void Init()
        {
            _view.newsButton.gameObject.SetActive(true);
        }

        public void Show()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) return;
            
            GameManager.Instance.warningController.SetTxts();
            _view.canvas.SetActive(true);
        }

        private void Update()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) return;

            if ((Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && Input.GetKey(KeyCode.Return) ||
               (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))
            {
                UpdateScreenMode();
            }
        }

        private void OnMinimizeButtonClick()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            ShowWindow(GetActiveWindow(), 2);
#endif
        }

        private void UpdateScreenMode()
        {
            var settings = GameManager.Instance.settingsController;
            var resolutions = settings.screenResolutions;
            if(resolutions == null) return;
            
            if (Screen.fullScreen)
            {
                PlayerPrefs.SetInt("RESOLUTION_INDEX", settings.GetResolutionIndex());
                PlayerPrefs.Save();
                
                settings.SelectResolutionInDropdown(resolutions.Count / 2);
                Screen.fullScreen = false;
            }
            else
            {
                var prevResolution = PlayerPrefs.HasKey("RESOLUTION_INDEX")
                    ? PlayerPrefs.GetInt("RESOLUTION_INDEX")
                    : resolutions.Count - 1;
                
                settings.SelectResolutionInDropdown(prevResolution);
                Screen.fullScreen = true;
            }
        }
    }
}
