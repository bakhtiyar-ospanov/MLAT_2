using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Assets
{
    public class InfraredThermometer : Carriable
    {
        private bool isBlocked;
        private List<string> contactPoints 
            = new List<string> {"RadialArteryR", "RadialArteryL"};
        private string checkUpId = "6186";

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
                name = TextData.Get(224),
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
            GameManager.Instance.assetController.assetInHands = this;

            asset.BlockRigidBody(true);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(true);

            if (XRSettings.enabled)
            {
                assetTrans.SetParent(_rightHand, false);
                assetTrans.localPosition = new Vector3(0.01215088f, 0.0002128211f, 0.005137273f);
                assetTrans.localEulerAngles = new Vector3(-11.06f, 142.96f,119.2f);
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
                assetTrans.localEulerAngles = new Vector3(0.0f, 180.0f, 90.0f);
            }
            
            isBlocked = false;
        }
        
        private void LateUpdate()
        {
            //if(isBlocked || EventSystem.current == null || EventSystem.current.IsPointerOverGameObject()) return;
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
                if (Starter.Cursor.IsVisible || cam == null)
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
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.InstrResearchParentd);
            
            if(rootResearch == null) return;

            foreach (var checkUp in rootResearch.children)
            {
                var question = checkUp.children.FirstOrDefault(x => x.id == checkUpId);
                if(question == null) continue;
               
                var answerId = question.children.Count > 0 ? question.children[0].id : "No_pathology";
                var answerTxt = question.children.Count > 0 ? question.children[0].GetInfo().name : TextData.Get(215);
               
                GameManager.Instance.diseaseHistoryController.AddNewValue(Config.InstrResearchParentd, question.id, 
                    question.GetInfo().name);
                GameManager.Instance.diseaseHistoryController.ExpandValue(Config.InstrResearchParentd, 
                    question.id, answerId, answerTxt);
                
                GameManager.Instance.instrumentalSelectorController.passedAnswers.Add(checkUpId);
                GameManager.Instance.checkTableController.RegisterTriggerInvoke(checkUpId);
                
                break;
            }
        }
    }
}
