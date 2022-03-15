using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Scenario
{
    public class DiseaseHistoryController : MonoBehaviour
    {
        private DiseaseHistoryView _view;

        private void Awake()
        {
            _view = GetComponent<DiseaseHistoryView>();
        }

        public void Init()
        {
            var patientInfo = GameManager.Instance.scenarioLoader.PatientInstance;
            GameManager.Instance.mainMenuController.AddModule("DiseaseHistory", "", 
                SetActivePanel, new [] {_view.root.transform});
            GameManager.Instance.mainMenuController.ShiftMenu(1);
            GameManager.Instance.mainMenuController.ActivateOutField(false);
            
            AddGroup("patientInfo", TextData.Get(69));
            
            AddNewValue("patientInfo", "patientNameSubtitle", TextData.Get(70));
            ExpandValue("patientInfo", "patientNameSubtitle","patientName", patientInfo.patientName);

            var gender = patientInfo.gender == 0 ? TextData.Get(173) : TextData.Get(172);
            var bmi = Math.Round(patientInfo.weight / Mathf.Pow(patientInfo.height / 100.0f, 2), 1);
            
            var ageGender = $"{TextData.Get(71)}: {patientInfo.age} {TextData.Get(62)}, " +
                                $"{TextData.Get(171)}: {gender}, " + 
                                $"{TextData.Get(91)}: {patientInfo.height} {TextData.Get(92)}, " + 
                                $"{TextData.Get(93)}: {patientInfo.weight} {TextData.Get(94)}";
            
            var physicalParams =  $"{TextData.Get(95)}: {bmi}";
            
            AddNewValue("patientInfo", "patientAgeGender", ageGender);
            AddNewValue("patientInfo", "patientPhysicalParams", physicalParams);
            AddLaunchButton("patientInfo", TextData.Get(313), GameManager.Instance.VSMonitorController.Show);

            var patient = GameManager.Instance.assetController.patientAsset;

            if (GameManager.Instance.complaintSelectorController.GetCorrectAction() != null)
            {
                AddGroup(Config.ComplaintParentId, TextData.Get(107));
                AddLaunchButton(Config.ComplaintParentId, TextData.Get(178), GameManager.Instance.complaintSelectorController.Init);
            }
            if (GameManager.Instance.anamnesisDiseaseSelectorController.GetCorrectAction() != null)
            {
                AddGroup(Config.AnamnesisDiseaseParentId, TextData.Get(177));
                AddLaunchButton(Config.AnamnesisDiseaseParentId, TextData.Get(180), GameManager.Instance.anamnesisDiseaseSelectorController.Init);
            }
            if (GameManager.Instance.anamnesisLifeSelectorController.GetCorrectAction() != null)
            {
                AddGroup(Config.AnamnesisLifeParentId, TextData.Get(176));
                AddLaunchButton(Config.AnamnesisLifeParentId, TextData.Get(179), GameManager.Instance.anamnesisLifeSelectorController.Init);
            }
            if (GameManager.Instance.physicalExamController.GetCorrectAction(Config.VisualExamParentId) != null)
            {
                AddGroup(Config.VisualExamParentId, TextData.Get(117));
                AddLaunchButton(Config.VisualExamParentId, TextData.Get(181), () => StartCoroutine(patient.VisualExam()));
            }
            if (GameManager.Instance.physicalExamController.GetCorrectAction(Config.PalpationParentId) != null)
            {
                AddGroup(Config.PalpationParentId, TextData.Get(114));
                AddLaunchButton(Config.PalpationParentId, TextData.Get(183), () => StartCoroutine(patient.Palpation()));
            }
            if (GameManager.Instance.physicalExamController.GetCorrectAction(Config.PercussionParentId) != null)
            {
                AddGroup(Config.PercussionParentId, TextData.Get(115));
                AddLaunchButton(Config.PercussionParentId, TextData.Get(184), () => StartCoroutine(patient.Percussion()));
            }
            if (GameManager.Instance.physicalExamController.GetCorrectAction(Config.AuscultationParentId) != null)
            {
                AddGroup(Config.AuscultationParentId, TextData.Get(116));
                AddLaunchButton(Config.AuscultationParentId, TextData.Get(182), () => StartCoroutine(patient.Auscultation()));
            }
            if (GameManager.Instance.labSelectorController.GetCorrectAction(false) != null)
            {
                AddGroup(Config.LabResearchParentId, TextData.Get(109));
                AddLaunchButton(Config.LabResearchParentId, TextData.Get(186), GameManager.Instance.labSelectorController.Init);
            }
            if (GameManager.Instance.instrumentalSelectorController.GetCorrectAction(false) != null)
            {
                AddGroup(Config.InstrResearchParentd, TextData.Get(108));
                AddLaunchButton(Config.InstrResearchParentd, TextData.Get(185), GameManager.Instance.instrumentalSelectorController.Init);
            }
            if (GameManager.Instance.diagnosisSelectorController.GetCorrectAction() != null)
            {
                AddGroup("diagnosis", TextData.Get(147));
                AddLaunchButton("diagnosis", TextData.Get(146), GameManager.Instance.diagnosisSelectorController.Init);
            }
            if (GameManager.Instance.treatmentSelectorController.GetCorrectAction() != null)
            {
                AddGroup("treatments", TextData.Get(155));
                AddLaunchButton("treatments", TextData.Get(156), GameManager.Instance.treatmentSelectorController.Init);
            }
        }
        public void AddGroup(string groupId, string text)
        {
            _view.AddGroup(groupId, text);
        }

        public bool CheckGroup(string groupId)
        {
            return _view.CheckGroup(groupId);
        }
        
        private void AddLaunchButton(string groupId, string text, UnityAction call)
        {
            _view.AddLaunchButton(groupId, text, call);
        }

        public void RemoveLaunchButton(string groupId)
        {
            _view.RemoveLaunchButton(groupId);
        }

        public void RemoveGroup(string groupId)
        {
            _view.RemoveGroup(groupId);
        }
        
        public void AddNewValue(string groupId, string id, string text)
        {
            _view.AddNewValue(groupId, id, text);
        }

        public void ExpandValue(string groupId, string parentId, string id, string newInfo)
        {
            _view.ExpandValue(groupId, parentId, id, newInfo);
        }

        public void AddNewLabValue(string groupId, FullCheckUp question)
        {
            _view.AddNewLabValue(groupId, question);
        }
        
        public void AddNewInstrumentalValue(string groupId, string text, FullCheckUp question)
        {
            _view.AddNewInstrumentalValue(groupId, text, question);
        }

        public void CleanGroup(string groupId)
        {
            _view.CleanGroup(groupId);
        }
        
        public void Clean()
        {
            GameManager.Instance.mainMenuController.RemoveModule("DiseaseHistory");
            GameManager.Instance.mainMenuController.ShiftMenu(0);
            GameManager.Instance.mainMenuController.ActivateOutField(true);
            _view.Clean();
        }

        private void SetActivePanel(bool val)
        {
            _view.SetActivePanel(val);
        }
    }
}
