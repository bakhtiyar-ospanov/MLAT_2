using System;
using System.Collections.Generic;
using System.Linq;
using Modules.MainMenu;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.News
{
    public class NewsController : MonoBehaviour
    {
        private NewsView _view;
        private bool _isShown;

        private void Awake()
        {
            _view = GetComponent<NewsView>();
        }

        public void Init()
        {
            if (_isShown)
            {
                _view.SetActivePanel(false);
                _isShown = false;
                return;
            }
            
            if(!_view.itemButtons.Any(x => x.gameObject.activeSelf))
                _view.loadingWarning.SetActive(true);
            
            PlayFabClientAPI.GetTitleNews(new GetTitleNewsRequest{Count = 20}, ProcessNews, 
                error => Debug.Log(error.GenerateErrorReport()));
            
            _view.SetActivePanel(true);
            _isShown = true;
        }

        private void ProcessNews(GetTitleNewsResult result)
        {
            var calls = result.News.Select(x => (UnityAction) (() => 
                Application.OpenURL(ProfileController.WebsiteUrl))).ToList();
            _view.SetItemValues(result.News, calls);
            _view.loadingWarning.SetActive(false);
        }
    }
}
