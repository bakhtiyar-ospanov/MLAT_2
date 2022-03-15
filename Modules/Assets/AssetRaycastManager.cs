using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Assets
{
    public class AssetRaycastManager : MonoBehaviour
    {
        public bool isBlocked;
        
        private Camera _cam;
        private float _hitRange = 100.0f;
        private AssetController _assetController;
        private Vector3 _pointerDownPos;
        
        private Transform _rightHand;
        private GameObject _vrPointer;

        private void Awake()
        {
            _assetController = GetComponent<AssetController>();
            GameManager.Instance.starterController.CameraInit += GetCamera;
            
            if (XRSettings.enabled)
            {
                _rightHand = GameManager.Instance.starterController.GetFPСVR().GetRightHand();
                _vrPointer = GameManager.Instance.starterController.GetFPСVR().GetPointer();
            }
        }

        void LateUpdate()
        {
            _assetController.ShowAssetName(null);
            if(isBlocked || EventSystem.current == null || _cam == null || !_cam.isActiveAndEnabled) return;

            if (XRSettings.enabled)
            {
#if UNITY_XR
                if (_vrPointer.activeSelf) return;

                var ray = new Ray(_rightHand.position + _rightHand.forward * 0.05f, _rightHand.forward);
                if (Physics.Raycast(ray, out var hit, _hitRange) && hit.transform != null)
                {
                    var bridge = hit.transform.GetComponent<ColliderBridge>();
                    if (bridge == null) return;
                    if (bridge.asset.assetId != "304" && bridge.asset.assetId != "8" && !(hit.distance <= 1.0f)) return;

                    _assetController.ShowAssetName(bridge.asset, hit.point);
                    if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.95f)
                        GameManager.Instance.assetMenuController.Init(bridge.asset);
                }
#endif
            }
            else if (Input.touchSupported)
            {
                foreach (var touch in Input.touches)
                {
                    if(EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.gameObject.layer == 5) return;

                    if (touch.phase == TouchPhase.Ended)
                    {
                        var ray = _cam.ScreenPointToRay(touch.position);
                        if (Physics.Raycast(ray, out var hit, _hitRange) && hit.transform != null &&
                            !GameManager.Instance.starterController.IsSwiping())
                        {
                            var bridge = hit.transform.GetComponent<ColliderBridge>();
                            if (bridge == null) return;
                            
                            GameManager.Instance.assetMenuController.Init(bridge.asset);
                        }
                    }                    
                }
            }
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
            
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, _hitRange) && hit.transform != null)
                {
                    var bridge = hit.transform.GetComponent<ColliderBridge>();
                    
                    if (bridge == null) return;

                    _assetController.ShowAssetName(bridge.asset);
                    
                    if (Input.GetMouseButtonDown(0))
                        _pointerDownPos = _cam.ScreenToViewportPoint(Input.mousePosition);
                    
                    if (Input.GetMouseButtonUp(0) && Vector2.Distance(_pointerDownPos, 
                                                      _cam.ScreenToViewportPoint(Input.mousePosition)) < 0.02f 
                                                  && GameManager.Instance.starterController.isFreeMode)
                        GameManager.Instance.assetMenuController.Init(bridge.asset);
                }
            }
        }

        public void OneClick(Vector3 mousePosition)
        {
            var ray = _cam.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out var hit, _hitRange) && hit.transform != null)
            {
                var bridge = hit.transform.GetComponent<ColliderBridge>();
                    
                if (bridge == null) return;

                if (Vector2.Distance(_pointerDownPos, _cam.ScreenToViewportPoint(mousePosition)) < 0.02f)
                    GameManager.Instance.assetMenuController.Init(bridge.asset);
            }
        }
        
        private void GetCamera(Camera _camera)
        {
            _cam = _camera;
        }
    }
}