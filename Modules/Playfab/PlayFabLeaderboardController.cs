using System;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

namespace Modules.Playfab
{
    public class PlayFabLeaderboardController : MonoBehaviour
    {
        private PlayFabLeaderboardView _view;
        private DateTime _startTime; 
        private bool _isUploaded;

        private void Awake()
        {
            _view = GetComponent<PlayFabLeaderboardView>();
            _view.closeButton.onClick.AddListener(() => SetActivePanel(false));
            _startTime = DateTime.Now;
        }

        public void Init()
        {
            var friendsRequest = new GetFriendsListRequest();
            PlayFabClientAPI.GetFriendsList(friendsRequest, GetFriendsList, Debug.Log);
        }

        private void GetFriendsList(GetFriendsListResult res)
        {
            var request = new GetLeaderboardRequest {StatisticName = "TimeSpent"};
            PlayFabClientAPI.GetLeaderboard(request, callback => GetLeaderboardResult(callback, res.Friends), Debug.Log);
        }

        private void GetLeaderboardResult(GetLeaderboardResult res, List<FriendInfo> friends)
        {
            var players = new List<(string, string, string, string)>();
            var actions = new List<UnityAction>();
            var meIndex = -1;
            var myPlayFabId = GameManager.Instance.playFabLoginController.playFabId;
            for (var i = 0; i < res.Leaderboard.Count; ++i)
            {
                var savedIndex = i;
                if (res.Leaderboard[i].PlayFabId == myPlayFabId) meIndex = i; 
                
                var isFriend = friends.Any(x => x.FriendPlayFabId == res.Leaderboard[i].PlayFabId);
                var score = TimeSpan.FromSeconds(res.Leaderboard[i].StatValue).ToString();
                players.Add(("" + (res.Leaderboard[i].Position + 1), res.Leaderboard[i].DisplayName, 
                    score, TextData.Get(isFriend ? 142 :141)));
                
                if(!isFriend)
                    actions.Add(() => AddToFriends(res.Leaderboard[savedIndex].PlayFabId));
                else
                    actions.Add(() => RemoveFromFriends(res.Leaderboard[savedIndex].PlayFabId));
            }
            
            _view.SetValues(players, actions, meIndex);
            SetActivePanel(true);
        }

        private void AddToFriends(string playFabId)
        {
            var request = new AddFriendRequest {FriendPlayFabId = playFabId};
            PlayFabClientAPI.AddFriend(request, res => Init(), Debug.Log);
        }
        
        private void RemoveFromFriends(string playFabId)
        {
            var request = new RemoveFriendRequest() {FriendPlayFabId = playFabId};
            PlayFabClientAPI.RemoveFriend(request, res => Init(), Debug.Log);
        }
        
        private bool OnQuit()
        {
            if(!_isUploaded)
                UpdateStatistics();
            return _isUploaded;
        }
        
        private void UpdateStatistics()
        {
            if(!GameManager.Instance.playFabLoginController.isLogged) return;
            var spentTime = (int) Math.Round(DateTime.Now.Subtract(_startTime).TotalSeconds);
            
            PlayFabClientAPI.UpdatePlayerStatistics( new UpdatePlayerStatisticsRequest {
                    // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
                    Statistics = new List<StatisticUpdate> {
                        new StatisticUpdate { StatisticName = "TimeSpent", Value = spentTime},
                    }
                },
                result => { Debug.Log("User statistics updated");
                    _isUploaded = true; Application.Quit();},
                error => { Debug.LogError(error.GenerateErrorReport()); });
        }

        private void SetActivePanel(bool val)
        {
            GameManager.Instance.profileController.ShowCentralBox(!val);
            _view.canvas.SetActive(val);
        }
    }
}
