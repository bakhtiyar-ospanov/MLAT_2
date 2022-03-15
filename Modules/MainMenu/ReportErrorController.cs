using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.MainMenu
{
    public class ReportErrorController : MonoBehaviour
    {
        private ReportErrorView _view;
        private string path;
        private MailMessage mail;
        private Attachment attachment;
        private Attachment logAttachment;

        private void Awake()
        {
            _view = GetComponent<ReportErrorView>();
            _view.reportError.onClick.AddListener(() => SetActivePanel(true));
            _view.closeButton.onClick.AddListener(() => SetActivePanel(false));
            _view.sendReport.onClick.AddListener(SendReport);
            _view.screenshot.onClick.AddListener(ScreenShot);
        
        }

        public void Init()
        {
            _view.reportError.gameObject.SetActive(true);
        }

        private void SetActivePanel(bool val)
        {
            _view.SetActivePanel(val);

            if (val)
            {
                SetDefaultUserInfo();
                _view.sendReport.interactable = true;
                _view.screenshot.interactable = true;
            } 
            
        }

        private void ScreenShot()
        {
            path = DirectoryPath.Screenshots + "Screenshot.png";

            try
            {
                _view.screenshotRoot.SetActive(true);
                _view.root.SetActive(false);
                ScreenCapture.CaptureScreenshot(path);
                StartCoroutine(LoadTexture());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private IEnumerator LoadTexture()
        {
            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(WebRequestHandler.Instance.TextureRequest(path, texture => 
            {
                _view.screenshotPreview.texture = texture;
                RectTransform rt = (RectTransform)_view.screenshotRoot.transform;
                float referenceLength = rt.rect.width;
                _view.screenshotPreview.rectTransform.sizeDelta = texture.width > texture.height ?
                    new Vector2(referenceLength, referenceLength * texture.height / texture.width) :
                    new Vector2(referenceLength * texture.height / texture.width, referenceLength);
            }));
        
            _view.root.SetActive(true);
        }
        private void SendReport()
        {
            var config = BookDatabase.Instance.Configurations;
            if (config == null) return;

            string feedback;
            config.TryGetValue("FeedbackInfo", out feedback);
            string[] separator = { ", " };

            var feedbackInfo = feedback.Split(separator, 3, StringSplitOptions.RemoveEmptyEntries);
            var receiverEmail = feedbackInfo[0];
            var senderEmail = feedbackInfo[1];
            var senderPass = feedbackInfo[2];
            var userName = string.IsNullOrEmpty(GameManager.Instance.playFabLoginController.userName) ? "" 
                : GameManager.Instance.playFabLoginController.userName;

            string[] _params = GetHardwareInfoSettings();

            mail = new MailMessage();
            mail.From = new MailAddress(senderEmail);
            mail.To.Add(receiverEmail);
            mail.Subject = TextData.Get(297);

            mail.Body =
                TextData.Get(296) + ":\n" + _view.description.text.Replace("<br>", "\n") +
                " \n -------------------------------------------- \n" +
                "name: " + _view.userName.text + "\n" +
                "playfab login: " + userName + "\n" +
                "e-mail: " + _view.userEmail.text +
                " \n ------------------------------------------- \n" +
                "session ticket: " + GameManager.Instance.playFabLoginController.sessionTicket + "\n" +
                "entity id: " + GameManager.Instance.playFabLoginController.entityId + "\n" +
                "playfab id: " + GameManager.Instance.playFabLoginController.playFabId +
                "\n-------------------------------------------- \n" +
                "productName: " + GameManager.Instance.defaultProduct.ToString() + "\n" +
                "applicationVersion: " + Application.version + "\n" +
                "language: " + Language.Code + "\n" +
                "activationKey: " + GameManager.Instance.GSAuthController.GetKey() + "\n" +
                "deviceName: " + _params[0] + "\n" +
                "processorType: " + _params[1] + "\n" +
                "graphicsDeviceName: " + _params[2] + "\n" +
                "systemMemorySize: " + _params[3] + "\n" +
                "deviceUniqueIdentifier: " + _params[4] + "\n" +
                "graphicsDeviceID: " + _params[5] + "\n";

            attachment = null;

            logAttachment = null;
            var logDir = Path.Combine(Application.persistentDataPath, "Logs");
            var logPath = Path.Combine(logDir, "L-" + DateTime.Now.ToString( "yyyy-MM-dd--HH-mm-ss" ) + ".txt" );
            File.WriteAllText(logPath, GameManager.Instance.debugLogManager.GetAllLogs());

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                attachment = new Attachment(path);
                mail.Attachments.Add(attachment);
            }

            if (!string.IsNullOrEmpty(logPath) && File.Exists(logPath))
            {
                logAttachment = new Attachment(logPath);
                mail.Attachments.Add(logAttachment);
            }

            SmtpClient smtpServer = new SmtpClient("smtp.yandex.com");
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(senderEmail, senderPass);
            smtpServer.EnableSsl = true;
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

            smtpServer.Timeout = 1000000;
            smtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);

            string userState = "test message1";
            smtpServer.SendAsync(mail, userState);

            _view.sendReport.interactable = false;
            _view.screenshot.interactable = false;
        }
        public string[] GetHardwareInfoSettings()
        {
            string[] parametres = new string[6];

            parametres[0] = SystemInfo.deviceName;
            parametres[1] = SystemInfo.processorType;
            parametres[2] = SystemInfo.graphicsDeviceName;
            parametres[3] = SystemInfo.systemMemorySize.ToString();
            parametres[4] = SystemInfo.deviceUniqueIdentifier;
            parametres[5] = SystemInfo.graphicsDeviceID.ToString();

            return parametres;
        }

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            string token = (string)e.UserState;
            SetActivePanel(false);

            if (e.Cancelled)
            {
                GameManager.Instance.warningController.ShowWarning(TextData.Get(294) + token) ;
                Debug.LogWarning("Send canceled " + token);
            }
            if (e.Error != null)
            {
                GameManager.Instance.warningController.ShowWarning(TextData.Get(294) + token + " " + e.Error.Message);
                Debug.LogWarning("Sending error - " + token + " " + e.Error.Message);
            }
            else
            {
                ClearInfo();
                GameManager.Instance.warningController.ShowWarning(TextData.Get(293));
                StartCoroutine(GameManager.Instance.statisticsManager.IncrementFeedbackCount());
                Debug.Log("Message sent.");
            }

            mail.Attachments.Clear();
            if (attachment != null)
                attachment.Dispose();
        }

        private void SetDefaultUserInfo()
        {
            var userName = GameManager.Instance.playFabLoginController.userName;
            var userEmail = GameManager.Instance.playFabLoginController.userEmail;
            var displayName = GameManager.Instance.playFabLoginController.displayName;

            if (string.IsNullOrEmpty(_view.userName.text))
                _view.userName.text = !string.IsNullOrEmpty(userName) ? userName : 
                    !string.IsNullOrEmpty(displayName) ? displayName : _view.userName.text;

            if (!string.IsNullOrEmpty(userEmail) && string.IsNullOrEmpty(_view.userEmail.text))
                _view.userEmail.text = userEmail;
        }

        private void ClearInfo()
        {
            path = "";
            _view.screenshotPreview.texture = null;
            _view.screenshotRoot.SetActive(false);
            _view.description.text = "";
        }
    }
}
