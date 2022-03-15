using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Modules.Books;
using Modules.WDCore;
#if UNITY_EDITOR
using ParrelSync;
#endif
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.ProfilesModels;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.Playfab
{
    public class PlayFabLoginController : MonoBehaviour
    {
        public string sessionTicket;
        public string entityToken;
        public string entityId;
        public string entityType;
        public string displayName;
        public string userName;
        public string userEmail;
        public string playFabId;
        public Action<string, string> OnDisplayNameUpdate;
        public Action<bool> OnUserLogin;
        public bool isLogged;

        private PlayFabLoginView _view;
        private Dictionary<string, UserDataRecord> _userData;

        private void Awake()
        {
            _view = GetComponent<PlayFabLoginView>();

            if(!PlayerPrefs.HasKey("USE_NAME_IN_DIALOG"))
            {
                PlayerPrefs.SetInt("USE_NAME_IN_DIALOG", 0);
                PlayerPrefs.Save();
            }
            
            _view.displayNameSubmit.onClick.AddListener(SubmitDisplayName);
            _view.loginSubmit.onClick.AddListener(Login);
            _view.registerSubmit.onClick.AddListener(Register);
            _view.resetPwdSubmit.onClick.AddListener(ResetPwd);
            OnDisplayNameUpdate += (dspName, _) => { _view.displayNameInput.text = dspName; };

            _view.closeDisplayName.onClick.AddListener(() => ShowDisplayNameForm(false));
            _view.closeLogin.onClick.AddListener(() => ShowLoginForm(false));
            _view.closeRegister.onClick.AddListener(() => ShowRegisterForm(false));
            _view.closeResetPwd.onClick.AddListener(() => ShowResetPwdForm(false));
            _view.loginResetPwdButton.onClick.AddListener(() => ShowResetPwdForm(true));
            _view.showPwdLoginButton.onClick.AddListener(() =>
            {
                _view.passwordLoginInput.contentType =
                    _view.passwordLoginInput.contentType == TMP_InputField.ContentType.Password ?
                    TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
                _view.passwordLoginInput.ForceLabelUpdate();
            });
            _view.showPwdRegisterButton.onClick.AddListener(() =>
            {
                _view.passwordRegisterInput.contentType =
                    _view.passwordRegisterInput.contentType == TMP_InputField.ContentType.Password ?
                    TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
                _view.passwordRegisterInput.ForceLabelUpdate();
            });
            
            _view.useNameInDialog.onValueChanged.AddListener(val => {
                PlayerPrefs.SetInt("USE_NAME_IN_DIALOG", val ? 1 : 0);
                PlayerPrefs.Save();});
            _view.useNameInDialog.SetIsOnWithoutNotify(PlayerPrefs.GetInt("USE_NAME_IN_DIALOG") == 1);
            

#if UNITY_XR            
            foreach (var view in _viewByProduct.Values)
            {
                view.activationInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.activationInput));
                view.emailLoginInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.emailLoginInput));
                view.passwordLoginInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.passwordLoginInput));
                view.displayNameInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.displayNameInput));
                view.emailRegisterInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.emailRegisterInput));
                view.passwordRegisterInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.passwordRegisterInput));
                view.repeatPasswordRegisterInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.repeatPasswordRegisterInput));
                view.usernameRegisterInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.usernameRegisterInput));
                view.resetPwdInput.onSelect.AddListener(val =>
                    GameManager.Instance.keyboardController.OpenKeyboard(view.resetPwdInput));
            }
            
#endif
        }
        
        public void Init()
        {
            Debug.Log("Playfab authorization: Start");

            var uniqueId = SystemInfo.deviceUniqueIdentifier;
            
            #if UNITY_EDITOR
            if (ClonesManager.IsClone())
                uniqueId += ClonesManager.GetArgument(); 
            #endif
            
            var request = new LoginWithCustomIDRequest { 
                CustomId = uniqueId, 
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserAccountInfo = true,
                    GetTitleData = true,
                    GetUserData = true
                }
            };
            
            PlayFabClientAPI.LoginWithCustomID(request, OnCustomIDLoginSuccess, OnError);
        }

        public void ShowDisplayNameForm(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.ShowDisplayNameForm(val);
        }

        public void ShowLoginForm(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.ShowLoginForm(val);
        }
        
        public void ShowRegisterForm(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.ShowRegisterForm(val);
        }
        
        public void ShowResetPwdForm(bool val)
        {
            _view.ShowLoginForm(false);
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.ShowResetPwdForm(val);
        }

        private void OnCustomIDLoginSuccess(LoginResult result)
        {
            BookDatabase.Instance.Configurations = result.InfoResultPayload.TitleData;
            var isRegistered = !string.IsNullOrEmpty(result.InfoResultPayload.AccountInfo.Username);
            userName = isRegistered ? result.InfoResultPayload.AccountInfo.Username : null;
            displayName = result.InfoResultPayload.PlayerProfile?.DisplayName;
            OnDisplayNameUpdate?.Invoke(displayName, userName);

            _userData = result.InfoResultPayload.UserData;
            entityId = result.EntityToken.Entity.Id;
            entityType = result.EntityToken.Entity.Type;
            entityToken = result.EntityToken.EntityToken;
            sessionTicket = result.SessionTicket;
            userEmail = result.InfoResultPayload.AccountInfo.PrivateInfo.Email;
            playFabId = result.PlayFabId;

            OnUserLogin?.Invoke(isRegistered);
            isLogged = true;
            Debug.Log("Playfab authorization: End");
        }
        

        private void OnError(PlayFabError error)
        {
            Debug.Log("Here's some debug information:");
            Debug.Log(error.Error);
            var errTxt = error.Error switch
            {
                PlayFabErrorCode.UsernameNotAvailable => 34,
                PlayFabErrorCode.EmailAddressNotAvailable => 35,
                PlayFabErrorCode.ConnectionError => 30,
                PlayFabErrorCode.InvalidUsername => 36,
                PlayFabErrorCode.InvalidEmailAddress => 37,
                PlayFabErrorCode.InvalidPassword => 38,
                PlayFabErrorCode.InvalidEmailOrPassword => 39,
                PlayFabErrorCode.InvalidUsernameOrPassword => 43,
                PlayFabErrorCode.AccountNotFound => 40,
                PlayFabErrorCode.NameNotAvailable => 252,
                _ => -1
            };

            _view.ShowError(errTxt != -1 ? TextData.Get(errTxt) : error.Error.ToString());
        }

        private void SubmitDisplayName()
        {
            var newDisplayName = _view.displayNameInput.text;
            if(string.IsNullOrEmpty(newDisplayName)) return;
            
            var request = new UpdateUserTitleDisplayNameRequest {DisplayName = newDisplayName};
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, result =>
            {
                OnDisplayNameUpdate?.Invoke(result.DisplayName, userName);
                ShowDisplayNameForm(false);
            }, OnError);
        }

        private void Register()
        {
            var username = _view.usernameRegisterInput.text;
            var email = _view.emailRegisterInput.text;
            var password = _view.passwordRegisterInput.text;
            var repeatPassword = _view.repeatPasswordRegisterInput.text;
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) 
                                               || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(repeatPassword))
            {
                _view.ShowError(TextData.Get(41));
                return;
            }
            
            if(ParamChecker(email, username, password)) return;
            
            if (!password.Equals(repeatPassword))
            {
                _view.ShowError(TextData.Get(279));
                return;
            }
            
            var request = new AddUsernamePasswordRequest
            {
                Username = username,
                Email = email,
                Password = password
            };
            PlayFabClientAPI.AddUsernamePassword(request, val => OnRegisterSuccess(val, email), OnError);
        }

        private void OnRegisterSuccess(AddUsernamePasswordResult result, string email)
        {
            ShowRegisterForm(false);

            userEmail = email;
            userName = result.Username;
            OnDisplayNameUpdate?.Invoke(displayName, userName);
            OnUserLogin?.Invoke(true);

            _view.usernameRegisterInput.text = "";
            _view.emailRegisterInput.text = "";
            _view.passwordRegisterInput.text = "";
            _view.repeatPasswordRegisterInput.text = "";

            StartCoroutine(SyncPortal(true));
        }

        public IEnumerator SyncPortal(bool isSyncUser)
        {
            yield break;
            // if(GameManager.Instance.defaultProduct != GameManager.Product.Dimedus) yield break;
            //
            // var url = isSyncUser ? 
            //     "https://portal.dimedus.com/api/v1/synchronize-user" :
            //     "https://portal.dimedus.com/api/v1/synchronize-statistics";
            //
            // var body = "{\"playfab_id\": \"" + playFabId + "\"}";
            // using var webRequest = new UnityWebRequest(url, "POST");
            // var bodyRaw = Encoding.UTF8.GetBytes(body);
            // webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            // webRequest.downloadHandler = new DownloadHandlerBuffer();
            // webRequest.SetRequestHeader("Content-Type", "application/json");
            // webRequest.SetRequestHeader("X-SESSION-TICKET", sessionTicket);
            //     
            // yield return webRequest.SendWebRequest();
            //
            // if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            // {
            //     Debug.Log($"URL: {url}. Error: {webRequest.error}");
            //     yield return new WaitForSeconds(2.0f);
            //     yield return StartCoroutine(SyncPortal(isSyncUser));
            // }
            // else
            // {
            //     Debug.Log("Portal SyncUser/SyncStatistics status: " + webRequest.result);
            // }
        }

        private void Login()
        {
            var email = _view.emailLoginInput.text;
            var password = _view.passwordLoginInput.text;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _view.ShowError(TextData.Get(41));
                return;
            }

            if (email.Contains("@"))
            {
                if(ParamChecker(email, "", password)) return;
                var request = new LoginWithEmailAddressRequest
                {
                    Email = email,
                    Password = password,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                    {
                        GetPlayerProfile = true,
                        GetUserAccountInfo = true
                    }
                };
            
                PlayFabClientAPI.LoginWithEmailAddress(request, OnEmailLoginSuccess, OnError);
            }
            else
            {
                if(ParamChecker("", email, password)) return;
                var request = new LoginWithPlayFabRequest()
                {
                    Username = email,
                    Password = password,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                    {
                        GetPlayerProfile = true,
                        GetUserAccountInfo = true,
                        GetUserData = true
                    }
                };
                PlayFabClientAPI.LoginWithPlayFab(request, OnEmailLoginSuccess, OnError);
            }
            
        }

        private void OnEmailLoginSuccess(LoginResult result)
        {
            ShowLoginForm(false);

            var request = new LinkCustomIDRequest()
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                ForceLink = true
            };
            PlayFabClientAPI.LinkCustomID(request, idResult => {}, OnError);

            _view.emailLoginInput.text = "";
            _view.passwordLoginInput.text = "";

            _userData = result.InfoResultPayload.UserData;
            entityId = result.EntityToken.Entity.Id;
            entityType = result.EntityToken.Entity.Type;
            sessionTicket = result.SessionTicket;
            userEmail = result.InfoResultPayload.AccountInfo.PrivateInfo.Email;
            userName = result.InfoResultPayload.AccountInfo.Username;
            playFabId = result.PlayFabId;
            displayName = result.InfoResultPayload.PlayerProfile?.DisplayName;
            OnDisplayNameUpdate?.Invoke(displayName, userName);
            
            OnUserLogin?.Invoke(true);
            isLogged = true;

            StartCoroutine(GameManager.Instance.statisticsManager.AnonUserAndOldUserSync());
        }
        

        private void ResetPwd()
        {
            var email = _view.resetPwdInput.text;
            if (string.IsNullOrEmpty(email))
            {
                _view.ShowError(TextData.Get(41));
                return;
            }
            if(ParamChecker(email)) return;

            BookDatabase.Instance.Configurations.TryGetValue("PlayFabTitleId", out var titleId);
            var request = new SendAccountRecoveryEmailRequest()
            {
                Email = email,
                TitleId = titleId
            };
            PlayFabClientAPI.SendAccountRecoveryEmail(request, result =>
            {
                ShowResetPwdForm(false);
                ShowLoginForm(true);
            }, OnError);
        }

        private bool ParamChecker(string email = "", string username = "", string password = "")
        {
            var isError = false;
            if (!string.IsNullOrEmpty(username))
            {
                isError = username.Length < 3 || username.Length > 20;
                if(isError)
                    _view.ShowError(TextData.Get(31));
            }
            if (!string.IsNullOrEmpty(email))
            {
                isError = !Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
                if(isError)
                    _view.ShowError(TextData.Get(33));
            } 
            if (!string.IsNullOrEmpty(password))
            {
                isError = password.Length < 6 || password.Length > 100;
                if(isError)
                    _view.ShowError(TextData.Get(32));
            }
            return isError;
        }
        
        public void SetUserData(string key, string val) {
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest {
                    Data = new Dictionary<string, string> {{key, val}}
                }, null, OnError);
        }

        public string GetUserData(string key)
        {
            _userData.TryGetValue(key, out var val);
            return val?.Value;
        }

        public IEnumerator ChangeProfileLanguage(string language)
        {
            var isProfileUpdated = false;
            var request = new SetProfileLanguageRequest
            {
                Language = language,
                Entity = new PlayFab.ProfilesModels.EntityKey{Type = entityType, Id = entityId}
            };
            PlayFabProfilesAPI.SetProfileLanguage(request, res =>
            {
                Debug.Log("The language on the entity's profile has been updated.");
                isProfileUpdated = true;
            }, OnError);

            yield return new WaitUntil(() => isProfileUpdated);
        }

        public void Logout()
        {
            PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier
            }, OnLogoutSuccess, OnError);
        }

        private void OnLogoutSuccess(UnlinkCustomIDResult result)
        {
            PlayFabClientAPI.ForgetAllCredentials();
            GameManager.Instance.statisticsManager.CleanStatisticsOnLogout();
            Init();
        }
    }
}