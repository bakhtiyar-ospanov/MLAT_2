using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Notification
{
    public class NotificationController : MonoBehaviour
    {
        private NotificationView _view;
        private const float DisplayTime = 5.0f;
        private List<IEnumerator> _callQueue = new();
        private bool _isClosedManually;
        private void Awake()
        {
            _view = GetComponent<NotificationView>();
            _view.closeButton.onClick.AddListener(() =>
            {
                _isClosedManually = true;
            });
        }

        public void Init(string header, string body, bool isAutoHide)
        {
            var call = CallRoutine(header, body, isAutoHide);
            _callQueue.Add(call);

            if (_callQueue.Count == 1)
                StartCoroutine(call);
        }
        

        private IEnumerator CallRoutine(string header, string body, bool isAutoHide)
        {
            _view.SetInfo(header, body);
            _view.SetActivePanel(true);

            if (isAutoHide)
            {
                float normalizedTime = 0;
                while(normalizedTime <= 1f && !_isClosedManually)
                {
                    normalizedTime += Time.deltaTime / DisplayTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitUntil(() => _isClosedManually);
            }
            
            _view.SetActivePanel(false);
            _isClosedManually = false;
            
            yield return new WaitForEndOfFrame();
            _callQueue.RemoveAt(0);

            if (_callQueue.Count > 0)
                StartCoroutine(_callQueue[0]);
        }
    }
}
