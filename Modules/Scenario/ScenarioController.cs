using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.SpeechKit;
using UnityEngine;

namespace Modules.Scenario
{
    public class ScenarioController : MonoBehaviour
    {
        public class Trigger
        {
            public class Action
            {
                public string actionName;
                public bool isDone;
                public StatusInstance.Status.CheckUp checkUpTrigger;
                public List<string> correctAnswers = new List<string>();
                public bool isCorrect; // only for Norma and Pathology
            }

            public Dictionary<string, Action> requiredAction;
            public bool isDoneInOrder = true;
        }
        private ScenarioView _view;
        private ScenarioModel _model;
        private WallHint _wallHint;
        public bool IsLaunched;

        private void Awake()
        {
            _view = GetComponent<ScenarioView>();
            
            _view.closeButton.onClick.AddListener(() =>
            {
                var patientAsset = GameManager.Instance.assetController.patientAsset;
                if(patientAsset.isDialogInProgress || !TextToSpeech.Instance.IsFinishedSpeaking()) return;
                
                GameManager.Instance.mainMenuController.ShowMenu(false);
                GameManager.Instance.warningController.ShowExitWarning(TextData.Get(227),
                    () => StartCoroutine(Unload(true)), false, 
                    () => GameManager.Instance.mainMenuController.ShowMenu(true));
            });
            _view.prevButton.onClick.AddListener(() => StartCoroutine(ChangeStep(false)));
            _view.nextButton.onClick.AddListener(() => StartCoroutine(ChangeStep(true)));
            
        }

        public void Init(ScenarioModel.Mode mode)
        {
            Debug.Log("ScenarioController: Start");
            GameManager.Instance.diseaseHistoryController.Init();
            GameManager.Instance.physicalExamController.Init();
            CreateModel(mode, false);
            GameManager.Instance.checkTableController.ParseNextTrigger();
            GameManager.Instance.checkTableController.SetAllTrigger(_model.GetAllTriggerIds());

            if (mode == ScenarioModel.Mode.Exam)
                _model.timerCoroutine = StartCoroutine(StartTimer());
            else
                _view.timer.SetActive(false);

            _view.SetActivePanel(true);
            IsLaunched = true;
        }

        public void CreateModel(ScenarioModel.Mode mode, bool isSimulation)
        {
            _model = new ScenarioModel(mode, isSimulation);
        }

        public void CleanModel()
        {
            _model = null;
        }

        private IEnumerator StartTimer()
        {
            var limit = new[] {0, 25, 30, 35, 40, 45, 50, 55, 60};
            var index = PlayerPrefs.GetInt("TIME_LIMIT");
            var timeLimit = limit[index] * 60;

            if (timeLimit == 0)
            {
                _view.timer.SetActive(false);
                yield break;
            }

            _view.timerTxt.text = TimeSpan.FromSeconds(timeLimit).ToString(@"mm\:ss");
            _view.timer.SetActive(true);
            var currentTime = 0;

            while (currentTime <= timeLimit)
            {
                var restTime = timeLimit - currentTime;
                _view.timerTxt.text = TimeSpan.FromSeconds(restTime).ToString(@"mm\:ss");
                yield return new WaitForSecondsRealtime(1.0f);
                currentTime += 1;
            }

            _view.timer.SetActive(false);
            StartCoroutine(Unload(true));
        }

        public IEnumerator PlayHint()
        {
            yield break;
            StopCoroutine(PlayHintCor());
            yield return StartCoroutine(PlayHintCor());
        }
        
        private IEnumerator PlayHintCor()
        {
            yield return new WaitUntil(() => _model == null || _model.isStarted);
            if(_model == null) yield break;

            var currentAction = _model.alternativeAction ?? _model.actionsBySection[_model.sectionIndex][_model.actionIndex];
            if (_model.lastHint != currentAction.name)
            {
                _model.lastHint = currentAction.name;
                yield return StartCoroutine(AnnounceHint(_model.lastHint));
            }

            if (string.IsNullOrEmpty(currentAction.nameButton) && string.IsNullOrEmpty(currentAction.trigger))
            {
                var doctorPhrase = currentAction.speech;
                var patientPhrase = currentAction.answer;

                var patientAsset = GameManager.Instance.assetController.patientAsset;
                yield return StartCoroutine(patientAsset.DialogRoutine("continue", doctorPhrase, patientPhrase));
            }
        }

        public IEnumerator AnnounceHint(string outPhrase)
        {
            if (_wallHint == null)
                InitWallHint();
            if(_wallHint != null)
                _wallHint.ShowHint(outPhrase);
            //TextToSpeech.Instance.SetText(outPhrase, TextToSpeech.Character.Assistant);
            yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
        }

        public IEnumerator AnnounceAction()
        {
            var currentAction = _model.actionsBySection[_model.sectionIndex][_model.actionIndex];
            
            if(string.IsNullOrEmpty(currentAction.trigger)) yield break;
            
            yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());

            var doctorPhrase = currentAction.speech;
            var patientPhrase = currentAction.answer;
            
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            yield return StartCoroutine(patientAsset.DialogRoutine(null, doctorPhrase, patientPhrase));
            
        }
        
        public IEnumerator AnnouncePointHint(string appGroup, Trigger requiredTriggers)
        {
            if(_model.mode != ScenarioModel.Mode.Learning) yield break;
            
            yield return new WaitForSeconds(1.0f);
            yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
            
            var checkups = GameManager.Instance.physicalExamController.GetGroupCheckUp(appGroup);
            if(checkups == null) yield break;
            var uncheckedCheckupId = requiredTriggers.requiredAction.FirstOrDefault(x => !x.Value.isDone).Key;
            if(uncheckedCheckupId == null) yield break;
            
            var uncheckedCheckup = appGroup == Config.InstrResearchParentd ? 
                checkups.children.SelectMany(checkUp => checkUp.children).FirstOrDefault(x => x.id == uncheckedCheckupId) : 
                checkups.children.FirstOrDefault(x => x.id == uncheckedCheckupId);
            
            if (uncheckedCheckup == null) yield break;
            yield return StartCoroutine(AnnounceHint(uncheckedCheckup.GetInfo().name));
        }

        public IEnumerator ChangeStep(bool isForward, bool isOnError = false)
        {
            if (_model.sectionIndex == 0 && _model.actionIndex == 0 && !isForward) { yield break; }
            
            if (_model.sectionIndex == _model.actionsBySection.Count - 1 &&
                _model.actionIndex == _model.actionsBySection[_model.sectionIndex].Count - 1 && isForward)
            {
                GameManager.Instance.checkTableController.ParseNextTrigger();
                if (_model.alternativeAction != null && _model.mode == ScenarioModel.Mode.Learning)
                    yield return StartCoroutine(PlayHint());
                else if(_wallHint != null)
                    _wallHint.ShowHint("");

                _view.closeButton.onClick.Invoke();
                yield break;
            }

            if (_model.actionIndex == _model.actionsBySection[_model.sectionIndex].Count - 1 && isForward)
            {
                _model.actionIndex = 0;
                _model.sectionIndex++;
            } else if (_model.actionIndex == 0 && !isForward)
            {
                _model.sectionIndex--;
                _model.actionIndex = _model.actionsBySection[_model.sectionIndex].Count - 1;
            }
            else
            {
                _model.actionIndex = isForward ? ++_model.actionIndex : --_model.actionIndex;
            }
            
            var currentTrigger = GetCurrentTrigger();

            if (currentTrigger.Item1.requiredAction.ContainsKey("skip"))
            {
                yield return StartCoroutine(ChangeStep(isForward));
                yield break;
            }

            var unordered = currentTrigger.Item3;
            if (currentTrigger.Item1.requiredAction.
                All(x => unordered.ContainsKey(x.Key) && x.Value.isDone))
            {
                StartCoroutine(ChangeStep(true));
                yield break;
            }

            GameManager.Instance.checkTableController.ParseNextTrigger();

            if((_model.mode == ScenarioModel.Mode.Learning && !isOnError) || (isOnError && _model.mode == ScenarioModel.Mode.Selfcheck))
                yield return StartCoroutine(PlayHint());
            else if(_wallHint != null && (isOnError && _model.mode == ScenarioModel.Mode.Selfcheck))
                _wallHint.ShowHint("");
        }

        public (Trigger, Trigger, Dictionary<string, Trigger.Action>) GetCurrentTrigger()
        {
            if (_model.actionsBySection == null) return (null, null, null);
            var currentAction = _model.actionsBySection[_model.sectionIndex][_model.actionIndex];
            var unorderedSet = _model.GetUnorderedActionsInSection();

            if (_model.alternativeAction != null && currentAction.id == _model.alternativeAction.id)
                _model.alternativeAction = null;

            var requiredTriggers = _model.allTriggers[currentAction.id];
            return (requiredTriggers, 
                _model.alternativeAction != null ? _model.allTriggers[_model.alternativeAction.id] : null, 
                unorderedSet);
        }

        public void SwitchTrigger(string id)
        {
            var actionId = "";
            var possibleActions = new List<(string, int)>();
            var j = 0;
            
            foreach (var allTrigger in _model.allTriggers)
            {
                foreach (var action in allTrigger.Value.requiredAction)
                {
                    if (action.Key == id)
                    {
                        action.Value.isDone = true;
                        allTrigger.Value.isDoneInOrder = false;
                        
                        if (allTrigger.Value.requiredAction.All(x => x.Value.isDone))
                            possibleActions.Add((allTrigger.Key, j));
                    }
                    j++;
                }
            }

            if (possibleActions.Count > 0)
            {
                var currentActionId = _model.actionsBySection[_model.sectionIndex][_model.actionIndex].id;
                var currentActionRank = 0;
                var k = 0;
                foreach (var action in _model.actionsBySection.SelectMany(section => section.Value))
                {
                    if (action.id == currentActionId)
                    {
                        currentActionRank = k;
                        break;
                    }
                    k++;
                }
                var closesRank = Math.Abs(possibleActions[0].Item2 - currentActionRank);
                actionId = possibleActions[0].Item1;

                foreach (var possibleAction in possibleActions)
                {
                    if (_model.alternativeAction != null && possibleAction.Item1 == _model.alternativeAction.id)
                    {
                        actionId = possibleAction.Item1;
                        break;
                    }
                    var potentialMin = Math.Abs(possibleAction.Item2 - currentActionRank);
                    if (potentialMin <= closesRank)
                    {
                        closesRank = potentialMin;
                        actionId = possibleAction.Item1;
                    } 
                }
            }

            if(string.IsNullOrEmpty(actionId)) return;
            
            foreach (var section in _model.actionsBySection)
            {
                for (var i = 0; i < section.Value.Count; ++i)
                {
                    if (section.Value[i].id == actionId)
                    {
                        _model.alternativeAction ??= _model.actionsBySection[_model.sectionIndex][_model.actionIndex];
                        _model.sectionIndex = section.Key;
                        _model.actionIndex = i;
                        
                        if (_model.alternativeAction ==
                            _model.actionsBySection[_model.sectionIndex][_model.actionIndex])
                            _model.alternativeAction = null;

                        StartCoroutine(ChangeStep(true, _model.alternativeAction != null));
                        break;
                    }
                }
            }
        }

        public Dictionary<string, Trigger> GetAllTriggers()
        {
            return _model?.allTriggers;
        }

        public IEnumerator Unload(bool isWithDebriefing)
        {
            Debug.Log("ScenarioController: Unload");
            
            if(_model.timerCoroutine != null)
                StopCoroutine(_model.timerCoroutine);

            if(GameManager.Instance.assetController.assetInHands != null)
                GameManager.Instance.assetController.assetInHands.ReturnItem();
            
            if (PlayerPrefs.HasKey("PROF_PIN"))
                yield return StartCoroutine(GameManager.Instance.settingsController.ShowProfPinModal());
            
            if(isWithDebriefing)
                yield return StartCoroutine(GameManager.Instance.blackout.Show());
            
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            if (patientAsset == null) yield break;
            Destroy(patientAsset.gameObject);
            
            GameManager.Instance.assetMenuController.SetActivePanel(false);

            if (_model == null)
                yield break;

            _model.isStarted = false;
            yield return StartCoroutine(GameManager.Instance.checkTableController.Finish(false));

            if(isWithDebriefing)
                GameManager.Instance.debriefingController.Init(false);
            
            GameManager.Instance.assetController.patientAsset = null;
            GameManager.Instance.physicalExamController.OnModeExit(false, true);
            GameManager.Instance.checkTableController.Clean();
            GameManager.Instance.diseaseHistoryController.Clean();
            GameManager.Instance.complaintSelectorController.Clean();
            GameManager.Instance.anamnesisDiseaseSelectorController.Clean();
            GameManager.Instance.anamnesisLifeSelectorController.Clean();
            GameManager.Instance.visualExamController.Clean();
            GameManager.Instance.labResultsController.Clean();
            GameManager.Instance.diagnosisSelectorController.Clean();
            GameManager.Instance.treatmentSelectorController.Clean();
            GameManager.Instance.labSelectorController.Clean();
            GameManager.Instance.instrumentalSelectorController.Clean();
            GameManager.Instance.VSMonitorController.Clean();
            GameManager.Instance.scenarioLoader.Unload();

            _view.SetActivePanel(false);
            _wallHint = null;
            CleanModel();
            IsLaunched = false;

            if (isWithDebriefing)
            {
                GameManager.Instance.mainMenuController.ShowMenu("Debriefing");
                if(!GameManager.Instance.debriefingController.isInit)
                    GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
            }
            else
                GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
            
            if(isWithDebriefing)
                yield return StartCoroutine(GameManager.Instance.blackout.Hide());
            
            StartCoroutine(GameManager.Instance.awardController.Init());
            Debug.Log("unload end");
        }

        private void InitWallHint()
        {
            var hintAsset = GameObject.Find("VA456");
            if(hintAsset == null) return;

            _wallHint = hintAsset.AddComponent<WallHint>();
        }

        public ScenarioModel.Mode GetMode()
        {
            return _model.mode;
        }
    }
}
