using System;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Modules.Statistics
{
    public class StatisticsView : MonoBehaviour
    {
        public GameObject canvas;
        public TMP_InputField searchField;
        public List<TMP_Dropdown> filters;
        public Button closeButton;
        [SerializeField] private List<TxtButton> itemButtons;
        
        [Header("Details Panel")]
        public GameObject detailsRoot;
        public TMP_InputField detailsSearchField;
        [SerializeField] private List<TxtButton> detailsButtons;
        [SerializeField] private TextMeshProUGUI detailsTitle;
        [SerializeField] private TextMeshProUGUI detailsPatientInfo;
        public Button closeDetailsButton;

        [Header("Graph")] 
        public UILineRenderer lineRenderer;
        public RectTransform area;
        //[SerializeField] private List<TextMeshProUGUI> dateTxts;
        [SerializeField] private List<TextMeshProUGUI> scoreTxts;

        private void Awake()
        {
            canvas.SetActive(false);
            detailsRoot.SetActive(false);
        }

        public void SetItemValues(List<string> texts, List<string> extraTexts, List<UnityAction> calls)
        {
            CheckButtons(texts.Count, itemButtons);
        
            for (var i = 0; i < texts.Count; ++i)
            {
                itemButtons[i].tmpText.text = texts[i];
                itemButtons[i].extraTmpTexts[0].text = extraTexts[i];
                itemButtons[i].button.onClick.RemoveAllListeners();
                itemButtons[i].button.onClick.AddListener(calls[i]);
                itemButtons[i].gameObject.SetActive(true);
            }
        }
        
        public void SetDetailsValues(List<string> texts, Dictionary<string, UnityAction> calls)
        {
            CheckButtons(texts.Count, detailsButtons);
        
            for (var i = 0; i < texts.Count; ++i)
            {
                detailsButtons[i].tmpText.text = texts[i];
                detailsButtons[i].button.onClick.RemoveAllListeners();
                detailsButtons[i].transform.GetChild(1).gameObject.SetActive(false);

                if (calls.Keys.Contains(texts[i]))
                {
                    detailsButtons[i].button.onClick.AddListener(calls[texts[i]]);
                    detailsButtons[i].transform.GetChild(1).gameObject.SetActive(true);
                }                  

                detailsButtons[i].gameObject.SetActive(true);
            }
        }
        
        private void CheckButtons(int requiredSize, List<TxtButton> txtButtons)
        {
            var currentSize = txtButtons.Count;
            if (requiredSize > currentSize)
            {
                var parent = txtButtons[0].transform.parent;
                var obj = txtButtons[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    txtButtons.Add(Instantiate(obj, parent).GetComponent<TxtButton>());
                }
            }
        
            foreach (var txtButton in txtButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        }

        public void SetDetailsTitle(string val, string info)
        {
            detailsTitle.text = val;
            detailsPatientInfo.text = info;
        }

        public void DrawGraph(List<string> dates, List<double> scores)
        {
            dates.Reverse();
            scores.Reverse();

            //CheckDateTxts(dates.Count, dateTxts);
            CheckDateTxts(6, scoreTxts);
            
            var fixedScoreInterval = area.sizeDelta.y / 5;
            for (var i = 0; i < 6; i++)
            {
                scoreTxts[i].text = i * 20 + "";
                scoreTxts[i].rectTransform.anchoredPosition = new Vector2(-15.0f, fixedScoreInterval * i);
                scoreTxts[i].gameObject.SetActive(true);
            }
            
            if (dates.Count == 0)
            {
                lineRenderer.Points = new Vector2[1];
                lineRenderer.Points[0] = new Vector2(0.0f, 0.0f);
                return;
            }
            
            var intervalX = area.sizeDelta.x / (dates.Count == 1 ? 1 : (dates.Count-1));
            var intervalY = area.sizeDelta.y / 100;

            lineRenderer.Points = new Vector2[dates.Count];

            for (var i = 0; i < dates.Count; i++)
            {
                lineRenderer.Points[i] = new Vector2(intervalX * i, intervalY * (float)scores[i]);
                // dateTxts[i].text = dates[i].Substring(0, dates[i].Length - 3);
                // dateTxts[i].rectTransform.anchoredPosition = new Vector2(intervalX * i, -25.0f);
                // dateTxts[i].gameObject.SetActive(true);
            }
        }

        private void CheckDateTxts(int requiredSize, List<TextMeshProUGUI> txtButtons)
        {
            var currentSize = txtButtons.Count;
            if (requiredSize > currentSize)
            {
                var parent = txtButtons[0].transform.parent;
                var obj = txtButtons[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    txtButtons.Add(Instantiate(obj, parent).GetComponent<TextMeshProUGUI>());
                }
            }
        
            foreach (var txtButton in txtButtons)
            {
                txtButton.gameObject.SetActive(false);
            }
        }
    }
}
