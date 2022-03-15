using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modules.Film
{
    public class SeekSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public CustomPlayer customPlayer;
        private bool dragging;
        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        public void OnPointerDown(PointerEventData e)
        {
            dragging = true;
            customPlayer.Pause();
        }

        public void OnPointerUp(PointerEventData e)
        {
            customPlayer.Seek(_slider.normalizedValue);
            dragging = false;
            customPlayer.Play();
        }

        void Update()
        {
            if (customPlayer != null && !dragging) {
                _slider.normalizedValue = customPlayer.GetProgress();
            }
        }
    }
}
