using System;
using System.Collections;
using System.Collections.Generic;
using Modules.Books;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Modules.GS_Auth
{
    public class GSConnector : MonoBehaviour
    {
        private class AuthData 
        {
            public string webServiceUrl;
            public string servicePassword;
        }

        private AuthData _authData;

        public void Init()
        {
            if(_authData != null) return;
            _authData = JsonConvert.DeserializeObject<AuthData>(BookDatabase.Instance.Configurations["Auth"]);
        }

        public IEnumerator CreateRequest(Dictionary<string, string> form, Action<string> callback)
        {
            if(!form.ContainsKey("pass"))
                form.Add("pass", _authData.servicePassword);
            
            var www = UnityWebRequest.Post(_authData.webServiceUrl, form);
            www.timeout = 10;
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                yield return new WaitForSeconds(5.0f);
                yield return StartCoroutine(CreateRequest(form, callback));
            }
            else
            {
                callback?.Invoke(www.downloadHandler.text);
            }
        }
    }
}
