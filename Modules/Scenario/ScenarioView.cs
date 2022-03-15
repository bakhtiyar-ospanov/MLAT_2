using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Scenario
{
    public class ScenarioView : MonoBehaviour
    {
        public GameObject canvas;
        public Button closeButton;
        public Button prevButton;
        public Button nextButton;

        [Header("Timer")] 
        public GameObject timer;
        public TextMeshProUGUI timerTxt;

        private void Awake()
        {
            canvas.SetActive(false);
            GameManager.Instance.settingsController.onBetaModeChange += val =>
            {
                prevButton.gameObject.SetActive(val);
                nextButton.gameObject.SetActive(val);
            };
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
        }
    }
}
