using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cursor = Modules.Starter.Cursor;

namespace Modules.Warning
{
    public class WarningView : MonoBehaviour
    {
        public GameObject canvas;
        
        [Header("Simple warning")]
        public GameObject simpleWarningRoot;
        public TextMeshProUGUI text;
        
        [Header("Exit warning")]
        public GameObject exitWarningRoot;
        public TextMeshProUGUI exitText;
        public TxtButton rejectButton;
        public TxtButton confirmButton;

        private void Awake()
        {
            canvas.SetActive(false);
        }

        public void SetLabel(string txt)
        {
            text.text = txt;
        }

        public void SetTxts(string[] vals)
        {
            confirmButton.tmpText.text = vals[4];
            rejectButton.tmpText.text = vals[5];
        }

        public void SetExitWarning(string txt, UnityAction confirmCall, UnityAction rejectCall)
        {
            exitText.text = txt;
            confirmButton.button.onClick.RemoveAllListeners();
            confirmButton.button.onClick.AddListener(confirmCall);
            
            rejectButton.button.onClick.RemoveAllListeners();
            rejectButton.button.onClick.AddListener(() => SetActivePanel(false, true));
            if(rejectCall != null)
                rejectButton.button.onClick.AddListener(rejectCall);
        }

        public void SetActivePanel(bool val, bool isExit)
        {
            simpleWarningRoot.SetActive(!isExit);
            exitWarningRoot.SetActive(isExit);
            
            canvas.SetActive(val);
            Cursor.ActivateCursor(val);
        }

        public void SetOfflineButtonsText()
        {
            rejectButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "No";
            confirmButton.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
        }
    }
}
