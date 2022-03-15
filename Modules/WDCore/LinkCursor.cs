using UnityEngine;
using UnityEngine.EventSystems;

namespace Modules.WDCore
{
    public class LinkCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Instance.starterController.SelectReticule(2);
            GameManager.Instance.starterController.isBlockReticule = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameManager.Instance.starterController.isBlockReticule = false;
            GameManager.Instance.starterController.SelectReticule(0);
        }
    }
}
