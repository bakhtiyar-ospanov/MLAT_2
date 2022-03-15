using Modules.WDCore;
using UnityEngine;
using UnityEngine.XR;

namespace Modules.SpeechKit
{
    public class SubtitleController : MonoBehaviour
    {
        private SubtitleView _subtitleView;
        private Transform _vrCamera;

        private void Awake()
        {
            _subtitleView = GetComponent<SubtitleView>();
            _subtitleView.skipButton.onClick.AddListener(TextToSpeech.Instance.StopSpeaking);
            
            if(XRSettings.enabled)
                _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
        }
        

        public void Init(string text)
        {
            if (XRSettings.enabled) return;
            _subtitleView.textTMP.text = text.Replace("+", "");
            SetActivePanel(true);
            
            // if (XRSettings.enabled)
            //     MoveCanvas();
        }

        public void SetActivePanel(bool val)
        {
            _subtitleView.canvas.SetActive(val);
        }
        
        private void MoveCanvas()
        {
            if(!XRSettings.enabled) return;
            
            var canvasTransform = _subtitleView.canvas.transform;
            canvasTransform.position = _vrCamera.position + _vrCamera.forward * 0.8f - new Vector3(0.0f, 0.7f, 0.0f);
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - _vrCamera.position);
        }
    }
}
