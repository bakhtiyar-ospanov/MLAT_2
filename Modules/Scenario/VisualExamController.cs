using System;
using System.Collections.Generic;
using System.Linq;

using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class VisualExamController : MonoBehaviour
    {
        public Dictionary<string, List<string>> passedAnswers = new Dictionary<string, List<string>>();
        private VisualExamView _view;
        private List<FullCheckUp> _noPointCheckups;
        private bool isFilled;

        private void Awake()
        {
            _view = GetComponent<VisualExamView>();
            _view.applyButton.onClick.AddListener(() => 
                { GameManager.Instance.physicalExamController.OnModeExit(); });
            _view.backToHistoryButton.onClick.AddListener(() => 
                { GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory"); });
        }

        public void InitPanel()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("VisualExam");
                return;
            }
            
            var caseCheckUp = GameManager.Instance.physicalExamController.GetGroupCheckUp(Config.VisualExamParentId);
            var appInstance = BookDatabase.Instance.allCheckUps.FirstOrDefault(x => x.id == Config.VisualExamParentId);
            if(appInstance == null) return;
            
            _noPointCheckups = new List<FullCheckUp>();
            foreach (var appInstanceChild in appInstance.children)
            {
                var pointInfo = appInstanceChild.GetPointInfo();
                if (pointInfo?.values == null || pointInfo.values.Count == 0)
                    _noPointCheckups.Add(appInstanceChild);
            }
            
            isFilled = true;
            _view.AddCheckboxGroup(_noPointCheckups, caseCheckUp?.children);
            GameManager.Instance.mainMenuController.AddPopUpModule("VisualExam", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("VisualExam");
        }

        public void AddToDiseaseHistory()
        {
            Debug.Log("AddToDiseaseHistory");
            GameManager.Instance.diseaseHistoryController.CleanGroup(Config.VisualExamParentId);

            var passedPhysicalAnswers = GameManager.Instance.physicalExamController.passedAnswers;
            var appInstance = BookDatabase.Instance.allCheckUps.FirstOrDefault(x => x.id == Config.VisualExamParentId);
            var caseCheckUps = GameManager.Instance.physicalExamController.GetGroupCheckUp(Config.VisualExamParentId);
            
            foreach (var physicalAnswer in passedPhysicalAnswers)
            {
                var fullCheckUp = appInstance?.children.FirstOrDefault(x => x.id == physicalAnswer.Key);
                if(fullCheckUp == null) continue;
                
                var pointName = fullCheckUp.GetPointInfo().name;
                string description;

                var caseCheckUp = caseCheckUps.children.FirstOrDefault(x => x.id == physicalAnswer.Key);
                if (caseCheckUp != null && caseCheckUp.children.Count != 0)
                    description = caseCheckUp.children[0].GetInfo().name;
                else
                    description = fullCheckUp.GetPointInfo().description;
                
                description = GameManager.Instance.physicalExamController.ReplaceCaseVariables(description);
                GameManager.Instance.diseaseHistoryController.AddNewValue(Config.VisualExamParentId, fullCheckUp.id, pointName + ": " + description);
            }

            for(var i = 0; i < _noPointCheckups.Count; ++i)
            {
                var val = _noPointCheckups[i].name + ": ";
                var id = _noPointCheckups[i].id;
                var selectedItems = _view.GetCheckboxValues(id);
                
                if (passedAnswers.ContainsKey(id))
                    passedAnswers[id] = selectedItems;
                else
                    passedAnswers.Add(id, selectedItems);

                if (selectedItems.Count == 0)
                {
                    passedAnswers.Remove(id);
                    continue;
                }
                    
                for (var j = 0; j < selectedItems.Count; ++j)
                {
                    val += j != 0 ? ", " : "";
                    var ans = _noPointCheckups[i].children.FirstOrDefault(x => x.id == selectedItems[j])?.name;
                    if (string.IsNullOrEmpty(ans)) ans = _noPointCheckups[i].GetPointInfo().description;
                    val += ans;
                }
                
                GameManager.Instance.diseaseHistoryController.AddNewValue(Config.VisualExamParentId, id, val);
            }
            
            SetActivePanel(false);
            GameManager.Instance.mainMenuController.ShowMenu(false);
            GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
        }              

        private void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }

        public void Clean()
        {
            isFilled = false;
            _view.Clean();
            passedAnswers.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("VisualExam");
        }
    }
}
