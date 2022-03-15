using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.MainMenu
{
    public class ProfileView : MonoBehaviour
    {
        public GameObject canvas;
        public GameObject root;
        public GameObject centralBox;
        public TextMeshProUGUI displayName;
        public Button changeNameButton;
        public Button leaderboardButton;
        public Button openStatisticsButton;
        public Button activationButton;
        public Button loginButton;
        public Button registerButton;
        public Button logoutButton;
        public Button websiteButton;
        public Button awardButton;

        private void Awake()
        {
            canvas.SetActive(false);
        }
        
    }
}
