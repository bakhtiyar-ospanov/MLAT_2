using UnityEngine;
using UnityEngine.UI;

namespace Modules.WDCore
{
    public class ScrollViewNormalizer : MonoBehaviour
    {
        public bool isReverse;
        private ScrollRect _scrollView;

        private void Awake()
        {
            _scrollView = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            _scrollView.verticalNormalizedPosition = isReverse ? 0.0f : 1.0f;
        }
    }
}
