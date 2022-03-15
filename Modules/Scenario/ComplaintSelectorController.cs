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
    public class ComplaintSelectorController : MonoBehaviour
    {
        public List<string> passedAnswers = new List<string>();
        public List<StatusInstance.Status.CheckUp> correctAnswers = 
            new List<StatusInstance.Status.CheckUp>();
        
        private ComplaintSelectorView _view;
        private bool isFilled;
        private List<string> _complaintIds;
        private List<UnityAction> _complaintActions;
        private List<string> _complaintNames;

        private void Awake()
        {
            _view = GetComponent<ComplaintSelectorView>();
            _view.applyButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectComplaint");
            });
            _view.backToHistoryButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("DiseaseHistory");
                GameManager.Instance.checkTableController.RegisterTriggerInvoke("SelectComplaint");
            });
            
        }

        public void Init()
        {
            if (isFilled)
            {
                GameManager.Instance.mainMenuController.ShowMenu("ComplaintSelector");
                return;
            }

            _complaintIds = new List<string>();
            _complaintNames = new List<string>();
            _complaintActions = new List<UnityAction>();
            passedAnswers.Clear();

            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var mainComplaint = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.ComplaintParentId);
            if (mainComplaint == null) return;

            _complaintIds.Add(mainComplaint.id);
            _complaintNames.Add(mainComplaint.GetInfo().details);
            _complaintActions.Add(() => StartCoroutine(AddRandomChildComplain(mainComplaint)));
            isFilled = true;

            _view.SetValues(_complaintNames, _complaintActions);
            GameManager.Instance.mainMenuController.AddPopUpModule("ComplaintSelector", SetActivePanel, new []{_view.root.transform});
            GameManager.Instance.mainMenuController.ShowMenu("ComplaintSelector");
        }
        
        private IEnumerator AddRandomChildComplain(StatusInstance.Status.CheckUp refComplaint)
        {
            var patientAsset = GameManager.Instance.assetController.patientAsset;

            if(patientAsset.isDialogInProgress || !TextToSpeech.Instance.IsFinishedSpeaking()) yield break;
            
            var possibleAnswers = ComplaintChildrenFinder(refComplaint);
            Shuffle(possibleAnswers);
            //var patientAnswer = possibleAnswers.FirstOrDefault(possibleAnswer => !passedAnswers.Contains(possibleAnswer.id));
            var patientAnswers = possibleAnswers.Where(possibleAnswer => !passedAnswers.Contains(possibleAnswer.id)).ToList();
            if(patientAnswers == null) yield break;
            

            for (int i=0; i < patientAnswers.Count; i++)
            {
                passedAnswers.Add(patientAnswers[i].id);

                if (refComplaint.GetInfo().level == 1)
                {
                    var playerText = refComplaint.GetInfo().details;

                    if (patientAnswers.Count > 1 && i != 0)
                        playerText = null;

                    yield return StartCoroutine(patientAsset.DialogRoutine(patientAnswers[i].id, playerText, patientAnswers[i].GetInfo().details));
                    GameManager.Instance.diseaseHistoryController.AddNewValue(Config.ComplaintParentId, patientAnswers[i].id, patientAnswers[i].GetInfo().name);
                }
                else
                {
                    yield return StartCoroutine(patientAsset.DialogRoutine(patientAnswers[i].id, refComplaint.GetInfo().details, patientAnswers[i].GetInfo().details));
                    GameManager.Instance.diseaseHistoryController.ExpandValue(Config.ComplaintParentId, refComplaint.GetInfo().parentId, patientAnswers[i].id, patientAnswers[i].GetInfo().name);
                }

                if (possibleAnswers.FirstOrDefault(possibleAnswer => !passedAnswers.Contains(possibleAnswer.id)) == null)
                {
                    RemoveCheckUpFromMenu(_complaintIds, _complaintActions, _complaintNames, refComplaint.id);
                }

                foreach (var newQuestion in patientAnswers[i].children)
                {
                    _complaintIds.Add(newQuestion.id);
                    _complaintNames.Add(newQuestion.GetInfo().details);
                    _complaintActions.Add(() => StartCoroutine(AddRandomChildComplain(newQuestion)));
                }
            }

            // if (_complaintIds.Count < 20)
            // {
            //     var randomQuestion = GetRandomQuestions(patientAnswer);
            //     if (randomQuestion.Count > 0)
            //     {
            //         _complaintIds.Add(randomQuestion[0].id);
            //         _complaintNames.Add(randomQuestion[0].details);
            //         _complaintActions.Add(() => AnnounceRandomQuestion(randomQuestion[0]));
            //     }
            // }
            
            _view.SetValues(_complaintNames, _complaintActions);

            if (_complaintNames.Count > 0)
            {
                GameManager.Instance.mainMenuController.ShowMenu("ComplaintSelector");
            }
            else
            {
                GameManager.Instance.diseaseHistoryController.RemoveLaunchButton(Config.ComplaintParentId);
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

        private List<StatusInstance.Status.CheckUp> ComplaintChildrenFinder(StatusInstance.Status.CheckUp refComplaint)
        {
            var possibleAnswers = new List<StatusInstance.Status.CheckUp>();
            foreach (var complaintChild in refComplaint.children)
            {
                if (complaintChild.GetInfo().level == 2)
                {
                    possibleAnswers.AddRange(complaintChild.children);
                }
                else
                {
                    possibleAnswers.Add(complaintChild);
                }
            }
            return possibleAnswers;
        }
    
        private static void Shuffle(List<StatusInstance.Status.CheckUp> alpha)  
        {  
            for (var i = 0; i < alpha.Count; i++) {
                var temp = alpha[i];
                var randomIndex = Random.Range(i, alpha.Count);
                alpha[i] = alpha[randomIndex];
                alpha[randomIndex] = temp;
            }
        }

        // private List<FullCheckUp> GetRandomQuestions(StatusInstance.Status.CheckUp patientAnswer)
        // {
        //     var allChildren = patientAnswer.GetInfo().children;
        //     var selectedChildren = patientAnswer.children.Select(x => x.id).ToList();
        //     var randomChildren = allChildren.Where(allChild => !selectedChildren.Contains(allChild.id)).ToList();
        //     
        //     for (var i = 0; i < randomChildren.Count; i++) {
        //         var temp = randomChildren[i];
        //         var randomIndex = Random.Range(i, randomChildren.Count);
        //         randomChildren[i] = randomChildren[randomIndex];
        //         randomChildren[randomIndex] = temp;
        //     }
        //     return randomChildren;
        // }
        //
        // private void AnnounceRandomQuestion(FullCheckUp checkUp)
        // {
        //     var vagueAnswers = new[] {203, 204, 205};
        //     var rand = new System.Random();
        //     DialogWithPatient(checkUp.id, checkUp.details, TextData.Get(vagueAnswers[rand.Next(vagueAnswers.Length)]));
        //     RemoveCheckUpFromMenu(_complaintIds, _complaintActions, _complaintNames, checkUp.id);
        //     GameManager.Instance.buttonListController.Init(_complaintNames, _complaintActions);
        // }

        private void GetAllComplaintsHelper(StatusInstance.Status.CheckUp refComplaint)
        {
            if(refComplaint == null)
                return;
            
            correctAnswers.Add(refComplaint);
            var firstOrderChildren = ComplaintChildrenFinder(refComplaint);
            foreach (var orderChild in firstOrderChildren)
            {
                if (orderChild.GetInfo().level % 2 == 0)
                    foreach (var orderChildChild in orderChild.children)
                        GetAllComplaintsHelper(orderChildChild);
                else
                    GetAllComplaintsHelper(orderChild);
            }
        }
        
        public ScenarioController.Trigger.Action GetCorrectAction()
        {
            var actionName = "";

            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var mainComplaint = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == Config.ComplaintParentId);
            correctAnswers.Clear();
            GetAllComplaintsHelper(mainComplaint);
            
            if(correctAnswers.Count > 0)
                correctAnswers.RemoveAt(0);

            foreach (var answer in correctAnswers)
            {
                var level = answer.GetInfo().level - 3;
                var answerName = answer.GetInfo().name;
                actionName += answerName.PadLeft(answerName.Length + level, ' ') + "\n";
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
            correctAnswers.Clear();
            passedAnswers.Clear();
            GameManager.Instance.mainMenuController.RemovePopUpModule("ComplaintSelector");
        }
    }
}
