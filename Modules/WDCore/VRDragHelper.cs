using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.WDCore
{
    public class VRDragHelper : MonoBehaviour
    {
        private bool _isInit;
#if UNITY_XR
        private OVRInputModule _ovrInput;
#endif
        private Transform _currentWindow;
        private Transform _currentController;
        private Transform _leftController;
        private Transform _rightController;
        private Transform _vrCamera;
        
        private Vector3 _originRoot;
        private Vector3 _originController;
        private bool _startDragging;
        private float _moveStrength = 1.5f;

        private void Awake()
        {
            if(!XRSettings.enabled) return;
#if UNITY_XR
            _ovrInput = FindObjectOfType<OVRInputModule>();
#endif
            _rightController = GameManager.Instance.starterController.GetFPСVR().GetRightHand();
            _leftController = GameManager.Instance.starterController.GetFPСVR().GetLeftHand();
            _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
            _isInit = true;
        }

        void Update()
        {
            if(!_isInit) return;
#if UNITY_XR

            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.9f)
                PressToDrag(false);
            else if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.9f)
                PressToDrag(true);
            else
            {
                _startDragging = false;
                _currentWindow = null;
            }

            if (_startDragging)
                MoveWindow();
#endif
        }

        private void MoveWindow()
        {
            if(_currentWindow == null) return;

            _currentWindow.position = 
                new Vector3(_originRoot.x + (_currentController.position.x-_originController.x) * _moveStrength, 
                    _originRoot.y + (_currentController.position.y - _originController.y) * _moveStrength, 
                    _originRoot.z + (_currentController.position.z - _originController.z) * _moveStrength);
            
            _currentWindow.rotation = Quaternion.LookRotation(_currentWindow.position - _vrCamera.position);
        }
        
        private void PressToDrag(bool isLeft)
         {
#if UNITY_XR
             if(_currentWindow != null) return;

             var activeRaycaster = _ovrInput.activeGraphicRaycaster;
             if(activeRaycaster == null) return;
             
             var draggable = activeRaycaster.GetComponentInParent<VRDraggable>();
             if(draggable == null) return;
             
             _currentController = isLeft ? _leftController : _rightController;
             _currentWindow = draggable.transform;
             _originRoot = _currentWindow.position;
             _originController = _currentController.position;
             _startDragging = true;
#endif
         }
        
    }
}
