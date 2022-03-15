using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.Statistics;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;

namespace Modules.Awards
{
    public class AwardController : MonoBehaviour
    {
        private AwardView _view;
        private StatisticsManager.Statistics _statistics;
        private bool _isAnyNewAchieved;
        private int _awardCount = 7;
        private void Awake()
        {
            _view = GetComponent<AwardView>();
            _view.backButton.onClick.AddListener(() => SetActivePanel(false));
        }

        public IEnumerator Init()
        {
            var filename = "Statistics.encrypted";
            var userStat = $"{DirectoryPath.Statistics}/{filename}";;

            ResetScores();

            byte[] byteResponse = null;
            yield return StartCoroutine(FileHandler.ReadBytesFile(userStat, val => byteResponse = val));

            var response = Decipher.DecryptStringFromBytes_Des(byteResponse);
            if (!string.IsNullOrEmpty(response))
            {
                _statistics = JsonConvert.DeserializeObject<StatisticsManager.Statistics>(response);
                _statistics.achievedAwards ??= new List<string>();
            }
                
            else
                yield break;
            
            CheckSotnikAward();
            CheckLevTolstoyAward();
            CheckDemoAward();
            CheckDrHideAward();
            CheckAibolitAward();
            CheckLearnLearnLearnAward();
            CheckDrHouseAward();

            CheckAnyAchieved();
        }
        
        public void SetActivePanel(bool val)
        {
            if(val)
                StartCoroutine(GameManager.Instance.awardController.Init());
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.root.SetActive(val);
        }

        private void CheckSotnikAward()
        {
            var awardId = "Sotnik";
            if(CheckAward(0, awardId)) return;

            var scenarioCount = _statistics.scenariosAcademix.Count;
            var score = Mathf.RoundToInt(Mathf.Clamp(scenarioCount / 100.0f, 0.0f, 1.0f) * 100.0f);
            
            _view.SetScore(0, score);
            CheckAchieved(awardId, score, TextData.Get(318));
        }

        private void CheckDemoAward()
        {
            var awardId = "Demo";
            if(CheckAward(1, awardId)) return;
            
            var allDemos = BookDatabase.Instance.MedicalBook.scenarios
                .Where(x => x.library.Contains("Demo"))
                .Select(x => x.id).ToList();

            var completed = _statistics.scenariosAcademix.Count(scenario => allDemos.Contains(scenario.id));
            var score = Mathf.RoundToInt(100.0f*completed / allDemos.Count);
            
            _view.SetScore(1, score);
            CheckAchieved(awardId, score, TextData.Get(320));
        }
        
        private void CheckLevTolstoyAward()
        {
            var awardId = "LevTolstoy";
            if(CheckAward(2, awardId)) return;
            
            var scenarioCount = _statistics.feedbackCount;
            var score = Mathf.RoundToInt(Mathf.Clamp(scenarioCount / 10.0f, 0.0f, 1.0f) * 100.0f);

            _view.SetScore(2, score);
            CheckAchieved(awardId, score, TextData.Get(322));
        }
        
        private void CheckDrHideAward()
        {
            var awardId = "DrHide";
            if(CheckAward(3, awardId)) return;
            
            var allBasic = BookDatabase.Instance.MedicalBook.scenarios
                .Where(x => x.library.Contains("Basic"))
                .Select(x => x.id).ToList();

            var completed = _statistics.scenariosAcademix.Count(scenario => allBasic.Contains(scenario.id) 
                                                                            && scenario.items.Any(x => x.score <= 30.0f));
            var score = Mathf.RoundToInt(Mathf.Clamp(completed / 10.0f, 0.0f, 1.0f) * 100.0f);
 
            _view.SetScore(3, score);
            CheckAchieved(awardId, score, TextData.Get(324));
        }
        
        private void CheckAibolitAward()
        {
            var awardId = "Aibolit";
            if(CheckAward(4, awardId)) return;
            
            var allBasic = BookDatabase.Instance.MedicalBook.scenarios
                .Where(x => x.library.Contains("Basic"))
                .Select(x => x.id).ToList();

            var completed = _statistics.scenariosAcademix.Count(scenario => allBasic.Contains(scenario.id) 
                                                                            && scenario.items.Any(x => x.score >= 50.0f));
            var score = Mathf.RoundToInt(100.0f*completed / allBasic.Count);
            
            _view.SetScore(4, score);
            CheckAchieved(awardId, score, TextData.Get(326));
        }

        private void CheckLearnLearnLearnAward()
        {
            var awardId = "LearnLearnLearn";
            if(CheckAward(5, awardId)) return;
            
            var allBasic = BookDatabase.Instance.MedicalBook.scenarios
                .Where(x => x.library.Contains("Basic"))
                .Select(x => x.id).ToList();

            var completed = _statistics.scenariosAcademix.Count(scenario => allBasic.Contains(scenario.id) 
                                                                            && scenario.items.Any(x => x.mode == 0));
            var score = Mathf.RoundToInt(100.0f*completed / allBasic.Count);

            _view.SetScore(5, score);
            CheckAchieved(awardId, score, TextData.Get(328));
        }

        private void CheckDrHouseAward()
        {
            var awardId = "DrHouse";
            if(CheckAward(6, awardId)) return;
            
            var volchanka = new List<string> { "150", "151", "152" };
            var completed = _statistics.scenariosAcademix.Count(scenario => volchanka.Contains(scenario.id));
            var score = Mathf.RoundToInt(100.0f*completed / volchanka.Count);
            
            _view.SetScore(6, score);
            CheckAchieved(awardId, score, TextData.Get(331));
        }

        private bool CheckAward(int index, string awardId)
        {
            if (!_statistics.achievedAwards.Contains(awardId)) return false;
            _view.SetScore(index, 100);
            return true;

        }

        private void CheckAchieved(string awardId, int score, string title)
        {
            if (score == 100)
            {
                _statistics.achievedAwards.Add(awardId);
                _isAnyNewAchieved = true;
                    
                var header = TextData.Get(153);
                var body = $"{TextData.Get(154)} \"{title}\". {TextData.Get(168)}";
            
                GameManager.Instance.notificationController.Init(header, body, true);
                
            }
        }

        private void CheckAnyAchieved()
        {
            if(!_isAnyNewAchieved) return;
            
            var filename = "Statistics.encrypted";
            var userStat = $"{DirectoryPath.Statistics}/{filename}";

            var encryptedBytes = Decipher.EncryptStringToBytes_Des(JsonConvert.SerializeObject(_statistics));
            FileHandler.WriteBytesFile(userStat, encryptedBytes);
            GameManager.Instance.playFabFileController.UploadFile("Statistics.json", userStat, serverDate =>
            {
                PlayerPrefs.SetString(filename, serverDate.ToString());
                PlayerPrefs.Save();
            }, true);
            
            _isAnyNewAchieved = false;
        }

        private void ResetScores()
        {
            for (var i = 0; i < _awardCount; i++)
                _view.SetScore(i, 0);
        }
    }
}
