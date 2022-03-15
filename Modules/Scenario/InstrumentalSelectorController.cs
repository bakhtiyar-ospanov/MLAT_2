using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class InstrumentalSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        private InstrumentalSelectorView _view;
        private bool _isMediaAccepted;
        private List<string> _shownMedia = new List<string>();

        private void Awake()
        {
            _view = GetComponent<InstrumentalSelectorView>();
            _view.applyButton.onClick.AddListener(AddToDiseaseHistory);
            _view.acceptMedia[0].onClick.AddListener(() => _isMediaAccepted = true);
            _view.acceptMedia[1].onClick.AddListener(() => _isMediaAccepted = true);
            _view.backToHistoryButton.onClick.AddListener(() => 
                { GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory"); });
        }

        public void Init()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.InstrResearchParentd);
            if(rootResearch == null) return;
            
            _view.AddCheckboxGroup(rootResearch.children, passedAnswers);
            GameManager.Instance.mainMenuController.AddPopUpModule("InstrumentalSelector", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("InstrumentalSelector");
        }

        private void AddToDiseaseHistory()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.InstrResearchParentd);
            
            if(rootResearch == null) return;
            
            GameManager.Instance.diseaseHistoryController.CleanGroup(Config.InstrResearchParentd);
            
            var allInstrumentals = BookDatabase.Instance.allCheckUps.
                FirstOrDefault(x => x.id == Config.InstrResearchParentd)?.children;

            var answeredCheckups = new List<FullCheckUp>();
            var answerTxts = new List<string>();

            foreach (var checkUp in allInstrumentals)
            {
                foreach (var child in checkUp.children)
                {
                   if(!passedAnswers.Contains(child.id)) continue;

                   var shortCheckup = rootResearch.children.
                       FirstOrDefault(x => x.id == checkUp.id)?.children.
                       FirstOrDefault(x => x.id == child.id);
                   
                   var answerId = shortCheckup?.children.Count > 0 ? shortCheckup.children[0].id : "No_pathology";
                   var answerTxt = shortCheckup?.children.Count > 0 ? shortCheckup.children[0].GetInfo().name : TextData.Get(215);
                   var answerCheckup = shortCheckup?.children.Count > 0 ? shortCheckup.children[0].GetInfo() : child;
                   answerTxts.Add($"{child.name}:\n{answerTxt}");
                   answeredCheckups.Add(answerCheckup);

                   if (answerCheckup.files.Count > 0)
                   {
                       GameManager.Instance.diseaseHistoryController.AddNewInstrumentalValue(
                           Config.InstrResearchParentd, $"{child.name}: {answerTxt}", answerCheckup);
                   }
                   else
                   {
                       GameManager.Instance.diseaseHistoryController.AddNewValue(Config.InstrResearchParentd, child.id, 
                           child.name);
                       GameManager.Instance.diseaseHistoryController.ExpandValue(Config.InstrResearchParentd, 
                           child.id, answerId, answerTxt);
                   }
                   
                }
            }
            GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectInstrumental");

            StartCoroutine(ShowMedia(answeredCheckups, answerTxts, false));
        }

        private void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }

        public void Clean()
        {
            _view.Clean();
            passedAnswers.Clear();
            _shownMedia.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("InstrumentalSelector");
        }
        
        public ScenarioController.Trigger.Action GetCorrectAction(bool isSimulation)
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.InstrResearchParentd);
            
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

                    if(isSimulation)
                        groupAnswers += $" ({childChild.children[0].GetInfo().name})";
                }
                
                actionName += string.IsNullOrEmpty(groupAnswers) ? "" : groupTitle + groupAnswers + "\n\n";
            }
            return string.IsNullOrEmpty(actionName) ? null :
                new ScenarioController.Trigger.Action {actionName = actionName, 
                    correctAnswers = correctAnswers, checkUpTrigger = rootResearch};
        }
        
        private IEnumerator ShowMedia(List<FullCheckUp> checkUps, List<string> answerTxts, bool isIgnoreShown)
        {
            if (checkUps.Count > 0)
            {
                GameManager.Instance.mainMenuController.ShowMenu(false);
                GameManager.Instance.mainMenuController.isBlocked = true;

                for(var i = 0; i < checkUps.Count; i++)
                {
                    if(!isIgnoreShown && _shownMedia.Contains(checkUps[i].id)) continue;

                    var isMedia = checkUps[i].files.Count > 0;
                    _view.mediaRoot[0].SetActive(isMedia);
                    _view.mediaRoot[1].SetActive(!isMedia);

                    _isMediaAccepted = false;

                    if (isMedia)
                    {
                        foreach (Transform child in _view.mediaContainer)
                            Destroy(child.gameObject);
                    
                        var txtMedia = Instantiate(_view.txtImagePrefab, _view.mediaContainer);
                        txtMedia.tmpText.text = answerTxts[i];
                        StartCoroutine(checkUps[i].GetMedia(val => _view.LoadMedia(val, txtMedia.images)));
                    }
                    else
                    {
                        _view.noMediaTxt.text = answerTxts[i];
                    }
                    
                    yield return new WaitUntil(() => _isMediaAccepted);
                    
                    _view.mediaRoot[0].SetActive(false);
                    _view.mediaRoot[1].SetActive(false);
                    
                    _shownMedia.Add(checkUps[i].id);
                }
            }
            GameManager.Instance.mainMenuController.isBlocked = false;
            GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
        }

        public void ShowMedia(string text, FullCheckUp checkUp)
        {
            StartCoroutine(ShowMedia(new List<FullCheckUp>{checkUp}, new List<string> {text}, true));
        }
    }
}
