using System;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cursor = Modules.Starter.Cursor;

namespace Modules.AppUpdate
{
    public class AppUpdateView : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;
        
        [Header("Prompt")]
        public GameObject prompt;
        public TextMeshProUGUI titleTxt;
        public TxtButton confirmUpdate;
        public TxtButton rejectUpdate;
        
        [Header("Progress Panel")] 
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;
        public TxtButton cancelDownload;
        public Button secretButton;

        private string downloadTxt;
        private string mbText;

        private void Awake()
        {
            canvas.SetActive(false);
            prompt.SetActive(false);
            progressPanel.SetActive(false);
        }

        public void SetTxt(string[] txt)
        {
            titleTxt.text = txt[3];
            confirmUpdate.tmpText.text = txt[4];
            rejectUpdate.tmpText.text = txt[5];
            cancelDownload.tmpText.text = txt[6];
            downloadTxt = txt[7];
            mbText = txt[8];
        }

        public void ShowPrompt(bool val)
        {
            prompt.SetActive(val);
            canvas.SetActive(val);
            Cursor.ActivateCursor(val);
        }

        public void ShowProgressPanel(bool val)
        {
            SetProgress(0, 1);
            progressPanel.SetActive(val);
            canvas.SetActive(val);
            Cursor.ActivateCursor(val);
        }

        public void SetProgress(long currentVal, long totalVal)
        {
            var frac = (float) currentVal / totalVal;
            progressText.text = downloadTxt + " : " + 
                                (int) Math.Round(currentVal / 1000000.0f) + " / " + 
                                (int) Math.Round(totalVal / 1000000.0f) + " " + 
                                mbText +
                                " (" + (int) Math.Round(frac * 100) + "%" + ")";
            progressBar.value = (float) Math.Round(frac, 2);
        }

    }
}
