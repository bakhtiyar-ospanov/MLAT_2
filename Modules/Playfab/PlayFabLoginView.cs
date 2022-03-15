
using System.Collections;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Playfab
{
    public class PlayFabLoginView : MonoBehaviour
    {
        public GameObject canvas;

        [Header("Display Name Form")] 
        public GameObject displayNameRoot;
        public TMP_InputField displayNameInput;
        public Button displayNameSubmit;
        public Button closeDisplayName;
        public Toggle useNameInDialog;

        [Header("Login Form")] 
        public GameObject loginRoot;
        public TMP_InputField emailLoginInput;
        public TMP_InputField passwordLoginInput;
        public Button showPwdLoginButton;
        public Button loginSubmit;
        public Button loginResetPwdButton;
        public Button closeLogin;
        
        [Header("Register Form")] 
        public GameObject registerRoot;
        public TMP_InputField usernameRegisterInput;
        public TMP_InputField emailRegisterInput;
        public TMP_InputField passwordRegisterInput;
        public TMP_InputField repeatPasswordRegisterInput;
        public Button showPwdRegisterButton;
        public Button registerSubmit;
        public Button closeRegister;
        
        [Header("Reset Password Form")] 
        public GameObject resetPwdRoot;
        public TMP_InputField resetPwdInput;
        public Button resetPwdSubmit;
        public Button closeResetPwd;
        
        [Header("Error Form")] 
        public GameObject errorRoot;
        public TextMeshProUGUI errorTxt;
        private Coroutine _errorCor;

        private void Awake()
        {
            canvas.SetActive(false);
            displayNameRoot.SetActive(false);
            loginRoot.SetActive(false);
            errorTxt.text = "";
        }

        public void ShowDisplayNameForm(bool val)
        {
            canvas.SetActive(val);
            displayNameRoot.SetActive(val);
        }

        public void ShowLoginForm(bool val)
        {
            canvas.SetActive(val);
            loginRoot.SetActive(val);
        }
        
        public void ShowRegisterForm(bool val)
        {
            canvas.SetActive(val);
            registerRoot.SetActive(val);
        }
        
        public void ShowResetPwdForm(bool val)
        {
            canvas.SetActive(val);
            resetPwdRoot.SetActive(val);
        }

        public void ShowError(string val)
        {
            if(_errorCor != null)
                StopCoroutine(_errorCor);
            if(!errorTxt.text.Contains(val))
                errorTxt.text += val + "\n";
            _errorCor = StartCoroutine(ShowErrorCor());
        }

        private IEnumerator ShowErrorCor()
        {
            errorRoot.SetActive(true);
            yield return new WaitForSeconds(5.0f);
            errorRoot.SetActive(false);
            errorTxt.text = "";
        }
    }
}
