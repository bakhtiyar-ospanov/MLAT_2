using UnityEngine;
using UnityEngine.UI;

namespace Modules.FPS
{
    public class FPSController : MonoBehaviour
    {
        public GameObject canvas;
        public Text mText;
        
        const float FPSMeasurePeriod = 0.5f;
        private int _mFpsAccumulator = 0;
        private float _mFpsNextPeriod = 0;
        private int _mCurrentFps;
        private string _appVersion;
        const string Display = "{0} FPS, {1}";
        private bool _isToShow;

        private void Awake()
        {
            _appVersion = Application.version;
            canvas.SetActive(_isToShow);
            enabled = _isToShow;
        }
        
        public void SetFPS(bool val)
        {
            PlayerPrefs.SetInt("SHOW_FPS", val ? 1 : 0);
            PlayerPrefs.Save();
            canvas.SetActive(val);
            _isToShow = val;
            enabled = val;
        }

        private void Update()
        {
            if(!_isToShow) return;
            _mFpsAccumulator++;
            if (!(Time.realtimeSinceStartup > _mFpsNextPeriod)) return;
            _mCurrentFps = (int)(_mFpsAccumulator / FPSMeasurePeriod);
            _mFpsAccumulator = 0;
            _mFpsNextPeriod += FPSMeasurePeriod;
            mText.text = string.Format(Display, _mCurrentFps, _appVersion);
        }
    }
}
