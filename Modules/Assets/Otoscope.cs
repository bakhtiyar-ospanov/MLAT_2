using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Assets
{
    public class Otoscope : Carriable
    {
        private bool isBlocked;
        private List<string> contactPoints 
            = new List<string> {"Ear"};
        private string checkUpId = "7762";

        private void Awake()
        {
            asset = GetComponent<Asset>();
            assetTrans = transform;
            InitialLocation.Item1 = assetTrans.position;
            InitialLocation.Item2 = assetTrans.eulerAngles;
            InitialLocation.Item3 = assetTrans.parent;
            isBlocked = true;
            
            var oneAction = new MedicalBase.MenuItem
            {
                name = TextData.Get(270),
                call = CustomAction
            };
            asset.assetMenu = new List<MedicalBase.MenuItem>{oneAction};
            
            if (XRSettings.enabled)
            {
                _rightHand = GameManager.Instance.starterController.GetFPСVR().GetRightHand();
                _vrPointer = GameManager.Instance.starterController.GetFPСVR().GetPointer();
            }
        }

        private void CustomAction()
        {
            Debug.Log("CustomAction");
            if(GameManager.Instance.scenarioLoader.contactPointManager == null) return;
            
            if(GameManager.Instance.assetController.assetInHands != null)
                GameManager.Instance.assetController.assetInHands.ReturnItem();

            GameManager.Instance.physicalExamController.OnModeExit();
            Starter.Cursor.ActivateCursor(false);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivatePoints(contactPoints);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(true);

            asset.BlockRigidBody(true);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(true);
            GameManager.Instance.assetController.assetInHands = this;
            
            if (XRSettings.enabled)
            {
                assetTrans.SetParent(_rightHand, false);
                assetTrans.localPosition = new Vector3(0.0209999997f,0.00200000009f,0.00600000005f);
                assetTrans.localEulerAngles = new Vector3(20.779974f,64.6692505f,34.6742325f);
            }
            else
            {
                cam = GameManager.Instance.starterController.GetCamera();
                assetTrans.position = Vector3.zero;
                assetTrans.localEulerAngles = Vector3.zero;
                assetTrans.SetParent(cam.transform);
            
                var (upShift, forwardShift) = UIExtensions.FitToBounds(cam, assetTrans);
                var putPosition = cam.transform.position + cam.transform.forward * Mathf.Clamp(forwardShift, 0.4f, 4.0f);
                putPosition = new Vector3(putPosition.x , putPosition.y - 0.2f, putPosition.z);
                
                assetTrans.position = putPosition;
                assetTrans.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            }
            isBlocked = false;
        }
        
        private void LateUpdate()
        {
            if (isBlocked || EventSystem.current == null) return;

            if (XRSettings.enabled)
            {
#if UNITY_XR
                if (_vrPointer.activeSelf) return;

                var ray = new Ray(_rightHand.position + _rightHand.forward * 0.05f, _rightHand.forward);
                if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
                {
                    if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.95f)
                        ActivateCustomAction(hit.transform.name);
                }
#endif
            }
            else if (Input.touchSupported)
            {
                if (cam == null) return;

                foreach (Touch touch in Input.touches)
                {
                    if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.gameObject.layer == 5) return;

                    var ray = cam.ScreenPointToRay(touch.position);
                    if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null &&
                        !GameManager.Instance.starterController.IsSwiping())
                    {
                        ActivateCustomAction(hit.transform.name);
                    }
                }
            }
            else
            {
                if (Starter.Cursor.IsVisible|| cam == null)
                {
                    ReturnItem();
                    return;
                }
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
                {
                    if(Input.GetMouseButtonDown(0))
                        ActivateCustomAction(hit.transform.name);
                }
            }
        }

        private void ActivateCustomAction(string pointName)
        {
            if(!contactPoints.Contains(pointName)) return;
            
            ReturnItem();
            AddToDiseaseHistory();
        }

        public override void ReturnItem()
        {
            isBlocked = true;
            
            assetTrans.position = InitialLocation.Item1;
            assetTrans.eulerAngles = InitialLocation.Item2;
            assetTrans.parent = InitialLocation.Item3;
            
            asset.BlockRigidBody(false);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(false);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(false);
            GameManager.Instance.assetController.assetInHands = null;
        }
        
        private void AddToDiseaseHistory()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootVisual = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.VisualExamParentId);
            
            if(rootVisual == null) return;
            
            foreach (var checkUp in rootVisual.children)
            {
                if(checkUp.id != checkUpId) continue;
                GameManager.Instance.physicalExamController.appGroupId = Config.VisualExamParentId;
                GameManager.Instance.physicalExamController.RegisterCheckUp(checkUp.GetInfo(), true);
                break;
            }
        }
    }
}
