using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Statistics
{
    public class StatisticsManager : MonoBehaviour
    {
        public class Statistics
        {
            public class Item
            {
                [JsonProperty(PropertyName = "date")] public string date;
                [JsonProperty(PropertyName = "time")] public string time;
                [JsonProperty(PropertyName = "mode")] public int mode;
                [JsonProperty(PropertyName = "score")] public double score;
                [JsonProperty(PropertyName = "pdfName")] public string pdfName;
            }

            public class Task
            {
                [JsonProperty(PropertyName = "id")] public string id;
                [JsonProperty(PropertyName = "specializationIds")] public string[] specializationIds;
                [JsonProperty(PropertyName = "instances")] public List<Item> items;
            }

            [JsonProperty(PropertyName = "ucourses")] public List<Task> courses;
            [JsonProperty(PropertyName = "scenarios")]  public List<Task> scenarios;
            [JsonProperty(PropertyName = "scenariosAcademix")]  public List<Task> scenariosAcademix;
            [JsonProperty(PropertyName = "feedbackCount")] public int feedbackCount;
            [JsonProperty(PropertyName = "achievedAwards")] public List<string> achievedAwards;
        }

        public IEnumerator AddReport(string id, string[] specializationIds, Statistics.Item data, int type, Action<string> onPathDefined, bool isSimulation = false)
        {
            var statPath = DirectoryPath.Statistics;
            var filename = "Statistics.encrypted";
            var userStat = $"{statPath}/{filename}";

            byte[] byteResponse = null;
            yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));
            var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
            var stats = string.IsNullOrEmpty(response) ? 
                new Statistics
                {
                    courses = new List<Statistics.Task>(),
                    scenarios = new List<Statistics.Task>(),
                    scenariosAcademix = new List<Statistics.Task>(),
                } 
                : JsonConvert.DeserializeObject<Statistics>(response);

            stats.scenariosAcademix ??= new List<Statistics.Task>();
            
            switch (type)
            {
                case 0:
                    if (stats.courses.FirstOrDefault(x => x.id == id) == null)
                        stats.courses.Add(new Statistics.Task{id = id, specializationIds = specializationIds, 
                            items = new List<Statistics.Item>()});
                    var course = stats.courses.FirstOrDefault(x => x.id == id);
                    course?.items.Insert(0, data);
                    statPath += "DimedusCourseReports";
                    break;
                case 1:
                    if (stats.scenarios.FirstOrDefault(x => x.id == id) == null)
                        stats.scenarios.Add(new Statistics.Task{id = id, specializationIds = specializationIds, 
                            items = new List<Statistics.Item>()});
                    var scenario = stats.scenarios.FirstOrDefault(x => x.id == id);
                    scenario?.items.Insert(0, data);
                    statPath += "ScenarioReports";
                    break;
                case 2:
                    if (stats.scenariosAcademix.FirstOrDefault(x => x.id == id) == null)
                        stats.scenariosAcademix.Add(new Statistics.Task{id = id, specializationIds = specializationIds, 
                            items = new List<Statistics.Item>()});
                    var scenarioAcademix = stats.scenariosAcademix.FirstOrDefault(x => x.id == id);
                    scenarioAcademix?.items.Insert(0, data);
                    statPath += "AcademixScenarioReports";
                    break;
            }
            DirectoryPath.CheckDirectory(statPath);
            var formattedTime = data.date.Replace('/', '-') + "_" 
                                                            + data.time.Replace(':', '-');
            data.pdfName =  id + "_" + formattedTime + ".pdf";

            if (!isSimulation)
            {
                var encryptedBytes = Decipher.EncryptStringToBytes_Des(JsonConvert.SerializeObject(stats));
                FileHandler.WriteBytesFile(userStat, encryptedBytes);
                GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, serverDate =>
                {
                    PlayerPrefs.SetString(filename, serverDate.ToString());
                    PlayerPrefs.Save();
                }, true);
            }

            onPathDefined?.Invoke( statPath + "/" + data.pdfName);
        }
        

        public void SyncStatistics()
        {
            Debug.Log("Sync Statistics.encrypted with Playfab");
            
            var filename = "Statistics.encrypted";
            var userStat = $"{DirectoryPath.Statistics}/{filename}";

            GameManager.Instance.playFabFileController.GetFileMeta("Statistics.json", (serverDate, downloadUrl) => 
            {   
                if(downloadUrl == null) return;
                if (!PlayerPrefs.HasKey(filename) || !File.Exists(userStat))
                {
                    GameManager.Instance.playFabFileController.GetActualFile(downloadUrl,userStat, true);
                    PlayerPrefs.SetString(filename, serverDate.ToString());
                    PlayerPrefs.Save();
                }
                else
                {
                    var localDate = PlayerPrefs.GetString(filename);
                    if (DateTime.Parse(localDate).CompareTo(serverDate) > 0)
                    {
                        GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, 
                            uploadServerDate =>
                        {
                            PlayerPrefs.SetString(filename, uploadServerDate.ToString());
                            PlayerPrefs.Save();
                        }, true);
                    }
                    else if (DateTime.Parse(localDate).CompareTo(serverDate) < 0)
                    {
                        GameManager.Instance.playFabFileController.GetActualFile(downloadUrl, userStat, true);
                        PlayerPrefs.SetString(filename, serverDate.ToString());
                        PlayerPrefs.Save();
                    }
                }
            });
        }

        public IEnumerator AnonUserAndOldUserSync()
        {
            Debug.Log("Синхронизация статистики анонимного пользователя и существующего пользователя");
            var filename = "Statistics.encrypted";
            var userStat = $"{DirectoryPath.Statistics}/{filename}";
            var isPlayfabStat = 0;
            Statistics anonStats = null;
            Statistics playfabStats = null;

            if (File.Exists(userStat)) // read local anonymous statistics
            {
                byte[] byteResponse = null;
                yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));
                var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
                anonStats = JsonConvert.DeserializeObject<Statistics>(response);
                Debug.Log("Статистика анонимного пользователя существует");
            }
            
            GameManager.Instance.playFabFileController.GetFileMeta("Statistics.json", (serverDate, downloadUrl) => 
            {
                if (downloadUrl == null)
                {
                    isPlayfabStat = -1;
                    return;
                }
                GameManager.Instance.playFabFileController.GetActualFile(downloadUrl,userStat, true, () =>
                {
                    isPlayfabStat = 1;
                });
                PlayerPrefs.SetString(filename, serverDate.ToString());
                PlayerPrefs.Save();
                
            });

            yield return new WaitUntil(() => isPlayfabStat != 0);
            
            if (isPlayfabStat == 1) // read downloaded old user statistics
            {
                byte[] byteResponse = null;
                yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));
                var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
                playfabStats = JsonConvert.DeserializeObject<Statistics>(response);
                Debug.Log("Статистика текущего пользователя существует");
            } 
            else if (anonStats != null) // upload local anonymous user statistics to old account
            {
                GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, 
                    uploadServerDate =>
                    {
                        PlayerPrefs.SetString(filename, uploadServerDate.ToString());
                        PlayerPrefs.Save();
                    }, true);
                Debug.Log("Статистика текущего пользователя не существует, загрузка статистики анон пользователя");
            }

            // Merge two statistics data
            if (playfabStats != null && anonStats != null)
            {
                Debug.Log("Объединение статистик");
                MergeTasks(playfabStats.scenarios, anonStats.scenarios);
                MergeTasks(playfabStats.courses, anonStats.courses);
                MergeTasks(playfabStats.scenariosAcademix, anonStats.scenariosAcademix);
                var encryptedBytes = Decipher.EncryptStringToBytes_Des(JsonConvert.SerializeObject(playfabStats));
                FileHandler.WriteBytesFile(userStat, encryptedBytes);
                GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, serverDate =>
                {
                    PlayerPrefs.SetString(filename, serverDate.ToString());
                    PlayerPrefs.Save();
                }, true);
            }
        }

        private void MergeTasks(List<Statistics.Task> target, List<Statistics.Task> source)
        {
            foreach (var scenario in source)
            {
                var scenarioGroup = target.FirstOrDefault(x => x.id == scenario.id);
                if(scenarioGroup != null)
                    scenarioGroup.items.AddRange(scenario.items);
                else
                    target.Add(scenario);
            }
        }

        public IEnumerator IncrementFeedbackCount()
        {
            var statPath = DirectoryPath.Statistics;
            var filename = "Statistics.encrypted";
            var userStat = $"{statPath}/{filename}";

            byte[] byteResponse = null;
            yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));
            var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
            var stats = JsonConvert.DeserializeObject<Statistics>(response);

            stats.feedbackCount++;
            
            var encryptedBytes = Decipher.EncryptStringToBytes_Des(JsonConvert.SerializeObject(stats));
            FileHandler.WriteBytesFile(userStat, encryptedBytes);
            GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, serverDate =>
            {
                PlayerPrefs.SetString(filename, serverDate.ToString());
                PlayerPrefs.Save();
            }, true);
            
            StartCoroutine(GameManager.Instance.awardController.Init());
        }

        public void CleanStatisticsOnLogout()
        {
            var statPath = DirectoryPath.Statistics;
            var filename = "Statistics.encrypted";
            var userStat = $"{statPath}/{filename}";
            FileHandler.DeleteFile(userStat);
        }
    }
}
