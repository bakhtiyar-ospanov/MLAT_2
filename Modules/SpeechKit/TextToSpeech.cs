using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Modules.Assets.Assistant;
using Modules.Books;
using Modules.S3;
using Modules.WDCore;
using UnityEngine;

namespace Modules.SpeechKit
{
    public class TextToSpeech : Singleton<TextToSpeech>
    {
        public enum Character
        {
            Doctor,
            Patient,
            Assistant
        }

        private AudioSource _mainAudioSource;
        private AudioSource _currentAudioSource;
        private SpeechKit.SPVoice _doctorVoice;
        private SpeechKit.SPVoice _assistantVoice;
        private SpeechKit.SPVoice _patientVoice;
        private bool _isDone = true;
        private string _bucket;
        private string _s3Folder;

        private void Awake()
        {
            _mainAudioSource = GetComponent<AudioSource>();
            _currentAudioSource = _mainAudioSource;
            
            Language.onLanguageChange += lang =>
            {
                SpeechKit.lang = lang switch
                {
                    "ru" => SpeechKit.SPLang.ru_RU,
                    "en" => SpeechKit.SPLang.en_US,
                    "uk" => SpeechKit.SPLang.en_US,
                    "de" => SpeechKit.SPLang.en_US,
                    "kk" => SpeechKit.SPLang.ru_RU,
                    "ky" => SpeechKit.SPLang.ru_RU,
                    _ => SpeechKit.SPLang.ru_RU
                };
            };
        }

        public void Init()
        {
            var urlInfo = BookDatabase.Instance.URLInfo;
            SpeechKit.AuthCode = urlInfo.YandexTTS;
            
            var split = urlInfo.SpeechPath.Split('/').ToList();
            _bucket = split[0];
            split.RemoveAt(0);
            _s3Folder = string.Join("/", split);
        }
        

        private IEnumerator SpeakingRoutine(string txt, SpeechKit.SPVoice voice, bool isShowSubtitle)
        {
            _isDone = false;
        
            if(isShowSubtitle)
                GameManager.Instance.subtitleController.Init(txt);
            
            AudioClip audioClip = default;
            yield return StartCoroutine(GetSpeech(txt, voice, val => audioClip = val));

            if (audioClip != null)
            {
                _currentAudioSource.clip = audioClip;
                _currentAudioSource.Play();

                while (_currentAudioSource != null && _currentAudioSource.isPlaying) { yield return null; }

                _currentAudioSource.clip.UnloadAudioData();
        
                if(isShowSubtitle)
                    GameManager.Instance.subtitleController.SetActivePanel(false);
            }
            
            _isDone = true;
        }

        public void PauseSpeaking()
        {
            Debug.Log("Pause Speaking...");
            if (_currentAudioSource != null)
                _currentAudioSource.Pause();
        }

        public void StopSpeaking()
        {
            StopAllCoroutines();
            if (_currentAudioSource != null)
                _currentAudioSource.Stop();

            if(_currentAudioSource != null && _currentAudioSource.clip != null)
                _currentAudioSource.clip.UnloadAudioData();
                
            GameManager.Instance.subtitleController.SetActivePanel(false);
            _isDone = true;
        }

        public void ResumeSpeaking()
        {
            Debug.Log("Resume Speaking...");
            if (_currentAudioSource != null && Math.Abs(_currentAudioSource.time) > 0.001f)
            {
                _currentAudioSource.Play();
                StopAllCoroutines();
                StartCoroutine(ResumeRoutine());
            }
        }

        private IEnumerator ResumeRoutine()
        {
            while (_currentAudioSource != null && _currentAudioSource.isPlaying)
            { yield return null; }

            _isDone = true;
        }

        public void Speak(string txt, Character character, bool isShowSubtitle = true, AudioSource audioSource = null)
        {
            if(string.IsNullOrEmpty(txt)) return;

            SpeechKit.SPVoice voice = default;
            switch (character)
            {
                case Character.Doctor:
                    voice = _doctorVoice;
                    _currentAudioSource = _mainAudioSource;
                    break;
                case Character.Patient:
                    voice = _patientVoice;
                    _currentAudioSource = audioSource;
                    break;
                case Character.Assistant:
                    voice = _assistantVoice;
                    _currentAudioSource = _mainAudioSource;
                    var assistant = GameManager.Instance.assetController.assistantAsset;
                    if(assistant != null)
                        _currentAudioSource = assistant.GetAudioSource();
                    break;
            }
            
            if(_currentAudioSource == null) return;
        
            Debug.Log("StartSpeaking");
            _currentAudioSource.Pause();
            StopAllCoroutines();
            StartCoroutine(SpeakingRoutine(txt, voice, isShowSubtitle));
        }

        public void SetGenderVoice(Character character, int gender)
        {
            switch (character)
            {
                case Character.Doctor:
                    _doctorVoice = gender == 0 ? SpeechKit.SPVoice.oksana : SpeechKit.SPVoice.ermil;
                    break;
                case Character.Patient:
                    _patientVoice = gender == 0 ? SpeechKit.SPVoice.alyss : SpeechKit.SPVoice.erkanyavas;
                    break;
                case Character.Assistant:
                    _assistantVoice = gender == 0 ? SpeechKit.SPVoice.alena : SpeechKit.SPVoice.filipp;
                    break;
            }
        }

        public bool IsFinishedSpeaking()
        {
            return _currentAudioSource == null || (!_currentAudioSource.isPlaying && _isDone);
        }

        private void OnDestroy()
        {
            if (_currentAudioSource != null)
                _currentAudioSource.Pause();
        }
        
        private IEnumerator GetSpeech(string txt, SpeechKit.SPVoice voice, Action<AudioClip> callback)
        {
            var filename = $"{ComputeSha256Hash(txt)}_{voice}.ogg";
            var localdir = $"{DirectoryPath.Speech}{Language.Code}/{voice}";
            var localpath = $"{localdir}/{filename}";
            
            DirectoryPath.CheckDirectory(localdir);

            byte[] bytes = default;
            if (File.Exists(localpath))
            {
                yield return StartCoroutine(FileHandler.ReadBytesFile(localpath, val => bytes = val));
            }
            else
            {
                if (AmazonS3.Instance.CheckFileExists(_bucket, $"{_s3Folder}{Language.Code}/{voice}/{filename}"))
                    yield return StartCoroutine(RequestSpeechFromAWS(filename, localpath, voice, val => bytes = val));
                else
                    yield return StartCoroutine(RequestSpeechFromYandex(txt, voice, filename, localpath, val => bytes = val));
            }
            
            
            if(bytes == default) yield break;
                
            callback?.Invoke(CreateAudioClip(bytes));
        }


        private IEnumerator RequestSpeechFromAWS(string filename, string filepath, SpeechKit.SPVoice voice, Action<byte[]> callback)
        {
            byte[] bytes = default;
            yield return StartCoroutine(AmazonS3.Instance.DownloadFile(_bucket, $"{_s3Folder}{Language.Code}/{voice}/{filename}", filepath));
            yield return StartCoroutine(FileHandler.ReadBytesFile(filepath, val => bytes = val));
            callback?.Invoke(bytes);
        }
        
        private IEnumerator RequestSpeechFromYandex(string txt, SpeechKit.SPVoice voice, string filename, string localpath, Action<byte[]> callback)
        {
            var speechRequest = SpeechKit.TextToSpeech(txt, localpath, voice);
            
            while (speechRequest is {IsCompleted: false})
            {
                if(speechRequest.IsFaulted) yield break;
                yield return null;
            }
            callback?.Invoke(speechRequest.Result);

            StartCoroutine(AmazonS3.Instance.UploadFile(localpath, _bucket, $"{_s3Folder}{Language.Code}/{voice}/{filename}"));
        }

        private static AudioClip CreateAudioClip(byte[] bytes)
        {
            const float max = -(float) short.MinValue;
            var samples = new float[bytes.Length / 2];

            for (var i = 0; i < samples.Length; i++)
            {
                var int16sample = BitConverter.ToInt16(bytes, i * 2);
                samples[i] = int16sample / max;
            }
            
            var clip = AudioClip.Create("MyPlayback", bytes.Length / 2, 1, 48000, false);
            clip.SetData(samples, 0);

            return clip;
        }
        
        private static string ComputeSha256Hash(string rawData)  
        {
            // Create a SHA256   
            using (var sha256Hash = SHA256.Create())  
            {
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();  
                
                foreach (var t in bytes)
                    builder.Append(t.ToString("x2"));
                
                return builder.ToString();  
            }  
        } 
    }
}
