using System.Collections;
using Mirror;
using Modules.Books;
using Modules.WDCore;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Modules.Multiplayer
{
    public class ClientNetworkManager : NetworkManager
    {
        public class MultiplayerInfo
        {
            public string ServerScene;
            public string IPAddress;
            public string Port;
        }
        
        private TelepathyTransport _telepathy;
        private MultiplayerInfo _info;
        private bool _isTimeout;

        public override void Awake()
        {
            base.Awake();
            _telepathy = GetComponent<TelepathyTransport>();
        }

        public void Init()
        {
            _info = JsonConvert.DeserializeObject<MultiplayerInfo>(BookDatabase.Instance.Configurations["Multiplayer"]);
        }

        public IEnumerator MultiplayerCheck()
        {
            yield return new WaitUntil(() => _info != null);
            
            var activeScene = SceneManager.GetActiveScene().name;
            if (_info.ServerScene.Equals(activeScene) && GameManager.Instance.starterController.isFreeMode)
                yield return StartCoroutine(ConnectToMultiplayer());
            else
                DisconnectMultiplayer();
        }

        public void DisconnectMultiplayer()
        {
            if(!GameManager.Instance.chatController.isRegistered()) return;
            Debug.Log("DisconnectMultiplayer");
            StopClient();
            GameManager.Instance.chatController.LeaveChannel();
            GameManager.Instance.starterController.RestoreLocalPlayer();
        }

        private IEnumerator ConnectToMultiplayer()
        {
            networkAddress = _info.IPAddress;
            //networkAddress = "localhost";
            _telepathy.port = ushort.Parse(_info.Port);
            StartClient();
            GameManager.Instance.chatController.JoinChannel();

            StartCoroutine(ConnectionTimeout());

            yield return new WaitUntil(() => GameManager.Instance.chatController.isRegistered() || _isTimeout);
        }

        private IEnumerator ConnectionTimeout()
        {
            _isTimeout = false;
            yield return new WaitForSecondsRealtime(5.0f);
            _isTimeout = true;
        }
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("connected ");
        }
        
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            Debug.Log("disconnected");
        }
    }
}
