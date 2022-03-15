using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Awards
{
    public class AwardView : MonoBehaviour
    {
        public GameObject root;
        public Button backButton;
        public TextMeshProUGUI[] scoresTxts;

        private void Awake()
        {
            root.SetActive(false);

            for (var i = 0; i < scoresTxts.Length; i++)
                SetScore(i, 0);
        }

        public void SetScore(int i, int score)
        {
            scoresTxts[i].text = score + "%";
        }
    }
}
