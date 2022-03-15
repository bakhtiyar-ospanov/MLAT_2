// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.RegularExpressions;
// using Modules.Books;
// using Modules.WDCore;
// using Modules.Scenario;
// using Modules.SpeechKit;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Events;
// using UnityEngine.EventSystems;
// using UnityEngine.Playables;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.UI;
//
// namespace Modules.Manipulation
// {
//     public class ManipulationController : MonoBehaviour
//     {
//         private ManipulationView _manipulationView;
//         private ManipulationModel _manipulationModel;
//         private AsyncOperationHandle<GameObject> _modelHandle;
//         private PlayableDirector _playable;
//         private Camera[] _modelCameras;
//         private Transform[] _assetChildren;
//         private Dictionary<string, Button> _existingButtonByName;
//         private Dictionary<string, Toggle> _existingToggleByName;
//         private Camera _currentCam;
//         private List<CoursesDimedus.ManipulationScenario> _complexActions;
//         private Dictionary<Transform, Transform> _buttonByPointer = new Dictionary<Transform, Transform>();
//         private List<(Button, Color)> _buttonDefaultColors = new List<(Button, Color)>();
//         public bool IsLaunched;
//         public bool IsStarted;
//         public ScenarioModel.Mode Mode;
//         public Action<int, string> onScoreAndPathSet;
//
//         private void Awake()
//         {
//             _manipulationView = GetComponent<ManipulationView>();
//
//             _manipulationView.closeButton.onClick.AddListener(() =>
//             {
//                 GameManager.Instance.warningController.ShowExitWarning(TextData.Get(227),
//                     () => StartCoroutine(Finish(true)));
//             });
//
//             _manipulationModel = new ManipulationModel();
//             
//             _manipulationView.btnPrev.onClick.AddListener(() => StartCoroutine(ChangeAction(false)));
//             _manipulationView.btnNext.onClick.AddListener(() => StartCoroutine(ChangeAction(true)));
//         }
//
//         public IEnumerator Init(MedicalBase.Scenario scenario, ScenarioModel.Mode mode)
//         {
//             Mode = mode;
//             IsStarted = true;
//
//             // Load Scene
//             var checkScene = Addressables.LoadResourceLocationsAsync(scenario.cabinetId);
//             yield return checkScene;
//             var count2 = checkScene.Result.Count;
//             Addressables.Release(checkScene);
//             if (count2 == 0)
//             {
//                 // No scene with this id is in Addressbales
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.cabinetId})");
//                 yield break;
//             }
//             
//             var check = Addressables.LoadResourceLocationsAsync(scenario.patientId);
//             yield return check;
//             var count = check.Result.Count;
//             Addressables.Release(check);
//             if (count == 0)
//             {
//                 // No course with this id is in Addressbales
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.patientId})");
//                 StartCoroutine(GameManager.Instance.scenarioSelectorController.Init(GameManager.Product.Dimedus));
//                 yield break;
//             }
//             
//             var sceneLoadStatus = false;
//             yield return StartCoroutine(GameManager.Instance.starterController.Init(scenario.cabinetId, 
//                 false, val => sceneLoadStatus = val));
//
//             if (!sceneLoadStatus)
//             {
//                 IsStarted = false;
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.cabinetId})");
//                 GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
//                 StopAllCoroutines();
//                 yield break;
//             }
//
//             TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Assistant, 0);
//
//             // Load Model
//             var completed = false;
//             yield return StartCoroutine(LoadModel(scenario, val => completed = val));
//             if(!completed) yield break;
//             
//             LightSetup(scenario.cabinetId);
//             _manipulationView.SetActivePanel(true);
//             StartCoroutine(InitAction());
//             StartCoroutine(GameManager.Instance.blackout.Hide());
//             IsLaunched = true;
//         }
//
//         private IEnumerator LoadModel(MedicalBase.Scenario scenario, Action<bool> completed)
//         {
//             // Get size of the model
//             var getDownloadSize = Addressables.GetDownloadSizeAsync(scenario.patientId);
//             yield return getDownloadSize;
//             var size= getDownloadSize.Result;
//             Addressables.Release(getDownloadSize);
//             
//             // Instantiate model
//             var loading = GameManager.Instance.loadingController;
//             loading.Init(TextData.Get(84));
//             _modelHandle = Addressables.InstantiateAsync(scenario.patientId);
//             while (!_modelHandle.IsDone)
//             {
//                 loading.SetProgress(_modelHandle.PercentComplete, size);
//                 yield return null;
//             }
//             
//             yield return _modelHandle;
//             
//             if (_modelHandle.Status != AsyncOperationStatus.Succeeded)
//             {
//                 IsStarted = false;
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.patientId})");
//                 GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
//                 Debug.Log("inn");
//                 loading.Hide();
//                 completed?.Invoke(false);
//                 StopAllCoroutines();
//                 yield break;
//             }
//
//             loading.Hide();
//             StartCoroutine(GameManager.Instance.blackout.Show());
//             
//             var model = _modelHandle.Result;
//             _modelCameras = model.GetComponentsInChildren<Camera>();
//             _assetChildren = model.GetComponentsInChildren<Transform>(true);
//             _existingButtonByName = model.GetComponentsInChildren<Button>(true).ToDictionary(x => x.name);
//             _existingToggleByName = model.GetComponentsInChildren<Toggle>(true).ToDictionary(x => x.name);
//             
//             var audioListeners = model.transform.GetComponentsInChildren<AudioListener>();
//             for (var i = 1; i < audioListeners.Length; i++)
//                 Destroy(audioListeners[i]);
//             
//             var cleanId = scenario.patientId.Replace("DM", "");
//             _complexActions = BookDatabase.Instance.CoursesDimedus.manipulationScenarios.Where(action => action.manipulationId == cleanId).ToList();
//             _manipulationModel.Init(scenario.id, _complexActions.Count, Mode);
//             _manipulationModel.reportInfo.complexActions = _complexActions;
//             _manipulationModel.reportInfo.specializationIds = scenario.specializationIds;
//             
//             _playable = model.GetComponentInChildren<PlayableDirector>();
//             _playable.playOnAwake = false;
//             _playable.Pause();
//             ActivateModelCamera(false);
//             completed?.Invoke(true);
//         }
//
//         private void LightSetup(string location)
//         {
//             if(_assetChildren == null) return;
//             
//             var cameraTrans = _assetChildren.FirstOrDefault(x => x.name == "Camera");
//             GameManager.Instance.lightController.LightSetup(location, cameraTrans);
//         }
//
//         public IEnumerator Finish(bool isWithDebriefing, string critical = null)
//         {
//             Debug.Log("FINISH");
//             if(!IsLaunched) yield break;
//             
//             Starter.Cursor.IsBlocked = false;
//             
//             if (PlayerPrefs.HasKey("PROF_PIN"))
//                 yield return StartCoroutine(GameManager.Instance.settingsController.ShowProfPinModal());
//             
//             if(isWithDebriefing)
//                 yield return StartCoroutine(GameManager.Instance.blackout.Show());
//             
//             Pause();
//             _manipulationModel.manipulationReport = new ManipulationReport();
//             _manipulationModel.manipulationReport.onScoreAndPathSet += onScoreAndPathSet;
//             yield return StartCoroutine(_manipulationModel.manipulationReport.CreateReport(_manipulationModel.reportInfo, 
//                 !string.IsNullOrEmpty(critical)));
//             GameManager.Instance.statisticsController.Open(false);
//             
//             if(GameManager.Instance.defaultProduct == GameManager.Product.Dimedus && isWithDebriefing)
//                 GameManager.Instance.debriefingController.Init(true, critical);
//
//             if(_modelHandle.IsValid() && _modelHandle.Status == AsyncOperationStatus.Succeeded)
//                 Addressables.Release(_modelHandle);
//             
//             GameManager.Instance.starterController.SetActiveFPC(true);
//
//             _manipulationView.ShowQuestionPanel(false);
//             _manipulationView.ShowButtonLists(false);
//             _manipulationView.SetActivePanel(false);
//             _currentCam = null;
//             _modelCameras = null;
//             _assetChildren = null;
//             _existingButtonByName = null;
//             IsLaunched = false;
//             IsStarted = false;
//
//             if (GameManager.Instance.defaultProduct == GameManager.Product.Dimedus && isWithDebriefing)
//             {
//                 GameManager.Instance.mainMenuController.ShowMenu("Debriefing");
//                 if(!GameManager.Instance.debriefingController.isInit)
//                     GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
//             }
//             else
//                 GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
//             
//             if(isWithDebriefing)
//                 yield return StartCoroutine(GameManager.Instance.blackout.Hide());
//         }
//
//         private void StartProcess()
//          {
//              if(_manipulationModel.currentActionIndex >= _manipulationModel.maxSteps || _manipulationModel.currentActionIndex < 0) return;
//              
//              _manipulationView.ShowButtonLists(false);
//              var currentAction = _complexActions[_manipulationModel.currentActionIndex];
//              var startTime = _manipulationModel.currentActionIndex == 0 ? 0.0f : _complexActions[_manipulationModel.currentActionIndex-1].timeEnd;
//              var endTime = currentAction.timeEnd;
//
//              if (_manipulationModel.currentActionIndex != 0 && startTime == 0.0f)
//              {
//                  var i = 2;
//                  while (startTime == 0.0f)
//                  {
//                      if(_manipulationModel.currentActionIndex <= i) break;
//                      startTime = _complexActions[_manipulationModel.currentActionIndex - i].timeEnd;
//                      ++i;
//                  }
//              }
//              
//              PlayAnimation(startTime, endTime);
//              
//              var outPhrase = currentAction.description;
//              // if(!string.IsNullOrEmpty(outPhrase))
//              //    TextToSpeech.Instance.SetText(outPhrase, TextToSpeech.Character.Assistant, false);
//              _manipulationView.SetActionName(currentAction.heading);
//          }
//
//          private IEnumerator InitAction()
//          {
//              if (_manipulationModel.currentActionIndex >= _manipulationModel.maxSteps ||
//                  _manipulationModel.currentActionIndex < 0)
//              {
//                  _manipulationView.ShowButtonLists(false);
//                                      
//                  if(_manipulationModel.currentActionIndex >= _manipulationModel.maxSteps)
//                      StartCoroutine(Finish(true));
//                  
//                  yield break;
//              }
//
//              var currentAction = _complexActions[_manipulationModel.currentActionIndex];
//              _manipulationView.Init(currentAction.heading);
//              
//              foreach (var button in _existingButtonByName.Values)
//                  button.gameObject.SetActive(false);
//              
//              foreach (var toggle in _existingToggleByName.Values)
//                  toggle.gameObject.SetActive(false);
//              
//              
//              if(_manipulationModel.currentActionIndex != 0)
//                  ActivateModelCamera(true);
//              
//              // question
//              if (currentAction.timeEnd <= 0.0f)
//              {
//                  // single answer question
//                  if (currentAction.buttons != null)
//                  {
//                      ParseButtons(currentAction.buttons, currentAction.criticalError);
//                  }
//                  else // multiple answer question
//                  {
//                      ParseMultiselect(currentAction.multiselectTrue, currentAction.multiselectFalse);
//                  }
//                  
//
//                  var outPhrase = currentAction.description;
//                  // if(!string.IsNullOrEmpty(outPhrase))
//                  //    TextToSpeech.Instance.SetText(outPhrase, TextToSpeech.Character.Assistant, false);
//              }
//              else // timeline action
//              {
//                  StartProcess();
//              }
//          }
//
//          public void Pause()
//          {
//              if(_manipulationModel.isPaused) return;
//              
//              _manipulationModel.pauseTime = _playable.time;
//              _playable.Pause();
//
//              TextToSpeech.Instance.PauseSpeaking();
//              _manipulationModel.isPaused = true;
//          }
//
//          private IEnumerator OnProcessFinished()
//          {
//              yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
//              yield return StartCoroutine(DoctorSpeech());
//              _manipulationModel.isFinished = true;
//
//              //yield return StartCoroutine(QuestionCheck());
//
//              if (_manipulationModel.currentActionIndex + 1 < _manipulationModel.maxSteps)
//              {
//                  _manipulationModel.currentActionIndex++;
//                  StartCoroutine(InitAction());
//              }
//              else
//              {
//                  StartCoroutine(Finish(true));
//              }
//          }
//
//          private void PlayAnimation(double startTime, double endTime)
//          {
//              _playable.Play();
//              _playable.time = startTime;
//              _manipulationModel.animEndTime = endTime;
//          }
//
//          private void Update()
//          {
//              if(!IsLaunched) return;
//              
//              if (_playable != null && _playable.time.CompareTo(_manipulationModel.animEndTime) >= 0)
//              {
//                  _playable.Pause();
//                  _playable.time = 0.0f;
//                  StartCoroutine(OnProcessFinished());
//              }
//
//              if (_currentCam != null)
//              {
//                  if (Input.touchSupported)
//                  {
//                      foreach (var touch in Input.touches)
//                      {
//                          _manipulationView.m_PointerEventData = new PointerEventData(_manipulationView.eventSystem)
//                          {
//                              position = _currentCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f))
//                          };
//                          var results = new List<RaycastResult>();
//                          _manipulationView.m_Raycaster.Raycast(_manipulationView.m_PointerEventData, results);
//                          foreach (var result in results.Where(result => result.gameObject.CompareTag("Button")))
//                              result.gameObject.GetComponent<TxtButton>().button.onClick?.Invoke();
//                      }
//                  }
//                  else
//                  {
//                      if (Input.GetMouseButtonDown(0))
//                      {
//                          _manipulationView.m_PointerEventData = new PointerEventData(_manipulationView.eventSystem)
//                          {
//                              position = _currentCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f))
//                          };
//                          var results = new List<RaycastResult>();
//                          _manipulationView.m_Raycaster.Raycast(_manipulationView.m_PointerEventData, results);
//                          foreach (var result in results.Where(result => result.gameObject.CompareTag("Button")))
//                              result.gameObject.GetComponent<TxtButton>().button.onClick?.Invoke();
//                      }
//                  }
//                  
//                  foreach (var list in _buttonByPointer)
//                  {
//                      if(_currentCam == null) return;
//                      list.Key.position = _currentCam.WorldToScreenPoint(list.Value.position);
//                  }
//              }
//          }
//
//          private void RecordMistake(Button btn)
//          {
//              GameManager.Instance.blackout.RedGreenBlackout(false);
//              _manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] = -1;
//              _buttonDefaultColors.Add((btn, btn.image.color));
//              btn.image.color = _manipulationView.colors[2];
//          }
//          
//
//          private void ActivateModelCamera(bool val)
//          {
//              if(val && _currentCam == null) return;
//              
//              foreach (var cam in _modelCameras)
//                  cam.gameObject.SetActive(val);
//
//              _currentCam = val ? null : GameManager.Instance.starterController.GetCamera();
//              _manipulationView.eventSystem.gameObject.SetActive(val);
//
//              Starter.Cursor.ActivateCursor(val);
//              
//              if (val)
//                  Starter.Cursor.IsBlocked = true;
//
//              GameManager.Instance.starterController.SetActiveFPC(!val);
//          }
//
//          private IEnumerator DoctorSpeech()
//          {
//              var currentAction = _complexActions[_manipulationModel.currentActionIndex];
//              var outPhrase = currentAction.doctorSpeech;
//              if(string.IsNullOrEmpty(outPhrase)) yield break;
//              
//              //TextToSpeech.Instance.SetText(outPhrase, TextToSpeech.Character.Doctor, false);
//              yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
//          }
//          
//          private IEnumerator ChangeAction(bool isForward)
//          {
//              Pause();
//              
//              // if(isForward)
//              //     yield return StartCoroutine(QuestionCheck());
//              // else
//              //    _manipulationView.ShowQuestionPanel(false);
//              //     
//              //
//              _manipulationModel.isFinished = true;
//              if (_manipulationModel.currentActionIndex == 0 && !isForward)
//              {
//                  StartCoroutine(InitAction());
//                  yield break;
//              }
//              
//              // if (_manipulationModel.currentActionIndex == _manipulationModel.maxSteps - 1 && isForward)
//              // {
//              //     StartCoroutine(InitEndQuestions());
//              //     yield break;
//              // }
//              // else if (_manipulationModel.currentActionIndex == _manipulationModel.maxSteps - 1 && !isForward)
//              // {
//              //     StopCoroutine(InitEndQuestions());
//              // }
//              
//              _manipulationModel.currentActionIndex = isForward ? ++_manipulationModel.currentActionIndex : --_manipulationModel.currentActionIndex;
//              // var currentAction = _complexActions[_manipulationModel.currentActionIndex];
//              // var question = currentAction.question;
//              //
//              // if (!string.IsNullOrEmpty(question) &&
//              //     _manipulationModel.reportInfo.answeredQs.ContainsKey(question))
//              // {
//              //     StartCoroutine(ChangeAction(isForward));
//              //     yield break;
//              // }
//
//              StartCoroutine(InitAction());
//          }
//
//          private IEnumerator RecordCorrect(Button btn)
//          {
//              GameManager.Instance.blackout.RedGreenBlackout(true);
//              if (_manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] != -1)
//                  _manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] = 1;
//
//              var prevColor = btn.image.color;
//              btn.image.color = _manipulationView.colors[1];
//
//              yield return new WaitForSecondsRealtime(1.0f);
//
//              btn.image.color = prevColor;
//
//              foreach (var buttonDefaultColor in _buttonDefaultColors)
//                  buttonDefaultColor.Item1.image.color = buttonDefaultColor.Item2;
//              
//              _buttonDefaultColors.Clear();
//                  
//              _manipulationModel.currentActionIndex++;
//              yield return StartCoroutine(InitAction());
//          }
//
//          private void ParseButtons(string[] buttons, string criticalError)
//          {
//              var names = new List<string>();
//              var pointers = new List<string>();
//              var existingButtons = new Button[buttons.Length];
//              var actions = new List<UnityAction<Button>>();
//              var i = 0;
//              
//              buttons = buttons.ToList().OrderBy(x => Guid.NewGuid()).ToArray();
//              
//              foreach (var button in buttons)
//              {
//                  var match = Regex.Match(button, @"(.*)<(.*),(.*),(.*)>");
//                  if(!match.Success) continue;
//                  
//                  if(Mode == ScenarioModel.Mode.Learning && match.Groups[2].Value == "0") continue;
//                  
//                  names.Add(match.Groups[1].Value);
//                  pointers.Add(string.IsNullOrEmpty(match.Groups[3].Value) ? 
//                      _modelHandle.Result.name : match.Groups[3].Value);
//
//                  var existingButtonName = match.Groups[4].Value;
//                  if (!string.IsNullOrEmpty(existingButtonName))
//                  {
//                      _existingButtonByName.TryGetValue(existingButtonName, out var btn);
//                      existingButtons[i] = btn;
//                      var pointer = match.Groups[3].Value;
//                      if (!string.IsNullOrEmpty(pointer))
//                      {
//                          var objPointer = _assetChildren.FirstOrDefault(x => x.name == pointer);
//                          if(objPointer != null)
//                             btn.transform.position = _modelCameras[0].WorldToScreenPoint(objPointer.position);
//                      }
//                  }
//
//                  i++;
//                  
//                  if (match.Groups[2].Value == "1")
//                  {
//                      actions.Add(btn => { StartCoroutine(RecordCorrect(btn)); });
//                  }
//                  else
//                  {
//                      if(string.IsNullOrEmpty(criticalError))
//                         actions.Add(RecordMistake);
//                      else
//                      {
//                          actions.Add(btn =>
//                          {
//                              RecordMistake(btn);
//                              StartCoroutine(Finish(true, criticalError));
//                          });
//                      }
//                  }
//              }
//              
//              _manipulationView.SetValues(names, pointers, actions, existingButtons);
//              _manipulationView.ShowButtonLists(true);
//              
//              _buttonByPointer.Clear();
//              
//              foreach (var transform1 in _manipulationView.listByPointer)
//              {
//                  var detailOnBoj = _assetChildren.FirstOrDefault(x => x.name == transform1.Key);
//                  detailOnBoj = detailOnBoj != null ? detailOnBoj : _modelHandle.Result.transform;
//                  _buttonByPointer.Add(transform1.Value, detailOnBoj);
//              }
//
//              if (_currentCam == null)
//              {
//                  foreach (var list in _buttonByPointer)
//                  {
//                      var rectTrans = (RectTransform) list.Key;
//                      rectTrans.ForceUpdateRectTransforms();
//                      LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
//                      
//                      if(list.Value == _modelHandle.Result.transform)
//                          rectTrans.anchoredPosition = new Vector2(0.0f, 50.0f);
//                      else
//                      {
//                          var objRelLoc = _modelCameras[0].WorldToScreenPoint(list.Value.position);
//                          
//                          if(objRelLoc.y > (Screen.height - rectTrans.sizeDelta.y) || objRelLoc.y < 0.0f ||
//                             objRelLoc.x > (Screen.width - rectTrans.sizeDelta.x/2.0f) || objRelLoc.x < rectTrans.sizeDelta.x/2.0f)
//                              _manipulationView.ChangeGroup(list.Key, _modelHandle.Result.name);
//                          else
//                              rectTrans.position = objRelLoc;
//                      }
//                  }
//              }
//          }
//         
//
//          private void ParseMultiselect(string[] trueOptions, string[] falseOptions)
//          {
//              var allOptions = Mode == ScenarioModel.Mode.Learning 
//                  ? trueOptions : trueOptions.Concat(falseOptions).ToArray();
//              
//              allOptions = allOptions.OrderBy(x => Guid.NewGuid()).ToArray();
//
//              var names = new string[allOptions.Length];
//              var pointers = new string[allOptions.Length];
//              var existingToggles = new Toggle[allOptions.Length];
//              var isExternal = false;
//              
//              for(var i = 0; i < allOptions.Length; i++)
//              {
//                  var match = Regex.Match(allOptions[i], @"(.*)<(.*),(.*)>");
//                  if(!match.Success) continue;
//                  
//                  names[i] = match.Groups[1].Value;
//                  pointers[i] = match.Groups[2].Value;
//                  var toggleName = match.Groups[3].Value;
//
//                  if (!string.IsNullOrEmpty(toggleName))
//                  {
//                      _existingToggleByName.TryGetValue(toggleName, out var tgl);
//                      existingToggles[i] = tgl;
//                      var pointer = match.Groups[2].Value;
//                      if (!string.IsNullOrEmpty(pointer))
//                      {
//                          var objPointer = _assetChildren.FirstOrDefault(x => x.name == pointer);
//                          if(objPointer != null)
//                              tgl.transform.position = _modelCameras[0].WorldToScreenPoint(objPointer.position);
//                      }
//                      
//                      isExternal = true;
//                  }
//              }
//              
//              _manipulationView.SetAnswerOptions(names, pointers, existingToggles);
//              
//              _manipulationView.checkMultiselectButton[0].gameObject.SetActive(!isExternal);
//              _manipulationView.checkMultiselectButton[1].gameObject.SetActive(isExternal);
//              var checkButton = _manipulationView.checkMultiselectButton[isExternal ? 1 : 0];
//              checkButton.tmpText.text = TextData.Get(169);
//              checkButton.button.onClick.RemoveAllListeners();
//              checkButton.button.onClick.AddListener(CheckMultiselect);
//              
//              _manipulationView.ShowQuestionPanel(true);
//          }
//
//          private void CheckMultiselect()
//          {
//              var checkButton = _manipulationView.checkMultiselectButton[0].isActiveAndEnabled ? 
//                  _manipulationView.checkMultiselectButton[0] : _manipulationView.checkMultiselectButton[1];
//              
//              checkButton.tmpText.text = TextData.Get(206);
//              checkButton.button.onClick.RemoveAllListeners();
//              checkButton.button.onClick.AddListener(() =>
//              {
//                  _manipulationModel.currentActionIndex++;
//                  _manipulationView.ShowQuestionPanel(false);
//                  StartCoroutine(InitAction());
//              });
//              
//              _manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] = 1;
//              
//              var currentAction = _complexActions[_manipulationModel.currentActionIndex];
//              var activeCheckboxes = _manipulationView.checkboxes.Where(x => x.isActiveAndEnabled).ToList();
//              activeCheckboxes.AddRange(_existingToggleByName.Values.Where(x => x.isActiveAndEnabled));
//              
//              foreach (var checkbox in activeCheckboxes)
//              {
//                  checkbox.onValueChanged.RemoveAllListeners();
//                  
//                  var txt = checkbox.GetComponentInChildren<TextMeshProUGUI>(true).text;
//                  
//                  if (checkbox.isOn && currentAction.multiselectTrue.FirstOrDefault(x => x.StartsWith(txt)) != default)
//                  {
//                      checkbox.graphic.color = _manipulationView.colors[1];
//                  }
//                  else if (checkbox.isOn && 
//                      currentAction.multiselectFalse.FirstOrDefault(x => x.StartsWith(txt)) != default || 
//                      currentAction.multiselectTrue.FirstOrDefault(x => x.StartsWith(txt)) != default)
//                  {
//                      checkbox.graphic.color = _manipulationView.colors[2];
//                      checkbox.isOn = true;
//                      _manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] = -1;
//                  }
//
//                  checkbox.interactable = false;
//              }
//              
//              if(_manipulationModel.reportInfo.answers[_manipulationModel.currentActionIndex] == -1 
//                 && !string.IsNullOrEmpty(currentAction.criticalError))
//                 StartCoroutine(Finish(true, currentAction.criticalError));
//          }
//          
//     }
// }
