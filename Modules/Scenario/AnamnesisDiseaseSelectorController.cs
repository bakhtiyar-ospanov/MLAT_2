using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.SpeechKit;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Scenario
{
    public class AnamnesisDiseaseSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        public List<StatusInstance.Status.CheckUp> correctAnswers = 
            new List<StatusInstance.Status.CheckUp>();
        
        private AnamnesisDiseaseSelectorView _view;
        private bool isFilled;
        private List<string> _ids;
        private List<UnityAction> _actions;
        private List<string> _names;
        private int _currentIndex;

        private void Awake()
        {
            _view = GetComponent<AnamnesisDiseaseSelectorView>();
            
            _view.backToHistoryButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectAnamDisease");
            });

            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectAnamDisease");
            });
        
        }

        public void Init()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("AnamnesisDisease");
                return;
            }
            
            _ids = new List<string>();
            _actions = new List<UnityAction>();
            _names  = new List<string>();

            _currentIndex = -1;
            var orderedAnamnesisDisease = GetOrderedAnamnesisDisease();
            if (orderedAnamnesisDisease == null) return;
            
            _ids.Add(orderedAnamnesisDisease.id);
            _names.Add(orderedAnamnesisDisease.GetInfo().details);
            _actions.Add(() => StartCoroutine(RecordAnamnesisDiseaseAnswer(orderedAnamnesisDisease)));

            isFilled = true;
            _view.SetValues(_names, _actions);
            GameManager.Instance.mainMenuController.AddPopUpModule("AnamnesisDisease", 
                SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("AnamnesisDisease");
        }
        
        private IEnumerator RecordAnamnesisDiseaseAnswer(StatusInstance.Status.CheckUp anamnesisDisease)
        {
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            
            if(patientAsset.isDialogInProgress || !TextToSpeech.Instance.IsFinishedSpeaking()) yield break;
            
            yield return StartCoroutine(patientAsset.DialogRoutine(anamnesisDisease.id, 
                anamnesisDisease.GetInfo().details, anamnesisDisease.children[0].GetInfo().details));
            
            GameManager.Instance.diseaseHistoryController.AddNewValue(Config.AnamnesisDiseaseParentId, 
                anamnesisDisease.children[0].id, anamnesisDisease.children[0].GetInfo().name);
            passedAnswers.Add(anamnesisDisease.children[0].id);
            
            RemoveCheckUpFromMenu(_ids, _actions, _names, anamnesisDisease.id);

            var orderedAnamnesisDisease = GetOrderedAnamnesisDisease();
            if (orderedAnamnesisDisease != null)
            {
                _ids.Add(orderedAnamnesisDisease.id);
                _names.Add(orderedAnamnesisDisease.GetInfo().details);
                _actions.Add(() => StartCoroutine(RecordAnamnesisDiseaseAnswer(orderedAnamnesisDisease)));
            }

            _view.SetValues(_names, _actions);
            
            if (_names.Count > 0)
            {
                GameManager.Instance.mainMenuController.ShowMenu("AnamnesisDisease");
            }
            else
            {
                GameManager.Instance.diseaseHistoryController.RemoveLaunchButton(Config.AnamnesisDiseaseParentId);
                _view.applyButton.onClick?.Invoke();
            }
            
        }

        private StatusInstance.Status.CheckUp GetOrderedAnamnesisDisease()
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var anamnesisDisease = caseInstance.FullStatus.checkUps.
                FirstOrDefault(x => x.id == Config.AnamnesisDiseaseParentId);

            if (anamnesisDisease == null) return null;
            
            _currentIndex++;
            var temp = anamnesisDisease.children.
                OrderBy(x => x.GetInfo().name.Substring(0, 2)).ToList();
            return _currentIndex == anamnesisDisease.children.Count ? null : temp[_currentIndex];
        }
        
        private void RemoveCheckUpFromMenu(List<string> ids, List<UnityAction> actions, List<string> names, string id)
        {
            var index = ids.IndexOf(id);
            if(index == -1) return;
            
            if(index < ids.Count)
                ids.RemoveAt(index);
            if(index < names.Count)
                names.RemoveAt(index);
            if(index < actions.Count)
                actions.RemoveAt(index);
        }

        public ScenarioController.Trigger.Action GetCorrectAction()
        {
            correctAnswers.Clear();
            var actionName = "";
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var mainCheckUp = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.AnamnesisDiseaseParentId);

            if (mainCheckUp != null)
            {
                foreach (var answer in mainCheckUp.children)
                {
                    var answerName = answer.children[0].GetInfo().name;
                    actionName +=  answerName + "\n";
                    correctAnswers.Add(answer.children[0]);
                }
            }

            return string.IsNullOrEmpty(actionName) ? null :
                new ScenarioController.Trigger.Action {actionName = actionName};
        }
        
        public void SetActivePanel(bool val)
        {
            _view.root.SetActive(val);
        }

        public void Clean()
        {
            isFilled = false;
            passedAnswers.Clear();
            correctAnswers.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("AnamnesisDisease");
        }
    }
}
