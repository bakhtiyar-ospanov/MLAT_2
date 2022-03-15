using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class VirtualKeyboardController : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ISelectHandler, IDeselectHandler
{
    private VirtualKeyboard _virtualKeyboard = new VirtualKeyboard();

    private bool _isActive;

    private enum EditState
    {
        Continue,
        Finish
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (!_isActive)
            return;

        var shouldContinue = KeyPressed();

        if (shouldContinue == EditState.Finish)
        {
            DeactivateOnScreenKeyboard();
        }
#endif
    }
    private void ActivateOnScreenKeyboard()
    {
        if (!Input.touchSupported)
            return;

        _isActive = true;
        _virtualKeyboard.ShowOnScreenKeyboard();
    }

    private void DeactivateOnScreenKeyboard()
    {
        _isActive = false;
        _virtualKeyboard.HideOnScreenKeyboard();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ActivateOnScreenKeyboard();
#endif
    }

    private EditState KeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            return EditState.Finish;
        }
        else
        {
            return EditState.Continue;
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        DeactivateOnScreenKeyboard();
#endif
    }

    public void OnSelect(BaseEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ActivateOnScreenKeyboard();
#endif
    }

    public void OnDeselect(BaseEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        DeactivateOnScreenKeyboard();
#endif
    }
}
