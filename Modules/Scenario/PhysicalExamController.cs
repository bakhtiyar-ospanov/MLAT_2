using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.SpeechKit;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace Modules.Scenario
{
    public class PhysicalExamController : MonoBehaviour
    {
        public string appGroupId;
        public bool isBlocked;
        public Dictionary<string, int> passedAnswers = new Dictionary<string, int>();
        
        private AudioSource _audioSource;
        private PhysicalExamView _view;
        private string triggerKeyword;
        private FullCheckUp _currentCheckup;
        private List<FullCheckUp> _randomCheckUps;
        private Dictionary<string, StatusInstance.Status.CheckUp> _allPhysicalCheckupsByGroup = new Dictionary<string, StatusInstance.Status.CheckUp>();
        private Dictionary<string, List<StatusInstance.Status.CheckUp>> _allPhysicalCheckupAnswersByGroup = new Dictionary<string, List<StatusInstance.Status.CheckUp>>();
        private Camera _cam;
        private Transform _rightHand;
        private GameObject _vrPointer;
        private Transform _vrCamera;
        private bool _isExternalRegister;
        private int _cameraType;
        private Vector3 _pointerDownPos;
        
        
        private void Awake()
        {
            _view = GetComponent<PhysicalExamView>();
            _view.tglMute.onValueChanged.AddListener(MuteAudio);
            _view.normaButton.onClick.AddListener(() => RegisterAnswer(1));
            _view.pathologyButton.onClick.AddListener(() => RegisterAnswer(2));
            _view.backButton.onClick.AddListener(() => SetActivePanel(false));
            _view.backButtonPointSelector.onClick.AddListener(() => ShowPointSelector(false));

            if (XRSettings.enabled)
            {
                var fpcVR = GameManager.Instance.starterController.GetFPСVR();
                _rightHand = fpcVR.GetRightHand();
                _vrPointer = fpcVR.GetPointer();
                _vrCamera = fpcVR.GetCamera().transform;
            }

            GameManager.Instance.starterController.CameraInit += val => _cam = val;

            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
            isBlocked = true;
        }

        private void LateUpdate()
        {
            if (isBlocked || EventSystem.current == null || _cam == null) return;

            if (XRSettings.enabled)
            {
#if UNITY_XR
                if (_vrPointer.activeSelf) return;

                SetContactPointName(null);
                
                var ray = new Ray(_rightHand.position + _rightHand.forward * 0.05f, _rightHand.forward);
                if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
                {
                    SetContactPointName(hit.transform.name);
                    
                    if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.95f)
                        Init(hit.transform.name);
                }
#endif
            }
            else if (Input.touchSupported)
            {
                foreach (var touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Ended)
                    {
                        var ray = _cam.ScreenPointToRay(touch.position);
                        if (Physics.Raycast(ray, out var hit, 100f) && hit.transform != null &&
                            !GameManager.Instance.starterController.IsSwiping())
                        {
                            Init(hit.transform.name);
                        }
                    }
                }
            }
            
            
            {
                SetContactPointName(null);

                if(EventSystem.current.IsPointerOverGameObject()) return;

                
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100.0f) && hit.transform != null)
                {
                    SetContactPointName(hit.transform.name);
                    
                    if (Input.GetMouseButtonDown(0))
                        _pointerDownPos = Input.mousePosition;
                    
                    if(Input.GetMouseButtonUp(0) && Vector3.Distance(_pointerDownPos, Input.mousePosition) < 0.5f)
                        Init(hit.transform.name);                    
                }
            }
        }

        public void Init()
        {
            _allPhysicalCheckupsByGroup.Clear();
            _allPhysicalCheckupAnswersByGroup.Clear();
            passedAnswers.Clear();
            ParseCheckUpGroups(Config.PercussionParentId);
            ParseCheckUpGroups(Config.PalpationParentId);
            ParseCheckUpGroups(Config.AuscultationParentId);
            ParseCheckUpGroups(Config.VisualExamParentId);
            ParseCheckUpGroups(Config.InstrResearchParentd);
            ParseCheckUpGroups(Config.LabResearchParentId);
        }

        private void ParseCheckUpGroups(string groupId)
        {
            var checkUps = GameManager.Instance.scenarioLoader.StatusInstance.FullStatus.checkUps
                .FirstOrDefault(x => x.id == groupId);
            
            if (checkUps == null) return;
            
            _allPhysicalCheckupsByGroup.Add(groupId, checkUps);
            var checkups = _allPhysicalCheckupsByGroup[groupId];
            _allPhysicalCheckupAnswersByGroup.Add(groupId, checkups.children.ToList());
        }

        public void Init(string pointName)
        {
            if (pointName == "EXIT")
            {
                OnModeExit(true); 
                return;
            }

            if (pointName == "MENU")
            {
                GameManager.Instance.assetMenuController.Init(GameManager.Instance.assetController.patientAsset);
                return;
            }

            var checkUps = GetCheckUpsByPointId(pointName);

            if (checkUps.Count == 1)
            {
                RegisterCheckUp(checkUps[0]); 
            }
            
            else if (checkUps.Count > 1)
            {
                var isContains = false;
                var actions = new List<UnityAction>();
                var actionNames = new List<string>();

                var currentTrigger = GameManager.Instance.scenarioController.GetCurrentTrigger();
                var triggerHolder = currentTrigger.Item2 ?? currentTrigger.Item1;

                foreach (var item in checkUps)
                {
                    if (triggerHolder.requiredAction.ContainsKey(item.id))
                    {
                        RegisterCheckUp(item);
                        isContains = true;
                        break;
                    }

                    actions.Add(() =>
                    {
                        ShowPointSelector(false);
                        RegisterCheckUp(item);
                    });
                    actionNames.Add(item.GetPointInfo().name);
                }

                if (actions.Count > 0 && actionNames.Count > 0 && !isContains)
                {
                    _view.SetPointButtons(actions, actionNames);
                    ShowPointSelector(true);
                }
            }
        }
        
        public void RegisterCheckUp(FullCheckUp checkUp, bool isExternal = false)
        {
            _isExternalRegister = isExternal;
            _currentCheckup = checkUp;
            _view.HideMedia();
            
            SetCheckUpInfo(_currentCheckup);
            SetActivePanel(true);
        }

        public List<FullCheckUp> GetCheckUpsByPointId(string pointId)
        {
            var checkUpGroup = GetGroupCheckUp();
            var checkUps = new List<FullCheckUp>();

            if (checkUpGroup != null)
                foreach (var checkUp in checkUpGroup.children)
                {
                    var pointInfo = checkUp.GetPointInfo();
                    if (pointInfo == null)
                        continue;
                    if (pointInfo.values.Contains(pointId))
                        checkUps.Add(checkUp.GetInfo());
                }

            if(_randomCheckUps != null) 
                checkUps.AddRange(from randomCheckUp in _randomCheckUps 
                    let pointInfo = randomCheckUp.GetPointInfo() 
                    where pointInfo != null where pointInfo.values.Contains(pointId) select randomCheckUp);

            return checkUps;
        }

        public StatusInstance.Status.CheckUp GetGroupCheckUp(string appGroup = null)
        {
            var groupId = appGroup ?? appGroupId;
            if (groupId == null) return null;
            return _allPhysicalCheckupsByGroup.ContainsKey(groupId) ? _allPhysicalCheckupsByGroup[groupId] : null;
        }
        
        public IEnumerator SetAPPGroupId(string val, string triggerWord, List<FullCheckUp> randomCheckUps)
        {
            appGroupId = val;
            triggerKeyword = triggerWord;
            _randomCheckUps = randomCheckUps;
            isBlocked = false;
            yield break;
        }

        private IEnumerator SwitchCamera(int type)
        {
            if(type == _cameraType) yield break;
            _cameraType = type;
            
            Debug.Log("Switch camera " + type);
            yield return StartCoroutine(GameManager.Instance.blackout.Show());
            GameManager.Instance.starterController.ActivateAnotherFPC(type);
            if(GameManager.Instance.assetController.patientAsset != null)
                GameManager.Instance.starterController.LookAt(GameManager.Instance.assetController.patientAsset.currentPoint);
            
            yield return StartCoroutine(GameManager.Instance.blackout.Hide());
        }
        
        private void SetActivePanel(bool val)
        {
            StopAudio();
            
            if(val)
                ShowPointSelector(false);
            
            _view.SetActivePanel(val);
        }

        private void StopAudio()
        {
            if( _audioSource != null && _audioSource.isPlaying)
                _audioSource.Stop();
        }
        
        public void ShowPointSelector(bool val)
        {
            if(val)
                SetActivePanel(false);
            _view.ShowPointSelector(val);
        }

        private void RegisterAnswer(int answer)
        {
            if (passedAnswers.ContainsKey(_currentCheckup.id))
                passedAnswers[_currentCheckup.id] = answer;
            else
                passedAnswers.Add(_currentCheckup.id, answer);

            var info = _view.simpleTxts[1].text + ": " + _view.simpleTxts[0].text;
            GameManager.Instance.diseaseHistoryController.AddNewValue(appGroupId, _currentCheckup.id, info);
            
            SetActivePanel(false);

            if (_isExternalRegister)
            {
                appGroupId = null;
                _isExternalRegister = false;
                _view.backButton.gameObject.SetActive(true);
            }
            
            GameManager.Instance.checkTableController.RegisterTriggerInvoke(triggerKeyword);
        }

        private void LoadMedia(Dictionary<string, Object> media)
        {
            foreach (var medium in media)
            {
                if (medium.Key.Contains("mp3") || medium.Key.Contains("ogg") || medium.Key.Contains("wav"))
                {
                    _view.ShowAudioMedia();
                    _audioSource.clip = (AudioClip) medium.Value;
                    if(Math.Abs(_audioSource.volume - 1.0f) < 0.01f)
                        _audioSource.Play();
                } 
                else if (medium.Key.Contains("jpg") || medium.Key.Contains("jpeg") || medium.Key.Contains("png"))
                {
                    _view.ShowImageMedia((Texture2D) medium.Value);
                }
            }
        }

        private void MuteAudio(bool val)
        {
            if (!val)
            {
                _audioSource.Stop();
                _audioSource.volume = 0.0f;
                _view.tglOffObj.SetActive(true);
            }
            else
            {
                _audioSource.Play();
                _audioSource.volume = 1.0f;
                _view.tglOffObj.SetActive(false);
            }
        }

        public void SetContactPointName(string pointName)
        {
            if(isBlocked) return;

            if (string.IsNullOrEmpty(pointName))
            {
                HideContactPoint();
                return;
            }
            
            if (pointName == "EXIT")
            {
                _view.SetContactPointName(TextData.Get(110));
                return;
            }
            
            if (pointName == "MENU")
            {
                _view.SetContactPointName(TextData.Get(237));
                return;
            }

            var checkUps = GetCheckUpsByPointId(pointName);
            var title = "";
            foreach (var checkUp in checkUps)
            {
                var pointInfo = checkUp.GetPointInfo();
                if(pointInfo == null || pointInfo.values.Count != 1)
                    continue;
                title = pointInfo.name;
                break;

            }

            if (string.IsNullOrEmpty(title))
            {
                foreach (var checkUp in checkUps)
                {
                    var pointInfo = checkUp.GetPointInfo();
                    if(pointInfo == null)
                        continue;
                    title = pointInfo.name;
                    break;

                }
            }

            if (XRSettings.enabled)
            {
                var objTransform = _view.contactPointCanvas.transform;
                objTransform.position = _vrCamera.position + _vrCamera.forward * 0.6f - new Vector3(0.0f, 0.15f, 0.0f);
                objTransform.rotation = Quaternion.LookRotation(objTransform.position - _vrCamera.position);
            }
            _view.SetContactPointName(title);
        }

        public void HideContactPoint()
        {
            _view.HideContactPoint();
        }
        

        public string ReplaceCaseVariables(string description)
        {
            var caseInstance = GameManager.Instance.scenarioLoader.CurrentScenario;
            if (description == null)
                return "";
            if (description.Contains("<pulse>"))
            {
                description = description.Replace("<pulse>", caseInstance.pulse.ToString());
                AddInstrumentalByTag("6188");
            }
            if (description.Contains("<breath>"))
            {
                description = description.Replace("<breath>", caseInstance.breath.ToString());
                AddInstrumentalByTag("6187");
            }

            return description;
        }
        
        private void AddInstrumentalByTag(string checkUpId)
        {
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var rootResearch = caseInstance.FullStatus.checkUps.FirstOrDefault(x => 
                x.id == Config.InstrResearchParentd);
            
            if(rootResearch == null) return;

            foreach (var checkUp in rootResearch.children)
            {
                var question = checkUp.children.FirstOrDefault(x => x.id == checkUpId);
                if(question == null) continue;
               
                var answerId = question.children.Count > 0 ? question.children[0].id : "No_pathology";
                var answerTxt = question.children.Count > 0 ? question.children[0].GetInfo().name : TextData.Get(215);
               
                GameManager.Instance.diseaseHistoryController.AddNewValue(Config.InstrResearchParentd, question.id, 
                    question.GetInfo().name);
                GameManager.Instance.diseaseHistoryController.ExpandValue(Config.InstrResearchParentd, 
                    question.id, answerId, answerTxt);
                
                GameManager.Instance.instrumentalSelectorController.passedAnswers.Add(checkUpId);
                break;
            }
        }

        private void SetCheckUpInfo(FullCheckUp fullCheckUp)
        {
            var pointName = fullCheckUp.GetPointInfo().name;
            _view.SetTitle(pointName);
            
            var groupCheckUp = GetGroupCheckUp();
            string description;

            var caseCheckUp = groupCheckUp.children.FirstOrDefault(x => x.id == fullCheckUp.id && x.children.Count != 0);
            if (caseCheckUp != null)
            {
                description = caseCheckUp.children[0].GetInfo().name;
                StartCoroutine(caseCheckUp.children[0].GetMedia(LoadMedia));
            } 
            else
            {
                description = fullCheckUp.GetPointInfo().description;
                StartCoroutine(fullCheckUp.GetPointInfo().GetMedia(LoadMedia));
            }

            description = ReplaceCaseVariables(description);
            _view.SetDescription(description);
        }

        public void OnModeExit(bool isShowMenu = false, bool isOnScenarioExit = false, bool isCameraSwitch = true)
        {
            if (isBlocked)
                return;
            
            isBlocked = true;

            StopAudio();

            if (appGroupId == Config.AuscultationParentId)
            {
                var stethoscope = GameManager.Instance.assetController.GetAssetById("VA308");
                if(stethoscope != null) stethoscope.gameObject.SetActive(true);
            }
            else if(appGroupId == Config.VisualExamParentId)
            {
                if(!isOnScenarioExit)
                    GameManager.Instance.visualExamController.AddToDiseaseHistory();
                var flashlight = GameManager.Instance.assetController.GetAssetById("VA318");
                if(flashlight != null) flashlight.gameObject.SetActive(true);
            }
            
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            if (patientAsset != null)
            {
                patientAsset.BlockRigidBody(false);
                patientAsset.isForcedStraightHead = false;
                StartCoroutine(patientAsset.ControlHead("HeadStraight"));
            }
            if(!string.IsNullOrEmpty(triggerKeyword) && !isOnScenarioExit)
                GameManager.Instance.checkTableController.RegisterTriggerInvoke(triggerKeyword);

            GameManager.Instance.scenarioLoader.contactPointManager.ActivateCurrentSet(false);
            GameManager.Instance.physicalExamController.HideContactPoint();
            
            if(!isOnScenarioExit)
                GameManager.Instance.assetController.patientAsset.ShowExternalMenuButton(true);

            appGroupId = null;

            SetActivePanel(false);
            ShowPointSelector(false);

            if(isShowMenu)
                GameManager.Instance.mainMenuController.ShowMenu(true);
        }

        // public List<StatusInstance.Status.CheckUp> GetCheckUpAnswersByGroup(string appGroup)
        // {
        //     List<StatusInstance.Status.CheckUp> triggerCheckups = null;
        //     if (_allPhysicalCheckupAnswersByGroup.ContainsKey(appGroup))
        //         triggerCheckups = _allPhysicalCheckupAnswersByGroup[appGroup];
        //
        //     if (triggerCheckups == null || triggerCheckups.Count == 0)
        //     {
        //         GameManager.Instance.diseaseHistoryController.RemoveGroup(appGroup);
        //         return new List<StatusInstance.Status.CheckUp>();
        //     }
        //     
        //     GameManager.Instance.checkTableController.onRequiredTriggersChange += val => 
        //             StartCoroutine(GameManager.Instance.scenarioController.AnnouncePointHint(appGroup, val));
        //     return triggerCheckups;
        // }

        public StatusInstance.Status.CheckUp GetCheckUpById(string id)
        {
            var checkup =  (from groupCheckup in _allPhysicalCheckupAnswersByGroup 
                from checkUp in groupCheckup.Value where checkUp.id == id select checkUp).FirstOrDefault();

            if (checkup == null)
            {
                checkup =  (from groupCheckup in _allPhysicalCheckupAnswersByGroup 
                    from checkUp in groupCheckup.Value
                    from childCheckUp in checkUp.children
                    where childCheckUp.id == id select childCheckUp).FirstOrDefault();
            }
            return checkup;
        }
        
        public ScenarioController.Trigger.Action GetCorrectAction(string groupId)
        {
            var actionName = "";
            var caseInstance = GameManager.Instance.scenarioLoader.StatusInstance;
            var mainCheckUp = caseInstance.FullStatus.checkUps.FirstOrDefault(x => x.id == groupId);

            if (mainCheckUp != null)
            {
                foreach (var answer in mainCheckUp.children)
                {
                    actionName +=  answer.GetInfo().name + ": ";
                    var pointInfo = answer.GetPointInfo();
                    
                    if (groupId == Config.VisualExamParentId && (pointInfo?.values == null || pointInfo.values.Count == 0))
                        actionName += GetVisualExamNoPointAnswer(answer);
                    else
                    {
                        if(answer.children.Count > 0)
                            actionName +=  answer.children[0].GetInfo().name + " - " + TextData.Get(112);
                        else
                            actionName +=  pointInfo.description + " - " + TextData.Get(74);
                    }
                    
                    actionName += "\n\n";
                }
            }

            actionName = GameManager.Instance.scenarioLoader.ReplaceCaseVariables(actionName);
            return string.IsNullOrEmpty(actionName) ? null :
                new ScenarioController.Trigger.Action {actionName = actionName};
        }

        private string GetVisualExamNoPointAnswer(StatusInstance.Status.CheckUp answer)
        {
            var answers = string.Join(", ",answer.children.Select(x => x.GetInfo().name).ToArray());
            if (string.IsNullOrEmpty(answers)) answers = answer.GetPointInfo().description;
            return answers;
        }
        public bool GetAnswerCheckState()
        {
            return _view != null && _view.root.activeSelf;
        }
    }
}
