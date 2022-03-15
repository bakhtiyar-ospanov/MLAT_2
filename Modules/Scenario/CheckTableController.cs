using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class CheckTableController : MonoBehaviour
    {
        public CheckTable checkTableInstance;
        public Action<int, string> onScoreAndPathSet;
        private ScenarioReport _scenarioReport;
        
        private List<string> _allTriggers;
        private ScenarioController.Trigger _requiredTrigger;
        private ScenarioController.Trigger _alternativeTrigger;
        private string _lastDoneTriggerId;
        private Dictionary<string, ScenarioController.Trigger.Action> _unorderedActions;
        public Action onTriggerChange;

        public void RegisterTriggerInvoke(string id)
        {
            if(_requiredTrigger == null || _lastDoneTriggerId == id)
                return;

            if (_unorderedActions.ContainsKey(id))
            {
                _unorderedActions[id].isDone = true;
                return;
            }
            
            return;

            _lastDoneTriggerId = id;
            
            Debug.Log(id + ": " + _requiredTrigger.requiredAction.ContainsKey(id));

            if (_requiredTrigger.requiredAction.ContainsKey(id))
            {
                _requiredTrigger.requiredAction[id].isDone = true;

                if (_requiredTrigger.requiredAction.All(x => x.Value.isDone))
                {
                    _requiredTrigger.isDoneInOrder = true;
                }
            } 
            else if (!_unorderedActions.ContainsKey(id) && _allTriggers.Contains(id))
            {
                if (_alternativeTrigger != null && _alternativeTrigger.requiredAction.ContainsKey(id))
                {
                    _alternativeTrigger.requiredAction[id].isDone = true;
                    if(_alternativeTrigger.requiredAction.All(x => x.Value.isDone))
                    {
                        ShowRedGreenBlackout(true);
                    }
                }
                else
                {
                    ShowRedGreenBlackout(false);
                }

                _requiredTrigger.isDoneInOrder = false;
                StopAllCoroutines();
                GameManager.Instance.scenarioController.StopAllCoroutines();
                GameManager.Instance.physicalExamController.StopAllCoroutines();
                GameManager.Instance.scenarioController.SwitchTrigger(id);
                return;
            } else if (_unorderedActions.ContainsKey(id))
            {
                _unorderedActions[id].isDone = true;
                ShowRedGreenBlackout(true);
            }

            if (_requiredTrigger.requiredAction.Count > 0 && _requiredTrigger.requiredAction.All(x => x.Value.isDone))
            {
                StopAllCoroutines();
                GameManager.Instance.scenarioController.StopAllCoroutines();
                GameManager.Instance.physicalExamController.StopAllCoroutines();
                ShowRedGreenBlackout(true);
                StartCoroutine(WaitChangeStep());
            }
            
            Debug.Log("NEED count: " + _requiredTrigger.requiredAction.Values.Count);
            foreach (var requiredTrigger in _requiredTrigger.requiredAction.Values)
                Debug.Log("NEED: " + requiredTrigger.actionName);
        }

        public void ParseNextTrigger()
        {
            Debug.Log("ParseNextTrigger");
            (_requiredTrigger, _alternativeTrigger, _unorderedActions) = 
                GameManager.Instance.scenarioController.GetCurrentTrigger();
            onTriggerChange.Invoke();
        }

        private IEnumerator WaitChangeStep()
        {
            yield return StartCoroutine(GameManager.Instance.scenarioController.AnnounceAction());
            yield return new WaitForSeconds(1.0f);
            yield return StartCoroutine(GameManager.Instance.scenarioController.ChangeStep(true));
        }

        public void SetAllTrigger(List<string> val)
        {
            _allTriggers = val;
        }

        public IEnumerator Finish(bool isSimulation)
        {
            _scenarioReport = new ScenarioReport();
            _scenarioReport.onScoreAndPathSet += onScoreAndPathSet;
            yield return StartCoroutine(_scenarioReport.CreateReport(isSimulation));
            GameManager.Instance.statisticsController.Open(false);
        }

        public CheckTable GetCheckTable()
        {
            return checkTableInstance;
        }

        public void Clean()
        {
            _scenarioReport = null;
            checkTableInstance = null;
            _allTriggers = null;
            _requiredTrigger = null;
            _alternativeTrigger = null;
            _lastDoneTriggerId = null;
            _unorderedActions = null;
        }

        private void ShowRedGreenBlackout(bool isGreen)
        {
            if (GameManager.Instance.scenarioController.GetMode() == ScenarioModel.Mode.Exam) return;
            GameManager.Instance.blackout.RedGreenBlackout(isGreen);
        }
    }
}
