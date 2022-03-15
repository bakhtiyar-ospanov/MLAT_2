using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Starter
{
    public class FPVMobile : MonoBehaviour
    {
        public FixedJoystick joystick;
        public GameObject canvas;
        public Transform player;
        public Camera cam;
        public Transform lookTarget;
        //public Outliner outliner;
        public FirstPersonAIO firstPersonAio;
        public FixedTouchField fixedTouchField;
        public Button mainMenuButton;
        public Button assetMenuInitButton;
        public GameObject[] reticules;
        public Slider heightSlider;
        

        private void Awake()
        {
            heightSlider.value = cam.transform.localPosition.y;
            heightSlider.onValueChanged.AddListener(val =>
            {
                var camPos = cam.transform.localPosition;
                cam.transform.localPosition = new Vector3(camPos.x, val, camPos.z);
            });
            //outliner.enabled = false;
            SetActivePanel(true);
            mainMenuButton.onClick.AddListener(ShowMainMenu);
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
        }

        public void ShowMainMenu()
        {
            GameManager.Instance.mainMenuController.ShowMenu();
        }
    }
}