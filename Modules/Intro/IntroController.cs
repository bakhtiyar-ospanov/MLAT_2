using System.Collections;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Video;
using UnityEngine.XR;

namespace Modules.Intro
{
    [RequireComponent(typeof(IntroView))]
    public class IntroController : MonoBehaviour
    {
        public float imageShowTime = 2.0f;
        private IntroView _introView;
        private bool _timeout;

        private void Awake()
        {
            _introView = GetComponent<IntroView>();
        }

        public IEnumerator Init()
        {
            var clip = Resources.LoadAsync("Intro/Intro");
            yield return clip;

            if (clip.asset == null) yield break;

            if (clip.asset is VideoClip clipAsset)
            {
                var videoPlayer = gameObject.AddComponent<VideoPlayer>();
                Camera cam = null;
                videoPlayer.clip = clipAsset;
                if (XRSettings.enabled)
                {
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    videoPlayer.targetTexture = _introView.renderTexture;
                    _introView.SetTexture(_introView.renderTexture);
                    // GameManager.Instance.starterController.GetFPСVR().Init(_introView.playerStart);
                }
                else
                {
                    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                    cam = gameObject.AddComponent<Camera>();
                    videoPlayer.targetCamera = cam;
                }
                
                videoPlayer.Play();
                yield return new WaitUntil(() => videoPlayer.isPrepared);
                yield return new WaitUntil(() => !videoPlayer.isPlaying || Input.anyKeyDown);
                Resources.UnloadAsset(clip.asset);
                DestroyImmediate(videoPlayer);
                
                if (cam != null)
                {
                    DestroyImmediate(GetComponent<UniversalAdditionalCameraData>());
                    DestroyImmediate(cam);
                }
                _introView.SetTexture(null);
                    
            } else if (clip.asset is Texture2D text)
            {
                _introView.SetTexture(text);
                _timeout = false;
                StartCoroutine(Waiter());
                yield return new WaitUntil(() => Input.anyKeyDown || _timeout);
                _introView.SetTexture(null);
            }
        }

        private IEnumerator Waiter()
        {
            yield return new WaitForSeconds(imageShowTime);
            _timeout = true;
        }
    }
}
