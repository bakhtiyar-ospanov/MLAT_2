using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Modules.SpeechKit;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using BookDatabase = Modules.Books.BookDatabase;

namespace Modules.Scenario
{
    public class ScenarioLoader : MonoBehaviour
    {
        public DateTime StartTime;
        public MedicalBase.Scenario CurrentScenario;
        public StatusInstance StatusInstance;
        public MedicalBase.Patient PatientInstance;
        public ScenarioModel.Mode Mode;
        [HideInInspector] public ContactPointManager contactPointManager;
        [SerializeField] private ContactPointManager contactPointManagerPrefab;
        [HideInInspector] public GameObject model;
        
        private PointDetector _pointDetector;
        private AsyncOperationHandle<GameObject> _modelHandle;

        private void Awake()
        {
            _pointDetector = GetComponent<PointDetector>();
        }

        public void Init(MedicalBase.Scenario scenario, ScenarioModel.Mode mode)
        {
            Mode = mode;
            StartCoroutine(InitRoutine(scenario));
        }

        private IEnumerator InitRoutine(MedicalBase.Scenario scenario)
        {
            Debug.Log($"Load scenario {scenario.id}: Start");
            CurrentScenario = scenario;
            
            var patientById = BookDatabase.Instance.MedicalBook.patientById;
            patientById.TryGetValue(scenario.patientId, out PatientInstance);

            if (PatientInstance == null || scenario.checkTableId == null)
            {
                Debug.Log("NO CASE OR PATIENT"); 
                Unload(); 
                yield break;
            }
            
            var patientId = "AS" + PatientInstance.id;
            var check = Addressables.LoadResourceLocationsAsync(patientId);
            yield return check;
            var count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No patient with this id is in Addressbales
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {patientId})");
                Unload();
                yield break;
            }

            PatientInstance.patientName = BookDatabase.Instance.MedicalBook.GetRandomName(PatientInstance.gender);
            PatientInstance.doctorFName = BookDatabase.Instance.MedicalBook.GetRandomName(0);
            PatientInstance.doctorMName = BookDatabase.Instance.MedicalBook.GetRandomName(1);
            TextToSpeech.Instance.SetGenderVoice(TextToSpeech.Character.Patient, PatientInstance.gender);
            
            // Load Scene
            var sceneId = SceneManager.GetActiveScene().name;
            check = Addressables.LoadResourceLocationsAsync(sceneId);
            yield return check;
            count = check.Result.Count;
            Addressables.Release(check);
            if (count == 0)
            {
                // No scene with this id is in Addressbales
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.cabinetId})");
                Unload();
                yield break;
            }

            var success = false;
            yield return StartCoroutine(GameManager.Instance.starterController.Init(scenario.cabinetId, 
                false, val => success = val));

            if (!success)
            {
                Unload();
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {scenario.cabinetId})");
                GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
                StopAllCoroutines();
                yield break;
            }
            
            // Load Status Config
            LoadStatusConfig();
            StartCoroutine(GameManager.Instance.VSMonitorController.Init(scenario));

            // Load CheckTable Config
            LoadCheckTableConfig();

            // Load Model
            success = false;
            yield return StartCoroutine(LoadModel(patientId, val => success = val));
            if(!success) yield break;

            // Physical Exam points setup
            contactPointManager = Instantiate(contactPointManagerPrefab);
            yield return StartCoroutine(_pointDetector.Init(model));
            
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            model.transform.SetParent(patientAsset.transform);
            patientAsset.model = model.transform;
            patientAsset.Init();
            
            StartCoroutine(patientAsset.LaunchPatientStateById(1));

            GameManager.Instance.scenarioController.Init(Mode);
            yield return new WaitForSeconds(1.0f);
            
            // if(XRSettings.enabled)
            //     GameManager.Instance.starterController.InitFPC();
            
            GameManager.Instance.loadingController.Hide();
            
            StartTime = DateTime.Now;

            if (XRSettings.enabled)
                yield return new WaitForSeconds(0.8f);
            
            GameManager.Instance.mainMenuController.ShowMenu(true);
            Debug.Log($"Load scenario {scenario.id}: End");
        }

        public IEnumerator SimulateInit(MedicalBase.Scenario scenario)
        {
            Debug.Log($"Simulate scenario {scenario.id}: Start");
            CurrentScenario = scenario;
            StartTime = DateTime.Now;
            Mode = ScenarioModel.Mode.Learning;
            
            // Load Status Config
            LoadStatusConfig();

            // Load CheckTable Config
            LoadCheckTableConfig();
            
            GameManager.Instance.scenarioController.CreateModel(Mode, true);
            
            yield return StartCoroutine(GameManager.Instance.checkTableController.Finish(true));
            
            GameManager.Instance.debriefingController.OpenReport();
            
            GameManager.Instance.checkTableController.Clean();
            GameManager.Instance.scenarioController.CleanModel();
            GameManager.Instance.debriefingController.Clean();
            Unload();
            Debug.Log($"Simulate scenario {scenario.id}: End");
        }

        private void LoadStatusConfig()
        {
            Debug.Log($"Load scenario: ParseStatus");
            var status = new StatusInstance.Status();
            StatusInstance = new StatusInstance {FullStatus = status};
            status.checkUps = new List<StatusInstance.Status.CheckUp>();

            var fullCheckups = BookDatabase.Instance.allCheckUps;
            var checkUpIds = new List<string>();
            var checkups = BookDatabase.Instance.MedicalBook.checkUps;

            foreach (var checkUpId in CurrentScenario.checkUpIds)
            {
                var checkUp = checkups.FirstOrDefault(x => x.id == checkUpId);
                if(checkUp == null) continue;
                
                var refList = BookDatabase.Instance.allCheckUps;
                var positions = checkUp.order.Split('.').Select(int.Parse).ToList();
                
                for (var i = 0; i < positions.Count; i++)
                {
                    var tempCheckup = refList?.FirstOrDefault(x => x.order == positions[i]);
                    checkUpIds.Add(tempCheckup?.id);
                    
                    if (i != positions.Count - 1)
                        refList = tempCheckup?.children;
                }
            }
            
            checkUpIds = checkUpIds.Distinct().ToList();

            CheckupTraverse(fullCheckups, status.checkUps, checkUpIds);
        }
        
        private void LoadCheckTableConfig()
        {
            Debug.Log($"Load scenario: ParseCheckTable");
            var checkTable = new CheckTable
            {
                id = "Academix",
                actions = new List<CheckTable.Action>()
            };

            var checkTables = BookDatabase.Instance.MedicalBook.checkTables;
            
            checkTable.actions.Add(new CheckTable.Action
            {
                id = "Academix",
                level = "0"
            });

            var i = 0;
            foreach (var table in checkTables)
            {
                checkTable.actions.Add(new CheckTable.Action
                {
                    id = "" + i,
                    name = table.name,
                    level = "1",
                    gradeMax = table.grade,
                    trigger = table.trigger,
                    unordered = true
                });
                i++;
            }

            GameManager.Instance.checkTableController.checkTableInstance = checkTable;
        }

        private IEnumerator LoadModel(string patientId, Action<bool> success)
        {
            Debug.Log($"Load scenario: LoadModel");
            var getDownloadSize = Addressables.GetDownloadSizeAsync(patientId);
            yield return getDownloadSize;
            var size= getDownloadSize.Result;
            Addressables.Release(getDownloadSize);
            
            var loading = GameManager.Instance.loadingController;
            loading.Init(TextData.Get(84));
            _modelHandle = Addressables.InstantiateAsync(patientId);
            while (!_modelHandle.IsDone)
            {
                loading.SetProgress(_modelHandle.PercentComplete, size);
                yield return null;
            }
            yield return _modelHandle;

            if (_modelHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Unload();
                GameManager.Instance.warningController.ShowWarning($"{TextData.Get(188)} (Key: {patientId})");
                GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
                loading.Hide();
                success?.Invoke(false);
                StopAllCoroutines();
                yield break;
            }
            
            model = _modelHandle.Result;
            LoadAnimator(patientId.Substring(2));
            success?.Invoke(true);
        }

        private void LoadAnimator(string patientId)
        {
            Debug.Log($"Load scenario: LoadAnimator");
            var animator = model.GetComponent<Animator>();
            animator.applyRootMotion = true;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            var animatorController = Resources.Load($"Animations/Characters/{patientId}/animator") as RuntimeAnimatorController;
            if(animatorController == null)
                animatorController = Resources.Load($"Animations/Characters/defaultAnimator") as RuntimeAnimatorController;
            animator.runtimeAnimatorController = animatorController;
        }

        public void Unload()
        {
            Debug.Log($"Load scenario: Unload");
            if(_modelHandle.IsValid() && _modelHandle.Status != AsyncOperationStatus.Failed)
                Addressables.Release(_modelHandle);
            CurrentScenario = null;
            PatientInstance = null;
            StatusInstance = null;
            
            if(contactPointManager != null)
                DestroyImmediate(contactPointManager);
        }
        
        public string ReplaceCaseVariables(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
       
            if (val.Contains("<age>"))
                val = val.Replace("<age>", PatientInstance.age);
            if (val.Contains("<patientName>"))
                val = val.Replace("<patientName>", PatientInstance.patientName);
            if (val.Contains("<pulse>"))
                val = val.Replace("<pulse>", CurrentScenario.pulse.ToString());
            if (val.Contains("<breath>"))
                val = val.Replace("<breath>", CurrentScenario.breath.ToString());
            if (val.Contains("<bp>"))
                val = val.Replace("<bp>", CurrentScenario.pressure);
            if (val.Contains("<saturation>"))
                val = val.Replace("<saturation>", CurrentScenario.saturation.ToString());
            if (val.Contains("<temperature>"))
                val = val.Replace("<temperature>", CurrentScenario.temperature);
            if (val.Contains("<doctorName>"))
            {
                var doctorName = PlayerPrefs.GetInt("USE_NAME_IN_DIALOG") == 1
                    ? GameManager.Instance.profileController.GetUsername()
                    : null;
                
                doctorName = string.IsNullOrEmpty(doctorName) ? PlayerPrefs.GetInt("DOCTOR_GENDER") == 0
                    ? PatientInstance.doctorFName
                    : PatientInstance.doctorMName : doctorName;
                
                val = val.Replace("<doctorName>", doctorName);
            }
            
            return val;
        }

        private void CheckupTraverse(List<FullCheckUp> checkUps, List<StatusInstance.Status.CheckUp> shortCheckups, List<string> checkUpIds)
        {
            if(checkUps == null || checkUps.Count == 0) return;
            
            foreach (var checkup in checkUps)
            {
                var shortCheckup =
                    new StatusInstance.Status.CheckUp
                {
                    id = checkup.id,
                    children = new List<StatusInstance.Status.CheckUp>()
                };
                
                if (checkUpIds.Contains(checkup.id))
                    shortCheckups.Add(shortCheckup);
                
                CheckupTraverse(checkup.children, shortCheckup.children, checkUpIds);
            }
        }
    }
}
