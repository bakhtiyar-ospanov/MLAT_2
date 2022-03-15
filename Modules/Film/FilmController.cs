using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Film
{
    public class FilmController : MonoBehaviour
    {
        private FilmView _view;
        private bool _isLaunched;
        private float _duration;
        private string _bucket;
        private string _path;

        private void Awake()
        {
            _view = GetComponent<FilmView>();
            _view.volumeButton.onClick.AddListener(ControlVolume);
            _view.backButton.onClick.AddListener(Back);
        }
        
        public void Init()
        {
            GameManager.Instance.mainMenuController.AddModule("Film", "", SetActivePanel, new []{_view.root.transform});

            var gsAuth = GameManager.Instance.GSAuthController;
            var allFilms = BookDatabase.Instance.MedicalBook.films;
            
            allFilms.ForEach(x => x.isAvailable = false);
            allFilms.Where(x => x.library != null && x.library.Contains("Demo")).
                ToList().ForEach(x => x.isAvailable = true);     

            if(gsAuth.isActivated && gsAuth.libraries != null)
            {
                foreach (var library in gsAuth.libraries)
                {
                    allFilms.Where(x => x.library != null && x.library.Contains(library)).
                        ToList().ForEach(x => x.isAvailable = true);
                }
            }
            
            allFilms = allFilms.OrderByDescending(x => x.isAvailable).ToList();
            
            var names = allFilms.Select(x => x.name).ToList();
            var actions = Language.Code switch
            {
                "ru" => allFilms.Select(x => (UnityAction) (() => StartCoroutine(OpenFilm(x.ru_filename, x.isAvailable)))).ToList(),
                "ky" => allFilms.Select(x => (UnityAction) (() => StartCoroutine(OpenFilm(x.ru_filename, x.isAvailable)))).ToList(),
                "uk" => allFilms.Select(x => (UnityAction) (() => StartCoroutine(OpenFilm(x.ru_filename, x.isAvailable)))).ToList(),
                "kk" => allFilms.Select(x => (UnityAction) (() => StartCoroutine(OpenFilm(x.kz_filename, x.isAvailable)))).ToList(),
                _ => allFilms.Select(x => (UnityAction) (() => StartCoroutine(OpenFilm(x.en_filename, x.isAvailable)))).ToList(),
            };
            var availability = allFilms.Select(x => x.isAvailable).ToList();
            
            _view.SetValues(names, actions, availability);
            
            var split = BookDatabase.Instance.URLInfo.FilmsPath.Split('/').ToList();
            _bucket = split[0];
            split.RemoveAt(0);
            _path = string.Join("/", split);
            
            GameManager.Instance.GSAuthController.onActivationChanged += Init;
        }

        private IEnumerator OpenFilm(string id, bool isAvailable)
        {
            if (!isAvailable)
            {
                GameManager.Instance.warningController.ShowExitWarning(TextData.Get(342), () => 
                {
                    GameManager.Instance.mainMenuController.ShowMenu("Profile");
                    GameManager.Instance.GSAuthController.ShowActivationForm(true);
                }, false, null, TextData.Get(315));
                yield break;
            }
            _view.loading.SetActive(true);
            _view.SetCurrentTime(0);
            _view.SetTotalTime(0);
            _view.VideoPlay(false);
            _view.playerCanvas.SetActive(true);
            _view.customPlayer.Start();
            
            _view.customPlayer.Load(GameManager.Instance.addressablesS3.GetSignedUrlVideo(_bucket, _path + id, 3.0f));
            yield return new WaitUntil(() => _view.customPlayer == null || _view.customPlayer.IsVideoMetadataLoaded());
            yield return new WaitUntil(() => _view.customPlayer == null || _view.customPlayer.IsPlaying());
            
            if(_view.customPlayer == null) yield break;

            _view.VideoPlay(true);
            _duration = _view.customPlayer.controller.GetDuration();
            _view.SetTotalTime(_duration);
            _view.ShowScreen(true);
            SetWatermark();
            _view.loading.SetActive(false);

            GameManager.Instance.starterController.EnableOrbitCamera(false);
            _isLaunched = true;
        }

        private void Update()
        {
            if(!_isLaunched) return;

            var currentTime = _view.slider.normalizedValue * _duration;
            _view.SetCurrentTime(currentTime);
            
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(_view.customPlayer.IsPlaying())
                    _view.customPlayer.Pause();
                else
                    _view.customPlayer.Play();
            }
            
            if(Input.GetKeyDown(KeyCode.M))
                ControlVolume();

            if (Input.GetKey(KeyCode.LeftArrow) && currentTime > 1.0f)
                Seek(currentTime - 1.0f);
            
            if (Input.GetKey(KeyCode.RightArrow) && currentTime < _duration)
                Seek(currentTime + 1.0f);
            
        }

        private void Seek(float val)
        {
            var normalized = val / _duration;
            _view.customPlayer.Pause();
            _view.customPlayer.Seek(normalized);
            _view.slider.normalizedValue = normalized;
            _view.customPlayer.Play();
        }

        private void SetActivePanel(bool val)
        {
            _view.canvas.SetActive(val);
        }
        
        private void ControlVolume()
        {
            var isMuted = AudioListener.volume == 0.0f;
            AudioListener.volume = isMuted ? 1.0f : 0.0f;
            _view.SetVolumeIcon(!isMuted);
        }

        private void Back()
        {
            _view.customPlayer.Pause();
            _isLaunched = false;
            _view.playerCanvas.SetActive(false);
            _view.ShowScreen(false);
            GameManager.Instance.starterController.EnableOrbitCamera(true);
        }

        private void SetWatermark()
        {
            var info = DateTime.Now.ToString("hh:mm, dd.MM.yyyy") + "\n" + 
                "Academix3D - " + GameManager.Instance.GSAuthController.GetKey();
            _view.watermark.text = info;
        }
    }
}
