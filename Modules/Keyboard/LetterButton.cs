using System.Collections;
using System.Collections.Generic;
using Modules.Keyboard;
using UnityEngine;

public class LetterButton : MonoBehaviour
{
    public string key;
    public string shiftedKey;

    [SerializeField] KeyboardController keyboard;

    public void KeyPressed()
    {
        if (keyboard.shifted)
        {
            keyboard.AddString(shiftedKey);
        }
        else
        {
            keyboard.AddString(key);
        }
    }
}
