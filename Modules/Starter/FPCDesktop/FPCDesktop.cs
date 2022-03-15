using Modules.WDCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.Starter
{
    public class FPCDesktop : FPC
    {
        public class PlayerComponents
        {
            public FirstPersonAIO FirstPersonAio;
            public Camera Cam;
            public Transform Target;
        }

        private PlayerComponents _localPlayerComponents;
        private FPVDesktop _view;

        public void Awake()
        {
            _view = GetComponent<FPVDesktop>();
            _view.tabletBtn.onClick.AddListener(GameManager.Instance.mainMenuController.ShowMenu);
            _view.openWorldBtn.onClick.AddListener(GameManager.Instance.starterController.ActivateFreeMode);
        }
        public override void Init(GameObject playerStart)
        {
            _view.player.position = playerStart == null ? new Vector3(0.0f, 1.3f, 0.0f) : 
                new Vector3(playerStart.transform.position.x, playerStart.transform.position.y + 1.3f, playerStart.transform.position.z);
            _view.player.eulerAngles = playerStart == null ? new Vector3() : playerStart.transform.eulerAngles;
            _view.firstPersonAio.targetAngles = playerStart == null ? new Vector3() : playerStart.transform.eulerAngles;

            SetKinematic(false);
        }

        public override void LookAt(Transform target)
        {
            _view.player.LookAt(target);
            _view.firstPersonAio.targetAngles = _view.player.eulerAngles - new Vector3(25.0f, 0.0f, 0.0f);
        }
        
        public override Camera GetCamera()
        {
            return _view.cam;
        }

        public override Transform GetLookTarget()
        {
            return _view.lookTarget;
        }

        public override void SetKinematic(bool val)
        {
            _view.firstPersonAio.fps_Rigidbody.isKinematic = val;
        }

        public override void SetActivePanel(bool val)
        {
            _view.SetActivePanel(val);
        }

        public void SelectReticule(int index)
        {
            foreach (var reticule in _view.reticules)
                reticule.SetActive(false);
            _view.reticules[index].SetActive(true);
        }

        public void ReplaceMPlayer(PlayerComponents playerComponents)
        {
            Debug.Log("ReplaceMPlayer");
            
            _localPlayerComponents = new PlayerComponents
            {
                FirstPersonAio = _view.firstPersonAio, Cam = _view.cam, Target = _view.lookTarget
            };
            _view.player.gameObject.SetActive(false);
            
            _view.firstPersonAio = playerComponents.FirstPersonAio;
            _view.cam = playerComponents.Cam;
            _view.lookTarget = playerComponents.Target;
            _view.player = playerComponents.FirstPersonAio.gameObject.transform;
        }

        public void RestoreLocalPlayerComponent()
        {
            if(_localPlayerComponents == null) return;
            _view.firstPersonAio = _localPlayerComponents.FirstPersonAio;
            _view.cam = _localPlayerComponents.Cam;
            _view.lookTarget = _localPlayerComponents.Target;
            _view.player = _localPlayerComponents.FirstPersonAio.transform;
            _view.player.gameObject.SetActive(true);
        }
    }
}
