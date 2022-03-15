// using Modules.Core;
// using UnityEngine;
//
// namespace Modules.Assets
// {
//     public class ManipulationAsset : MonoBehaviour
//     {
//         private void Awake()
//         {
//             StartCoroutine(GameManager.Instance.manipulationSelectorController.Init(GameManager.Product.Dimedus));
//         }
//
//         private void OnDestroy()
//         {
//             if(GameManager.Instance != null && GameManager.Instance.mainMenuController != null)
//                 GameManager.Instance.mainMenuController.RemoveModule("ManipulationSelector");
//             if(GameManager.Instance != null)
//                 GameManager.Instance.onProductChange?.Invoke(GameManager.Instance.defaultProduct);
//         }
//     }
// }
