using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR;

namespace Modules.Scenario
{
    public class ScenarioSelectorController : MonoBehaviour
    {
        public enum FilterTypes
        {
            Skills,
            Competences,
            Specializations
        }
        private ScenarioSelectorView _view;
        private List<MedicalBase.Scenario> _allScenarios;
        private List<MedicalBase.Scenario> _currentScenarios;
        private Action<bool> onMenuShow;
        private AsyncOperationHandle<Texture2D> _previewHandle;

        private List<string> filteredSpecIds;
        private List<string> filteredSkillIds;
        private List<string> filteredCompetenceIds;

        public MedicalBase.Scenario SelectedScenario;
        public Action<List<string>> onFilteredSpecIdsChange;

        private void Awake()
        {
            _view = GetComponent<ScenarioSelectorView>();
            GameManager.Instance.GSAuthController.onActivationChanged += SetScenarioList;
            GameManager.Instance.settingsController.onBetaModeChange += val => { SetScenarioList(); };
            GameManager.Instance.settingsController.onTeacherModeChange += val => { SetScenarioList(); };

            _view.launchButton[0].onClick.AddListener(() => StartCoroutine(LaunchScenario(ScenarioModel.Mode.Learning)));
            _view.launchButton[1].onClick.AddListener(() => StartCoroutine(LaunchScenario(ScenarioModel.Mode.Selfcheck)));
            _view.launchButton[2].onClick.AddListener(() => StartCoroutine(LaunchScenario(ScenarioModel.Mode.Exam)));
            _view.launchButton[5].onClick.AddListener(() => StartCoroutine(GameManager.Instance.scenarioLoader.SimulateInit(SelectedScenario)));
            _view.closePopUpButton.onClick.AddListener(() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
                if(_previewHandle.IsValid())
                    Addressables.Release(_previewHandle);
            });
            _view.searchField.onValueChanged.AddListener(SearchCourse);
            _view.AddFilterListener(FilterCases);
            _view.randomScenario.onClick.AddListener(SelectRandomScenario);

#if UNITY_XR
                if (XRSettings.enabled)
                {
                    view.searchField.onSelect.AddListener(val => 
                        GameManager.Instance.keyboardController.OpenKeyboard(view.searchField));
                
                    view.filterDropdowns[0].dropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
                    view.filterDropdowns[1].dropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
                    view.filterDropdowns[2].dropdown.template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
                }
#endif
            

            onMenuShow = val =>
            {
                if(val) return;
                GameManager.Instance.mainMenuController.onMenuShow -= onMenuShow;
                GameManager.Instance.mainMenuController.RemoveModule("ScenarioSelector");
            };
        }

        // public void InitDimedusByArgs(string args)
        // {
        //     StartCoroutine(InitDimedusByArgsRoutine(args));
        // }
        //
        // private IEnumerator InitDimedusByArgsRoutine(string args)
        // {
        //     if(GameManager.Instance.scenarioController.IsLaunched)
        //         GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
        //     else
        //     {
        //         yield return StartCoroutine(Init());
        //         GameManager.Instance.mainMenuController.onMenuShow += onMenuShow;
        //     }
        //
        //     if (string.IsNullOrEmpty(args))
        //     {
        //         _view.SetFilterValue(FilterTypes.Specializations, 0);
        //         _view.SetFilterValue(FilterTypes.Competences, 0);
        //         yield break;
        //     }
        //     args = args.Replace(" ", "");
        //     var splitArgs = args.Split(',');
        //     if (splitArgs.Length > 0)
        //     {
        //         _view.SetFilterValue(FilterTypes.Specializations,
        //             1+filteredSpecIds.IndexOf(filteredSpecIds.FirstOrDefault(x => x == splitArgs[0])));
        //     }
        //
        //     if (splitArgs.Length > 1)
        //     {
        //         _view.SetFilterValue(FilterTypes.Competences,
        //             1+filteredCompetenceIds.IndexOf(filteredCompetenceIds.FirstOrDefault(x => x == splitArgs[1])));
        //     }
        // }

        public IEnumerator Init()
        {
            yield return new WaitUntil(() => !GameManager.Instance.scenarioController.IsLaunched);

            AddTabToMenu();
            SetScenarioList();
            SetFilters();
            
            if (XRSettings.enabled)
                yield return new WaitForSeconds(0.8f);
            
            if(GameManager.Instance.defaultProduct == GameManager.Product.Academix)
                GameManager.Instance.mainMenuController.ShowMenu("ScenarioSelector");
            _view.searchField.SetTextWithoutNotify("");
            SearchCourse("");
        }

        private void SetFilters()
        {
            var allSpecs = new List<string>();
            var allSkills = new List<string>();
            var allCompetences = new List<string>();

            foreach (var scenario in _allScenarios)
            {
                if(scenario.specializationIds != null)
                    allSpecs.AddRange(scenario.specializationIds);
                if(scenario.skillsIds != null)
                    allSkills.AddRange(scenario.skillsIds);
                if(scenario.competencesIds != null)
                    allCompetences.AddRange(scenario.competencesIds);
            }

            allSpecs = allSpecs.Distinct().ToList();
            allSkills = allSkills.Distinct().ToList();
            allCompetences = allCompetences.Distinct().ToList();

            var specList = BookDatabase.Instance.MedicalBook.specializations
                .Where(x => allSpecs.Contains(x.id)).ToList();

            specList = specList.OrderBy(x => x.name).ToList();
            
            var specNames = specList.Select(x => x.name).ToList();

            filteredSpecIds = specList.Select(x => x.id).ToList();
            onFilteredSpecIdsChange.Invoke(new List<string>(specNames));
            _view.SetFilter(FilterTypes.Specializations, specNames, TextData.Get(64));
            _view.SetFilter(FilterTypes.Skills, null, null);
            _view.SetFilter(FilterTypes.Competences, null, null);
            _view.SetFilterValue(FilterTypes.Specializations, _view.GetFilterValue(FilterTypes.Specializations));
        }

        private void SetScenarioList()
        {
            if(!GameManager.Instance.mainMenuController.CheckModule("ScenarioSelector")) return;

            _allScenarios = BookDatabase.Instance.MedicalBook.scenarios.
                Where(x => !string.IsNullOrEmpty(x.name)).OrderBy(x => x.name).ToList();

            if (!GameManager.Instance.isBetaTest)
                _allScenarios = _allScenarios.Where(x => x.status == "1").ToList();

            if (XRSettings.enabled)
                _allScenarios = _allScenarios.Where(x => x.skillsIds != null && !x.skillsIds.Contains("3")).ToList();

            var gsAuth = GameManager.Instance.GSAuthController;
            
            _allScenarios.ForEach(x => x.isAvailable = false);
            _allScenarios.Where(x => x.library != null && x.library.Contains("Demo")).
                ToList().ForEach(x => x.isAvailable = true);     

            if(gsAuth.isActivated && gsAuth.libraries != null)
            {
                foreach (var library in gsAuth.libraries)
                {
                    _allScenarios.Where(x => x.library != null && x.library.Contains(library)).
                        ToList().ForEach(x => x.isAvailable = true);
                }
            }

            _allScenarios = _allScenarios.OrderByDescending(x => x.isAvailable).ToList();
            
            _currentScenarios = _allScenarios;
            SearchCourse(_view.searchField.text);
        }

        public void AddTabToMenu()
        {
            GameManager.Instance.mainMenuController.AddModule("ScenarioSelector", "", 
                SetActivePanel, new [] {_view.selectorRoot.transform,});
            GameManager.Instance.mainMenuController.RemovePopUpModule("ScenarioDetails");
        }
        
        public void OpenPopup(MedicalBase.Scenario scenario)
        {
            SelectedScenario = scenario;

            var isManipulation = scenario.patientId.StartsWith("DM");
            
            _view.launchButton[0].gameObject.SetActive(!isManipulation);
            _view.launchButton[2].gameObject.SetActive(!isManipulation);
            _view.launchButton[3].gameObject.SetActive(isManipulation);
            _view.launchButton[4].gameObject.SetActive(isManipulation);
            _view.launchButton[5].gameObject.SetActive(GameManager.Instance.isBetaTest);
            
            StartCoroutine(SetPreview(scenario.id));
            
            var isTeacher = PlayerPrefs.GetInt("TEACHER_MODE") == 1;
            var scenarioName = isTeacher ? scenario.name : scenario.nameComplaintBased;
            var scenarioDescription = isTeacher ? scenario.description : scenario.descriptionComplaintBased;

            _view.SetPopUpInfo(scenarioName, scenarioDescription, scenario.patientInfo);
            GameManager.Instance.mainMenuController.AddPopUpModule("ScenarioDetails", 
                _view.SetActivePopUp, new []{_view.popUpRoot.transform});
            GameManager.Instance.mainMenuController.ShowMenu("ScenarioDetails");
        }

        public IEnumerator LaunchScenario(ScenarioModel.Mode mode)
        {
            GameManager.Instance.mainMenuController.onMenuShow -= onMenuShow;
            GameManager.Instance.mainMenuController.ShowMenu(false);
            GameManager.Instance.mainMenuController.RemovePopUpModule("ScenarioDetails");
            
            if(_previewHandle.IsValid())
                Addressables.Release(_previewHandle);
            
            if (GameManager.Instance.scenarioController.IsLaunched)
            {
                if (!PlayerPrefs.HasKey("PROF_PIN"))
                    GameManager.Instance.loadingController.Init(TextData.Get(210));
                yield return StartCoroutine(GameManager.Instance.scenarioController.Unload(false));
                yield return new WaitUntil(() => !GameManager.Instance.scenarioController.IsLaunched);
                GameManager.Instance.mainMenuController.ShowMenu(false);
            }
            
            // if (GameManager.Instance.manipulationController.IsLaunched)
            //     yield return StartCoroutine(GameManager.Instance.manipulationController.Finish(false));

            GameManager.Instance.scenarioLoader.Init(SelectedScenario, mode);
        }
        
        // public IEnumerator LaunchManipulation(ScenarioModel.Mode mode)
        // {
        //     GameManager.Instance.mainMenuController.ShowMenu(false);
        //     GameManager.Instance.mainMenuController.RemovePopUpModule("ScenarioDetails");
        //     
        //     if (GameManager.Instance.scenarioController.IsLaunched)
        //     {
        //         if (!PlayerPrefs.HasKey("PROF_PIN"))
        //             GameManager.Instance.loadingController.Init(TextData.Get(210));
        //         yield return StartCoroutine(GameManager.Instance.scenarioController.Unload(false));
        //         yield return new WaitUntil(() => !GameManager.Instance.scenarioController.IsLaunched);
        //         
        //         GameManager.Instance.mainMenuController.ShowMenu(false);
        //     }
        //     
        //     if (GameManager.Instance.manipulationController.IsLaunched)
        //         yield return StartCoroutine(GameManager.Instance.manipulationController.Finish(false));
        //     
        //     yield return StartCoroutine(GameManager.Instance.manipulationController.Init(SelectedScenario, mode));
        // }

        private void FilterCases(FilterTypes filterType, int selectedIndex)
        {
            var selectedSpecialization = _view.GetFilterValue(FilterTypes.Specializations);
            var selectedSkills = _view.GetFilterValue(FilterTypes.Skills);
            var selectedCompetences = _view.GetFilterValue(FilterTypes.Competences);

            var specializationId = selectedSpecialization != 0 ? filteredSpecIds[selectedSpecialization - 1] : "-1";
            var skillsId = selectedSkills != 0 ? filteredSkillIds[selectedSkills - 1] : "-1";
            var competencesId = selectedCompetences != 0 ? filteredCompetenceIds[selectedCompetences - 1] : "-1";
            _currentScenarios = _allScenarios;

            if (specializationId != "-1")
                _currentScenarios = _currentScenarios.Where(x => x.specializationIds != null && x.specializationIds.Contains(specializationId)).ToList();

            if (skillsId != "-1")
                _currentScenarios = _currentScenarios.Where(x => x.skillsIds != null && x.skillsIds.Contains(skillsId)).ToList();
            
            if (competencesId != "-1")
                _currentScenarios = _currentScenarios.Where(x => x.competencesIds != null && x.competencesIds.Contains(competencesId)).ToList();

            _view.searchField.SetTextWithoutNotify("");
            _view.SetValue(_currentScenarios);
        }

        private void SearchCourse(string val)
        {
            if(_view.GetFilterValue(FilterTypes.Specializations) != 0)
                _view.SetFilterValue(FilterTypes.Specializations, 0);

            var courses = string.IsNullOrEmpty(val) ?
                _currentScenarios : _currentScenarios.Where(x => x.name.ToLower().Contains(val.ToLower())).ToList();
            _view.SetValue(courses);
        }

        public void SetActivePanel(bool val)
        {
            if(_view == null) return;
            _view.SetActivePanel(val);
        }
        
        private IEnumerator SetPreview(string scenarioId)
        {
            _view.SetPreview(null);
            yield return StartCoroutine(GameManager.Instance.previewDownloader.DownloadPreview("academix",
                scenarioId, _view.SetPreview));
        }

        public void SelectRandomScenario()
        {
            int randomIndex = UnityEngine.Random.Range(0, _currentScenarios.Count);

            SelectedScenario = _currentScenarios[randomIndex];

            StartCoroutine(LaunchScenario(ScenarioModel.Mode.Exam));

        }
    }
}
