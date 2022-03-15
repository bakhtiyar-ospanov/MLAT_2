// using System;
// using System.Collections.Generic;
// using Modules.Books;
// using Modules.WDCore;
// using Modules.DimedusCourse;
//
// namespace Modules.Manipulation
// {
//     public class ManipulationModel
//     {
//         public class ReportInfo
//         {
//             public string scenarioId;
//             public string[] specializationIds;
//             public List<CoursesDimedus.ManipulationScenario> complexActions;
//             public int[] answers;
//             public Scenario.ScenarioModel.Mode mode;
//             public DateTime sTime;
//         }
//         public int currentActionIndex;
//         public int maxSteps;
//         public bool isFinished;
//         public bool isPaused;
//         public string audioFolder;
//         public double animEndTime;
//         public double pauseTime;
//         public ManipulationReport manipulationReport;
//         public ReportInfo reportInfo;
//
//         public void Init(string scenarioId, int _maxSteps, Scenario.ScenarioModel.Mode mode)
//         {
//             currentActionIndex = 0;
//             maxSteps = _maxSteps;
//             isFinished = true;
//             isPaused = true;
//             pauseTime = 0.0f;
//             animEndTime = 100.0f;
//             manipulationReport = null;
//             audioFolder = DirectoryPath.Speech + "/Scenarios/" + scenarioId + "/";
//             DirectoryPath.CheckDirectory(audioFolder);
//
//             reportInfo = new ReportInfo {
//                 scenarioId = scenarioId, 
//                 sTime = DateTime.Now,
//                 answers = new int[_maxSteps],
//                 mode = mode
//             };
//         }
//     }
// }
