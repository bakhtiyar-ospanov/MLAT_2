using System;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.MainMenu
{
    public class MainMenuController : MonoBehaviour
    {
        public Action<bool> onMenuShow;
        public bool isBlocked = true;
        public bool isInit;
        
        private MainMenuView _view;
        private Dictionary<string, Transform[]> _moduleTrans = new Dictionary<string, Transform[]>();
        private int _side = 0;
        private Transform _vrCamera;

        private void Awake()
        {
            _view = GetComponent<MainMenuView>();
            _view.outFieldButton.onClick.AddListener(() => ShowMenu(false));
            _moduleTrans.Add(_view.GetInstanceID() + "", new []{_view.root.transform});
            _view.onMenuShow += val => onMenuShow?.Invoke(val);

            if(XRSettings.enabled)
                _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
        }

        public void Init()
        {
            isInit = true;
            isBlocked = false;
        }

        public void AddModule(string id, string txt, UnityAction<bool> call, Transform[] trans)
        {
            _view.AddModule(id, txt, call);
            
            if(!_moduleTrans.ContainsKey(id))
                _moduleTrans.Add(id, trans);

            ShiftModule(trans);
        }

        public bool CheckModule(string id)
        {
            return _view.CheckModule(id);
        }

        public void AddPopUpModule(string id, UnityAction<bool> call, Transform[] trans)
        {
            _view.AddPopUpModule(id, call);
            
            if(!_moduleTrans.ContainsKey(id))
                _moduleTrans.Add(id, trans);

            ShiftModule(trans);
        }

        public void RemoveModule(string id)
        {
            _view.RemoveModule(id);

            if(_moduleTrans.ContainsKey(id))
                _moduleTrans.Remove(id);
        }

        public void RemovePopUpModule(string id)
        {
            _view.RemovePopUpModule(id);

            if(_moduleTrans.ContainsKey(id))
                _moduleTrans.Remove(id);
        }

        private void Update()
        {
            if(!isInit || isBlocked || InputCheck()) return;

            if (GameManager.Instance.warningController.GetCanvasState()) return;
            if (GameManager.Instance.physicalExamController.GetAnswerCheckState()) return;

            if (Input.GetKeyDown(KeyCode.Q))
                ShowMenu();

#if UNITY_XR
            if (XRSettings.enabled && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch))
                ShowMenu();
#endif
        }
        
        public void ShowMenu()
        {
            if(_view == null) return;
            
            if(XRSettings.enabled)
                MoveCanvas();
            
            _view.ShowMenu();
        }

        public void ShowMenu(string moduleId)
        {
            if(_view == null) return;
            GameManager.Instance.assetMenuController.SetActivePanel(false);
            
            if(XRSettings.enabled)
                MoveCanvas();
            
            _view.ShowMenu(moduleId);
        }

        public void ShowMenu(bool val)
        {
            if(_view == null) return;
            if(val)
                GameManager.Instance.assetMenuController.SetActivePanel(false);
            
            if(XRSettings.enabled && val)
                MoveCanvas();
            
            _view.ShowMenu(val);
        }
        
        public void ShiftMenu(int side)
        {
            _side = side;
            foreach (var moduleTran in _moduleTrans)
                ShiftModule(moduleTran.Value);
        }

        private void ShiftModule(Transform[] trans)
        {
            if(XRSettings.enabled) return;

            var leftShift = 400.0f;
            var isTouch = Input.touchSupported;
            
            if(!isTouch)
                leftShift = 480.0f;
            
            foreach (RectTransform tran in trans)
            {
                tran.anchorMax = _side switch
                {
                    0 => new Vector2(0.5f, 0.5f),
                    1 => new Vector2(0.0f, isTouch ? 1.0f : 0.5f),
                    2 => new Vector2(1.0f, 0.5f),
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                tran.anchorMin = _side switch
                {
                    0 => new Vector2(0.5f, 0.5f),
                    1 => new Vector2(0.0f, isTouch ? 1.0f : 0.5f),
                    2 => new Vector2(1.0f, 0.5f),
                    _ => throw new ArgumentOutOfRangeException()
                };

                var pos = tran.anchoredPosition;
                tran.anchoredPosition = _side switch
                {
                    0 => new Vector3(0.0f, 0.0f),
                    1 => new Vector3(leftShift, isTouch ? -510.0f : pos.y),
                    2 => new Vector3(1470.0f, pos.y),
                    _ => pos
                };
            }
        }

        public void ActivateOutField(bool val)
        {
            _view.ActivateOutField(val);
        }
        
        private void MoveCanvas()
        {
            if(_view == null || _view.canvas.activeSelf) return;
            
            var canvasTransform = transform.parent;
            canvasTransform.position = _vrCamera.position + _vrCamera.forward * 0.6f - new Vector3(0.0f, 0.15f, 0.0f);
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - _vrCamera.position);
        }
        
        private bool InputCheck()
        {
            var selectedObj = EventSystem.current.currentSelectedGameObject;
            return selectedObj != null && 
                   (selectedObj.TryGetComponent(out TMP_InputField _) || selectedObj.TryGetComponent(out InputField _));
        }
    }
}
