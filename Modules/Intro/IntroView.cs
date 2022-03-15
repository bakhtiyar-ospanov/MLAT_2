using UnityEngine;
using UnityEngine.UI;

namespace Modules.Intro
{
    public class IntroView : MonoBehaviour
    {
        [SerializeField] private GameObject canvas;
        [SerializeField] private RawImage image;
        public RenderTexture renderTexture;
        public GameObject playerStart;

        private void Awake()
        {
            canvas.SetActive(false);
        }

        public void SetTexture(Texture text)
        {
            image.texture = text;
            canvas.SetActive(text != null);
        }
    }
}
