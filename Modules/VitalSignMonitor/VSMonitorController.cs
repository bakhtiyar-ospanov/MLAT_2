using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Modules.Books;
using Modules.S3;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Modules.VitalSignMonitor
{
    public class VSMonitorController : MonoBehaviour
    {
       private VSMonitorView _view;
       private int _mediaStatus;
       private List<Coroutine> _vitalsRoutines = new List<Coroutine>();
       private Dictionary<string, DateTime> _localDateById;
       private Dictionary<string, DateTime> _serverDateById;
       private string _pathOut;
       private string _pathIn;
       private string _infoPath;

       private void Awake()
       {
           _view = GetComponent<VSMonitorView>();
       }

       public IEnumerator Init()
       {
           _pathOut = DirectoryPath.VitalSignMonitors;
           BookDatabase.Instance.URLInfo.S3BookPaths.TryGetValue("VitalSignMonitors", out _pathIn);
           _infoPath = Path.Combine(_pathOut, "Info.json");
           _serverDateById = new Dictionary<string, DateTime>();
           
           if (!string.IsNullOrEmpty(_pathIn))
               yield return StartCoroutine(AmazonS3.Instance.GetLastModifiedDates(_pathIn,
                   val => _serverDateById = _serverDateById.Union(val).
                       ToDictionary(t => t.Key, t => t.Value)));
           
           if (File.Exists(_infoPath))
           {
               yield return StartCoroutine(FileHandler.ReadTextFile(_infoPath,
                   val => _localDateById = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(val)));
           }
           else
           {
               _localDateById = new Dictionary<string, DateTime>();
           }
       }

       public IEnumerator Init(MedicalBase.Scenario scenario)
       {
           yield return new WaitUntil(() => _localDateById != null && _serverDateById != null);
           
           var pulse = scenario.pulse;
           var speed = pulse / 60.0f;
           _mediaStatus = 0;
           
           if(pulse == 0)
               yield break;
           
           yield return StartCoroutine(LoadMedia(scenario.id));
           yield return new WaitUntil(() => _mediaStatus != 0);
           
           if(_mediaStatus == -1) yield break;
           
           _vitalsRoutines.Add(StartCoroutine(SetPulse(scenario.pulse)));
           _vitalsRoutines.Add(StartCoroutine(SetSaturation(scenario.saturation)));
           _vitalsRoutines.Add(StartCoroutine(SetBreath(scenario.breath)));
           _vitalsRoutines.Add(StartCoroutine(SetPressure(scenario.pressure)));
           _view.SetTemperature(scenario.temperature);
           
           _view.ActivatePulsating(true);
           _view.SetPulseSpeed(speed);
           _view.SetActivePanel(true);
       }

       private IEnumerator LoadMedia(string id)
       {
           var fileName = id + ".png";
           var pathOut = Path.Combine(_pathOut, fileName);
           var split = _pathIn.Split('/').ToList();
           var bucket = split[0];
           split.RemoveAt(0);
           var awsPath = string.Join("/", split);

           awsPath += fileName;

           _localDateById.TryGetValue(id, out var localTime);
           _serverDateById.TryGetValue(id, out var serverTime);

           if (serverTime == default)
           {
               yield return StartCoroutine(LoadMedia("norma"));
               yield break;
           }

           if ((serverTime != localTime || !File.Exists(pathOut)) && serverTime != default)
           {
               yield return StartCoroutine(AmazonS3.Instance.DownloadFile(bucket, awsPath, pathOut));

               if (localTime == default)
                   _localDateById.Add(id, serverTime);
               else
                   _localDateById[id] = serverTime;
                
               FileHandler.WriteTextFile(_infoPath, JsonConvert.SerializeObject(_localDateById));
           }

           if (File.Exists(pathOut))
           {
               yield return StartCoroutine(WebRequestHandler.Instance.TextureRequest(pathOut, _view.SetGraph));
               _mediaStatus = 1;
           }
           else
           {
               _mediaStatus = -1;
           }
       }

       private IEnumerator SetPulse(int pulse)
       {
           while (true)
           {
               _view.SetPulse(pulse + Random.Range(-2, 3));
               yield return new WaitForSecondsRealtime(Random.Range(5, 10));
           }
       }
       
       private IEnumerator SetBreath(int breath)
       {
           while (true)
           {
               _view.SetBreath(breath + Random.Range(-2, 3));
               yield return new WaitForSecondsRealtime(Random.Range(5, 10));
           }
       }
       
       private IEnumerator SetSaturation(int saturation)
       {
           while (true)
           {
               var tempSaturation = saturation + Random.Range(-2, 3);
               _view.SetSaturation(tempSaturation);
               yield return new WaitForSecondsRealtime(Random.Range(5, 10));
           }
       }
       
       private IEnumerator SetPressure(string pressure)
       {
           if(string.IsNullOrEmpty(pressure)) yield break;
           
           int.TryParse(pressure.Split('/')[0], out var pressure1);
           int.TryParse(pressure.Split('/')[1], out var pressure2);
           
           while (true)
           {
               var tempPressure1 = pressure1 + Random.Range(-2, 3);
               var tempPressure2 = pressure2 + Random.Range(-2, 3);
               var tempPressure = $"{tempPressure1}/{tempPressure2}";
               
               _view.SetPressure(tempPressure);
               yield return new WaitForSecondsRealtime(Random.Range(5, 10));
           }
       }

       public void Show()
       {
           _view.Show();
       }

       public void Clean()
       {
           foreach (var coroutine in _vitalsRoutines)
           {
               if(coroutine == null) continue;
               StopCoroutine(coroutine);
           }
           _vitalsRoutines.Clear();
           _view.SetActivePanel(false);
       }
    }
}
