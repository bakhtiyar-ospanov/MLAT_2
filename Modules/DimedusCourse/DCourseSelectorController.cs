// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using Modules.Books;
// using Modules.WDCore;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseSelectorController : MonoBehaviour
//     {
//         public enum FilterTypes
//         {
//             Skills,
//             Specializations
//         }
//
//         private DCourseSelectorView _selectorView;
//         private string _selectedCourseId;
//         private List<CoursesDimedus.Course> _allCourses;
//         private List<CoursesDimedus.Course> _currentCourses;
//         private (bool, bool) _sortDirs;
//         
//         private void Awake()
//         {
//             _selectorView = GetComponent<DCourseSelectorView>();
//             _selectorView.launchButton.onClick.AddListener(LaunchCourse);
//             _selectorView.searchField.onValueChanged.AddListener(SearchCourse);
//             _selectorView.sortByDateButton.onClick.AddListener(() => SortCourses(1));
//             _selectorView.sortByNameButton.onClick.AddListener(() => SortCourses(2));
//             _selectorView.AddFilterListener(FilterCourses);
//         }
//
//         public void Init()
//         {
//             _sortDirs = (false, false);
//             _allCourses = BookDatabase.Instance.CoursesDimedus.courses.Where(x => !string.IsNullOrEmpty(x.name)).ToList();
//             
//             if (!GameManager.Instance.isDevMode)
//                 _allCourses = _allCourses.Where(x => x.status == "1").ToList();
//             
//             _currentCourses = _allCourses;
//
//             _selectorView.SetFilter(FilterTypes.Specializations, 
//                 BookDatabase.Instance.MedicalBook.specializations.Select(x => x.name).ToList(), 
//                 TextData.Get(64));
//             _selectorView.SetFilter(FilterTypes.Skills, 
//                 BookDatabase.Instance.MedicalBook.skills.Select(x => x.name).ToList(), 
//                 TextData.Get(66));
//             
//             _selectorView.searchField.SetTextWithoutNotify("");
//             _selectorView.SetActivePanel(true);
//             SortCourses(1);
//         }
//         
//
//         public void OpenPopup(CoursesDimedus.Course course)
//         {
//             _selectedCourseId = course.id;
//             var specialization = "";
//             if (course.specializations != null)
//             {
//                 for (var i = 0; i < course.specializations.Length; ++i)
//                 {
//                     BookDatabase.Instance.MedicalBook.specializationById.TryGetValue(course.specializations[i], out var sp);
//                     specialization += i == 0 ? sp?.name : ", " + sp?.name;
//                 }
//             }
//
//             var skills = "";
//             if (course.skills != null)
//             {
//                 for (var i = 0; i < course.skills.Length; ++i)
//                 {
//                     BookDatabase.Instance.MedicalBook.skillById.TryGetValue(course.skills[i], out var sp);
//                     skills += i == 0 ? sp?.name : ", " + sp?.name;
//                 }
//             }
//
//             _selectorView.ShowPopUp(course.name, course.description, specialization, 
//                 skills, course.preview);
//         }
//
//         private void LaunchCourse()
//         {
//             _selectorView.SetActivePanel(false);
//             StartCoroutine(GameManager.Instance.dCourseController.Init(_selectedCourseId));
//         }
//
//         private void FilterCourses(FilterTypes filterType, int selectedIndex)
//         {
//             var selectedSpecialization = _selectorView.GetFilterValue(FilterTypes.Specializations);
//             var selectedSkills = _selectorView.GetFilterValue(FilterTypes.Skills);
//             var specializationId = selectedSpecialization != 0 ? BookDatabase.Instance.MedicalBook.specializations[selectedSpecialization-1].id : "-1";
//             var skillsId = selectedSkills != 0 ? BookDatabase.Instance.MedicalBook.skills[selectedSkills-1].id : "-1";
//
//             _currentCourses = _allCourses;
//             
//             if (specializationId != "-1")
//                 _currentCourses = _currentCourses.Where(x => x.specializations == null || x.specializations.Contains(specializationId)).ToList();
//
//             if (skillsId != "-1")
//                 _currentCourses = _currentCourses.Where(x => x.skills == null || x.skills.Contains(skillsId)).ToList();
//
//             SearchCourse(_selectorView.searchField.text);
//         }
//         
//         private void SearchCourse(string val)
//         {
//             var courses = string.IsNullOrEmpty(val) ?
//                 _currentCourses : _currentCourses.Where(x => x.name.ToLower().Contains(val.ToLower())).ToList();
//             _selectorView.SetValue(courses);
//         }
//
//         private void SortCourses(int type)
//         {
//             switch (type)
//             {
//                 case 1:
//                     _currentCourses = _sortDirs.Item1 ? _currentCourses.OrderBy(x => x.id).ToList()
//                             : _currentCourses.OrderByDescending(x => x.id).ToList();
//                     _selectorView.sortTitle.text = _sortDirs.Item1 ? TextData.Get(151) : TextData.Get(152);
//                     _sortDirs.Item1 = !_sortDirs.Item1;
//                     break;
//                 case 2:
//                     _currentCourses = _sortDirs.Item2 ? _currentCourses.OrderBy(x => x.name).ToList()
//                         : _currentCourses.OrderByDescending(x => x.name).ToList();
//                     _selectorView.sortTitle.text = _sortDirs.Item2 ? TextData.Get(153) : TextData.Get(154);
//                     _sortDirs.Item2 = !_sortDirs.Item2;
//                     break;
//             }
//             
//             _selectorView.SetValue(_currentCourses);
//             StopAllCoroutines();
//             StartCoroutine(SetPreviews());
//         }
//
//         private IEnumerator SetPreviews()
//         {
//             foreach (var course in _currentCourses)
//                 yield return StartCoroutine(GetPreview(course));
//         }
//
//         private IEnumerator GetPreview(CoursesDimedus.Course course)
//         {
//             if(course.preview != null) yield break;
//             
//             var previewId = "PVDC" + course.id;
//             var check = Addressables.LoadResourceLocationsAsync(previewId);
//             yield return check;
//             var count = check.Result.Count;
//             Addressables.Release(check);
//             if(count == 0) yield break;
//             
//             var handle = Addressables.LoadAssetAsync<Texture2D>(previewId);
//             yield return handle;
//             
//             if(handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
//                 course.preview = handle.Result;
//         }
//     }
// }
