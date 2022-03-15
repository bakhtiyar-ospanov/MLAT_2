using System;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class LabSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        private LabSelectorView _view;

        private void Awake()
        {
            _view = GetComponent<LabSelectorView>();
            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                AddToDiseaseHistory();
            });
            _view.backToHistoryButton.onClick.AddListener(() => 
                { GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory"); });
        }

        public void Init()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.LabResearchParentId);
            if(rootResearch == null) return;
            
            _view.AddCheckboxGroup(rootResearch.children, passedAnswers);
            GameManager.Instance.mainMenuController.AddPopUpModule("LabSelector", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("LabSelector");
        }

        private void AddToDiseaseHistory()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.LabResearchParentId);
            
            if(rootResearch == null) return;
            
            GameManager.Instance.diseaseHistoryController.CleanGroup(Config.LabResearchParentId);
            
            var allLabs = BookDatabase.Instance.allCheckUps.
                FirstOrDefault(x => x.id == Config.LabResearchParentId)?.children;
            var answeredCheckups = new List<FullCheckUp>();

            foreach (var checkUp in allLabs)
            {
                var isAny = checkUp.children.Any(x => passedAnswers.Contains(x.id));
                if (isAny)
                {
                    GameManager.Instance.diseaseHistoryController.AddNewLabValue(Config.LabResearchParentId, checkUp);
                    answeredCheckups.Add(checkUp);
                }
                    
            }
            GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectLab");

            StartCoroutine(GameManager.Instance.labResultsController.Init(answeredCheckups, false));
        }

        private void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }

        public void Clean()
        {
            _view.Clean();
            passedAnswers.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("LabSelector");
        }

        public ScenarioController.Trigger.Action GetCorrectAction(bool isSimulation)
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var labValueById = BookDatabase.Instance.MedicalBook.labValueById;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.LabResearchParentId);

            if (rootResearch == null) return null;

            var actionName = "";
            var correctAnswers = new List<string>();
            
            foreach (var researchChild in rootResearch.children)
            {
                var groupTitle = researchChild.GetInfo().name + ":";
                var groupAnswers = "";
                
                foreach (var childChild in researchChild.children)
                {
                    if(childChild.children.Count == 0) continue;
                    groupAnswers += "\n   " + childChild.GetInfo().name;
                    correctAnswers.Add(childChild.id);
                    
                    labValueById.TryGetValue(childChild.id, out var labValue);
                    if(labValue == null) continue;
                    
                    var comment = childChild.children[0].GetInfo().name;
                    var result = labValue.values1.
                        FirstOrDefault(x => x.Split(':')[0] == childChild.children[0].id)?.Split(':')[1];

                    if (isSimulation)
                    {
                        var extraInfo = new List<string>();
                        if(!string.IsNullOrEmpty(result))
                            extraInfo.Add($"{TextData.Get(212)}: {result}");
                        if(!string.IsNullOrEmpty(comment))
                            extraInfo.Add($"{TextData.Get(213)}: {comment}");
                        if(!string.IsNullOrEmpty(labValue.nameReference1))
                            extraInfo.Add($"{TextData.Get(214)}: {labValue.nameReference1}");
                        if(!string.IsNullOrEmpty(labValue.nameMeasure1))
                            extraInfo.Add($"{TextData.Get(73)}: {labValue.nameMeasure1}");
                        if (extraInfo.Count > 0)
                            groupAnswers += $"\n      {string.Join(", ", extraInfo)}";
                    }
                }
                
                actionName += string.IsNullOrEmpty(groupAnswers) ? "" : groupTitle + groupAnswers + "\n\n";
            }
            
            return string.IsNullOrEmpty(actionName) ? null 
                : new ScenarioController.Trigger.Action {actionName = actionName, 
                    correctAnswers = correctAnswers, checkUpTrigger = rootResearch};
        }
    }
}
