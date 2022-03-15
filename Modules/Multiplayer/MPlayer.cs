using System;
using Mirror;
using Modules.Starter;
using Modules.WDCore;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace Modules.Multiplayer
{
    public class MPlayer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameChanged))]
        public string playerName;
        
        [SyncVar(hook = nameof(OnAgoraUIDChanged))]
        public uint agoraUID;
        
        public FirstPersonAIO firstPersonAio;
        public Camera cam;
        public Transform target;
        public GameObject headMesh;
        public TextMeshProUGUI playerNameTxt;
        
        public override void OnStartServer()
        {
            Debug.Log("OnStartServer");
            DisableAnotherClientExtras();
        }

        public override void OnStartClient()
        {
            if (!isLocalPlayer)
            {
                DisableAnotherClientExtras();
            }
        }

        public override void OnStopClient()
        {
            Debug.Log("OnStopClient");
            base.OnStopClient();
            
            if(isLocalPlayer)
                GameManager.Instance.chatController.UnregisterLocalPlayer();
            else
                GameManager.Instance.chatController.UnregisterRemotePlayer(this);
        }

        public override void OnStartLocalPlayer()
        {
            Debug.Log("OnStartLocalPlayer");
            headMesh.SetActive(false);
            GameManager.Instance.starterController.ReplaceMPlayer(new FPCDesktop.PlayerComponents
            {
                FirstPersonAio = firstPersonAio, Cam = cam, Target = target
            });

            var playfabName = GameManager.Instance.playFabLoginController.displayName;
            var userName = GameManager.Instance.playFabLoginController.userName;
            ChangeNameOnServer(playfabName, userName);
            GameManager.Instance.playFabLoginController.OnDisplayNameUpdate += ChangeNameOnServer;
            GameManager.Instance.chatController.OnAgoraUIDChanged += ChangeAgoraUIDOnServer;
            GameManager.Instance.chatController.RegisterLocalPlayer(this);
        }

        private void DisableAnotherClientExtras()
        {
            headMesh.SetActive(true);
            firstPersonAio.enabled = false;
            foreach (Behaviour component in cam.GetComponents(typeof(Behaviour)))
                component.enabled = false;
        }

        [Command]
        private void ChangeNameOnServer(string newName, string usrname)
        {
            if (string.IsNullOrEmpty(newName))
                newName = "Anonymous#" + UnityEngine.Random.Range(0, 1000);
            playerName = newName;
        }
        
        [Command]
        private void ChangeAgoraUIDOnServer(uint uid)
        {
            agoraUID = uid;
        }

        private void OnNameChanged(string oldName, string newName)
        {
            playerNameTxt.text = newName;
        }
        
        private void OnAgoraUIDChanged(uint oldUid, uint newUid)
        {
            if(isLocalPlayer) return;
            
            GameManager.Instance.chatController.RegisterRemotePlayer(this, newUid);
        }
    }
}
