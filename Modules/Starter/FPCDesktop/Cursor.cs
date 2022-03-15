using System.Collections;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR;

namespace Modules.Starter
{
    public class Cursor : Singleton<Cursor>
    {
        public static bool IsDesktop;
        public static bool IsBlocked;
        public static bool IsVisible;
        private void Awake()
        {
            if (!Input.touchSupported)
            {
                IsDesktop = true;
            }
            else
            {
                IsDesktop = false;
            }
        }

        public static void ActivateCursor(bool val)
        {
            if(!IsDesktop || IsBlocked) return;
            UnityEngine.Cursor.lockState = val ? CursorLockMode.None : CursorLockMode.Locked;
            UnityEngine.Cursor.visible = val;
            if(ExtensionCoroutine.Instance != null)
            	ExtensionCoroutine.Instance.StartCoroutine(CursorLateStateUpdate(val));
        }

        private void Update()
        {
            if(!IsDesktop || IsBlocked) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                ActivateCursor(true);
            
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                ActivateCursor(false);
        }

        private static IEnumerator CursorLateStateUpdate(bool val)
        {
            yield return new WaitForEndOfFrame();
            IsVisible = val;
        }
    }
    
}