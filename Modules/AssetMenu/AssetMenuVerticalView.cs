using System.Collections.Generic;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cursor = Modules.Starter.Cursor;

namespace Modules.AssetMenu
{
    public class AssetMenuVerticalView : MonoBehaviour
    {
        public GameManager.Product product;
        public GameObject canvas;
        public GameObject root;
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private List<TxtButton> txtButtons;
        public Button closeButton;
        public Button closeArea;

        private void Awake()
        {
            canvas.SetActive(false);
            root.SetActive(false);
            
        }
        
        public void SetTitle(string val)
        {
            title.text = val;
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            root.SetActive(val);
            if(!val)
                Cursor.ActivateCursor(false);
        }
        
        public void SetValues(List<string> texts, List<UnityAction> calls)
        {
            CheckButtons(texts.Count);
        
            for (var i = 0; i < texts.Count; ++i)
            {
                txtButtons[i].tmpText.text = texts[i];
                txtButtons[i].button.onClick.RemoveAllListeners();
                txtButtons[i].button.onClick.AddListener(calls[i]);
                txtButtons[i].gameObject.SetActive(true);
            }
        }
        
        private void CheckButtons(int requiredSize)
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
    }
}
