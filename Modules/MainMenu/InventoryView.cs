using System.Collections.Generic;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.MainMenu
{
    public class InventoryView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject root;
        [SerializeField] private List<TxtButton> txtButtons;
        [SerializeField] private GameObject emptySign;
        private void Awake()
        {
            canvas.SetActive(false);
            emptySign.SetActive(true);
        }

        public void SetValues(List<string> texts, List<UnityAction> calls)
        {
            CheckButtons(texts.Count);
            emptySign.SetActive(texts.Count == 0);
        
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
