// using PlayFab;
// using UnityEditor;
// using UnityEngine;
//
// namespace Modules.Playfab.Editor
// {
//     [InitializeOnLoad]
//     public class PlayfabProductSetter
//     {
//         static PlayfabProductSetter()
//         {
//             ProductDetection();
//             EditorApplication.update += Update;
//         }
//         
//         static void Update ()
//         {
//             ProductDetection();
//         }
//
//         private static void ProductDetection()
//         {
//             var productName = Application.productName;
//
//             if (productName.ToLower().Contains("dimedus"))
//                 PlayFabSettings.TitleId = "DD96A";
//             else if (productName.ToLower().Contains("academix"))
//                 PlayFabSettings.TitleId = "DF7FB";
//         }
//     }
// }
