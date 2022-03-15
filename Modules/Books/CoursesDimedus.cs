using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Books
{
    public class CoursesDimedus
    {
        public class Course
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "status")] public string status;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "location")] public string location;
            [JsonProperty(PropertyName = "specializations")] public string[] specializations;
            [JsonProperty(PropertyName = "skills")] public string[] skills;
            [JsonProperty(PropertyName = "description")] public string description;
            [JsonIgnore] public Texture2D preview;
        }
        
        public class CourseScenario
        {
            [JsonProperty(PropertyName = "courseId")] public string courseId;
            [JsonProperty(PropertyName = "timeEnd")] public float timeEnd;
            [JsonProperty(PropertyName = "heading")] public string heading;
            [JsonProperty(PropertyName = "description")] public string description;
            [JsonProperty(PropertyName = "pointer")] public string pointer;
            [JsonProperty(PropertyName = "command")] public string command;
            [JsonProperty(PropertyName = "question")] public string question;
            [JsonProperty(PropertyName = "answers")] public string[] answers;
            [JsonProperty(PropertyName = "filter")] public string[] filters;
        }

        public class ManipulationScenario
        {
            [JsonProperty(PropertyName = "manipulationId")] public string manipulationId;
            [JsonProperty(PropertyName = "timeEnd")] public float timeEnd;
            [JsonProperty(PropertyName = "heading")] public string heading;
            [JsonProperty(PropertyName = "description")] public string description;
            [JsonProperty(PropertyName = "buttons")] public string[] buttons;
            [JsonProperty(PropertyName = "multiselect_true")] public string[] multiselectTrue;
            [JsonProperty(PropertyName = "multiselect_false")] public string[] multiselectFalse;
            [JsonProperty(PropertyName = "doctorSpeech")] public string doctorSpeech;
            [JsonProperty(PropertyName = "criticalError")] public string criticalError;
        }
        
        [JsonProperty(PropertyName = "Courses")] public List<Course> courses;
        [JsonProperty(PropertyName = "CourseScenarios")] public List<CourseScenario> courseScenarios;
        [JsonProperty(PropertyName = "ManipulationScenarios2")] public List<ManipulationScenario> manipulationScenarios;
        
        public Dictionary<string, Course> courseById;

        public void CreateDictionaries()
        {
            courseById = courses?.Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
        }
    }
}
