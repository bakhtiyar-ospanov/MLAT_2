// using System.Collections.Generic;
// using System.Linq;
// using Modules.WDCore;
// using Newtonsoft.Json;
// using Random = UnityEngine.Random;
//
// namespace Modules.Books
// {
//     public class AcademixBase
//     {
//         public class Status
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "underlyingDisease")] public string underlyingDisease;
//             [JsonProperty(PropertyName = "concomitantDiseases")] public string[] concomitantDiseases;
//             [JsonProperty(PropertyName = "destructorsDiseases")] public string[] destructorsDiseases;
//             [JsonProperty(PropertyName = "treatments")] public string[] treatments;
//         }
//         
//
//         public class Scenario
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "key")] public string key;
//             [JsonProperty(PropertyName = "status")] public string status;
//             [JsonProperty(PropertyName = "name_description")] public string description;
//             [JsonProperty(PropertyName = "country")] public string country;
//             [JsonProperty(PropertyName = "name")] public string name;
//             [JsonProperty(PropertyName = "specialization_ids")] public string[] specializationIds;
//             [JsonProperty(PropertyName = "department_id")] public string departmentId;
//             [JsonProperty(PropertyName = "cabinet_id")] public string cabinetId;
//             [JsonProperty(PropertyName = "patient_id")] public string patientId;
//             [JsonProperty(PropertyName = "status_id")] public string statusId;
//             [JsonProperty(PropertyName = "check_table_id")] public string checkTableId;
//             [JsonProperty(PropertyName = "skills_ids")] public string[] skillsIds;
//             [JsonProperty(PropertyName = "competences_ids")] public string[] competencesIds;
//             [JsonProperty(PropertyName = "patientState_id")] public string patientStateId;
//             [JsonProperty(PropertyName = "bundle")] public string[] bundle;
//             [JsonProperty(PropertyName = "gender")] public int gender;
//             [JsonProperty(PropertyName = "age")] public string age;
//             [JsonProperty(PropertyName = "height")] public float height;
//             [JsonProperty(PropertyName = "weight")] public float weight;
//             [JsonProperty(PropertyName = "waist")] public float waist;
//             [JsonIgnore] public string patientInfo;
//             [JsonIgnore] public bool isAvailable;
//             [JsonIgnore] public string patientName;
//             [JsonIgnore] public string doctorFName;
//             [JsonIgnore] public string doctorMName;
//         }
//         
//         public class Specialization
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "name")] public string name;
//         }
//
//         public class ICD10
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "name")] public string name;
//         }
//         
//         public class Treatment
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "name")] public string name;
//         }
//         
//         public class PatientState
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "patientState")] public string patientState;
//         }
//
//         public class Point
//         {
//             public string id;
//             public float posX;
//             public float posY;
//             public float posZ;
//             public float rotX;
//             public float rotY;
//             public float rotZ;
//             public string bone;
//             public string humanBone;
//         }
//
//         public class HumanName
//         {
//             [JsonProperty(PropertyName = "gender")] public int gender;
//             [JsonProperty(PropertyName = "lang")] public string lang;
//             [JsonProperty(PropertyName = "firstName")] public string[] firstName;
//             [JsonProperty(PropertyName = "lastName")] public string[] lastName;
//             [JsonProperty(PropertyName = "patronymicName")] public string[] patronymicName;
//             [JsonProperty(PropertyName = "title")] public string title;
//         }
//         
//         public class Lab
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "name_lab")] public string name;
//             [JsonProperty(PropertyName = "name_description")] public string description;
//         }
//         
//         public class LabValues
//         {
//             [JsonProperty(PropertyName = "checkUpId")] public string checkUpId;
//             [JsonProperty(PropertyName = "name_reference_1")] public string nameReference1;
//             [JsonProperty(PropertyName = "name_normal_1")] public string nameNormal1;
//             [JsonProperty(PropertyName = "name_measure_1")] public string nameMeasure1;
//             [JsonProperty(PropertyName = "values_1")] public string[] values1;
//             [JsonProperty(PropertyName = "price_1")] public string price1;
//         }
//         
//         public class Film
//         {
//             [JsonProperty(PropertyName = "id")] public string id;
//             [JsonProperty(PropertyName = "id_vimeo")] public string idVimeo;
//             [JsonProperty(PropertyName = "id_vimeo_en")] public string idVimeoEn;
//             [JsonProperty(PropertyName = "id_vimeo_kz")] public string idVimeoKz;
//             [JsonProperty(PropertyName = "name")] public string name;
//         }
//         
//         [JsonProperty(PropertyName = "Status")] public List<Status> statuses;
//         [JsonProperty(PropertyName = "Scenario")] public List<Scenario> scenarios;
//         [JsonProperty(PropertyName = "Specialization")] public List<Specialization> specializations;
//         [JsonProperty(PropertyName = "ContactPoint")] public List<Point> points;
//         [JsonProperty(PropertyName = "HumanName")] public List<HumanName> humanNames;
//         [JsonProperty(PropertyName = "ICD-10")] public List<ICD10> ICD10s;
//         [JsonProperty(PropertyName = "ATC")] public List<Treatment> ATCs;
//         [JsonProperty(PropertyName = "Recommendations")] public List<Treatment> recommendations;
//         [JsonProperty(PropertyName = "Surgery")] public List<Treatment> surgeries;
//         [JsonProperty(PropertyName = "Therapy")] public List<Treatment> therapies;
//         [JsonProperty(PropertyName = "patientState")] public List<PatientState> patientStates;
//         [JsonProperty(PropertyName = "Lab")] public List<Lab> labs;
//         [JsonProperty(PropertyName = "LabValues")] public List<LabValues> labValues;
//         [JsonProperty(PropertyName = "Films")] public List<Film> films;
//         
//         public Dictionary<string, Status> statusById;
//         public Dictionary<string, Point> pointById;
//         public Dictionary<string, Specialization> specializationById;
//         public Dictionary<string, ICD10> ICD10ById;
//         public Dictionary<string, Treatment> ATCById;
//         public Dictionary<string, Treatment> recommendationById;
//         public Dictionary<string, Treatment> surgeryById;
//         public Dictionary<string, Treatment> therapyById;
//         public Dictionary<string, PatientState> patientStateById;      
//         public Dictionary<string, Lab> labById;      
//         public Dictionary<string, LabValues> labValueById;      
//
//         public string GetRandomName(int gender)
//         {
//             var names = humanNames.FirstOrDefault(x => x.gender == gender);
//             return $"{names.title} {names.lastName[Random.Range(0, names.lastName.Length)]} " +
//                    $"{names.firstName[Random.Range(0, names.firstName.Length)]} " +
//                    $"{names.patronymicName[Random.Range(0, names.patronymicName.Length)]}";
//         }
//         
//         public void CreateDictionaries()
//         {
//             statusById = statuses?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             pointById = points?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             specializationById = specializations?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             ICD10ById = ICD10s?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             ATCById = ATCs?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             recommendationById = recommendations?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             surgeryById = surgeries?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             therapyById = therapies?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             patientStateById = patientStates?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             labById = labs?
//                 .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
//             labValueById = labValues?
//                 .Where(x => !string.IsNullOrEmpty(x.checkUpId)).ToDictionary(x => x.checkUpId);
//
//             foreach (var scenario in scenarios)
//             {
//                 var gender = scenario.gender == 0 ? TextData.Get(173) : TextData.Get(172);
//                 scenario.patientInfo = $"{TextData.Get(71)}: {scenario.age}  |  {TextData.Get(171)}: {gender}  |  " +
//                                        $"{TextData.Get(91)}: {scenario.height}  |  {TextData.Get(93)}: {scenario.weight}";
//             }
//         }
//     }
// }
