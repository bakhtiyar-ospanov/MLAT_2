using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class LabResultsController : MonoBehaviour
    {
        private LabResultsView _view;
        private bool _isResultAccepted;
        private List<string> _shownResults = new List<string>();
        private void Awake()
        {
            _view = GetComponent<LabResultsView>();
            _view.btnClose.onClick.AddListener(() => _isResultAccepted = true);
        }

        public IEnumerator Init(List<FullCheckUp> checkUps, bool isIgnoreShown)
        {
            if (checkUps.Count > 0)
            {
                for(var i = 0; i < checkUps.Count; i++)
                {
                    if(!isIgnoreShown && _shownResults.Contains(checkUps[i].id)) continue;

                    _isResultAccepted = false;

                    Init(checkUps[i], isIgnoreShown);
                    
                    yield return new WaitUntil(() => _isResultAccepted);
                }
            }
            
            SetActivePanel(false);
            GameManager.Instance.mainMenuController.isBlocked = false;
            GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
        }

        private void Init(FullCheckUp checkUp, bool isIgnoreShown)
        {
            _view.title.text = checkUp.name;
            _view.Clean();
            var labValueById = BookDatabase.Instance.MedicalBook.labValueById;
            var answers = GameManager.Instance.labSelectorController.passedAnswers;
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.
                FirstOrDefault(x => x.id == Config.LabResearchParentId)?.children.
                FirstOrDefault(x => x.id == checkUp.id);
            var isAnyNew = false;
            
            foreach (var answer in answers)
            {
                var answerCheckup = checkUp.children.FirstOrDefault(x => x.id == answer);
                if(answerCheckup == null) continue;
                
                labValueById.TryGetValue(answerCheckup.id, out var labValue);
                if(labValue == null) continue;

                var result = labValue.nameNormal1;
                var comment = TextData.Get(74);

                var shortCheckup = rootResearch?.children.FirstOrDefault(x => x.id == answer);
                
                if (shortCheckup?.children.Count > 0)
                {
                    comment = shortCheckup.children[0].GetInfo().name;
                    result = labValue.values1.
                        FirstOrDefault(x => x.Split(':')[0] == shortCheckup.children[0].id)?.Split(':')[1];
                }
                
                _view.AddNewParameter(answerCheckup.name, result, comment, 
                    labValue.nameReference1, labValue.nameMeasure1);

                if (!_shownResults.Contains(answerCheckup.id))
                {
                    _shownResults.Add(answerCheckup.id);
                    isAnyNew = true;
                }
            }

            if (!isIgnoreShown && !isAnyNew)
                _isResultAccepted = true;
            else
            {
                GameManager.Instance.mainMenuController.ShowMenu(false);
                GameManager.Instance.mainMenuController.isBlocked = true;
                SetActivePanel(true);
            }
        }
        
        private void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }

        public void Clean()
        {
            _view.Clean();
            _shownResults.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("LabResults");
        }
    }
}
