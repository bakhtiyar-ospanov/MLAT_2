using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Statistics
{
    public class StatisticsController : MonoBehaviour
    {        
        private StatisticsManager.Statistics _statistics;
        private int _selectedSection;
        private (StatisticsManager.Statistics.Task, int) _currentItem;
        private string _userPath;
        private StatisticsView _view;

        private void Awake()
        {
            _view = GetComponent<StatisticsView>();
            _view.filters[0].onValueChanged.AddListener(FillUpStat);
            _view.filters[1].onValueChanged.AddListener(val => FillUpStat(_view.filters[0].value));
            _view.filters[2].onValueChanged.AddListener(val => FillUpStat(_view.filters[0].value));
            _view.searchField.onValueChanged.AddListener(val => FillUpStat(_view.filters[0].value));
            _view.detailsSearchField.onValueChanged.AddListener(val => OpenItemStat(_currentItem.Item1, _currentItem.Item2));
            _view.closeButton.onClick.AddListener(() => SetActivePanel(false));
            _view.closeDetailsButton.onClick.AddListener(() =>
            {
                _currentItem = default;
                SetActiveDetailsPanel(false);
            });
#if UNITY_XR
            
            foreach (var view in _viewByProduct.Values)
            {
                view.searchField.onSelect.AddListener(val =>
                GameManager.Instance.keyboardController.OpenKeyboard(view.searchField));

                view.detailsSearchField.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.detailsSearchField));

                view.filters[0].template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
                view.filters[1].template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
                view.filters[2].template.gameObject.AddComponent<OVRRaycaster>().sortOrder = 5;
            }            
#endif

            GameManager.Instance.scenarioSelectorController.onFilteredSpecIdsChange = UpdateSpecializationOptions;
        }
        

        public void Init()
        {
            var specs = BookDatabase.Instance.MedicalBook.specializations.Select(x => x.name).ToList();
            specs.Insert(0, $"{TextData.Get(64)} ({TextData.Get(120)})");
            _view.filters[1].options = specs.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
            _view.filters[2].options = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData(TextData.Get(231)),
                new TMP_Dropdown.OptionData(TextData.Get(232)),
            };
        }

        public void Open(bool isToShow)
        {
            StartCoroutine(InitRoutine(isToShow));
        }
        
        private IEnumerator InitRoutine(bool isToShow)
        {
            _userPath = DirectoryPath.Statistics;
            var filename = "Statistics.encrypted";
            var userStat = $"{_userPath}/{filename}";;

            byte[] byteResponse = null;
            yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));

            var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
            _statistics = !string.IsNullOrEmpty(response) ? JsonConvert.DeserializeObject<StatisticsManager.Statistics>(response) : null;
            
            _view.filters[0].SetValueWithoutNotify(0);
            _view.filters[0].onValueChanged?.Invoke(0);

            if (isToShow)
            {
                if (PlayerPrefs.HasKey("PROF_PIN"))
                    yield return StartCoroutine(GameManager.Instance.settingsController.ShowProfPinModal());
                
                SetActivePanel(true);
            }
        }

        public void SetActivePanel(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.canvas.SetActive(val);
        }

        private void FillUpStat(int type)
        {
            type += 2;

            var names = new List<string>();
            var patientIfo = new List<string>();
            var actions = new List<UnityAction>();
            
            if(_statistics == null) 
            {
                _view.SetItemValues(names, patientIfo, actions);
                return;
            }
            
            List<StatisticsManager.Statistics.Task> data = null;
            switch (type)
            {
                case 0: data = _statistics.scenarios; break;
                case 1: data = _statistics.courses; break;
                case 2: data = _statistics.scenariosAcademix; break;
            }

            if (data == null)
            {
                _view.SetItemValues(names, patientIfo, actions);
                return;
            }

            var spec = _view.filters[1].value;
            var specializationId = spec != 0 ? BookDatabase.Instance.MedicalBook.specializations[spec - 1].id : "-1";

            var sortType = _view.filters[2].value;
            data = sortType == 1 ? 
                data.OrderBy(x => x.items[0].score).ToList() : 
                data.OrderByDescending(x => x.items[0].score).ToList();

            var searchValue = _view.searchField.text.ToLower();
            
            foreach (var item in data)
            {
                if(specializationId != "-1" && !item.specializationIds.Contains(specializationId)) continue;
                
                var itemTitle = "";
                var info = "";
                switch (type)
                {
                    case 0:
                        itemTitle = BookDatabase.Instance.MedicalBook.scenarios.FirstOrDefault(x => 
                            x.id == item.id)?.name;
                        break;
                    // case 1: 
                    //     itemTitle = BookDatabase.Instance.CoursesDimedus.courseById[item.id].name;
                    //     break;
                    case 2:
                        var scenario = BookDatabase.Instance.MedicalBook.scenarios.FirstOrDefault(x =>
                            x.id == item.id);
                        itemTitle = scenario?.name;
                        info = scenario?.patientInfo;
                        break;
                }
                var title = itemTitle + " (" + item.items[0].score + "%)";
                
                if(!string.IsNullOrWhiteSpace(searchValue) && !title.ToLower().Contains(searchValue)) continue;

                names.Add(title);
                patientIfo.Add(info);
                actions.Add(() =>
                {
                    _currentItem = (item, type);
                    _view.detailsSearchField.text = "";
                    OpenItemStat(item, type);
                });

                if (_currentItem != default && _currentItem.Item1.id == item.id)
                    _currentItem.Item1 = item;
            }
            
            _view.SetItemValues(names, patientIfo, actions);
            
            if (_currentItem != default)
                OpenItemStat(_currentItem.Item1, _currentItem.Item2);
        }

        private void OpenItemStat(StatisticsManager.Statistics.Task item, int type)
        {
            var path = "";
            var itemTitle = "";
            var itemInfo = "";
            switch (type)
            {
                case 0:
                    path = _userPath + "ScenarioReports/";
                    itemTitle = BookDatabase.Instance.MedicalBook.scenarios.FirstOrDefault(x => 
                        x.id == item.id)?.name;
                    break;
                // case 1: 
                //     path = _userPath + "DimedusCourseReports/";
                //     itemTitle = BookDatabase.Instance.CoursesDimedus.courseById[item.id].name;
                //     break;
                case 2: 
                    path = _userPath + "AcademixScenarioReports/";
                    var scenario = BookDatabase.Instance.MedicalBook.scenarios.FirstOrDefault(x =>
                        x.id == item.id);
                    itemTitle = scenario?.name;
                    itemInfo = scenario?.patientInfo;
                    break;
            }

            var searchValue = _view.detailsSearchField.text.ToLower();
            var names = new List<string>();
            var actions = new Dictionary<string, UnityAction>();
            var dates = new List<string>();
            var scores = new List<double>();

            foreach (var val in item.items)
            {
                var startDate = val.date + "  |  ";
                var startTime = val.time + "  |  ";
                var modeTxt = val.mode switch
                {
                    -1 => null,
                    0 => TextData.Get(207),
                    1 => TextData.Get(208),
                    2 => TextData.Get(209),
                    _ => ""
                };
                var modeTitle = string.IsNullOrEmpty(modeTxt) ? "" : modeTxt + "  |  ";
                var score = val.score + "%";
                var title = startDate + startTime + modeTitle + score;
                
                if(!string.IsNullOrWhiteSpace(searchValue) && !title.ToLower().Contains(searchValue)) continue;
                
                names.Add(title);
                if (System.IO.File.Exists(path + val.pdfName) && !actions.ContainsKey(title))
                    actions.Add(title, () => DirectoryPath.OpenPDF(path + val.pdfName));
                dates.Add(val.date + " "+ val.time);
                scores.Add(val.score);
            }
            _view.SetDetailsTitle(itemTitle, itemInfo);
            _view.SetDetailsValues(names, actions);
            _view.DrawGraph(dates, scores);
            SetActiveDetailsPanel(true);
        }
        
        private void SetActiveDetailsPanel(bool val)
        {
            _view.canvas.SetActive(!val);
            _view.detailsRoot.SetActive(val);
        }

        private void UpdateSpecializationOptions(List<string> specializations)
        {
            _view.filters[1].options.Clear();

            var specs = specializations;
            specs.Insert(0, $"{TextData.Get(64)} ({TextData.Get(120)})");
            _view.filters[1].options = specs.Select(text => new TMP_Dropdown.OptionData(text)).ToList();
        }
    }
}
