// using Modules.Books;
// using Modules.Core;
// using PolyAndCode.UI;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Modules.Manipulation
// {
//     public class DManipulationPreview : MonoBehaviour, ICell
//     {
//         public RawImage preview;
//         public TextMeshProUGUI scenarioName;
//         public TextMeshProUGUI scenarioDescription;
//         public Button button;
//
//         private CoursesDimedus.Manipulation _manipulation;
//
//         private void Awake()
//         {
//             button.onClick.AddListener(ButtonListener);
//         }
//
//         public void ConfigureCell(CoursesDimedus.Manipulation manipulation)
//         {
//             _manipulation = manipulation;
//             scenarioName.text = manipulation.name;
//             scenarioDescription.text = manipulation.description;
//             //preview.texture = null;
//             //StopAllCoroutines();
//             //StartCoroutine(SetPreviewRoutine(course));
//         }
//
//         // private IEnumerator SetPreviewRoutine(CoursesDimedus.Course course)
//         // {
//         //     yield return new WaitUntil(() => course.preview != null);
//         //     preview.texture = course.preview;
//         // }
//
//         private void ButtonListener()
//         {
//             GameManager.Instance.manipulationSelectorController.OpenPopup(_manipulation);
//         }
//     }
// }
