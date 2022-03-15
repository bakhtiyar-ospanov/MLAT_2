using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.Scenario
{
    public class AreaTarget : MonoBehaviour
    {
        private float _fixedSize = 0.03f;
        private bool _isOn;
        private Camera _cam;

        public void SetSize(float val)
        {
            _fixedSize = val;
        }

        public void TurnOn(Camera cam)
        {
            _cam = cam;
            CheckPointSize();
            _isOn = true;
        }

        private void TurnOff()
        {
            _isOn = false;
        }

        private void Update()
        {
            if(!_isOn) return;

            CheckPointSize();
        }

        private void CheckPointSize()
        {
            if(_cam == null) return;
            
            var distance = (_cam.transform.position - transform.position).magnitude;
            var size = Mathf.Clamp(distance * _fixedSize, 0.01f, 0.03f);
            transform.localScale = Vector3.one * size;

            transform.LookAt(_cam.transform);
        }
    }
}
