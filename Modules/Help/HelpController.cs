using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.Help
{
    public class HelpController : MonoBehaviour
    {
        private HelpView _view;
        private bool _isInit;

        private void Awake()
        {
            _view = GetComponent<HelpView>();
        }

        public void Init()
        {
            _isInit = true;
        }

        public void Show()
        {
            _view.canvas.SetActive(!_view.canvas.activeSelf);
        }

        private void Update()
        {
            if(_isInit || InputCheck()) return;

            if (Input.GetKeyDown(KeyCode.H))
                Show();
        }
        
        private bool InputCheck()
        {
            var selectedObj = EventSystem.current.currentSelectedGameObject;
            return selectedObj != null && 
                   (selectedObj.TryGetComponent(out TMP_InputField _) || selectedObj.TryGetComponent(out InputField _));
        }
    }
}
