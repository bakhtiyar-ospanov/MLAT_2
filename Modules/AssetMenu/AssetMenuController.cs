using System.Collections.Generic;
using System.Linq;
using Modules.Assets;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using Cursor = Modules.Starter.Cursor;

namespace Modules.AssetMenu
{
    public class AssetMenuController : MonoBehaviour
    {
        public enum AssetMenuType
        {
            Vertical,
            Radial
        }

        private Dictionary<GameManager.Product, AssetMenuRadialView> _radialViewByProduct;
        private Dictionary<GameManager.Product, AssetMenuVerticalView> _verticalViewByProduct;

        public AssetMenuType menuType;
        private AssetMenuVerticalView _assetMenuVerticalView;
        private AssetMenuRadialView _assetMenuRadialView;
        private FixedJoystick _joystick;
        private Transform _vrCamera;

        private void Awake()
        {
            _radialViewByProduct = GetComponents<AssetMenuRadialView>().ToDictionary(x => x.product);
            _verticalViewByProduct = GetComponents<AssetMenuVerticalView>().ToDictionary(x => x.product);
            GameManager.Instance.onProductChange += p =>
            {
                _radialViewByProduct.TryGetValue(p, out _assetMenuRadialView);
                _verticalViewByProduct.TryGetValue(p, out _assetMenuVerticalView);
                
                if(_assetMenuRadialView == null) _radialViewByProduct.TryGetValue(GameManager.Product.Academix, out _assetMenuRadialView);
                if(_assetMenuVerticalView == null) _verticalViewByProduct.TryGetValue(GameManager.Product.Academix, out _assetMenuVerticalView);

                if (_assetMenuRadialView == null) _radialViewByProduct.TryGetValue(GameManager.Product.Academix, out _assetMenuRadialView);
                if (_assetMenuVerticalView == null) _verticalViewByProduct.TryGetValue(GameManager.Product.Academix, out _assetMenuVerticalView);
            };

            /*_assetMenuVerticalView = GetComponent<AssetMenuVerticalView>();
            _assetMenuRadialView = GetComponent<AssetMenuRadialView>();*/

            foreach (var view in _radialViewByProduct.Values)
            {
                view.closeArea.onClick.AddListener(() => SetActivePanel(false));
            }
            foreach (var view in _verticalViewByProduct.Values)
            {
                view.closeButton.onClick.AddListener(() => SetActivePanel(false));
                view.closeArea.onClick.AddListener(() => SetActivePanel(false));
            }            
            
            if(Input.touchSupported && !XRSettings.enabled)
                _joystick = GameManager.Instance.starterController.GetJoystick();
            if(XRSettings.enabled)
                _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
        }
        
        public void Init(Asset asset, bool isInInventory = false)
        {
            if(asset == null || asset.assetMenu == null || asset.assetMenu.Count < 1) return;
            
            var actionNames = new List<string>(); 
            var actionIds = new List<string>(); 
            var actionCalls = new List<UnityAction>();

            for(var i = 0; i < asset.assetMenu.Count; ++i)
            {
                if(isInInventory && asset.assetMenu[i].name == TextData.Get(57))
                    continue;
                if(!isInInventory && asset.assetMenu[i].name == TextData.Get(58))
                    continue;
            
                actionIds.Add("" + i);
                actionNames.Add(asset.assetMenu[i].name);
                actionCalls.Add(asset.assetMenu[i].call);
            }
            
            if (XRSettings.enabled)
                MoveCanvas(asset);

            InitMenu(actionIds, actionNames, actionCalls, asset.assetName);
        }

        public void InitMenu(List<string> actionIds, List<string> actionNames, List<UnityAction> actionCalls, string assetName)
        {
            if (actionCalls.Count == 1)
            {
                actionCalls[0]?.Invoke();
                return;
            }
        
            switch (menuType)
            {
                case AssetMenuType.Vertical:
                    _assetMenuVerticalView.SetTitle(assetName);
                    _assetMenuVerticalView.SetValues(actionNames, actionCalls);
                    SetActivePanel(true);
                    break;
                case AssetMenuType.Radial:
                    _assetMenuRadialView.SetValues(actionIds, actionNames, actionCalls);
                    SetActivePanel(true);
                    break;
            }
            
            Cursor.ActivateCursor(true);
        }

        public void SetActivePanel(bool val)
        {
            if (!Input.touchSupported || XRSettings.enabled)
            {
                if (val)
                    GameManager.Instance.mainMenuController.ShowMenu(false);
            }
            else
            {
                if (_joystick != null)
                {
                    if (val)
                    {
                        GameManager.Instance.mainMenuController.ShowMenu(false);
                        _joystick.handle.anchoredPosition = Vector2.zero;
                        _joystick.input = Vector2.zero;
                        _joystick.gameObject.SetActive(false);
                    }
                    else
                    {
                        _joystick.gameObject.SetActive(true);
                    }
                }
            }

            switch (menuType)
            {
                case AssetMenuType.Vertical:
                    _assetMenuVerticalView.SetActivePanel(val);
                    break;
                case AssetMenuType.Radial:
                    _assetMenuRadialView.SetActivePanel(val);
                    break;
            }
        }
        
        private void MoveCanvas(Asset asset)
        {
            if(!XRSettings.enabled) return;

            var assetCenter = UIExtensions.EncapsulateBounds(asset.transform).center;
            var canvasTransform = _assetMenuRadialView.canvas.transform;
            var potentialPos = _vrCamera.position + _vrCamera.forward * 0.7f - new Vector3(0.0f, 0.15f, 0.0f);
            
            if (Vector3.Distance(potentialPos, assetCenter) < 1.0f)
            {
                potentialPos += _vrCamera.right * 0.5f;
            }
            
            canvasTransform.position = potentialPos;
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - _vrCamera.position);
        }
    }
}
