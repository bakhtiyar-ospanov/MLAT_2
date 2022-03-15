using System.Linq;
using Modules.Books;
using UnityEngine;

namespace Modules.Light
{
    public class LightController : MonoBehaviour
    {
        private LightView _lightView;

        public void Awake()
        {
            _lightView = GetComponent<LightView>();
        }
        
        public void LightSetup(string sceneId, Transform camParent)
        {
            BookDatabase.Instance.VargatesBook.locationById.TryGetValue(sceneId, out var location);

            if (string.IsNullOrEmpty(location?.camlight)) return;

            var lightPrefab = _lightView.lightPrefabs.FirstOrDefault(x => x.name == location.camlight);
            if(lightPrefab == null) return;

            Instantiate(lightPrefab, camParent);
        }
    }
}
