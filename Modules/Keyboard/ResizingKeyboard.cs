// using UnityEngine;
//
// namespace Modules.Keyboard
// {
//     public class ResizingKeyboard : MonoBehaviour
//     {
//         [SerializeField] RectTransform rootMove;
//         [SerializeField] Transform controllerLeft;
//         [SerializeField] Transform controllerRight;
//
//         float originDistance;
//         bool startDragging = false;
//         // Start is called before the first frame update
//         void Awake()
//         {
//         
//         }
//
//         // Update is called once per frame
//         void Update()
//         {
//             if (startDragging)
//             {
//                 float scale = Vector3.Distance(controllerLeft.position, controllerRight.position)/originDistance;
//                 rootMove.localScale = Vector3.one*scale;
//             }
// #if UNITY_XR
//             if (OVRInput.GetUp(OVRInput.Button.Two))
//             {
//                 startDragging = false;
//             }
// #endif
//         }
//
//         public void ClickToDrag()
//         {
//             if (!startDragging)
//             {
//                 startDragging = true;
//                 originDistance = Vector3.Distance(controllerLeft.position, controllerRight.position);
//             }else
//             {
//                 startDragging = false;
//             }
//         }
//     }
// }
