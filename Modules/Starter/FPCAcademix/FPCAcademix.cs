using Modules.WDCore;
using System;
using System.Linq;
using UnityEngine;

namespace Modules.Starter
{
    public class FPCAcademix : FPC
    {
        private FPVAcademix _view;
        private int _currentReticule;
        public Action<bool> onMovingChanged;

        private void Awake()
        {
            _view = GetComponent<FPVAcademix>();
            _view.tabletBtn.onClick.AddListener(GameManager.Instance.mainMenuController.ShowMenu);
            _view.helpBtn.onClick.AddListener(GameManager.Instance.helpController.Show);
            _view.openWorldBtn.onClick.AddListener(GameManager.Instance.starterController.ActivateFreeMode);
            _view.orbitCamera.onMovingChanged += val => onMovingChanged?.Invoke(val);
        }

        public override void Init(GameObject playerStart)
        {
            if (playerStart != null)
            {
                _view.orbitCamera.target.SetPositionAndRotation(playerStart.transform.position, playerStart.transform.rotation);
                _view.orbitCamera.transform.SetPositionAndRotation(playerStart.transform.position, playerStart.transform.rotation);
            }
            else
            {
                _view.orbitCamera.target.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                _view.orbitCamera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            
            _view.orbitCamera.transform.eulerAngles = new Vector3(_view.orbitCamera.transform.eulerAngles.x + 20.0f,
                _view.orbitCamera.transform.eulerAngles.y, _view.orbitCamera.transform.eulerAngles.z);
            _view.orbitCamera.UpdateYLimitReference();
            _view.orbitCamera.targetDistance = 2.0f;
            _view.orbitCamera.distance = 2.0f;
            _view.orbitCamera.Update();

            Cursor.ActivateCursor(true);
        }

        public override Camera GetCamera()
        {
            return _view.cam;
        }

        public override Transform GetLookTarget()
        {
            return _view.lookTarget;
        }

        public override void LookAt(Transform target)
        {
            _view.orbitCamera.ResetCam();

            var targetPosition = target.position;
            var targetDistance = 2.0f;

            if (target.name.ToLower().Contains("stand"))
                targetPosition = new Vector3(targetPosition.x, targetPosition.y + 1.0f, targetPosition.z);

            _view.orbitCamera.target.position = targetPosition;
            _view.orbitCamera.transform.position = targetPosition;

            if (target.name.ToLower().Contains("lay"))
            {
                var targetRotation = new Vector3(50.0f, -90.0f, 0f);
                _view.orbitCamera.target.eulerAngles = targetRotation;
                _view.orbitCamera.transform.eulerAngles = targetRotation;
                targetDistance = 1.3f;
            }
            else
            {
                var targetRotation = new Vector3(20.0f, 180.0f, 0);
                _view.orbitCamera.target.eulerAngles = targetRotation;
                _view.orbitCamera.transform.eulerAngles = targetRotation;
            }
            _view.orbitCamera.UpdateYLimitReference();
            _view.orbitCamera.targetDistance = targetDistance;
            _view.orbitCamera.distance = targetDistance;
            _view.orbitCamera.Update();
        }

        public override void SetKinematic(bool val)
        {
            
        }
        
        public override void SetActivePanel(bool val)
        {
            _view.canvas.SetActive(val);
        }

        public void EnableOrbitCamera(bool val)
        {
            _view.orbitCamera.enabled = val;
        }
        
        public void SelectReticule(int index)
        {
            if (_currentReticule != index)
            {
                UnityEngine.Cursor.SetCursor(_view._cursorTextures[index], Vector2.zero, CursorMode.ForceSoftware);
                _currentReticule = index;
            }
        }
    }
}
