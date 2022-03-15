using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Assets
{
    public class PulseOximeter : Carriable
    {
        private bool isBlocked;
        private List<string> contactPoints 
            = new List<string> {"ForefingerL", "ForefingerR"};
        private string[] checkUpIds = { "6190", "6188" };
        

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
                name = TextData.Get(225),
                call = () => StartCoroutine(CustomAction())
            };
            asset.assetMenu = new List<MedicalBase.MenuItem>{oneAction};
            
            if (XRSettings.enabled)
            {
                _rightHand = GameManager.Instance.starterController.GetFPСVR().GetRightHand();
                _vrPointer = GameManager.Instance.starterController.GetFPСVR().GetPointer();
            }
        }

        private IEnumerator CustomAction()
        {
            Debug.Log("CustomAction");
            if(GameManager.Instance.scenarioLoader.contactPointManager == null) yield break;
            
            if(GameManager.Instance.assetController.assetInHands != null)
                GameManager.Instance.assetController.assetInHands.ReturnItem();
            
            GameManager.Instance.physicalExamController.OnModeExit();
            Starter.Cursor.ActivateCursor(false);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivatePoints(contactPoints);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(true);

            asset.BlockRigidBody(true);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(true);
            GameManager.Instance.assetController.assetInHands = this;
            
            var mesh = assetTrans.GetChild(0);
            mesh.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);

            if (XRSettings.enabled)
            {
                assetTrans.SetParent(_rightHand, false);
                assetTrans.localPosition = new Vector3(0.0204000007f,0.0105999997f,0.0241999999f);
                assetTrans.localEulerAngles = new Vector3(327.919159f,208.612305f,31.0847378f);
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
                assetTrans.eulerAngles = Vector3.zero;
            }

            yield return new WaitForSeconds(0.1f);
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
                        ActivateCustomAction(hit.transform);
                }
#endif
            }
            else if (Input.touchSupported)
            {
                if (cam == null) return;

                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Ended)
                    {
                        if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.gameObject.layer == 5) return;

                        var ray = cam.ScreenPointToRay(touch.position);
                        if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null &&
                                !GameManager.Instance.starterController.IsSwiping() && !GameManager.Instance.starterController.IsSwipingTouch())
                        {
                            ActivateCustomAction(hit.transform);
                        }
                    }
                    
                }
            }
            else
            {
                if (cam == null)
                {
                    ReturnItem();
                    return;
                }
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
                {
                    if(Input.GetMouseButtonDown(0))
                        ActivateCustomAction(hit.transform);
                }
            }
        }

        private void ActivateCustomAction(Transform point)
        {
            Debug.Log("ActivateCustomAction " + point);
            if (!contactPoints.Contains(point.name))
            {
                ReturnItem();
                return;
            }
            
            isBlocked = true;

            assetTrans.position = point.position;
            assetTrans.parent = point.parent;
            assetTrans.localEulerAngles = point.name == "ForefingerL" ? 
                new Vector3(-238, -267, -352) : new Vector3(-238, 267, -352);

            var mesh = assetTrans.GetChild(0);
            mesh.localEulerAngles = new Vector3(-30.0f, 0.0f, 0.0f);

            AddToDiseaseHistory();
            
            asset.BlockCollider(false);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(false);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(false);
        }

        private void AddToDiseaseHistory()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.InstrResearchParentd);
            
            if(rootResearch == null) return;

            foreach (var checkUp in rootResearch.children)
            {
                foreach (var checkUpId in checkUpIds)
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
                }
            }
        }

        public override void ReturnItem()
        {
            isBlocked = true;
            
            assetTrans.position = InitialLocation.Item1;
            assetTrans.eulerAngles = InitialLocation.Item2;
            assetTrans.parent = InitialLocation.Item3;
            
            var mesh = assetTrans.GetChild(0);
            mesh.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            
            asset.BlockRigidBody(false);
            GameManager.Instance.assetController.patientAsset.BlockRigidBody(false);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(false);
            GameManager.Instance.assetController.assetInHands = null;
        }

        private Ray GetRayByType()
        {
            if (XRSettings.enabled)
            {
                return new Ray(_rightHand.position + _rightHand.forward * 0.05f, _rightHand.forward);
            }
            
            else if (Input.touchSupported)
            {
                foreach (Touch touch in Input.touches)
                {
                    return cam.ScreenPointToRay(touch.position);
                }
            }
            
            return cam.ScreenPointToRay(Input.mousePosition);
        }

    }
}
