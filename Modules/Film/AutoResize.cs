using UnityEngine;
using UnityEngine.UI;

namespace Modules.Film
{
    [RequireComponent(typeof(CustomPlayer))]
    public class AutoResize : MonoBehaviour
    {

        private CustomPlayer CustomPlayer;
        private bool isLoaded = false;
        private float targetHeight;

        public Vector3 heightAxis = new Vector3(0f, 1f, 0f);
        private float aspectRatio;

        void Start()
        {
            CustomPlayer = GetComponent<CustomPlayer>();
            CustomPlayer.OnVideoStart += OnVideoStart;
        }

        void OnDisable()
        {
            CustomPlayer.OnVideoStart -= OnVideoStart;
        }

        private void OnVideoStart()
        {
            if (CustomPlayer.GetWidth() > CustomPlayer.GetHeight()) {
                aspectRatio = (float)CustomPlayer.GetHeight() / CustomPlayer.GetWidth();
            } else {
                aspectRatio = (float)CustomPlayer.GetWidth() / CustomPlayer.GetHeight();
            }

            targetHeight = aspectRatio * CustomPlayer.videoScreen.transform.localScale.x;

            isLoaded = true;
        }

        void Update()
        {
            if (targetHeight > 0 && isLoaded) {
                if (CustomPlayer.videoScreen.GetComponent<RawImage>() == null) {
                    Vector3 scale = CustomPlayer.videoScreen.transform.localScale;

                    CustomPlayer.videoScreen.transform.localScale = new Vector3(
                        heightAxis.x == 1 ? targetHeight : scale.x,
                        heightAxis.y == 1 ? targetHeight : scale.y,
                        heightAxis.z == 1 ? targetHeight : scale.z
                    );
                } else {
                    RawImage img = CustomPlayer.videoScreen.GetComponent<RawImage>();
                    float height = Screen.width * aspectRatio;
                    float offset = (Screen.height - height) / 2;

                    if (height > Screen.height) {
                        float width = Screen.height / aspectRatio;
                        offset = (Screen.width - width) / 2;

                        img.rectTransform.offsetMin = new Vector2(offset, 0);
                        img.rectTransform.offsetMax = new Vector2(-offset, 0);
                    } else {
                        img.rectTransform.offsetMin = new Vector2(0, offset);
                        img.rectTransform.offsetMax = new Vector2(0, -offset);
                    }
                }
            }
        }
    }
}