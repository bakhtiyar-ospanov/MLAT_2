// using System;
// using System.Collections.Generic;
// using Modules.Books;
// using Modules.WDCore;
// using UnityEngine;
// using UnityEngine.ResourceManagement.AsyncOperations;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseModel
//     {
//         public class ReportInfo
//         {
//             public string courseId;
//             public string[] specializationIds;
//             public List<CoursesDimedus.CourseScenario> complexActions;
//             public Dictionary<string, int> answeredQs;
//             public DateTime sTime;
//         }
//         public int currentActionIndex;
//         public int maxSteps;
//         public bool isFinished;
//         public bool isPaused;
//         public string audioFolder;
//         public double animEndTime;
//         public double pauseTime;
//         public DCourseReport courseReport;
//         public ReportInfo reportInfo;
//
//         public void Init(string courseId, int _maxSteps)
//         {
//             currentActionIndex = 0;
//             maxSteps = _maxSteps;
//             isFinished = true;
//             isPaused = true;
//             pauseTime = 0.0f;
//             animEndTime = double.MaxValue;
//             courseReport = null;
//             audioFolder = DirectoryPath.Speech + "/Courses/" + courseId + "/";
//             DirectoryPath.CheckDirectory(audioFolder);
//
//             reportInfo = new ReportInfo {
//                 courseId = courseId, 
//                 sTime = DateTime.Now,
//                 answeredQs = new Dictionary<string, int>()
//             };
//         }
//     }
// }
