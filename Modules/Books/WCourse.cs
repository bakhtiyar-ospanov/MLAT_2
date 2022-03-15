using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Books
{
    public class WCourse
    {
        public class Course
        {
            [JsonProperty(PropertyName = "wd_id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "description")] public string description;
            [JsonProperty(PropertyName = "filter")] public string[] filter;
            [JsonProperty(PropertyName = "location")] public string location;
            [JsonIgnore] public Texture2D preview;
        }

        public List<string> courseFilter;

        public class CourseData
        {
            public class Object
            {
                [JsonProperty(PropertyName = "id")] public string id;
                [JsonProperty(PropertyName = "name")] public string name;
                [JsonProperty(PropertyName = "description")] public string description;
                [JsonProperty(PropertyName = "collider")] public string collider;
                [JsonProperty(PropertyName = "links_unit_id")] public string links_unit_id;
            }
            
            public class Process
            {
                [JsonProperty(PropertyName = "id")] public string id;
                [JsonProperty(PropertyName = "description")] public string description;
                [JsonProperty(PropertyName = "detailing")] public string detailing;
                [JsonProperty(PropertyName = "animation_start")] public float animationStart;
                [JsonProperty(PropertyName = "animation_time")] public float animationTime;
                [JsonProperty(PropertyName = "filter")] public string[] filter;
                [JsonProperty(PropertyName = "instruments")] public string[] instruments;
                [JsonProperty(PropertyName = "mesh")] public string mesh;
            }

            [JsonProperty(PropertyName = "Objects")] public List<Object> objects;
            [JsonProperty(PropertyName = "Processes")] public List<Process> processes;
        }
        

        [JsonProperty(PropertyName = "Courses")] public List<Course> courses;

        public void CreateDictionaries()
        {
            // Mechanicum Courses
            courseFilter = courses?.SelectMany(x => x.filter).Distinct().ToList();
        }
    }
}
