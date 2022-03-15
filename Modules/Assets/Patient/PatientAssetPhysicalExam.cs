using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.Scenario;
using UnityEngine;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset
    {
        private void OnBackToMainMenu()
        {
            GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
        }

        public IEnumerator VisualExam()
        {
            if(!GameManager.Instance.diseaseHistoryController.CheckGroup(Config.VisualExamParentId)) yield break;
            
            if (string.IsNullOrEmpty(GameManager.Instance.physicalExamController.appGroupId))
            {
                yield return StartCoroutine(ShowClothes(false, false));
                var pointIds = GetPhysicalExamPoints(Config.VisualExamParentId);

                if (pointIds.Item1 == null)
                {
                    OnBackToMainMenu();
                    yield break;
                }
            
                yield return StartCoroutine(GameManager.Instance.physicalExamController.SetAPPGroupId(Config.VisualExamParentId, 
                    "SelectVisualExam", pointIds.Item2));
                OnPhysicalExam(pointIds.Item1);
            } else if (GameManager.Instance.physicalExamController.appGroupId != Config.VisualExamParentId)
            {
                GameManager.Instance.physicalExamController.OnModeExit(true, false, false);
                yield return StartCoroutine(VisualExam());
                yield break;
            }
            
            if (GameManager.Instance.physicalExamController.appGroupId == Config.VisualExamParentId)
                GameManager.Instance.visualExamController.InitPanel();
            
            var flashlight = GameManager.Instance.assetController.GetAssetById("VA318");
            if(flashlight != null) flashlight.gameObject.SetActive(false);
        }

        public IEnumerator Palpation()
        {
            if (!GameManager.Instance.diseaseHistoryController.CheckGroup(Config.PalpationParentId)) yield break;
            
            if (string.IsNullOrEmpty(GameManager.Instance.physicalExamController.appGroupId))
            {
                yield return StartCoroutine(ShowClothes(false, false));
                var pointIds = GetPhysicalExamPoints(Config.PalpationParentId);

                if (pointIds.Item1 == null)
                {
                    OnBackToMainMenu();
                    yield break;
                }

                yield return StartCoroutine(GameManager.Instance.physicalExamController.SetAPPGroupId(Config.PalpationParentId, 
                    "SelectPalpation", pointIds.Item2));
                OnPhysicalExam(pointIds.Item1);
            }
            else if (GameManager.Instance.physicalExamController.appGroupId != Config.PalpationParentId)
            {
                GameManager.Instance.physicalExamController.OnModeExit(true, false, false);
                yield return StartCoroutine(Palpation());
                yield break;
            }
        }
        
        public IEnumerator Percussion()
        {
            if(!GameManager.Instance.diseaseHistoryController.CheckGroup(Config.PercussionParentId)) yield break;
            
            if (string.IsNullOrEmpty(GameManager.Instance.physicalExamController.appGroupId))
            {
                yield return StartCoroutine(ShowClothes(false, false));
                var pointIds = GetPhysicalExamPoints(Config.PercussionParentId);

                if (pointIds.Item1 == null)
                {
                    OnBackToMainMenu();
                    yield break;
                }

                yield return StartCoroutine(GameManager.Instance.physicalExamController.SetAPPGroupId(Config.PercussionParentId, 
                    "SelectPercussion", pointIds.Item2));
                OnPhysicalExam(pointIds.Item1);
            }
            else if (GameManager.Instance.physicalExamController.appGroupId != Config.PercussionParentId)
            {
                GameManager.Instance.physicalExamController.OnModeExit(true, false, false);
                yield return StartCoroutine(Percussion());
                yield break;
            }
        }

        public IEnumerator Auscultation()
        {
            if (!GameManager.Instance.diseaseHistoryController.CheckGroup(Config.AuscultationParentId)) yield break;
            
            if (string.IsNullOrEmpty(GameManager.Instance.physicalExamController.appGroupId))
            {
                yield return StartCoroutine(ShowClothes(false, false));
                var pointIds = GetPhysicalExamPoints(Config.AuscultationParentId);

                if (pointIds.Item1 == null)
                {
                    OnBackToMainMenu();
                    yield break;
                }
                
                yield return StartCoroutine(GameManager.Instance.physicalExamController.SetAPPGroupId(Config.AuscultationParentId, 
                    "SelectAuscultation", pointIds.Item2));
                OnPhysicalExam(pointIds.Item1);
            } 
            else if (GameManager.Instance.physicalExamController.appGroupId != Config.AuscultationParentId)
            {
                GameManager.Instance.physicalExamController.OnModeExit(true, false, false);
                yield return StartCoroutine(Auscultation());
                yield break;
            }
            
            var stethoscope = GameManager.Instance.assetController.GetAssetById("VA308");
            if(stethoscope != null) stethoscope.gameObject.SetActive(false);
        }

        private void OnPhysicalExam(List<string> pointsInfo)
        {
            var patient = GameManager.Instance.assetController.patientAsset;
            patient.BlockRigidBody(true);
            patient.isForcedStraightHead = true;
            StartCoroutine(patient.ControlHead("HeadStraight"));
            GameManager.Instance.scenarioLoader.contactPointManager.ActivatePoints(pointsInfo);
            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(true);
        }

        private static (List<string>, List<FullCheckUp>) GetPhysicalExamPoints(string appGroupId)
        {
            var appInstance = GameManager.Instance.physicalExamController.GetGroupCheckUp(appGroupId);

            var ids = new List<string> {"EXIT", "MENU"};
            var caseCheckUpIds = new List<string>();
            
            if (appInstance == null || appInstance.children.Count == 0)
                return (ids, null);
            
            foreach (var appInstanceChild in appInstance.children)
            {
                var pointInfo = appInstanceChild.GetPointInfo();
                if (pointInfo != null)
                {
                    caseCheckUpIds.Add(appInstanceChild.id);
                    ids.AddRange(pointInfo.values);
                }
            }
            
            var allOtherPointInfos = BookDatabase.Instance.allCheckUps
                .FirstOrDefault(x => x.id == appGroupId)?.children
                .Where(x => !caseCheckUpIds.Contains(x.id) && x.GetPointInfo() != null).ToList();

            var randomCheckups = new List<FullCheckUp>();

            if (allOtherPointInfos != null && GameManager.Instance.scenarioController.GetMode() != ScenarioModel.Mode.Learning)
            {
                for (var i = 0; i < 2; i++)
                {
                    var randomItem = allOtherPointInfos[Random.Range(0, allOtherPointInfos.Count - 1)];
                    if (randomItem == null) break;
                    ids.AddRange(randomItem.GetPointInfo().values);
                    allOtherPointInfos.Remove(randomItem);
                    randomCheckups.Add(randomItem);
                }
            }
            

            return (ids.Distinct().ToList(), randomCheckups);
        }
    }
}
