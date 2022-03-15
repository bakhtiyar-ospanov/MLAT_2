using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.S3;
using Modules.Assets;
using Modules.Books;
using Modules.SpeechKit;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using VargatesOpenSDK;

namespace Modules.WorldCourse
{
    public class WCourseController : MonoBehaviour
    {
        private WCourseView _dCourseView;
        private WCourseModel _dCourseModel;
        private AsyncOperationHandle<SceneInstance> _sceneHandle;
        private AsyncOperationHandle<GameObject> _modelHandle;
        private PlayableDirector _playable;
        //private LookAtConstraint _lookAtConstraint;
        private bool _isInit;
        private WCourse.CourseData _courseData;

        private void Awake()
        {
            _dCourseView = GetComponent<WCourseView>();
            // _dCourseView.closeButton.onClick.AddListener(() => StartCoroutine(Finish()));
            // _dCourseModel = new WCourseModel();
            // _dCourseView.controlButtons[0].onClick.AddListener(() =>
            //     StartCoroutine(ChangeAction(false)));
            // _dCourseView.controlButtons[1].onClick.AddListener(() =>
            //     { if (_dCourseModel.isPaused) Play(); else Pause(); });
            // _dCourseView.controlButtons[2].onClick.AddListener(() =>
            //     StartCoroutine(ChangeAction(true)));
        }

        public IEnumerator Init(WorldCourseAsset info, WCourse.Course course)
        {
            var courseId = course.id;
            yield return StartCoroutine(FetchBook(info, courseId));

            var locationId = course.location;
            if (string.IsNullOrEmpty(locationId))
            {
                ShowNoResourceWarning("no_location_indicated_courseId: " + courseId);
                yield break;
            }
            
            // Resources Check
            var check = Addressables.LoadResourceLocationsAsync(locationId);
            yield return check;
            var count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No scene with this id is in Addressbales
                ShowNoResourceWarning(locationId);
                yield break;
            }
            
            yield return StartCoroutine(GameManager.Instance.addressablesS3.Init(info.uniqueId, info.accessKey,
                info.secretKey, info.serverUrl, new List<string> {info.catalogPath}));
            
            var paddedId = "rc" + courseId.PadLeft(12, '0');
            check = Addressables.LoadResourceLocationsAsync(paddedId);
            yield return check;
            count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No course with this id is in Addressbales
                ShowNoResourceWarning(paddedId);
                yield break;
                
            }
            TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Assistant, 1);
            
            GameManager.Instance.addressablesS3.RestoreDefaultWorld();

            // Scene Load
            var success = false;
            yield return StartCoroutine(GameManager.Instance.starterController.Init(locationId, false, val => success = val, info.uniqueId));
            if(!success) yield break;

            // Model load
            success = false;
            yield return StartCoroutine(LoadModel(info, courseId, val => success = val));
            if(!success) yield break;
            
            //GameManager.Instance.lightController.LightSetup(locationId, cameraTrans);
            
            //_dCourseView.SetActivePanel(true);
            _isInit = true;
        }
        
        private IEnumerator FetchBook(WorldCourseAsset info, string courseId)
        {
            GameManager.Instance.loadingController.Init(TextData.Get(254));
                
            var client = new AmazonS3Client(info.accessKey, info.secretKey, new AmazonS3Config {ServiceURL = "https://" + info.serverUrl});
            
            var split = info.courseFolderPath.Split('/').ToList();
            var folder = $"{DirectoryPath.ExternalCourses}{info.uniqueId}/Courses/";
            var bucket = split[0];
            split.RemoveAt(0);
            var path = $"{string.Join("/", split)}/{courseId}.json";
            
            DirectoryPath.CheckDirectory(folder);
            
            var loader = new BookLoaderS3(bucket, path, folder,
                s => _courseData = JsonConvert.DeserializeObject<WCourse.CourseData>(s), false, false, client);
            
            yield return new WaitUntil(() => loader.IsDone);
        }
        
        private void ShowNoResourceWarning(string id)
        {
            GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {id})");
            GameManager.Instance.loadingController.Hide();
            //GameManager.Instance.wCourseSelectorController.Init();
        }

        private IEnumerator LoadModel(WorldCourseAsset info, string courseId, Action<bool> success)
        {
            yield return StartCoroutine(GameManager.Instance.addressablesS3.Init("courses", info.accessKey,
                info.secretKey, info.serverUrl, new List<string> {info.catalogPath}));
            
            var paddedId = "rc" + courseId.PadLeft(12, '0');
            var getDownloadSize = Addressables.GetDownloadSizeAsync(paddedId);
            yield return getDownloadSize;
            var size= getDownloadSize.Result;
            Addressables.Release(getDownloadSize);
            
            var loading = GameManager.Instance.loadingController;
            loading.Init(TextData.Get(84));
            _modelHandle = Addressables.InstantiateAsync(paddedId);
            while (!_modelHandle.IsDone)
            {
                loading.SetProgress(_modelHandle.PercentComplete, size);
                yield return null;
            }
            yield return _modelHandle;
            
            if (_modelHandle.Status != AsyncOperationStatus.Succeeded)
            {
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {courseId})");

                var handle = Addressables.UnloadSceneAsync(_sceneHandle);
                yield return handle;
                
                loading.Hide();
                success?.Invoke(false);
                StopAllCoroutines();
                yield break;
            }
            
            yield return StartCoroutine(GameManager.Instance.blackout.Show());
            loading.Hide();
            
            GameManager.Instance.addressablesS3.RestoreDefaultWorld();
            
            var model = _modelHandle.Result;
            model.GetComponentsInChildren<Camera>().ToList().ForEach(x => x.enabled = false);
            //_dCourseModel.Init(courseId, _courseData.processes.Count);
            //_dCourseModel.reportInfo.complexActions = _complexActions;
            _playable = model.GetComponentInChildren<PlayableDirector>();
            if (_playable != null)
            {
                _playable.playOnAwake = false;
                _playable.Pause();
            }
            
            //Pause();
            //Play();
            GameManager.Instance.starterController.ActivateFreeMode();
            yield return StartCoroutine(GameManager.Instance.blackout.Hide());
            success?.Invoke(true);
        }
        

        // private IEnumerator Finish()
        // {
        //     // if (_dCourseModel.courseReport == null)
        //     // {
        //     //     _dCourseModel.courseReport = new MCourseReport();
        //     //     yield return StartCoroutine(_dCourseModel.courseReport.CreateReport(_dCourseModel.reportInfo));
        //     // }
        //     
        //     Pause();
        //     _isInit = false;
        //     
        //     if(_modelHandle.IsValid() && _modelHandle.Status == AsyncOperationStatus.Succeeded)
        //         Addressables.Release(_modelHandle);
        //     
        //     GameManager.Instance.starterController.SetActiveFPC(true);
        //     
        //     if(_sceneHandle.IsValid() && _sceneHandle.Status == AsyncOperationStatus.Succeeded)
        //         yield return Addressables.UnloadSceneAsync(_sceneHandle);
        //     
        //     _dCourseView.SetActivePanel(false);
        //     ActivateOldScene(true);
        // }
        
         // private void Play()
         // {
         //     _dCourseModel.isPaused = false;
         //     _dCourseView.SetPlaySprite(false);
         //     ActivateLook(true);
         //     
         //     if (_dCourseModel.isFinished)
         //         StartProcess();
         //     else
         //     {
         //         ContinueAnimation();
         //         TextToSpeech.Instance.ResumeSpeaking();
         //     }
         //     
         //     _dCourseModel.isFinished = false;
         // }

         // private void StartProcess()
         // {
         //     if(_dCourseModel.currentActionIndex >= _dCourseModel.maxSteps || _dCourseModel.currentActionIndex < 0) return;
         //     
         //     var currentAction = _complexActions[_dCourseModel.currentActionIndex];
         //     var startTime = _dCourseModel.currentActionIndex == 0 ? 0.0f : _complexActions[_dCourseModel.currentActionIndex-1].timeEnd;
         //     var endTime = currentAction.timeEnd;
         //     
         //     PlayAnimation(startTime, endTime);
         //     
         //     var outPhrase = currentAction.description;
         //     //TextToSpeech.Instance.SetText(outPhrase, TextToSpeech.Character.Assistant, false);
         //     _dCourseView.SetActionName(currentAction.heading);
         //     _dCourseView.SetActionDescription(outPhrase);
         // }

         public void Pause()
         {
             _dCourseView.SetPlaySprite(true);
             if (_playable != null)
             {
                 _dCourseModel.pauseTime = _playable.time;
                 _playable.Pause();
             }
             

             TextToSpeech.Instance.PauseSpeaking();
             _dCourseModel.isPaused = true;
         }

         // private IEnumerator ChangeAction(bool isForward)
         // {
         //     Pause();
         //     
         //     if(isForward)
         //         yield return StartCoroutine(QuestionCheck());
         //     
         //     _dCourseModel.isFinished = true;
         //     if (_dCourseModel.currentActionIndex == 0 && !isForward)
         //     {
         //         Play();
         //         yield break;
         //     }
         //     
         //     if (_dCourseModel.currentActionIndex == _dCourseModel.maxSteps - 1 && isForward)
         //     {
         //         StartCoroutine(InitEndQuestions());
         //         yield break;
         //     }
         //     
         //     _dCourseModel.currentActionIndex = isForward ? ++_dCourseModel.currentActionIndex : --_dCourseModel.currentActionIndex;
         //     Play();
         // }
         
         // private IEnumerator OnProcessFinished()
         // {
         //     yield return new WaitUntil(() => TextToSpeech.Instance.IsFinishedSpeaking());
         //     _dCourseModel.isFinished = true;
         //     
         //     yield return StartCoroutine(QuestionCheck());
         //
         //     if (_dCourseModel.currentActionIndex + 1 < _dCourseModel.maxSteps)
         //     {
         //         _dCourseModel.currentActionIndex++;
         //         Play();
         //     }
         //     else
         //     {
         //         Pause();
         //         StartCoroutine(InitEndQuestions());
         //     }
         // }

         private void PlayAnimation(double startTime, double endTime)
         {
             if (_playable != null)
             {
                 _playable.Play();
                 _playable.time = startTime;
             }
             
             _dCourseModel.animEndTime = endTime;
         }

         // private void Update()
         // {
         //     if(_isInit && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
         //         ActivateLook(false);
         //
         //     if (_playable != null && _playable.time.CompareTo(_dCourseModel.animEndTime) >= 0)
         //     {
         //         _playable.Pause();
         //         _playable.time = 0.0f;
         //         StartCoroutine(OnProcessFinished());
         //     }
         // }

         private void ContinueAnimation()
         {
             PlayAnimation(_dCourseModel.pauseTime, _dCourseModel.animEndTime);
         }

        //  private IEnumerator QuestionCheck(CoursesDimedus.CourseScenario action = null)
        //  {
        //      var currentAction = action ?? _complexActions[_dCourseModel.currentActionIndex];
        //      var question = currentAction.question;
        //      var answers = currentAction.answers;
        //
        //      if (string.IsNullOrEmpty(question) || answers.Length != 4 ||
        //          _dCourseModel.reportInfo.answeredQs.ContainsKey(question)) yield break;
        //      
        //      var correctAnswer = answers[0];
        //      var shuffledAnswers = answers.ToList().OrderBy(x => Guid.NewGuid()).ToList();
        //      _dCourseView.SetQuestion(question);
        //      _dCourseView.SetAnswers(shuffledAnswers, shuffledAnswers.IndexOf(correctAnswer));
        //      _dCourseView.ShowQuestionPanel(true);
        //
        //      Pause();
        //      yield return new WaitUntil(() => _dCourseView.isAnswered);
        //      _dCourseView.isAnswered = false;
        //      _dCourseModel.reportInfo.answeredQs.Add(question, _dCourseView.mistakeCount);
        //      
        //  }
        //
        //  private IEnumerator InitEndQuestions()
        //  {
        //      // var questions = _complexActions.Where(x => x.timeEnd <= 0.0f).ToList();
        //      // foreach (var question in questions)
        //      // {
        //      //     yield return StartCoroutine(QuestionCheck(question));
        //      // }
        //      //
        //      // _dCourseModel.courseReport = new MCourseReport();
        //      // yield return StartCoroutine(_dCourseModel.courseReport.CreateReport(_dCourseModel.reportInfo));
        //      yield break;
        //  }
        //
        //  private void CameraSetup(GameObject obj)
        //  {
        //     var allChildren = obj.transform.GetComponentsInChildren<Transform>();
        //      var cam = allChildren.FirstOrDefault(x => x.name == "Camera")?.gameObject;
        //      cameraTargetTrans = allChildren.FirstOrDefault(x => x.name == "CameraTarget");
        //
        //      if (cameraTargetTrans == null) cameraTargetTrans = new GameObject("CameraTarget").transform;
        //      
        //      if(cam.GetComponent<AudioListener>() == null) cam.AddComponent<AudioListener>();
        //      var cameraComp = cam.GetComponent<Camera>();
        //      var universal = cam.GetComponent<UniversalAdditionalCameraData>();
        //      if (universal != null) DestroyImmediate(universal);
        //      if (cameraComp != null) DestroyImmediate(cameraComp);
        //      cameraComp = cam.AddComponent<Camera>();
        //      cameraComp.nearClipPlane = 0.001f;
        //      cameraComp.usePhysicalProperties = false;
        //      cameraComp.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        //      cameraTrans = cameraComp.transform;
        //      _orbitCamera = cam.AddComponent<OrbitCamera>();
        //      _orbitCamera.minDistance = 0.001f;
        //      _lookAtConstraint = cam.GetComponent<LookAtConstraint>();
        //      if(_lookAtConstraint != null) DestroyImmediate(_lookAtConstraint);
        //      _lookAtConstraint = cam.AddComponent<LookAtConstraint>();
        //      _lookAtConstraint.AddSource(new ConstraintSource {sourceTransform = cameraTargetTrans, weight = 1});
        //      _lookAtConstraint.constraintActive = false;
        //      _orbitCamera.target = cameraTargetTrans;
        //      var dist = Vector3.Distance(cameraTrans.position, cameraTargetTrans.position);
        //      _orbitCamera.targetDistance = dist;
        //      _orbitCamera.distance = dist;
        //  }
        //  
        // private void ActivateLook(bool val)
        // {
        //     if (val && !_orbitCamera.enabled || !val && _orbitCamera.enabled) return;
        //
        //     if (!val)
        //     {
        //         Pause();
        //         var dist = Vector3.Distance(cameraTrans.position, cameraTargetTrans.position);
        //         _orbitCamera.targetDistance = dist;
        //         _orbitCamera.distance = dist;
        //     }
        //     
        //     _lookAtConstraint.constraintActive = val;
        //     _orbitCamera.enabled = !val;
        // }
        //
        // private void TempClearing()
        // {
        //     var cams = FindObjectsOfType<Camera>();
        //     
        //     foreach (var cam in cams)
        //     {
        //         var obj = cam.transform.root.gameObject;
        //         if(obj.scene.name == "DontDestroyOnLoad") continue;
        //         DestroyImmediate(obj);
        //     }
        //     
        //     var volumes = FindObjectsOfType<Volume>();
        //     foreach (var volume in volumes)
        //     {
        //         var obj = volume.gameObject;
        //         if(obj.scene.name == "DontDestroyOnLoad") continue;
        //         DestroyImmediate(obj);
        //     }
        // }

    }
}
