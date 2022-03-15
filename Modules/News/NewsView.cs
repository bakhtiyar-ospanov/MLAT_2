using System.Collections.Generic;
using System.Globalization;
using Modules.WDCore;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.News
{
    public class NewsView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject loadingWarning;
        public List<TxtButton> itemButtons;
        
        private void Awake()
        {
            canvas.gameObject.SetActive(false);
        }
        
        public void SetItemValues(List<TitleNewsItem> news, List<UnityAction> calls)
        {
            CheckButtons(news.Count, itemButtons);
        
            for (var i = 0; i < news.Count; ++i)
            {
                itemButtons[i].tmpText.text = news[i].Title;
                itemButtons[i].extraTmpTexts[0].text = news[i].Body;
                itemButtons[i].extraTmpTexts[1].text = news[i].Timestamp.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                itemButtons[i].button.onClick.RemoveAllListeners();
                itemButtons[i].button.onClick.AddListener(calls[i]);
                itemButtons[i].gameObject.SetActive(true);
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

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
        }
    }
}
