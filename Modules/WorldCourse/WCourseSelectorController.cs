using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.S3;
using Modules.Assets;
using Modules.Books;
using Modules.Scenario;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;
using VargatesOpenSDK;

namespace Modules.WorldCourse
{
    public class WCourseSelectorController : MonoBehaviour
    {
        public enum FilterTypes
        {
            Key,
            Category
        }
        
        public enum SortTypes
        {
            AZ,
            ZA,
            New,
            Old
        }

        private WCourseSelectorView _selectorView;
        private WCourse.Course _selectedCourse;
        private WCourse wCourse;
        private List<WCourse.Course> _allCourses;
        private List<WCourse.Course> _currentCourses;
        private (bool, bool) _sortDirs;
        private SortTypes _currentSort;
        private Coroutine _initRoutine;
        private WorldCourseAsset _info;
        
        private void Awake()
        {
            _selectorView = GetComponent<WCourseSelectorView>();
            _selectorView.launchButton.onClick.AddListener(LaunchCourse);
            _selectorView.searchField.onValueChanged.AddListener(SearchCourse);
            _selectorView.sortByDateButton.onClick.AddListener(() => SortCourses(1));
            _selectorView.sortByNameButton.onClick.AddListener(() => SortCourses(2));
            _selectorView.AddFilterListener(FilterCourses);
        }

        public void Init(WorldCourseAsset info)
        {
            if(_initRoutine != null) return;
            _initRoutine = StartCoroutine(InitRoutine(info));
        }

        private IEnumerator InitRoutine(WorldCourseAsset info)
        {
            GameManager.Instance.warningController.ShowWarning(TextData.Get(341));
            
            yield return StartCoroutine(FetchBook(info));
            yield return StartCoroutine(InitPreviews(info));

            _sortDirs = (false, false);
            _info = info;
            _allCourses = wCourse.courses.Where(x => !string.IsNullOrEmpty(x.name)).ToList();
            _currentCourses = _allCourses;

            _selectorView.SetFilter(FilterTypes.Category, wCourse.courseFilter, TextData.Get(162));
            
            _selectorView.searchField.SetTextWithoutNotify("");
            _selectorView.SetActivePanel(true);
            SortCourses(1);
            _initRoutine = null;
            
            GameManager.Instance.warningController.HideWarningView();
        }

        private IEnumerator InitPreviews(WorldCourseAsset info)
        {
            var localPath = $"{DirectoryPath.ExternalCourses}{info.uniqueId}/Previews";
            DirectoryPath.CheckDirectory(localPath);
            
            var previewInfo = new PreviewDownloader.PreviewInfo
            {
                pathOut = localPath,
                infoPath = Path.Combine(localPath, "Info.json"),
                serverDateById = new Dictionary<string, DateTime>(),
                previewPath = info.previewsFolderPath
            };
            
            yield return StartCoroutine(GameManager.Instance.previewDownloader.Init(info.uniqueId, previewInfo));
        }
        
        private IEnumerator FetchBook(WorldCourseAsset info)
        {
            var client = new AmazonS3Client(info.accessKey, info.secretKey, new AmazonS3Config {ServiceURL = "https://" + info.serverUrl});
            
            var split = info.courseListPath.Split('/').ToList();
            var folder = $"{DirectoryPath.ExternalCourses}{info.uniqueId}/";
            var bucket = split[0];
            split.RemoveAt(0);
            var path = string.Join("/", split);
            
            var loader = new BookLoaderS3(bucket, path, folder,
                s => wCourse = JsonConvert.DeserializeObject<WCourse>(s), false, false, client);
            
            yield return new WaitUntil(() => loader.IsDone);
            
            wCourse?.CreateDictionaries();
        }

        public void OpenPopup(WCourse.Course course)
        {
            _selectedCourse = course;
            _selectorView.ShowPopUp(course.name, course.description, course.filter, course.preview);
        }

        private void LaunchCourse()
        {
            _selectorView.SetActivePanel(false);
            StartCoroutine(GameManager.Instance.wCourseController.Init(_info, _selectedCourse));
        }

        private void FilterCourses(FilterTypes filterType, int selectedIndex)
        {
            var selectedCategory = _selectorView.GetFilterValue(FilterTypes.Category);
            var category = selectedCategory != 0 ? wCourse.courseFilter[selectedCategory-1] : "-1";

            _currentCourses = _allCourses;

            if (category != "-1")
                _currentCourses = _currentCourses.Where(x => x.filter != null && x.filter.Contains(category)).ToList();
            
            SearchCourse(_selectorView.searchField.text);
        }

        private void SearchCourse(string val)
        {
            _currentCourses = _currentSort switch
            {
                SortTypes.New => _currentCourses.OrderBy(x => x.id).ToList(),
                SortTypes.Old => _currentCourses.OrderByDescending(x => x.id).ToList(),
                SortTypes.AZ => _currentCourses.OrderBy(x => x.name).ToList(),
                SortTypes.ZA => _currentCourses.OrderByDescending(x => x.name).ToList(),
                _ => _currentCourses
            };

            var courses = string.IsNullOrEmpty(val) ?
                _currentCourses : _currentCourses.Where(x => x.name.ToLower().Contains(val.ToLower())).ToList();
            _selectorView.SetValue(courses);
        }

        private void SortCourses(int type)
        {
            switch (type)
            {
                case 1:
                    _currentSort = _sortDirs.Item1 ? SortTypes.New : SortTypes.Old;
                    _selectorView.sortTitle.text = _sortDirs.Item1 ? TextData.Get(337) : TextData.Get(338);
                    _sortDirs.Item1 = !_sortDirs.Item1;
                    break;
                case 2:
                    _currentSort = _sortDirs.Item2 ? SortTypes.AZ : SortTypes.ZA;
                    _selectorView.sortTitle.text = _sortDirs.Item2 ? TextData.Get(339) : TextData.Get(340);
                    _sortDirs.Item2 = !_sortDirs.Item2;
                    break;
            }
            
            SearchCourse(_selectorView.searchField.text);
            StopAllCoroutines();
            StartCoroutine(SetPreviews());
        }

        private IEnumerator SetPreviews()
        {
            foreach (var course in _currentCourses)
                yield return StartCoroutine(GetPreview(course));
        }

        private IEnumerator GetPreview(WCourse.Course course)
        {
            if(course.preview != null) yield break;
            
            var previewId = "cp" + course.id;
            yield return StartCoroutine(GameManager.Instance.previewDownloader.DownloadPreview(_info.uniqueId,
                previewId, val => course.preview = val));
        }
    }
}
