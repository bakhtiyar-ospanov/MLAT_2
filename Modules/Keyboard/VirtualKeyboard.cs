using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class VirtualKeyboard
{
    private static Process _onScreenKeyboardProcess = null;

    public void ShowOnScreenKeyboard()
    {
        try
        {
            if (_onScreenKeyboardProcess == null || _onScreenKeyboardProcess.HasExited)
                _onScreenKeyboardProcess = ExternalCall("TabTip", null, false);
        }
        catch (Exception e)
        {
            Debug.Log("Virtual keyboard (TabTip.exe) exception: " + e);
        }
    }

    public void HideOnScreenKeyboard()
    {
        if (_onScreenKeyboardProcess != null && !_onScreenKeyboardProcess.HasExited)
            _onScreenKeyboardProcess.Kill();
    }

    private static Process ExternalCall(string filename, string arguments, bool hideWindow)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = filename;
        startInfo.Arguments = arguments;

        if (hideWindow)
        {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
        }

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        return process;
    }
}
