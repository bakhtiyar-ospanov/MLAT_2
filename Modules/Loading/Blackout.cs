using System.Collections;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace Modules.Loading
{
    public class Blackout : MonoBehaviour
    {
        public GameObject canvas;
        [SerializeField] private Animator animator;
        [SerializeField] private Image redGreenImage;
        private Coroutine redGreenRoutine;
        private bool isBlackInProgress;
#if UNITY_XR
        public OVRScreenFade ovrScreenFade;
#endif
        
        private void Awake()
        {
            canvas.SetActive(false);
#if UNITY_XR
            if (XRSettings.enabled)
            {
                ovrScreenFade = GameManager.Instance.starterController.GetFPСVR().ovrScreenFade;
                ovrScreenFade.SetExplicitFade(0);
                ovrScreenFade.SetUIFade(0);
            }
#endif
        }

        public IEnumerator Show(float delay = 0.0f)
        {
            if (redGreenRoutine != null)
                yield return new WaitUntil(() => redGreenRoutine == null);

            if(isBlackInProgress) yield break;
            
            isBlackInProgress = true;
            
            if (XRSettings.enabled)
            {
#if UNITY_XR
                ovrScreenFade.SetExplicitFade(0);
                ovrScreenFade.SetUIFade(0);
                ovrScreenFade.fadeColor = Color.black;
                yield return StartCoroutine(ovrScreenFade.Fade(0.0f, 1.0f));
#endif
                
            }
            else
            {
                redGreenImage.enabled = false;
                canvas.SetActive(true);
                animator.enabled = true;
                animator.Play("Fade In");
                yield return new WaitForSeconds(1.0f + delay);
                animator.enabled = false;
            }
        }

        public IEnumerator Hide(float delay = 0.0f)
        {
            if (XRSettings.enabled)
            {
#if UNITY_XR
                yield return new WaitForSeconds(delay);
                yield return StartCoroutine(ovrScreenFade.Fade(1.0f, 0.0f));
                ovrScreenFade.SetExplicitFade(0);
                ovrScreenFade.SetUIFade(0);
#endif
            }
            else
            {
                yield return new WaitForSeconds(delay);
                redGreenImage.enabled = false;
                canvas.SetActive(true);
                animator.enabled = true;
                animator.Play("Fade Out");
                yield return new WaitForSeconds(0.8f);
                canvas.SetActive(false);
                animator.enabled = false;
            }
            
            isBlackInProgress = false;
        }

        public void RedGreenBlackout(bool isGreen)
        {
            return;
            
            redGreenImage.enabled = false;
            canvas.SetActive(false);
            animator.enabled = false;
            
            if(redGreenRoutine != null)
                StopCoroutine(redGreenRoutine);
            
            redGreenRoutine = StartCoroutine(XRSettings.enabled ? 
                RegGreenBlackoutRoutineXR(isGreen) :
                RegGreenBlackoutRoutine(isGreen));
        }

        private IEnumerator RegGreenBlackoutRoutine(bool isGreen)
        {
            yield return new WaitUntil(() => !isBlackInProgress);
            
            canvas.SetActive(true);
            redGreenImage.enabled = true;
            redGreenImage.color = isGreen ? Color.green : Color.red;
            animator.enabled = true;
            animator.Play("Red-Green-Blackout");
            yield return new WaitForSeconds(1.1f);
            redGreenImage.enabled = false;
            animator.enabled = false;
            canvas.SetActive(false);
            redGreenRoutine = null;
        }
        
        private IEnumerator RegGreenBlackoutRoutineXR(bool isGreen)
        {
            yield return new WaitUntil(() => !isBlackInProgress);
#if UNITY_XR
            ovrScreenFade.fadeColor = isGreen ? Color.green : Color.red;
            yield return StartCoroutine(ovrScreenFade.Fade(0.0f, 0.05f));
            yield return StartCoroutine(ovrScreenFade.Fade(0.05f, 0.0f));
            ovrScreenFade.fadeColor = Color.black;
            ovrScreenFade.SetExplicitFade(0);
            ovrScreenFade.SetUIFade(0);
#endif
            redGreenRoutine = null;
        }
    }
}
