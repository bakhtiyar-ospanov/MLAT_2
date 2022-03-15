using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.MainMenu
{
    public class ProfileController : MonoBehaviour
    {
        public const string WebsiteUrl = "http://vardix-group.com/academix/?utm_source=academix&utm_medium=aboutlink";
        
        private ProfileView _view;
        private Coroutine _updateNameRoutine;
        private string _userName;

        private void Awake()
        {
            _view = GetComponent<ProfileView>();
            _view.changeNameButton.onClick.AddListener(() => GameManager.Instance.playFabLoginController.ShowDisplayNameForm(true));
            //_view.leaderboardButton.onClick.AddListener(GameManager.Instance.playFabLeaderboardController.Init);
            _view.openStatisticsButton.onClick.AddListener(() => GameManager.Instance.statisticsController.Open(true));
            _view.awardButton.onClick.AddListener(() => GameManager.Instance.awardController.SetActivePanel(true));
            _view.activationButton.onClick.AddListener(() => GameManager.Instance.GSAuthController.ShowActivationForm(true));
            _view.loginButton.onClick.AddListener(() => GameManager.Instance.playFabLoginController.ShowLoginForm(true));
            _view.registerButton.onClick.AddListener(() => GameManager.Instance.playFabLoginController.ShowRegisterForm(true));
            _view.logoutButton.onClick.AddListener(() => GameManager.Instance.playFabLoginController.Logout());
            _view.websiteButton.onClick.AddListener(() => Application.OpenURL(WebsiteUrl));

            GameManager.Instance.playFabLoginController.OnDisplayNameUpdate += UpdateDisplayName;
            GameManager.Instance.playFabLoginController.OnUserLogin += val =>
            {
                _view.loginButton.gameObject.SetActive(!val);
                _view.registerButton.gameObject.SetActive(!val);
                _view.logoutButton.gameObject.SetActive(val);
            };
            
        }

        public void Init()
        {
            GameManager.Instance.mainMenuController.AddModule("Profile", "", SetActivePanel,new []{_view.root.transform});
        }

        private void SetActivePanel(bool val)
        {
            _view.canvas.SetActive(val);
        }

        public void ShowCentralBox(bool val)
        {
            _view.centralBox.SetActive(val);
        }

        private void UpdateDisplayName(string newName, string usrname)
        {
            if(_updateNameRoutine != null)
                StopCoroutine(_updateNameRoutine);
            _updateNameRoutine = StartCoroutine(UpdateDisplayNameRoutine(newName, usrname));
        }

        private IEnumerator UpdateDisplayNameRoutine(string newName, string usrnm)
        {
            _userName = newName;
            yield return new WaitUntil(() => BookDatabase.Instance.isDone);
            var outTxt = TextData.Get(15);
            outTxt += string.IsNullOrEmpty(newName) ? "" : $", {newName}!";
            outTxt += string.IsNullOrEmpty(usrnm) ? "" : string.IsNullOrEmpty(newName) ? $" @{usrnm}" : $"\n@{usrnm}";
            _view.displayName.text = outTxt;
        }

        public void HideActivateKey()
        {
            _view.activationButton.gameObject.SetActive(false);
        }
        
        public string GetUsername()
        {
            return _userName;
        }
    }
}
