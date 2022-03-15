// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Modules.Books;
// using Modules.WDCore;
// using Modules.SpeechKit;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.Animations;
// using UnityEngine.EventSystems;
// using UnityEngine.Playables;
// using UnityEngine.Rendering;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using UnityEngine.ResourceManagement.ResourceProviders;
// using UnityEngine.SceneManagement;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseController : MonoBehaviour
//     {
//         private DCourseView _dCourseView;
//         private DCourseModel _dCourseModel;
//         private AsyncOperationHandle<SceneInstance> _sceneHandle;
//         private AsyncOperationHandle<GameObject> _modelHandle;
//         private PlayableDirector _playable;
//         private List<CoursesDimedus.CourseScenario> _complexActions;
//         private OrbitCamera _orbitCamera;
//         private bool _isInit;
//         private Transform cameraTrans;
//         private Transform cameraTargetTrans;
//         private LightmapData[] _prevLightMapData;
//         private LightProbes _prevLightProbes;
//         public Action<int> onActionChanged;
//
//         private void Awake()
//         {
//             _dCourseView = GetComponent<DCourseView>();
//             _dCourseView.closeButton.onClick.AddListener(() => StartCoroutine(Finish()));
//             _dCourseModel = new DCourseModel();
//             _dCourseView.controlButtons[0].onClick.AddListener(() =>
//                 StartCoroutine(ChangeAction(false)));
//             _dCourseView.controlButtons[1].onClick.AddListener(() =>
//                 { if (_dCourseModel.isPaused) Play(); else Pause(); });
//             _dCourseView.controlButtons[2].onClick.AddListener(() =>
//                 StartCoroutine(ChangeAction(true)));
//             _dCourseView.timelineSlider.onValueChanged.AddListener(ScrollTimeline);
//             _dCourseView.volumeButton.onClick.AddListener(ControlVolume);
//             onActionChanged += _dCourseView.SetSegment;
//         }
//
//         public IEnumerator Init(string courseId)
//         {
//             BookDatabase.Instance.CoursesDimedus.courseById.TryGetValue(courseId, out var course);
//             if (string.IsNullOrEmpty(course?.location))
//             {
//                 ShowNoResourceWarning("no_location_indicated_courseId: " + courseId);
//                 yield break;
//             }
//             
//             // Resources Check
//             var check = Addressables.LoadResourceLocationsAsync(course?.location);
//             yield return check;
//             var count = check.Result.Count;
//             Addressables.Release(check);
//             if (count == 0)
//             {
//                 // No scene with this id is in Addressbales
//                 ShowNoResourceWarning(course?.location);
//                 yield break;
//             }
//             
//             check = Addressables.LoadResourceLocationsAsync("DC" + courseId);
//             yield return check;
//             count = check.Result.Count;
//             Addressables.Release(check);
//             if (count == 0)
//             {
//                 // No course with this id is in Addressbales
//                 ShowNoResourceWarning("DC" + courseId);
//                 yield break;
//             }
//             
//             ActivateOldScene(false);
//             TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Assistant, 0);
//             
//             // Scene Load
//             var success = false;
//             yield return StartCoroutine(LoadScene(course?.location, val => success = val));
//             if(!success) yield break;
//             
//             // Model load
//             success = false;
//             yield return StartCoroutine(LoadModel(courseId, val => success = val));
//             if(!success) yield break;
//             
//             GameManager.Instance.lightController.LightSetup(course?.location, cameraTrans);
//
//             SliderSetup();
//             _dCourseView.Init();
//             _dCourseView.SetActivePanel(true);
//
//             var actionButtons = _complexActions.Where(x => x.timeEnd > 0.0f).
//                 Select((t, i) => (i, t.heading)).ToList();
//             _dCourseView.SetValue(actionButtons);
//
//             yield return new WaitUntil(() => _dCourseView._recyclableScrollRect.IsInitialized);
//             onActionChanged?.Invoke(0);
//             _isInit = true;
//             
//             StartCoroutine(GameManager.Instance.blackout.Hide());
//             Play();
//         }
//         
//         private void ShowNoResourceWarning(string id)
//         {
//             GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {id})");
//             GameManager.Instance.dCourseSelectorController.Init();
//         }
//
//         private IEnumerator LoadScene(string sceneId, Action<bool> success)
//         {
//             var getDownloadSize = Addressables.GetDownloadSizeAsync(sceneId);
//             yield return getDownloadSize;
//             var size= getDownloadSize.Result;
//             Addressables.Release(getDownloadSize);
//             
//             _sceneHandle = Addressables.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
//             var loading = GameManager.Instance.loadingController;
//             loading.Init(TextData.Get(4));
//             while (!_sceneHandle.IsDone)
//             {
//                 loading.SetProgress(_sceneHandle.PercentComplete, size);
//                 yield return null;
//             }
//
//             if (_sceneHandle.Status != AsyncOperationStatus.Succeeded)
//             {
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {sceneId})");
//                 GameManager.Instance.dCourseSelectorController.Init();
//                 loading.Hide();
//                 ActivateOldScene(true);
//                 success?.Invoke(false);
//                 StopAllCoroutines();
//                 yield break;
//             }
//
//             SceneManager.SetActiveScene(_sceneHandle.Result.Scene);
//             LightProbes.TetrahedralizeAsync();
//             
//             //TODO delete later
//             TempClearing();
//             
//             GameManager.Instance.postProcessController.Init();
//             
//             success?.Invoke(true);
//         }
//         
//         private IEnumerator LoadModel(string courseId, Action<bool> success)
//         {
//             // Get size of the model
//             var getDownloadSize = Addressables.GetDownloadSizeAsync("DC" + courseId);
//             yield return getDownloadSize;
//             var size= getDownloadSize.Result;
//             Addressables.Release(getDownloadSize);
//             
//             // Instantiate model
//             var loading = GameManager.Instance.loadingController;
//             loading.Init(TextData.Get(84));
//             _modelHandle = Addressables.InstantiateAsync("DC" + courseId);
//             while (!_modelHandle.IsDone)
//             {
//                 loading.SetProgress(_modelHandle.PercentComplete, size);
//                 yield return null;
//             }
//             yield return _modelHandle;
//             
//             if (_modelHandle.Status != AsyncOperationStatus.Succeeded)
//             {
//                 GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {courseId})");
//                 GameManager.Instance.dCourseSelectorController.Init();
//                 
//                 var handle = Addressables.UnloadSceneAsync(_sceneHandle);
//                 yield return handle;
//                 
//                 loading.Hide();
//                 ActivateOldScene(true);
//                 success?.Invoke(false);
//                 StopAllCoroutines();
//                 yield break;
//             }
//             
//             yield return StartCoroutine(GameManager.Instance.blackout.Show());
//             loading.Hide();
//
//             var model = _modelHandle.Result;
//             model.transform.position = Vector3.zero;
//             model.transform.eulerAngles = Vector3.zero;
//             var course = BookDatabase.Instance.CoursesDimedus.courseById[courseId];
//             
//             _complexActions = BookDatabase.Instance.CoursesDimedus.courseScenarios.Where(action => action.courseId == courseId).ToList();
//             _dCourseModel.Init(courseId, _complexActions.Count(x => x.timeEnd > 0.0f));
//             _dCourseModel.reportInfo.complexActions = _complexActions;
//             _dCourseModel.reportInfo.specializationIds = course.specializations;
//             
//             _playable = model.GetComponentInChildren<PlayableDirector>();
//             _playable.playOnAwake = false;
//             _playable.time = 0.0f;
//             _playable.Pause();
//             CameraSetup(model);
//
//             GameManager.Instance.starterController.SetActiveFPC(false);
//
//             success?.Invoke(true);
//         }
//
//         private void ActivateOldScene(bool val)
//         {
//             var objs = SceneManager.GetActiveScene().GetRootGameObjects();
//             foreach (var o in objs)
//                 o.SetActive(val);
//             
//             if (!val)
//             {
//                 _prevLightMapData = LightmapSettings.lightmaps;
//                 _prevLightProbes = LightmapSettings.lightProbes;
//                 LightmapSettings.lightmaps = null;
//                 LightmapSettings.lightProbes = null;
//             }
//             else
//             {
//                 LightmapSettings.lightmaps = _prevLightMapData;
//                 LightmapSettings.lightProbes = _prevLightProbes;
//                 _prevLightMapData = null;
//                 _prevLightProbes = null;
//             }
//             
//             GameManager.Instance.mainMenuController.isBlocked = !val;
//             GameManager.Instance.starterController.SetKinematic(!val);
//         }
//
//         private IEnumerator Finish()
//         {
//             if (_dCourseModel.courseReport == null)
//             {
//                 _dCourseModel.courseReport = new DCourseReport();
//                 yield return StartCoroutine(_dCourseModel.courseReport.CreateReport(_dCourseModel.reportInfo));
//                 GameManager.Instance.statisticsController.Open(false);
//             }
//             
//             Pause();
//             AudioListener.volume = 1.0f;
//             _isInit = false;
//             
//             if(_modelHandle.IsValid() && _modelHandle.Status == AsyncOperationStatus.Succeeded)
//                 Addressables.Release(_modelHandle);
//             
//             GameManager.Instance.starterController.SetActiveFPC(true);
//
//             if(_sceneHandle.IsValid() && _sceneHandle.Status == AsyncOperationStatus.Succeeded)
//                 yield return Addressables.UnloadSceneAsync(_sceneHandle);
//             
//             _dCourseView.SetActivePanel(false);
//             ActivateOldScene(true);
//             StopAllCoroutines();
//         }
//         
//          private void Play()
//          {
//              _dCourseModel.isPaused = false;
//              _dCourseView.SetPlaySprite(false);
//              ActivateLook(true);
//              
//              if (_dCourseModel.isFinished)
//                  StartProcess();
//              else
//              {
//                  ContinueAnimation();
//                  TextToSpeech.Instance.ResumeSpeaking();
//              }
//              
//              _dCourseModel.isFinished = false;
//          }
//
//          private void StartProcess()
//          {
//              if(_dCourseModel.currentActionIndex >= _dCourseModel.maxSteps || _dCourseModel.currentActionIndex < 0) return;
//              
//              var currentAction = _complexActions[_dCourseModel.currentActionIndex];
//              var startTime = _dCourseModel.currentActionIndex == 0 ? 0.0f : _complexActions[_dCourseModel.currentActionIndex-1].timeEnd;
//              var endTime = currentAction.timeEnd;
//              
//              PlayAnimation(startTime, endTime);
//              
//              var outPhrase = currentAction.description;
//              //TextToSpeech.Instance.SetText(outPhrase ,TextToSpeech.Character.Assistant, false);
//              _dCourseView.SetActionName(currentAction.heading);
//              //_dCourseView.SetActionDescription(outPhrase);
//          }
//
//          public void Pause()
//          {
//              _dCourseView.SetPlaySprite(true);
//              _dCourseModel.pauseTime = _playable.time;
//              _playable.Pause();
//
//              TextToSpeech.Instance.PauseSpeaking();
//              _dCourseModel.isPaused = true;
//          }
//
//          private IEnumerator ChangeAction(bool isForward)
//          {
//              Pause();
//              
//              if(isForward)
//                  yield return StartCoroutine(QuestionCheck());
//              
//              _dCourseModel.isFinished = true;
//              if (_dCourseModel.currentActionIndex == 0 && !isForward)
//              {
//                  Play();
//                  yield break;
//              }
//              
//              if (_dCourseModel.currentActionIndex == _dCourseModel.maxSteps - 1 && isForward)
//              {
//                  StartCoroutine(InitEndQuestions());
//                  yield break;
//              }
//              
//              _dCourseModel.currentActionIndex = isForward ? ++_dCourseModel.currentActionIndex : --_dCourseModel.currentActionIndex;
//              onActionChanged?.Invoke(_dCourseModel.currentActionIndex);
//              Play();
//          }
//          
//          private IEnumerator OnProcessFinished()
//          {
//              yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
//              _dCourseModel.isFinished = true;
//              
//              yield return StartCoroutine(QuestionCheck());
//
//              if (_dCourseModel.currentActionIndex + 1 < _dCourseModel.maxSteps)
//              {
//                  _dCourseModel.currentActionIndex++;
//                  onActionChanged?.Invoke(_dCourseModel.currentActionIndex);
//                  Play();
//              }
//              else
//              {
//                  Pause();
//                  StartCoroutine(InitEndQuestions());
//              }
//          }
//
//          private void PlayAnimation(double startTime, double endTime)
//          {
//              _playable.Play();
//              _playable.time = startTime;
//              _dCourseModel.animEndTime = endTime;
//          }
//
//          private void Update()
//          {
//              if (_isInit && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
//              {
//                  if (Input.touchSupported && Input.touchCount > 0 && Input.touches.Any(x => x.phase == TouchPhase.Began))
//                  {
//                     ActivateLook(false);
//                  }
//                  else if (Input.GetMouseButtonDown(0))
//                  {
//                     ActivateLook(false);
//                  }
//              }
//
//              if (_playable != null &&  _playable.time.CompareTo(_dCourseModel.animEndTime) >= 0)
//              {
//                  _playable.Pause();
//                  _playable.time = 0.0f;
//                  StartCoroutine(OnProcessFinished());
//              }
//              else if (_playable != null && _playable.time > 0.0f && _playable.time.CompareTo(_dCourseModel.animEndTime) < 0 && !_dCourseModel.isPaused)
//              {
//                  var time = _playable.time;
//                  _dCourseView.timelineSlider.SetValueWithoutNotify((float) time);
//                  _dCourseView.SetCurrentTime(time);
//              }
//          }
//
//          private void ContinueAnimation()
//          {
//              PlayAnimation(_dCourseModel.pauseTime, _dCourseModel.animEndTime);
//          }
//
//          private IEnumerator QuestionCheck(CoursesDimedus.CourseScenario action = null)
//          {
//              var currentAction = action ?? _complexActions[_dCourseModel.currentActionIndex];
//              var question = currentAction.question;
//              var answers = currentAction.answers;
//
//              if (string.IsNullOrEmpty(question) || answers.Length != 4 ||
//                  _dCourseModel.reportInfo.answeredQs.ContainsKey(question)) yield break;
//              
//              var correctAnswer = answers[0];
//              var shuffledAnswers = answers.ToList().OrderBy(x => Guid.NewGuid()).ToList();
//              _dCourseView.SetQuestion(question);
//              _dCourseView.SetAnswers(shuffledAnswers, shuffledAnswers.IndexOf(correctAnswer));
//              _dCourseView.ShowQuestionPanel(true);
//
//              Pause();
//              yield return new WaitUntil(() => _dCourseView.isAnswered);
//              _dCourseView.isAnswered = false;
//              _dCourseModel.reportInfo.answeredQs.Add(question, _dCourseView.mistakeCount);
//              
//          }
//
//          private IEnumerator InitEndQuestions()
//          {
//              var questions = _complexActions.Where(x => x.timeEnd <= 0.0f).ToList();
//              foreach (var question in questions)
//              {
//                  yield return StartCoroutine(QuestionCheck(question));
//              }
//              
//              _dCourseModel.courseReport = new DCourseReport();
//              yield return StartCoroutine(_dCourseModel.courseReport.CreateReport(_dCourseModel.reportInfo));
//              GameManager.Instance.statisticsController.Open(false);
//          }
//
//          private void CameraSetup(GameObject obj)
//          {
//             var allChildren = obj.transform.GetComponentsInChildren<Transform>();
//              var cam = allChildren.FirstOrDefault(x => x.name == "Camera")?.gameObject;
//              cameraTargetTrans = allChildren.FirstOrDefault(x => x.name == "Camera_Target" || x.name == "CameraTarget");
//
//              if (cameraTargetTrans == null)
//                  cameraTargetTrans = new GameObject("CameraTarget").transform;
//              
//              if(cam.GetComponent<AudioListener>() == null) cam.AddComponent<AudioListener>();
//              var cameraComp = cam.GetComponent<Camera>();
//              cameraTrans = cameraComp.transform;
//              _orbitCamera = cam.AddComponent<OrbitCamera>();
//              _orbitCamera.minDistance = 0.001f;
//              
//              _orbitCamera.target = cameraTargetTrans;
//              var dist = Vector3.Distance(cameraTrans.position, cameraTargetTrans.position);
//              _orbitCamera.targetDistance = dist;
//              _orbitCamera.distance = dist;
//          }
//
//          private void ActivateLook(bool val)
//         {
//             if (_orbitCamera == null || val && !_orbitCamera.enabled || !val && _orbitCamera.enabled) return;
//
//             if (!val)
//             {
//                 Pause();
//                 var dist = Vector3.Distance(cameraTrans.position, cameraTargetTrans.position);
//                 _orbitCamera.targetDistance = dist;
//                 _orbitCamera.distance = dist;
//             }
//             
//             _orbitCamera.enabled = !val;
//         }
//
//         private void ScrollTimeline(float val)
//         {
//             _dCourseView.SetCurrentTime(val);
//             var action = _complexActions.FindLastIndex(x => x.timeEnd > 0.0f && x.timeEnd < val);
//             LaunchActionByIndex(action == -1 ? 0 : action);
//         }
//
//         public void LaunchActionByIndex(int actionIndex)
//         {
//             Pause();
//             _dCourseModel.currentActionIndex = actionIndex;
//             onActionChanged?.Invoke(_dCourseModel.currentActionIndex);
//             _dCourseModel.isFinished = true;
//             Play();
//         }
//
//         private void SliderSetup()
//         {
//             var lastAction = _complexActions.LastOrDefault(x => x.timeEnd > 0.0f);
//             var totalTime = lastAction?.timeEnd ?? 0.0f;
//             _dCourseView.SetTotalTime(totalTime);
//             _dCourseView.timelineSlider.SetValueWithoutNotify(0.0f);
//             _dCourseView.timelineSlider.minValue = 0.0f;
//             _dCourseView.timelineSlider.maxValue = totalTime;
//         }
//
//         private void ControlVolume()
//         {
//             var isMuted = AudioListener.volume == 0.0f;
//             AudioListener.volume = isMuted ? 1.0f : 0.0f;
//             _dCourseView.SetVolumeIcon(!isMuted);
//         }
//         
//         private void TempClearing()
//         {
//             var cams = FindObjectsOfType<Camera>();
//             
//             foreach (var cam in cams)
//             {
//                 var obj = cam.transform.root.gameObject;
//                 if(obj.scene.name == "DontDestroyOnLoad") continue;
//                 DestroyImmediate(obj);
//             }
//             
//             var volumes = FindObjectsOfType<Volume>();
//             foreach (var volume in volumes)
//             {
//                 var obj = volume.gameObject;
//                 if(obj.scene.name == "DontDestroyOnLoad") continue;
//                 DestroyImmediate(obj);
//             }
//         }
//
//     }
// }
