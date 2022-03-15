using System;
using System.Collections.Generic;
using Modules.WDCore;
using Modules.SpeechKit;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.Scenario
{
    public class PhysicalExamView : MonoBehaviour
    {
        public GameObject root;
        public Toggle tglMute;
        public GameObject tglOffObj;
        public Button backButton;
        public Button backButtonPointSelector;
        public Button normaButton;
        public Button pathologyButton;
        public List<TextMeshProUGUI> simpleTxts;
        public GameObject contactPointGo;
        public GameObject contactPointCanvas;
        
        [SerializeField] private TextMeshProUGUI contactPointName;
        [SerializeField] private GameObject audioRoot;
        [SerializeField] private GameObject imageRoot;
        [SerializeField] private RawImage imagePlaceholder;

        [Header("Point Selector")]
        public GameObject pointSelectorRoot;
        [SerializeField] private List<TxtButton> pointButtons;

        private void Awake()
        {
            if (XRSettings.enabled)
            {
                contactPointCanvas = new GameObject("PointNameCanvas", typeof(Canvas));
                contactPointCanvas.transform.SetParent(GameManager.Instance.transform);
                contactPointCanvas.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                contactPointCanvas.transform.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f);
                contactPointCanvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                contactPointCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920.0f, 1080.0f);
                contactPointGo.transform.SetParent(contactPointCanvas.transform);
                contactPointGo.transform.position = Vector3.zero;
                contactPointGo.transform.localScale = Vector3.one;
                contactPointCanvas.SetActive(false);
                contactPointGo.SetActive(true);
            }
        }

        public void SetActivePanel(bool val)
        {
            root.SetActive(val);
            Starter.Cursor.ActivateCursor(val);
        }

        public void SetTitle(string val)
        {
            simpleTxts[1].text = val;
        }
        
        public void SetDescription(string val)
        {
            simpleTxts[0].text = val;
        }

        public void SetContactPointName(string val)
        {
            contactPointName.text = val;
            
            if (XRSettings.enabled)
                contactPointCanvas.SetActive(!string.IsNullOrEmpty(val));
            else
                contactPointGo.SetActive(!string.IsNullOrEmpty(val));
        }

        public void HideContactPoint()
        {
            if (XRSettings.enabled)
                contactPointCanvas.SetActive(false);
            else
                contactPointGo.SetActive(false);
        }

        public void HideMedia()
        {
            audioRoot.SetActive(false);
            imageRoot.SetActive(false);
        }

        public void ShowAudioMedia()
        {
            audioRoot.SetActive(true);
            tglMute.isOn = true;
        }

        public void ShowImageMedia(Texture2D val)
        {
            imagePlaceholder.texture = val;
            RectTransform rt = (RectTransform)imageRoot.transform;
            float referenceLength = rt.rect.width;
            imagePlaceholder.rectTransform.sizeDelta = val.width > val.height ? 
                new Vector2(referenceLength, referenceLength*(float) val.height / val.width) : 
                new Vector2(referenceLength*(float) val.height / val.width, referenceLength);
            imageRoot.SetActive(true);
        }
        
        public void SetPointButtons(List<UnityAction> calls, List<string> texts)
        {
            CheckButtons(calls.Count);
        
            for (var i = 0; i < calls.Count; ++i)
            {
                pointButtons[i].tmpText.text = texts[i];
                pointButtons[i].button.onClick.RemoveAllListeners();
                pointButtons[i].button.onClick.AddListener(calls[i]);
                pointButtons[i].gameObject.SetActive(true);
            }
        }
        
        private void CheckButtons(int requiredSize)
        {
            var currentSize = pointButtons.Count;
            if (requiredSize > currentSize)
            {
                var parent = pointButtons[0].transform.parent;
                var obj = pointButtons[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    pointButtons.Add(Instantiate(obj, parent).GetComponent<TxtButton>());
                }
            }
        
            foreach (var txtButton in pointButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        }

        public void ShowPointSelector(bool val)
        {
            pointSelectorRoot.SetActive(val);
            Starter.Cursor.ActivateCursor(val);
        }
        
    }
}
