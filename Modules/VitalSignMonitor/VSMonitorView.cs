using System;
using Modules.Scenario;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.VitalSignMonitor
{
    public class VSMonitorView : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public GameObject canvas;
        public TextMeshProUGUI pulseTxt;
        public TextMeshProUGUI saturationTxt;
        public TextMeshProUGUI respiratoryTxt;
        public TextMeshProUGUI temperatureTxt;
        public TextMeshProUGUI pressureTxt;
        public Button scaleButton;
        public Button closeButton;
        public GameObject[] scaleIcons;
        public RawImage monitorImg;
        public RectTransform monitorRect;
        public Animator[] animators;
        private float _scaleFactor;
        private bool _isScaled;
        private Vector3 _initialPos;

        private void Awake()
        {
            canvas.SetActive(false);
            _initialPos = monitorRect.anchoredPosition;
            scaleButton.onClick.AddListener(ScalePanel);
            closeButton.onClick.AddListener(() => SetActivePanel(false));
            scaleIcons[0].SetActive(true);
            scaleIcons[1].SetActive(false);
        }

        public void SetPulse(int pulse)
        {
            pulseTxt.text = pulse.ToString();
        }
        
        public void SetSaturation(int saturation)
        {
            saturationTxt.text = saturation.ToString();
        }
        
        public void SetBreath(int breath)
        {
            respiratoryTxt.text = breath.ToString();
        }
        
        public void SetTemperature(string temperature)
        {
            temperatureTxt.text = temperature;
        }
        
        public void SetPressure(string pressure)
        {
            pressureTxt.text = pressure;
        }

        public void SetActivePanel(bool val)
        {
            monitorRect.anchoredPosition = _initialPos;
            if(_isScaled)
                ScalePanel();
            
            canvas.SetActive(val);
        }

        public void Show()
        {
            SetActivePanel(!canvas.activeSelf);
        }

        public void SetGraph(Texture2D texture2D)
        {
            monitorImg.texture = texture2D;
        }

        public void OnDrag(PointerEventData eventData)
        {
            monitorRect.anchoredPosition += eventData.delta * _scaleFactor;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _scaleFactor = GameManager.Instance.settingsController.GetCanvasScaleFactor();
        }

        private void ScalePanel()
        {
            monitorRect.localScale = _isScaled ? new Vector3(1.0f, 1.0f, 1.0f) : new Vector3(4.0f, 4.0f, 1.0f);
            scaleIcons[0].SetActive(_isScaled);
            scaleIcons[1].SetActive(!_isScaled);

            if (!_isScaled)
            {
                monitorRect.anchorMin = new Vector2(0.5f, 0.5f);
                monitorRect.anchorMax = new Vector2(0.5f, 0.5f);
                monitorRect.anchoredPosition = new Vector2(0.0f, 0.0f);
            }
            else
            {
                monitorRect.anchorMin = new Vector2(1.0f, 1.0f);
                monitorRect.anchorMax = new Vector2(1.0f, 1.0f);
                monitorRect.anchoredPosition = _initialPos;
            }
            
            _isScaled = !_isScaled;
        }

        public void SetPulseSpeed(float val)
        {
            foreach (var anim in animators)
                anim.speed = val;
        }

        public void ActivatePulsating(bool val)
        {
            foreach (var anim in animators)
            {
                if(val)
                    anim.Play("Pulsating");
                else
                    anim.StopPlayback();
            }
        }
    }
}
