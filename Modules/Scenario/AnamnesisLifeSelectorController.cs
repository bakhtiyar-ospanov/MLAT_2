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
    public class AnamnesisLifeSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        public List<StatusInstance.Status.CheckUp> correctAnswers = 
            new List<StatusInstance.Status.CheckUp>();
        
        private AnamnesisLifeSelectorView _view;
        private bool isFilled;
        private List<string> _ids;
        private List<UnityAction> _actions;
        private List<string> _names;

        private void Awake()
        {
            _view = GetComponent<AnamnesisLifeSelectorView>();
            _view.backToHistoryButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectAnamLife");
            });
            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectAnamLife");
            });
            
        }

        public void Init()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("AnamnesisLife");
                return;
                
            }
            _ids = new List<string>();
            _actions = new List<UnityAction>();
            _names  = new List<string>();

            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var anamnesisLife = caseInstance.FullStatus.checkUps.
                FirstOrDefault(x => x.id == Config.AnamnesisLifeParentId);
                
            if(anamnesisLife == null) return;
       
            foreach (var anamnesisLifeChild in anamnesisLife.children)
            {
                _ids.Add(anamnesisLifeChild.id);
                _names.Add(anamnesisLifeChild.GetInfo().details);
                _actions.Add(() => StartCoroutine(RecordAnamnesisLifeAnswer(anamnesisLifeChild)));
            }

            isFilled = true;
            _view.SetValues(_names, _actions);
            GameManager.Instance.mainMenuController.AddPopUpModule("AnamnesisLife",
                SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("AnamnesisLife");
        }
        
        private IEnumerator RecordAnamnesisLifeAnswer(StatusInstance.Status.CheckUp anamnesisLife)
        {
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            
            if(patientAsset.isDialogInProgress || !TextToSpeech.Instance.IsFinishedSpeaking()) yield break;
            
            yield return StartCoroutine(patientAsset.DialogRoutine(anamnesisLife.id, 
                anamnesisLife.GetInfo().details, anamnesisLife.children[0].GetInfo().details));
            
            GameManager.Instance.diseaseHistoryController.AddNewValue(Config.AnamnesisLifeParentId, 
                anamnesisLife.children[0].id, anamnesisLife.children[0].GetInfo().name);
            passedAnswers.Add(anamnesisLife.children[0].id);

            RemoveCheckUpFromMenu(_ids, _actions, _names, anamnesisLife.id);

            _view.SetValues(_names, _actions);

            if (_names.Count > 0)
            {
                GameManager.Instance.mainMenuController.ShowMenu("AnamnesisLife");
            }
            else
            {
                GameManager.Instance.diseaseHistoryController.RemoveLaunchButton(Config.AnamnesisLifeParentId);
                _view.applyButton.onClick?.Invoke();
            }
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
            var mainCheckUp = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.AnamnesisLifeParentId);

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
            GameManager.Instance.mainMenuController.RemovePopUpModule("AnamnesisLife");
        }
    }
}
