using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Modules.Loading
{
    public class LoadingController : MonoBehaviour
    {
        private LoadingView _loadingView;
        private Dictionary<GameManager.Product, LoadingView> _viewByProduct;
        private string _currentHint;
        private void Awake()
        {
            _viewByProduct = GetComponents<LoadingView>().ToDictionary(x => x.product);
            GameManager.Instance.onProductChange += p =>
            {
                _viewByProduct.TryGetValue(p, out _loadingView);
                if (_loadingView == null) _viewByProduct.TryGetValue(GameManager.Product.Academix, out _loadingView);
            };
#if !UNITY_XR
            foreach (var view in _viewByProduct.Values)
            {
                view.cancelButton.onClick.AddListener(() => StartCoroutine(GameManager.Instance.addressablesS3.CancelDownload()));
            }  
#endif
        }

        public void Init(string val, bool isCancel = false)
        {
            // if (XRSettings.enabled)
            //     GameManager.Instance.starterController.GetFPСVR().Init(_loadingView.playerStart);

            if (string.IsNullOrEmpty(_currentHint))
                SetHint();
            
            _loadingView.SetText(val);
            _loadingView.SetProgress("");
            _loadingView.SetActivePanel(true);
#if !UNITY_XR
            _loadingView.cancelButton.gameObject.SetActive(isCancel);
#endif

            GameManager.Instance.mainMenuController.ShowMenu(false);
            GameManager.Instance.mainMenuController.isBlocked = true;
        }

        public void SetProgress(float val, long size)
        {
            var progressTxt = "";
            if (size != 0)
            {
                var totalSize = Math.Round(size / 1e6, 1);
                var currSize = Math.Round(val * totalSize, 1);
                var perc = Math.Round(val * 100.0f, 0);
                progressTxt = $"{currSize} / {totalSize} {TextData.Get(9)} ({perc}%)";
            }
            
            _loadingView.SetProgress(progressTxt);
        }

        public void Hide()
        {
            _loadingView.SetActivePanel(false);
            
            // if (XRSettings.enabled)
            //     GameManager.Instance.starterController.GetFPСVR().Init(GameObject.Find("PlayerStart"));
            
            GameManager.Instance.mainMenuController.isBlocked = false;
            _currentHint = null;
        }

        public void SetProgressCount(int val, int size)
        {
            var progressTxt = "";

            if (size != 0)
            {
                progressTxt = $"{val} / {size}";
            }

            _loadingView.SetProgress(progressTxt);
        }

        private void SetHint()
        {
            if (BookDatabase.Instance.MedicalBook == null || BookDatabase.Instance.MedicalBook.messages == null)
            {
                _loadingView.SetHint(null);
                return;
            }
            
            var msgs = BookDatabase.Instance.MedicalBook.messages;
            var hint = msgs[Random.Range(0, msgs.Count)];
            _currentHint = hint.name;
            _loadingView.SetHint(_currentHint);
        }
    }
}
