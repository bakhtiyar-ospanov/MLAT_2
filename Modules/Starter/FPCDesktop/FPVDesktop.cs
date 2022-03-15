using UnityEngine;
using UnityEngine.UI;

namespace Modules.Starter
{
    public class FPVDesktop : MonoBehaviour
    {
        public GameObject canvas;
        public Transform player;
        public Camera cam;
        public Transform lookTarget;
        public FirstPersonAIO firstPersonAio;
        public Button tabletBtn;
        public Button openWorldBtn;
        public GameObject[] reticules;
        
        private void Awake()
        {
            SetActivePanel(true);
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
        }
    }
}
