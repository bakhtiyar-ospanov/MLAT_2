using System.Collections.Generic;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Modules.Playfab
{
    public class PlayFabLeaderboardView : MonoBehaviour
    {
        public GameObject canvas;
        public Button closeButton;
        public List<LeaderboardPlayer> players;

        private void Awake()
        {
            canvas.SetActive(false);
        }
        
        public void SetValues(List<(string, string, string, string)> texts, List<UnityAction> calls, int meIndex)
        {
            CheckButtons(texts.Count);
        
            for (var i = 0; i < texts.Count; ++i)
            {
                players[i].position.text = texts[i].Item1;
                players[i].displayName.text = texts[i].Item2;
                players[i].score.text = texts[i].Item3;
                players[i].addFriend.tmpText.text = texts[i].Item4;
                players[i].addFriend.button.onClick.RemoveAllListeners();
                players[i].addFriend.button.onClick.AddListener(calls[i]);
                players[i].addFriend.gameObject.SetActive(i != meIndex);
                players[i].gameObject.SetActive(true);
            }
        }

        private void CheckButtons(int requiredSize)
        {
            var currentSize = players.Count;
            if (requiredSize > currentSize)
            {
                var parent = players[0].transform.parent;
                var obj = players[0].gameObject;
            
                for (var i = 0; i < requiredSize - currentSize; i++)
                {
                    players.Add(Instantiate(obj, parent).GetComponent<LeaderboardPlayer>());
                }
            }
        
            foreach (var player in players)
            {
                player.gameObject.SetActive(false);
            }
        }
    }
}
