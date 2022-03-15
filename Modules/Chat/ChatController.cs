using System;
using System.Collections.Generic;
using agora_gaming_rtc;
using Modules.Books;
using Modules.Multiplayer;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking.Types;

namespace Modules.Chat
{
    public class ChatController : MonoBehaviour
    {
        private class Info
        {
            public string AgoraAppID;
            public string AgoraChannel;
        }

        public Action<uint> OnAgoraUIDChanged;
        private IRtcEngine _mRtcEngine;
        private Info _info;
        private ChatView _view;
        private bool _isMuted;
        private Dictionary<MPlayer, uint> _UIDsByMplayer = new();
        private MPlayer _localPlayer;

        private void Awake()
        {
#if (UNITY_2018_3_OR_NEWER)
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
			
            } 
            else 
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#endif

            _view = GetComponent<ChatView>();
            _view.volumeSlider.onValueChanged.AddListener(SetVolume);
            _view.micButton.onClick.AddListener(() =>
            {
                _isMuted = !_isMuted;
                MuteMic();
            });
        }

        public void Init()
        {
            _info = JsonConvert.DeserializeObject<Info>(BookDatabase.Instance.Configurations["Multiplayer"]);
        }

        public void JoinChannel()
        {
            _mRtcEngine = IRtcEngine.GetEngine(_info.AgoraAppID);
            _mRtcEngine.OnJoinChannelSuccess += (channelName, uid, elapsed) =>
            {
                Debug.Log("Joined audio channel: " + channelName + ", with uid: " + uid);
                OnAgoraUIDChanged?.Invoke(uid);
            };
            
            _mRtcEngine.OnLeaveChannel += (RtcStats stats) =>
            {
                Debug.Log("Channel left");
            };
            
            _mRtcEngine.OnUserJoined += (uint uid, int elapsed) =>
            {
                var userJoinedMessage = string.Format("onUserJoined callback uid {0} {1}", uid, elapsed);
                Debug.Log(userJoinedMessage);
            };

            _mRtcEngine.OnUserOffline += (uint uid, USER_OFFLINE_REASON reason) =>
            {
                var userOfflineMessage = string.Format("onUserOffline callback uid {0} {1}", uid, reason);
                Debug.Log(userOfflineMessage);
            };
            
            _mRtcEngine.OnWarning += (int warn, string msg) =>
            {
                var description = IRtcEngine.GetErrorDescription(warn);
                var warningMessage = string.Format("onWarning callback {0} {1} {2}", warn, msg, description);
                Debug.Log(warningMessage);
            };

            _mRtcEngine.OnError += (int error, string msg) =>
            {
                string description = IRtcEngine.GetErrorDescription(error);
                string errorMessage = string.Format("onError callback {0} {1} {2}", error, msg, description);
                Debug.Log(errorMessage);
            };
            
            _mRtcEngine.OnRequestToken += () =>
            {
                string requestKeyMessage = string.Format("OnRequestToken");
                Debug.Log(requestKeyMessage);
            };

            _mRtcEngine.OnConnectionInterrupted += () =>
            {
                var interruptedMessage = string.Format("OnConnectionInterrupted");
                Debug.Log(interruptedMessage);
            };

            _mRtcEngine.OnConnectionLost += () =>
            {
                var lostMessage = string.Format("OnConnectionLost");
                Debug.Log(lostMessage);
            };
            
            _mRtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_COMMUNICATION);
            _view.volumeSlider.SetValueWithoutNotify(1.0f);
            SetVolume(1.0f);

            _mRtcEngine.JoinChannel(_info.AgoraChannel, "extra");
            
            _isMuted = false;
            MuteMic();
            _view.canvas.SetActive(true);
        }
        
        public void LeaveChannel()
        {
            if (_mRtcEngine == null) return;
            _mRtcEngine.LeaveChannel();
            _view.canvas.SetActive(false);
            Debug.Log($"left channel name {_info.AgoraChannel}");
            IRtcEngine.Destroy();
            _mRtcEngine = null;
        }

        void OnApplicationQuit()
        {
            LeaveChannel();
        }

        private void SetVolume(float normalizedValue)
        {
            Debug.Log("Volume level: " + normalizedValue);
            _mRtcEngine.AdjustRecordingSignalVolume((int)Math.Round(normalizedValue * 100.0f, 0));
            AudioListener.volume = normalizedValue;
        }

        private void MuteMic()
        {
            Debug.Log("Mic status: " + !_isMuted);
            _view.micIcons[0].SetActive(_isMuted);
            _view.micIcons[1].SetActive(!_isMuted);
            _mRtcEngine.EnableLocalAudio(!_isMuted);
        }

        public void RegisterLocalPlayer(MPlayer mPlayer)
        {
            _localPlayer = mPlayer;
        }
        
        public void UnregisterLocalPlayer()
        {
            _localPlayer = null;
        }

        public bool isRegistered()
        {
            return _localPlayer != null;
        }

        public void RegisterRemotePlayer(MPlayer mPlayer, uint uid)
        {
            if (_UIDsByMplayer.ContainsKey(mPlayer))
                _UIDsByMplayer[mPlayer] = uid;
            else
                _UIDsByMplayer.Add(mPlayer, uid);
        }
        
        public void UnregisterRemotePlayer(MPlayer mPlayer)
        {
            if (_UIDsByMplayer.ContainsKey(mPlayer))
                _UIDsByMplayer.Remove(mPlayer);
        }

        private void Update()
        {
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if(_localPlayer == null || _UIDsByMplayer.Count == 0) return;

            foreach (var remotePlayer in _UIDsByMplayer)
            {
                var distance = Vector3.Distance(_localPlayer.transform.position, 
                    remotePlayer.Key.transform.position);

                var volume = 100 - Math.Clamp((int) Math.Round(distance) * 7, 0, 100);
                //Debug.Log($"Distance to {remotePlayer.Value} player: {distance}, vol: {volume}");
                
                _mRtcEngine.AdjustUserPlaybackSignalVolume(remotePlayer.Value, volume);
            }
        }
    }
}
