using System;
using System.Collections.Generic;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Modules.WorldCourse
{
    public class WCourseModel
    {
        public class ReportInfo
        {
            public string courseId;
            public string[] specializationIds;
            public List<CoursesDimedus.CourseScenario> complexActions;
            public Dictionary<string, int> answeredQs;
            public DateTime sTime;
        }
        public int currentActionIndex;
        public int maxSteps;
        public bool isFinished;
        public bool isPaused;
        public string audioFolder;
        public double animEndTime;
        public double pauseTime;
        public AsyncOperationHandle<GameObject> goHandle;
        //public MCourseReport courseReport;
        public ReportInfo reportInfo;

        public void Init(string courseId, int _maxSteps)
        {
            currentActionIndex = 0;
            maxSteps = _maxSteps;
            isFinished = true;
            isPaused = true;
            pauseTime = 0.0f;
            animEndTime = 100.0f;
            //courseReport = null;
            audioFolder = DirectoryPath.Speech + "/Courses/" + courseId + "/";
            DirectoryPath.CheckDirectory(audioFolder);

            reportInfo = new ReportInfo {
                courseId = courseId, 
                sTime = DateTime.Now,
                answeredQs = new Dictionary<string, int>()
            };
        }
    }
}
