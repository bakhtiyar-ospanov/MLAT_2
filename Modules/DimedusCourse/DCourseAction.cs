// using Modules.WDCore;
// using PolyAndCode.UI;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace Modules.DimedusCourse
// {
//     public class DCourseAction : MonoBehaviour, ICell
//     {
//         public TextMeshProUGUI text;
//         public Button button;
//         public GameObject highlight;
//         private int _actionIndex;
//
//         private void Awake()
//         {
//             button.onClick.AddListener(ButtonListener);
//             GameManager.Instance.dCourseController.onActionChanged += Highlight;
//         }
//
//         public void ConfigureCell(int actionIndex, string val)
//         {
//             _actionIndex = actionIndex;
//             text.text = val;
//             highlight.SetActive(false);
//         }
//
//         private void ButtonListener()
//         {
//             GameManager.Instance.dCourseController.LaunchActionByIndex(_actionIndex);
//         }
//
//         private void Highlight(int index)
//         {
//             if(highlight == null) return;
//             highlight.SetActive(_actionIndex == index);
//         }
//     }
// }
