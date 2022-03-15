using UnityEngine;

namespace Modules.Film
{
    public class CustomPlayer : MonoBehaviour
    {
        public delegate void VimeoEvent();
        public event VimeoEvent OnStart;
        public event VimeoEvent OnVideoMetadataLoad;
        public event VimeoEvent OnVideoStart;
        public event VimeoEvent OnPause;
        public event VimeoEvent OnPlay;
        public event VimeoEvent OnFrameReady;
        public event VimeoEvent OnLoadError;

        public GameObject videoScreen;
        public AudioSource audioSource;
        public VideoController controller;
        
        public bool muteAudio = false;

        public void Start()
        {
            Application.runInBackground = true;

            SetupVideoController();

            if (OnStart != null) {
                OnStart();
            }
        }

        private void SetupVideoController()
        {
            // TODO abstract this out into a VideoPlayerManager (like EncoderManager.cs)
            if (controller == null) {
                controller = gameObject.AddComponent<VideoController>();
                controller.playerSettings = this;
                controller.videoScreenObject = videoScreen;

                controller.OnVideoStart += VideoStarted;
                controller.OnPlay += VideoPlay;
                controller.OnPause += VideoPaused;
                controller.OnFrameReady += VideoFrameReady;

                if (audioSource && audioSource is AudioSource) {
                    if (audioSource != null) {
                        controller.audioSource = audioSource;
                    } else {
                        videoScreen.gameObject.AddComponent<AudioSource>();
                    }
                }
            } else {
                controller.videoScreenObject = videoScreen;
                controller.Setup();
            }
            
        }
        
        public bool IsPlaying()
        {
            if (controller.videoPlayer == null) return false;

            return IsPlayerSetup() && controller.videoPlayer.isPlaying;
        }

        public bool IsVideoMetadataLoaded()
        {
            if (controller.videoPlayer == null) return false;
            
            if(!controller.videoPlayer.isPrepared)
                controller.videoPlayer.Play();
            return controller.videoPlayer.isPrepared;
        }

        public bool IsPlayerSetup()
        {
            return controller != null && controller.videoPlayer != null;
        }

        public void Load(string url)
        {
            controller.PlayVideoByUrl(url);
        }

        public void Play()
        {
            controller.Play();
        }
        
        public void Pause()
        {
            controller.Pause();
        }

        public void Seek(float seek)
        {
            controller.Seek(seek);
        }

        public void SeekBySeconds(int seconds)
        {
            controller.SeekBySeconds(seconds);
        }

        public void SeekBackward(float seek)
        {
            controller.SeekBackward(seek);
        }

        public void SeekForward(float seek)
        {
            controller.SeekForward(seek);
        }

        public void ToggleVideoPlayback()
        {
            controller.TogglePlayback();
        }

        public int GetWidth()
        {
            return controller.width;
        }

        public int GetHeight()
        {
            return controller.height;
        }

        public float GetProgress()
        {
            if (controller != null && controller.videoPlayer != null) {
                return (float)controller.GetCurrentFrame() / (float)controller.GetTotalFrames();
            }
            return 0;
        }

        public string GetTimecode()
        {
            if (controller != null) {
                float sec = Mathf.Floor((float)controller.videoPlayer.time % 60);
                float min = Mathf.Floor((float)controller.videoPlayer.time / 60f);

                string secZeroPad = sec > 9 ? "" : "0";
                string minZeroPad = min > 9 ? "" : "0";

                return minZeroPad + min + ":" + secZeroPad + sec;
            }

            return null;
        }

        // Events below
        private void VideoStarted(VideoController controller)
        {
            if (OnVideoStart != null) {
                OnVideoStart();
            }
        }

        private void VideoPlay(VideoController controller)
        {
            if (OnPlay != null) {
                OnPlay();
            }
        }

        private void VideoPaused(VideoController controller)
        {
            if (OnPause != null) {
                OnPause();
            }
        }

        private void VideoFrameReady(VideoController controller)
        {
            if (OnFrameReady != null) {
                OnFrameReady();
            }
        }

    }
}
