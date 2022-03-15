using UnityEngine;

namespace Modules.Starter
{
    public class FPCMobile : FPC
    {
        private FPVMobile _view;

        public void Awake()
        {
            _view = GetComponent<FPVMobile>();
        }

        public override void Init(GameObject playerStart)
        {
            _view.player.position = playerStart == null ? new Vector3(0.0f, 1.3f, 0.0f) : 
                new Vector3(playerStart.transform.position.x, playerStart.transform.position.y + 1.3f, playerStart.transform.position.z);
            _view.player.eulerAngles = playerStart == null ? new Vector3() : playerStart.transform.eulerAngles;
            _view.firstPersonAio.targetAngles = playerStart == null ? new Vector3() : playerStart.transform.eulerAngles;

            SetKinematic(false);
            SetActivePanel(true);
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
        
        public bool IsSwiping()
        {
            return _view.fixedTouchField.isSwiping;
        }

        public bool IsSwipingTouch()
        {
            return _view.fixedTouchField.isSwipingTouch;
        }
       
        public FixedJoystick GetJoystick()
        {
            if(_view == null)
                _view = GetComponent<FPVMobile>();
            
            return _view.joystick;
        }
    }
}