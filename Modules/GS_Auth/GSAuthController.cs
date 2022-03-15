using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Modules.GS_Auth
{
    public class GSAuthController : MonoBehaviour
    {
        public Action onActivationChanged;
        public bool isActivated;
        public List<string> licenceTypes;
        public List<string> libraries;

        public class Activation
        {
            [JsonProperty("key")] public string key;
            [JsonProperty("expDate")] public string expirationDate;
            [JsonProperty("organization")] public string organization;
            [JsonProperty("licenceType")] public string licenceType;
            [JsonProperty("licenceName")] public string licenceName;
            [JsonProperty("library")] public string library;
            [JsonIgnore] public List<string> libraryCaptures;
        }
        
        private class Device
        {
            [JsonProperty("Key")] public string key;
            [JsonProperty("Unique_ID")] public string uniqueId;
            [JsonProperty("Device_Name")] public string deviceName;
            [JsonProperty("Processor")] public string processor;
            [JsonProperty("RAM")] public string RAM;
            [JsonProperty("Graphic_Card")] public string graphicCard;
            [JsonProperty("Language")] public string language;
            [JsonProperty("Version")] public string version;
            [JsonProperty("Username")] public string username;
            [JsonProperty("Email")] public string email;
            [JsonProperty("PlayfabId")] public string playfabId;
            [JsonProperty("EntityId")] public string entityId;
            [JsonProperty("Name")] public string name;
        }
        
        private class PartialDevice
        {
            [JsonProperty("Unique_ID")] public string uniqueId;
            [JsonProperty("Language")] public string language;
            [JsonProperty("Version")] public string version;
            [JsonProperty("Username")] public string username;
            [JsonProperty("Email")] public string email;
            [JsonProperty("PlayfabId")] public string playfabId;
            [JsonProperty("EntityId")] public string entityId;
            [JsonProperty("Name")] public string name;
        }
        
        private GSAuthView _view;
        private GSConnector _gsConnector;
        private List<Activation> _activations;
        private const string LicenseWebsiteUrl = "http://vardix-group.com/academix/?utm_source=academix&utm_medium=purchaselicense";

        private void Awake()
        {
            _view = GetComponent<GSAuthView>();
            _gsConnector = GetComponent<GSConnector>();
            _view.closeActivation.onClick.AddListener(() => ShowActivationForm(false));
            _view.buyLicence.onClick.AddListener(() => Application.OpenURL(LicenseWebsiteUrl));
            _view.activationSubmit.onClick.AddListener(Activate);
            _view.deviceIdTxt.text = SystemInfo.deviceUniqueIdentifier;
        }

        public void CheckActivation()
        {
            _gsConnector.Init();

            libraries = new List<string>();
            licenceTypes = new List<string>();
            isActivated = false;
            
            var playfab = GameManager.Instance.playFabLoginController;
            var device = new PartialDevice
            {
                uniqueId = SystemInfo.deviceUniqueIdentifier,
                language = Language.Code,
                version = Application.version,
                username = playfab.userName,
                email = playfab.userEmail,
                playfabId = playfab.playFabId,
                entityId = playfab.entityId,
                name = playfab.displayName
            };
            
            var rawJson = JsonConvert.SerializeObject(device);
            var form = new Dictionary<string, string> {{"action", "checkActivation"},{"jsonData", rawJson}};

            StartCoroutine(_gsConnector.CreateRequest(form, OnCheckActivation));
        }

        public void ShowActivationForm(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.root.SetActive(val);
        }

        private void Activate()
        {
            var key = _view.activationInput.text;
            
            if(string.IsNullOrEmpty(key))
            {
                _view.ShowError($"{TextData.Get(307)}");
                return;
            }
            
            key = key.ToUpper();
            
            var playfab = GameManager.Instance.playFabLoginController;
            var device = new Device
            {
                key = key,
                uniqueId = SystemInfo.deviceUniqueIdentifier,
                deviceName = SystemInfo.deviceName,
                processor = SystemInfo.processorType,
                RAM = SystemInfo.systemMemorySize.ToString(),
                graphicCard = SystemInfo.graphicsDeviceName,
                language = Language.Code,
                version = Application.version,
                username = playfab.userName,
                email = playfab.userEmail,
                playfabId = playfab.playFabId,
                entityId = playfab.entityId,
                name = playfab.displayName
            };

            var rawJson = JsonConvert.SerializeObject(device);
            var form = new Dictionary<string, string> {{"action", "activateKey"},{"jsonData", rawJson}};

            _view.activationSubmit.interactable = false;
            StartCoroutine(_gsConnector.CreateRequest(form, OnAddActivation));
        }

        private void OnAddActivation(string rawJson)
        {
            if(string.IsNullOrEmpty(rawJson)) return;

            var parsed = JObject.Parse(rawJson);
            var status = parsed["status"]?.ToString();

            switch (status)
            {
                case "NOT_VALID":
                    _view.ShowError($"{TextData.Get(238)}");
                    break;
                case "EXPIRED":
                    _view.ShowError($"{TextData.Get(240)} ({parsed["expirationDate"]})");
                    break;
                case "DEVICE_LIMIT":
                    _view.ShowError($"{TextData.Get(239)} ({parsed["deviceLimit"]})");
                    break;
                case "ALREADY_ACTIVATED":
                    _view.ShowError($"{TextData.Get(308)}");
                    break;
                case "ACTIVE":
                    ProcessActivations(parsed["activations"]?.ToString());
                    break;
            }
            
            _view.activationSubmit.interactable = true;
        }

        private void ProcessActivations(string response)
        {
            _activations = JsonConvert.DeserializeObject<List<Activation>>(response);
            _view.activationInput.text = "";

            licenceTypes = _activations.Where(x => !string.IsNullOrEmpty(x.licenceType)).
                Select(x => x.licenceType).Distinct().ToList();

            var libsById = BookDatabase.Instance.MedicalBook.libraryById;

            foreach (var activation in _activations)
            {
                activation.libraryCaptures = new List<string>();
                if(string.IsNullOrEmpty(activation.library)) continue;
                
                var parsed = activation.library.Replace(" ", "").Split(";");
                foreach (var libId in parsed)
                {
                    libsById.TryGetValue(libId, out var libs);
                    if(libs == null) continue;
                    libraries.AddRange(libs.captures);
                    activation.libraryCaptures.AddRange(libs.captures);
                }
            }

            _view.SetKeyInfo(_activations);
            libraries = libraries.Distinct().ToList();
            
            isActivated = true;
            onActivationChanged?.Invoke();
            _view.buyLicence.gameObject.SetActive(false);
        }

        private void OnCheckActivation(string rawJson)
        {
            if(string.IsNullOrEmpty(rawJson)) return;

            var parsed = JObject.Parse(rawJson);
            var status = parsed["status"]?.ToString();

            switch (status)
            {
                case "NOT_VALID":
                    break;
                case "ACTIVE":
                    ProcessActivations(parsed["activations"]?.ToString());
                    break;
            }
        }
        
        public string GetKey()
        {
            return _activations == null ? "" : _activations[0].key;
        }

    }
}
