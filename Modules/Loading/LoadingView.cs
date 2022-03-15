using TMPro;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.Loading
{
    public class LoadingView : MonoBehaviour
    {
        public GameManager.Product product;
        [SerializeField] private GameObject canvas;
        [SerializeField] private GameObject blackbox;
        [SerializeField] private TextMeshProUGUI loadingTxt;
        [SerializeField] private TextMeshProUGUI progressTxt;
        [SerializeField] private TextMeshProUGUI hintTxt;
        public Button cancelButton;
        public GameObject playerStart;

        private void Awake()
        {
            loadingTxt.text = "";
            SetActivePanel(false);
        }

        public void SetText(string val)
        {
            if(!string.IsNullOrEmpty(val))
                loadingTxt.text = val;
        }
        
        public void SetHint(string val)
        {
            hintTxt.text = val;
            hintTxt.gameObject.SetActive(!string.IsNullOrEmpty(val));
        }

        public void SetProgress(string val)
        {
            progressTxt.text = val;
            progressTxt.gameObject.SetActive(!string.IsNullOrEmpty(val));
        }
        
        public void SetActivePanel(bool val)
        {
            if(val && !canvas.activeSelf)
                canvas.SetActive(true);
            else if(!val && canvas.activeSelf)
                canvas.SetActive(false);
            
            if(XRSettings.enabled)
                blackbox.SetActive(val);
        }
    }
}
