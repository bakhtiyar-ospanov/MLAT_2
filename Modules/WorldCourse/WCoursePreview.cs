using System.Collections;
using Modules.Books;
using Modules.WDCore;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.WorldCourse
{
    public class WCoursePreview : MonoBehaviour, ICell
    {
        public RawImage preview;
        public TextMeshProUGUI courseName;
        public TextMeshProUGUI courseDescription;
        public Button button;

        private WCourse.Course _course;

        private void Awake()
        {
            button.onClick.AddListener(ButtonListener);
        }

        public void ConfigureCell(WCourse.Course course)
        {
            _course = course;
            courseName.text = course.name;
            courseDescription.text = course.description;
            preview.texture = null;
            StopAllCoroutines();
            StartCoroutine(SetPreviewRoutine(course));
        }

        private IEnumerator SetPreviewRoutine(WCourse.Course course)
        {
            yield return new WaitUntil(() => course.preview != null);
            preview.texture = course.preview;
        }

        private void ButtonListener()
        {
            GameManager.Instance.wCourseSelectorController.OpenPopup(_course);
        }
    }
}
