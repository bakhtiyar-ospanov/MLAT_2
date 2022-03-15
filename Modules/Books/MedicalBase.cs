using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.WDCore;
using Modules.S3;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Modules.Books
{
    public class MedicalBase
    {
        public class Patient
        {
            [JsonIgnore] public string patientName;
            [JsonIgnore] public string doctorFName;
            [JsonIgnore] public string doctorMName;
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "gender")] public int gender;
            [JsonProperty(PropertyName = "age")] public string age;
            [JsonProperty(PropertyName = "height")] public float height;
            [JsonProperty(PropertyName = "weight")] public float weight;
            [JsonProperty(PropertyName = "waist")] public float waist;
        }

        public class Scenario
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "status")] public string status;
            [JsonProperty(PropertyName = "name_description")] public string description;
            [JsonProperty(PropertyName = "name_description_complaint_based")] public string descriptionComplaintBased;
            [JsonProperty(PropertyName = "country")] public string country;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "name_complaint_based")] public string nameComplaintBased;
            [JsonProperty(PropertyName = "specialization_ids")] public string[] specializationIds;
            [JsonProperty(PropertyName = "department_id")] public string departmentId;
            [JsonProperty(PropertyName = "cabinet_id")] public string cabinetId;
            [JsonProperty(PropertyName = "patient_id")] public string patientId;
            [JsonProperty(PropertyName = "status_id")] public string statusId;
            [JsonProperty(PropertyName = "check_table_id")] public string checkTableId;
            [JsonProperty(PropertyName = "skills_ids")] public string[] skillsIds;
            [JsonProperty(PropertyName = "competences_ids")] public string[] competencesIds;
            [JsonProperty(PropertyName = "patientState_id")] public string patientStateId;
            [JsonProperty(PropertyName = "library")] public string[] library;
            [JsonIgnore] public string patientInfo;
            [JsonIgnore] public bool isAvailable;
            
            // Academix extra fields
            [JsonIgnore] public string patientName;
            [JsonIgnore] public string doctorFName;
            [JsonIgnore] public string doctorMName;
            [JsonProperty(PropertyName = "gender")] public int gender;
            [JsonProperty(PropertyName = "age")] public string age;
            [JsonProperty(PropertyName = "height")] public float height;
            [JsonProperty(PropertyName = "weight")] public float weight;
            [JsonProperty(PropertyName = "waist")] public float waist;
            [JsonProperty(PropertyName = "hRate")] public int pulse;
            [JsonProperty(PropertyName = "reRate")] public int breath;
            [JsonProperty(PropertyName = "blPres")] public string pressure;
            [JsonProperty(PropertyName = "oxSat")] public int saturation;
            [JsonProperty(PropertyName = "temp")] public string temperature;
            [JsonProperty(PropertyName = "treatments")] public string[] treatments;
            [JsonProperty(PropertyName = "checkUpIds")] public string[] checkUpIds;
        }
        
        public class Diagnosis
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "underlyingDisease_ICD10")] public string underlyingDiseaseICD10;
            [JsonProperty(PropertyName = "concomitantDiseases_ICD10")] public string[] concomitantDiseasesICD10;
            [JsonProperty(PropertyName = "destructorsDiseases_ICD10")] public string[] destructorsDiseasesICD10;
            [JsonProperty(PropertyName = "underlyingDisease_ICD11")] public string underlyingDiseaseICD11;
            [JsonProperty(PropertyName = "concomitantDiseases_ICD11")] public string[] concomitantDiseasesICD11;
            [JsonProperty(PropertyName = "destructorsDiseases_ICD11")] public string[] destructorsDiseasesICD11;
        }
        
        public class Specialization
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class Skill
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class Competence
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class ICD
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        
        public class Treatment
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class PatientState
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "patientState")] public string patientState;
        }

        public class Point
        {
            public string id;
            public float posX;
            public float posY;
            public float posZ;
            public float rotX;
            public float rotY;
            public float rotZ;
            public string bone;
            public string humanBone;
        }

        public class HumanName
        {
            [JsonProperty(PropertyName = "gender")] public int gender;
            [JsonProperty(PropertyName = "lang")] public string lang;
            [JsonProperty(PropertyName = "firstName")] public string[] firstName;
            [JsonProperty(PropertyName = "lastName")] public string[] lastName;
            [JsonProperty(PropertyName = "patronymicName")] public string[] patronymicName;
            [JsonProperty(PropertyName = "title")] public string title;
        }
        
        public class Lab
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name_lab")] public string name;
            [JsonProperty(PropertyName = "name_description")] public string description;
        }
        
        public class LabValues
        {
            [JsonProperty(PropertyName = "checkUpId")] public string checkUpId;
            [JsonProperty(PropertyName = "name_reference")] public string nameReference1;
            [JsonProperty(PropertyName = "name_normal")] public string nameNormal1;
            [JsonProperty(PropertyName = "name_measure")] public string nameMeasure1;
            [JsonProperty(PropertyName = "values_1")] public string[] values1;
            [JsonProperty(PropertyName = "price_1")] public string price1;
        }
        
        public class Film
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "ru_filename")] public string ru_filename;
            [JsonProperty(PropertyName = "en_filename")] public string en_filename;
            [JsonProperty(PropertyName = "kz_filename")] public string kz_filename;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "library")] public string[] library;
            [JsonIgnore] public bool isAvailable;
        }

        public class CheckUp
        {
            [JsonProperty(PropertyName = "order")] public string order;
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "point_id")] public string pointId;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "details")] public string details;
            [JsonProperty(PropertyName = "files_aws")] public string[] filesAws;
        }
        
        public class AcademixPhysicalPoint
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "contactPoints")] public List<string> contactPoints;
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "description")] public string description;
            [JsonProperty(PropertyName = "files_aws")] public List<string> filesAws;
        }

        public class CheckTable
        {
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "grade")] public float grade;
            [JsonProperty(PropertyName = "trigger")] public string trigger;
        }
        
        public class Interface
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class Message
        {
            [JsonProperty(PropertyName = "name")] public string name;
        }
        
        public class Asset
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "inventory")] public string inventory;
            [JsonProperty(PropertyName = "name")] public string name;
        }
        public class MenuItem
        {
            [JsonProperty(PropertyName = "assetId")] public string assetId;
            [JsonProperty(PropertyName = "actions")] public string[] actions;
            [JsonProperty(PropertyName = "name")] public string name;
            public UnityAction call;
        }

        public class Library
        {
            [JsonProperty(PropertyName = "id")] public string id;
            [JsonProperty(PropertyName = "captures")] public string[] captures;
        }
        
        [JsonProperty(PropertyName = "PatientDimedus")] public List<Patient> patients;
        [JsonProperty(PropertyName = "Scenario")] public List<Scenario> scenarios;
        [JsonProperty(PropertyName = "Specialization")] public List<Specialization> specializations;
        [JsonProperty(PropertyName = "Skill")] public List<Skill> skills;
        [JsonProperty(PropertyName = "Competence")] public List<Competence> competences;
        [JsonProperty(PropertyName = "ContactPoint")] public List<Point> points;
        [JsonProperty(PropertyName = "HumanName")] public List<HumanName> humanNames;
        [JsonProperty(PropertyName = "ICD-10")] public List<ICD> ICD10s;
        [JsonProperty(PropertyName = "ICD-11")] public List<ICD> ICD11s;
        [JsonProperty(PropertyName = "ATC")] public List<Treatment> ATCs;
        [JsonProperty(PropertyName = "Recommendations")] public List<Treatment> recommendations;
        [JsonProperty(PropertyName = "Surgery")] public List<Treatment> surgeries;
        [JsonProperty(PropertyName = "Therapy")] public List<Treatment> therapies;
        [JsonProperty(PropertyName = "LabValues")] public List<LabValues> labValues;
        [JsonProperty(PropertyName = "Films")] public List<Film> films;
        [JsonProperty(PropertyName = "CheckUp")] public List<CheckUp> checkUps;
        [JsonProperty(PropertyName = "PhysicalPoint")] public List<AcademixPhysicalPoint> physicalPoints;
        [JsonProperty(PropertyName = "CheckTable")] public List<CheckTable> checkTables;
        [JsonProperty(PropertyName = "Interface")] public List<Interface> interfaces;
        [JsonProperty(PropertyName = "Messages")] public List<Message> messages;
        [JsonProperty(PropertyName = "Assets")] public List<Asset> assets;
        [JsonProperty(PropertyName = "AssetMenus")] public List<MenuItem> assetMenus;
        [JsonProperty(PropertyName = "Treatment")] public List<Treatment> treatments;
        [JsonProperty(PropertyName = "Diagnosis")] public List<Diagnosis> diagnoses;
        [JsonProperty(PropertyName = "Library")] public List<Library> libraries;
        
        public Dictionary<string, Patient> patientById;
        public Dictionary<string, Point> pointById;
        public Dictionary<string, Specialization> specializationById;
        public Dictionary<string, Skill> skillById;
        public Dictionary<string, Competence> competenceById;
        public Dictionary<string, ICD> ICD10ById;
        public Dictionary<string, ICD> ICD11ById;
        public Dictionary<string, Treatment> ATCById;
        public Dictionary<string, Treatment> recommendationById;
        public Dictionary<string, Treatment> surgeryById;
        public Dictionary<string, Treatment> therapyById;
        public Dictionary<string, LabValues> labValueById;
        public Dictionary<string, Interface> interfaceById;
        public Dictionary<string, Asset> assetById;
        public Dictionary<string, List<MenuItem>> assetMenuById;
        public Dictionary<string, Diagnosis> diagnosisById;
        public Dictionary<string, Library> libraryById;

        public string GetRandomName(int gender)
        {
            var namesByLang = humanNames.Where(x => x.lang == Language.Code).ToList();

            if (namesByLang.Count == 0)
            {
                switch (Language.Code)
                {
                    case "kk": 
                        namesByLang = humanNames.Where(x => x.lang == "ru").ToList();
                        break;
                    case "ky": 
                        namesByLang = humanNames.Where(x => x.lang == "ru").ToList();
                        break;
                    case "uk": 
                        namesByLang = humanNames.Where(x => x.lang == "ru").ToList();
                        break;
                    default:
                        namesByLang = humanNames.Where(x => x.lang == "en").ToList();
                        break;
                }
                
            }
            
            var names = namesByLang.FirstOrDefault(x => x.gender == gender);

            if (names == default)
                return "John Doe";

            return $"{names.title} {names.lastName[Random.Range(0, names.lastName.Length)]} " +
                       $"{names.firstName[Random.Range(0, names.firstName.Length)]} ";
        }
        
        public void CreateDictionaries()
        {
            if (patients == null)
            {
                patients = new List<Patient>();
                foreach (var scenario in scenarios)
                {
                    scenario.checkTableId = "Academix";
                    scenario.patientId = scenario.id;
                    patients.Add(new Patient
                    {
                        id = scenario.patientId,
                        waist = scenario.waist,
                        height = scenario.height,
                        gender = scenario.gender,
                        age = scenario.age,
                        weight = scenario.weight
                    });
                }
            }
            
            if (checkUps != null)
                ParseCheckups();
            if (physicalPoints != null)
                ParsePhysicalPoints();
            if (treatments != null)
                ParseTreatments();
            
            patientById = patients?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            pointById = points?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            specializationById = specializations?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            skillById = skills?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            competenceById = competences?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            ICD10ById = ICD10s?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            ICD11ById = ICD11s?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            ATCById = ATCs?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            recommendationById = recommendations?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            surgeryById = surgeries?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            therapyById = therapies?
                .Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            labValueById = labValues?
                .Where(x => !string.IsNullOrEmpty(x.checkUpId)).ToDictionary(x => x.checkUpId);
            interfaceById = interfaces?.
                Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            assetById = assets?.
                Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            diagnosisById = diagnoses?.
                Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);
            libraryById = libraries?.
                Where(x => !string.IsNullOrEmpty(x.id)).ToDictionary(x => x.id);

            TextData.Set(interfaceById);
            
            foreach (var scenario in scenarios)
            {
                Patient patient = null;
                patientById?.TryGetValue(scenario.patientId, out patient);
                if (patient == null) continue;

                var gender = patient.gender == 0 ? TextData.Get(173) : TextData.Get(172);
                scenario.patientInfo = $"{TextData.Get(71)}: {patient.age}  |  {TextData.Get(171)}: {gender}  |  " +
                                       $"{TextData.Get(91)}: {patient.height} {TextData.Get(92)}  |  " +
                                       $"{TextData.Get(93)}: {patient.weight} {TextData.Get(94)}";
            }
            
            assetMenuById = new Dictionary<string, List<MenuItem>>();
            if (assetMenus != null)
            {
                foreach (var assetMenu in assetMenus)
                {
                    if(!assetMenuById.ContainsKey(assetMenu.assetId))
                        assetMenuById.Add(assetMenu.assetId, new List<MenuItem>());
                
                    assetMenuById[assetMenu.assetId].Add(assetMenu);
                }
            }
        }

        private void ParseCheckups()
        {
           var allCheckups = new List<FullCheckUp>();
           checkUps = checkUps.OrderBy(x => x.order).ToList();
           foreach (var checkUp in checkUps)
           {
               var order = checkUp.order;
               var refList = allCheckups;
               var positions = order.Split('.').Select(int.Parse).ToList();
               string parentId = null;
               string typeId = (allCheckups.Count <= positions[0]-1 ? checkUp.id : allCheckups[positions[0]-1].id) switch
               {
                   Config.AuscultationParentId => "11",
                   Config.PercussionParentId => "10",
                   Config.PalpationParentId => "9",
                   Config.VisualExamParentId => "12",
                   Config.ComplaintParentId => "2",
                   Config.InstrResearchParentd => "6",
                   Config.LabResearchParentId => "7",
                   _ => null
               };

               for (var i = 0; i < positions.Count; i++)
               {
                   if (i == positions.Count - 1)
                   {
                       var files = checkUp.filesAws?.Select(filesAws => 
                           new FullCheckUp.File {name = filesAws, mime = 
                               filesAws.Contains(".mp3") || filesAws.Contains(".ogg") ? "audio" : "image"}).ToList();
                       var newCheckup = new FullCheckUp
                       {
                           id = checkUp.id,
                           name = checkUp.name,
                           details = checkUp.details,
                           pointId = checkUp.pointId,
                           parentId = parentId,
                           typeId = typeId,
                           level = i + 1,
                           order = positions[i],
                           children = new List<FullCheckUp>(),
                           files = files ?? new List<FullCheckUp.File>()
                       };
                       refList.Add(newCheckup);
                   }
                   else
                   {
                       parentId = refList[refList.Count - 1].id;
                       refList = refList[refList.Count - 1].children;
                   }
               }
           }
           
           BookDatabase.Instance.allCheckUps = allCheckups;
        }

        private void ParsePhysicalPoints()
        {
            var physicalPointById = new Dictionary<string, PhysicalPoint>();

            foreach (var physicalPoint in physicalPoints)
            {
                var files = physicalPoint.filesAws?.Select(filesAws => 
                    new PhysicalPoint.File {name = filesAws, mime = 
                        filesAws.Contains(".mp3") || filesAws.Contains(".ogg") ? "audio" : "image"}).ToList();
                
                physicalPointById.Add(physicalPoint.id, new PhysicalPoint
                {
                    id = physicalPoint.id,
                    name = physicalPoint.name,
                    description = physicalPoint.description,
                    values = physicalPoint.contactPoints ?? new List<string>(),
                    files = files ?? new List<PhysicalPoint.File>()
                });
            }

            BookDatabase.Instance.physicalPointById = physicalPointById;
        }

        private void ParseTreatments()
        {
            recommendations = treatments.Where(x => !string.IsNullOrEmpty(x.id) && x.id.StartsWith("RE")).ToList();
            surgeries = treatments.Where(x => !string.IsNullOrEmpty(x.id) && x.id.StartsWith("SG")).ToList();
            therapies = treatments.Where(x => !string.IsNullOrEmpty(x.id) && x.id.StartsWith("TH")).ToList();
        }

        // private void Extra()
        // {
        //     var data = new List<(string, string)>();
        //     foreach (var scenario in scenarios)
        //     {
        //         var status = new StatusInstance.Status();
        //         var StatusInstance = new StatusInstance {FullStatus = status};
        //         status.checkUps = new List<StatusInstance.Status.CheckUp>();
        //
        //         var fullCheckups = BookDatabase.Instance.allCheckUps;
        //         var checkUpIds = scenario.checkUpIds;
        //         var readyList = new List<string>();
        //     
        //         CheckupTraverse(fullCheckups, status.checkUps, checkUpIds);
        //         ShortCheckupTraverse(status.checkUps, readyList);
        //         
        //         var checkups = string.Join(";", readyList);
        //         data.Add((scenario.id, checkups));
        //     }
        //     
        //     FileHandler.WriteTextFile(DirectoryPath.Books + "/ggg.json", JsonConvert.SerializeObject(data));
        // }
        //
        // private void CheckupTraverse(List<FullCheckUp> checkUps, List<StatusInstance.Status.CheckUp> shortCheckups, string[] checkUpIds)
        // {
        //     if(checkUps == null || checkUps.Count == 0) return;
        //     
        //     foreach (var checkup in checkUps)
        //     {
        //         var shortCheckup = new StatusInstance.Status.CheckUp
        //         {
        //             id = checkup.id,
        //             children = new List<StatusInstance.Status.CheckUp>()
        //         };
        //
        //         if (checkUpIds.Contains(checkup.id))
        //             shortCheckups.Add(shortCheckup);
        //         
        //
        //         CheckupTraverse(checkup.children, shortCheckup.children, checkUpIds);
        //     }
        // }
        //
        // private void ShortCheckupTraverse(List<StatusInstance.Status.CheckUp> shortCheckups, List<string> readyList)
        // {
        //     if(shortCheckups == null || shortCheckups.Count == 0) return;
        //
        //     foreach (var shortCheckup in shortCheckups)
        //     {
        //         if(shortCheckup.children == null || shortCheckup.children.Count == 0)
        //             readyList.Add(shortCheckup.id);
        //
        //         ShortCheckupTraverse(shortCheckup.children, readyList);
        //     }
        // }
    }

    public class ATCCS
    {
        [JsonProperty(PropertyName = "ATCCS")] public List<MedicalBase.Treatment> atccs;
    }
    
    public class ICD10
    {
        [JsonProperty(PropertyName = "ICD-10")] public List<MedicalBase.ICD> icd10s;
    }
    
    public class ICD11
    {
        [JsonProperty(PropertyName = "ICD-11")] public List<MedicalBase.ICD> icd11s;
    }

    public class FullCheckUp
    {
        public class File
        {
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "originalName")] public string originalName;
            [JsonProperty(PropertyName = "mime")] public string mime;  
        }
        
        [JsonProperty(PropertyName = "id")] public string id;
        [JsonProperty(PropertyName = "pointId")] public string pointId;
        [JsonProperty(PropertyName = "parent_id")] public string parentId;
        [JsonProperty(PropertyName = "typeId")] public string typeId;
        [JsonProperty(PropertyName = "measureId")] public string measureId;
        [JsonProperty(PropertyName = "name")] public string name;
        [JsonProperty(PropertyName = "level")] public int level;
        [JsonProperty(PropertyName = "order")] public int order;
        [JsonProperty(PropertyName = "details")] public string details;
        [JsonProperty(PropertyName = "files")] public List<File> files;
        [JsonProperty(PropertyName = "children")] public List<FullCheckUp> children;
        
        public PhysicalPoint GetPointInfo()
        {
            if (pointId == null) return null;
            BookDatabase.Instance.physicalPointById.TryGetValue(pointId, out var physicalPoint);
            return physicalPoint;
        }
        
        public IEnumerator GetMedia(Action<Dictionary<string, Object>> callback)
        {
            var folder = $"{id.PadLeft(12, '0')}/media/";
            var localFiles = new List<string>();

            if(Directory.Exists($"{DirectoryPath.CheckUps}{folder}"))
            {
                localFiles = Directory.GetFiles($"{DirectoryPath.CheckUps}{folder}").Select(Path.GetFileName).ToList();
                var fileNames = files.Select(x => x.name).ToList();

                foreach (var localFile in localFiles)
                {
                    if(fileNames.Contains(localFile)) continue;
                    FileHandler.DeleteFile($"{DirectoryPath.CheckUps}{folder}{localFile}");
                }
            }
            
            var checkupMedia = BookDatabase.Instance.URLInfo.S3BookPaths["CheckupMedia"];
            var split = checkupMedia.Split('/').ToList();
            var bucket = split[0];
            split.RemoveAt(0);
            var path = string.Join("/", split);
            
            foreach (var file in files)
            {
                if(localFiles.Contains(file.name)) continue;
                var filePath = $"{folder}{file.name}";
                yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                    AmazonS3.Instance.DownloadFile(bucket,
                    $"{path}/{filePath}", $"{DirectoryPath.CheckUps}{filePath}"));
            }

            var outObjects = new Dictionary<string, Object>();
            foreach (var file in files)
            {
                var filePath = $"{DirectoryPath.CheckUps}{folder}{file.name}";
                if(!System.IO.File.Exists(filePath)) continue;
                
                if (file.mime.Contains("audio"))
                {
                    ExtensionCoroutine.Instance.StartExtendedCoroutineNoWait(WebRequestHandler.Instance.AudioRequest(filePath, audioClip =>
                    {
                        outObjects.Add(file.name, audioClip);
                    }));
                    
                } else if (file.mime.Contains("image"))
                {
                    yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                        WebRequestHandler.Instance.TextureRequest(filePath, texture => {outObjects.Add(file.name, texture);}));
                }
            }
            yield return new WaitUntil(() => outObjects.Count == files.Count);
            callback?.Invoke(outObjects);
        }
    }
    
    public class PhysicalPoint
    {
        public class File
        {
            [JsonProperty(PropertyName = "name")] public string name;
            [JsonProperty(PropertyName = "originalName")] public string originalName;
            [JsonProperty(PropertyName = "mime")] public string mime;  
        }
        
        [JsonProperty(PropertyName = "id")] public string id;
        [JsonProperty(PropertyName = "name")] public string name;
        [JsonProperty(PropertyName = "description")] public string description;
        [JsonProperty(PropertyName = "values")] public List<string> values;
        [JsonProperty(PropertyName = "files")] public List<File> files;
        
        public IEnumerator GetMedia(Action<Dictionary<string, Object>> callback)
        {
            var folder = $"{id.PadLeft(12, '0')}/";
            var localFiles = new List<string>();
            if(Directory.Exists($"{DirectoryPath.Points}{folder}"))
            {
                localFiles = Directory.GetFiles($"{DirectoryPath.Points}{folder}").Select(Path.GetFileName).ToList();
                var fileNames = files.Select(x => x.name).ToList();

                foreach (var localFile in localFiles)
                {
                    if(fileNames.Contains(localFile)) continue;
                    FileHandler.DeleteFile($"{DirectoryPath.Points}{folder}{localFile}");
                }
            }

            var checkupMedia = BookDatabase.Instance.URLInfo.S3BookPaths["PointMedia"];
            var split = checkupMedia.Split('/').ToList();
            var bucket = split[0];
            split.RemoveAt(0);
            var path = string.Join("/", split);
            
            foreach (var file in files)
            {
                if(localFiles.Contains(file.name)) continue;
                var filePath = $"{folder}{file.name}";
                yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                    AmazonS3.Instance.DownloadFile(bucket,
                    $"{path}/{filePath}", $"{DirectoryPath.Points}{filePath}"));
            }

            var outObjects = new Dictionary<string, Object>();
            foreach (var file in files)
            {
                var filePath = $"{DirectoryPath.Points}{folder}{file.name}";
                if(!System.IO.File.Exists(filePath)) continue;
                
                if (file.mime.Contains("audio"))
                {
                    ExtensionCoroutine.Instance.StartExtendedCoroutineNoWait(WebRequestHandler.Instance.AudioRequest(filePath, audioClip =>
                    {
                        outObjects.Add(file.name, audioClip);
                    }));
                    
                } else if (file.mime.Contains("image"))
                {
                    yield return ExtensionCoroutine.Instance.StartExtendedCoroutine(
                        WebRequestHandler.Instance.TextureRequest(filePath, texture => {outObjects.Add(file.name, texture);}));
                }
            }
            yield return new WaitUntil(() => outObjects.Count == files.Count);
            callback?.Invoke(outObjects);
        }
    }
    
    public class WDPhysicalPoint
    {
        [JsonProperty(PropertyName = "values")] public List<string> values;
        [JsonProperty(PropertyName = "items")] public List<PhysicalPoint> items;
    }
    
        public class CheckTable
     {
         public class Action
         {
             [JsonProperty(PropertyName = "id")] public string id;
             [JsonProperty(PropertyName = "name")] public string name;
             [JsonProperty(PropertyName = "grade")] public string grade;
             [JsonProperty(PropertyName = "namebutton")] public string nameButton;
             [JsonProperty(PropertyName = "speech")] public string speech;
             [JsonProperty(PropertyName = "answer")] public string answer;
             [JsonProperty(PropertyName = "level")] public string level;
             [JsonProperty(PropertyName = "gradeMax", NullValueHandling=NullValueHandling.Ignore)] public float gradeMax;
             [JsonProperty(PropertyName = "gradeAuto")] public string gradeAuto;
             [JsonProperty(PropertyName = "credits")] public string credits;
             [JsonProperty(PropertyName = "unordered")] public bool unordered;
             [JsonProperty(PropertyName = "trigger")] public string trigger;
         }
         [JsonProperty(PropertyName = "id")] public string id;
         [JsonProperty(PropertyName = "actions")] public List<Action> actions;
     }
        
        public static class Config
        {
            public const string InstrResearchParentd = "2815";
            public const string LabResearchParentId = "3420";
            public const string PalpationParentId = "5787";
            public const string PercussionParentId = "6067";
            public const string AuscultationParentId = "5849";
            public const string VisualExamParentId = "5831";
            public const string ComplaintParentId = "2732";
            public const string AnamnesisLifeParentId = "945";
            public const string AnamnesisDiseaseParentId = "946";
            public const string VitalsParentId = "6404";
            public const string TemperatureParentId = "6186";
            public const string RespiratoryParentId = "6187";
            public const string PulseParentId = "6188";
            public const string PressureParentId = "6189";
            public const string SaturationParentId = "6190";
        }
    
}
