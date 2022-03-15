using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Warning
{
    public class WarningController : MonoBehaviour
    {        
        private WarningView _view;
        private string exitAppTxt;
        private Dictionary<string, string[]> langs;

        private void Awake()
        {
            _view = GetComponent<WarningView>();
        }

        public void ShowWarning(string txt)
        {
            _view.SetLabel(txt);
            _view.SetActivePanel(true, false);
        }

        public void SetTxts()
        {
            langs = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(
                BookDatabase.Instance.Configurations["Langs"]);
            _view.SetTxts(langs[Language.Code]);
            exitAppTxt = langs[Language.Code][9];
        }
        
        public void ShowExitWarning(string txt, UnityAction confirmCall, bool isOneButton = false, UnityAction rejectCall = null, string btnTxt = null)
        {
            if (XRSettings.enabled)
                MoveCanvas();

            if (string.IsNullOrEmpty(txt))
                txt = exitAppTxt;
            
            _view.SetExitWarning(txt, confirmCall, rejectCall);
            _view.SetActivePanel(true, true);
            _view.rejectButton.gameObject.SetActive(!isOneButton);

            if (string.IsNullOrEmpty(btnTxt) && langs != null)
            {
                _view.SetTxts(langs[Language.Code]);
            }
            else
            {
                _view.confirmButton.tmpText.text = btnTxt;
            }
               
        }

        private void MoveCanvas()
        {
            var vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
            
            var canvasTransform = _view.canvas.transform;
            canvasTransform.position = vrCamera.position + vrCamera.forward * 0.8f;
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - vrCamera.position);
        }

        public bool GetCanvasState()
        {
            return _view.canvas.activeSelf;
        }

        public void SetOfflineButtonsText()
        {
            _view.SetOfflineButtonsText();
        }

        public void HideWarningView()
        {
            _view.rejectButton.button.onClick.Invoke();
        }

        public IEnumerator StartShutdownTimer(float time)
        {
            var buttonText = _view.confirmButton.transform.GetComponentInChildren<TextMeshProUGUI>();
            var countdown = time;
            while (countdown > 0)
            {
                buttonText.text = $"{TextData.Get(187)} ({Mathf.RoundToInt(countdown)})";
                yield return new WaitForSeconds(1.0f);
                countdown -= 1;
            }
            Application.Quit();
            yield break;
        }
    }
}
