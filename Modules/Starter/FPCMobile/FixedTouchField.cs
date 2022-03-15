using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class FixedTouchField : MonoBehaviour
{
    [HideInInspector]
    public Vector2 touchDist;
    private Vector2 oldPosition;
    [HideInInspector]

    public bool isSwiping = false;
    public bool isSwipingTouch = false;

    private bool _gestureStartedOverUI;

    public Touch swipingTouch;  

    void LateUpdate()
    {
        UpdateTouchInput();
    }
    private bool IsPointingOverUI(List<RaycastResult> raycastResults)
    {
        return raycastResults.Any(result => result.gameObject.layer == 5);
    }

    private static List<RaycastResult> GetRaycastResults(Vector3 pointerPosition)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = pointerPosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);
        return raycastResults;
    }
    public void UpdateTouchInput()
    {
        if (!Input.touchSupported) return;

        if (Input.touchCount == 0) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            var results = GetRaycastResults(Input.touches[i].position);
            _gestureStartedOverUI = IsPointingOverUI(results);

            if (_gestureStartedOverUI || EventSystem.current.currentSelectedGameObject == null) continue;

            swipingTouch = Input.touches[i];
        }
        ProcessSwipeGesture(swipingTouch);
    }

    public void ProcessSwipeGesture(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                isSwipingTouch = false;
                oldPosition = touch.position;
                break;
            case TouchPhase.Moved:
                isSwiping = true;
                touchDist = (touch.position - oldPosition) * 0.07f;
                oldPosition = touch.position;
                break;
            case TouchPhase.Stationary:
                oldPosition = touch.position;
                break;
            case TouchPhase.Ended:
                if (isSwiping == true)
                    isSwipingTouch = true;
                isSwiping = false;
                oldPosition = Vector2.zero;
                touchDist = Vector2.zero;
                break;
            case TouchPhase.Canceled:
                if (isSwiping == true)
                    isSwipingTouch = true;
                isSwiping = false;
                oldPosition = Vector2.zero;
                touchDist = Vector2.zero;
                break;
        }
    }
}