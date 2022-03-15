using IngameDebugConsole;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Console
{
    public class CommandCenter : MonoBehaviour
    {
        [ConsoleMethod( "openlink", "Opens website link [url]" )]
        public static void OpenLink(string url)
        {
            if (!url.StartsWith("http"))
                url = "https://" + url;
            
            Application.OpenURL(url);
        }
        
        [ConsoleMethod( "vardoor", "Loads a scene [locationId] from default world" )]
        public static void LoadScene(string locationId)
        {
            GameManager.Instance.starterController.InitNoWait(locationId);
        }
        
        [ConsoleMethod( "vardoor", "Loads a scene [locationId] from external world [world]" )]
        public static void LoadSceneExternal(string locationId, string world)
        {
            GameManager.Instance.starterController.InitNoWait(locationId, world);
        }
    }
}
