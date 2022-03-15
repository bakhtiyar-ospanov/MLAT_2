using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Modules.PostProcessing
{
    public class PostProcessController : MonoBehaviour
    {
        private Volume _postProcessVolume;

        public void Init()
        {
            _postProcessVolume = FindObjectOfType<Volume>();

            if(_postProcessVolume != null)
                _postProcessVolume.enabled = PlayerPrefs.GetInt("POST_FX") == 1;
        }
        
        public void EnablePostFX(bool val)
        {
            PlayerPrefs.SetInt("POST_FX", val ? 1 : 0);
            PlayerPrefs.Save();
            
            if(_postProcessVolume != null)
                _postProcessVolume.enabled = val;
        }
    }
}
