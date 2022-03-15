using System;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Cursor = Modules.Starter.Cursor;

namespace Modules.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject root;
        public Button outFieldButton;
        public TxtButton moduleButtonPrefab;
        public Transform headerContainer;
        private readonly Dictionary<string, (TxtButton, UnityAction<bool>)> moduleButtonById 
            = new Dictionary<string, (TxtButton, UnityAction<bool>)>();
        private readonly Dictionary<string, UnityAction<bool>> popupModuleById =
            new Dictionary<string, UnityAction<bool>>();
        private string _lastActiveId = "Profile";
        public Action<bool> onMenuShow;
        
        private void Awake()
        {
            canvas.SetActive(false);
        }
        
        public void AddModule(string id, string txt, UnityAction<bool> call)
        {
            if(moduleButtonById.ContainsKey(id)) return;
            var newButton = Instantiate(moduleButtonPrefab, headerContainer);
            newButton.transform.SetAsFirstSibling();
            newButton.tmpText.text = txt;
            newButton.button.onClick.AddListener(() => ShowModule(id));
            moduleButtonById.Add(id, (newButton, call));
            _lastActiveId = id;
        }

        public bool CheckModule(string id)
        {
            return moduleButtonById.ContainsKey(id);
        }

        public void AddPopUpModule(string id, UnityAction<bool> call)
        {
            if(popupModuleById.ContainsKey(id)) return;
            popupModuleById.Add(id, call);
            _lastActiveId = id;
        }

        public void RemoveModule(string id)
        {
            moduleButtonById.TryGetValue(id, out var moduleButton);
            if(moduleButton.Item1 == null) return;
            DestroyImmediate(moduleButton.Item1.gameObject);
            moduleButtonById.Remove(id);
            if(_lastActiveId == id)
                _lastActiveId = moduleButtonById.Select(x => x.Key).ToList()[moduleButtonById.Count-1];
        }
        
        public void RemovePopUpModule(string id)
        {
            popupModuleById.TryGetValue(id, out var moduleButton);
            if(moduleButton == null) return;
            popupModuleById.Remove(id);
            if(_lastActiveId == id) 
                _lastActiveId = moduleButtonById.Select(x => x.Key).ToList()[moduleButtonById.Count-1];
        }

        private void ShowModule(string id)
        {
            foreach (var valueTuple in moduleButtonById)
            {
                valueTuple.Value.Item2?.Invoke(false);
                var col = valueTuple.Value.Item1.tmpText.color;
                valueTuple.Value.Item1.tmpText.color = new Color(col.r, col.g, col.b, 0.5f);
            }

            foreach (var popUpModule in popupModuleById)
                popUpModule.Value?.Invoke(false);

            if (moduleButtonById.ContainsKey(id))
            {
                headerContainer.parent.gameObject.SetActive(true);
                moduleButtonById.TryGetValue(id, out var module);

                module.Item2?.Invoke(true);
                var selectedCol = module.Item1.tmpText.color;
                module.Item1.tmpText.color = new Color(selectedCol.r, selectedCol.g, selectedCol.b, 1.0f);
                _lastActiveId = id;
            } else if (popupModuleById.ContainsKey(id))
            {
                headerContainer.parent.gameObject.SetActive(false);
                popupModuleById.TryGetValue(id, out var module);
                module?.Invoke(true);
                _lastActiveId = id;
            }
            else
            {
                ShowModule("Settings");
            }
        }

        public void ShowMenu()
        {
            var val = !canvas.activeSelf;
            canvas.SetActive(val);
            if (val)
            {
                GameManager.Instance.assetMenuController.SetActivePanel(false);
                ShowModule(_lastActiveId);
            }
            else
            {
                foreach (var valueTuple in moduleButtonById)
                    valueTuple.Value.Item2?.Invoke(false);
                
                foreach (var popUpModule in popupModuleById)
                    popUpModule.Value?.Invoke(false);
            }
            onMenuShow?.Invoke(val);
            Cursor.ActivateCursor(val);
        }
        public void ShowMenu(string moduleId)
        {
            if(string.IsNullOrEmpty(moduleId)) return;

            canvas.SetActive(true);
            onMenuShow?.Invoke(true);
            ShowModule(moduleId);
            Cursor.ActivateCursor(true);
        }
        
        public void ShowMenu(bool val)
        {
            canvas.SetActive(val);
            if (val)
            {
                ShowModule(_lastActiveId);
            }
            else
            {
                foreach (var valueTuple in moduleButtonById)
                    valueTuple.Value.Item2?.Invoke(false);
                
                foreach (var popUpModule in popupModuleById)
                    popUpModule.Value?.Invoke(false);
            }
            onMenuShow?.Invoke(val);
            Cursor.ActivateCursor(val);
        }

        public void ActivateOutField(bool val)
        {
            outFieldButton.gameObject.SetActive(val);
        }
    }
}
