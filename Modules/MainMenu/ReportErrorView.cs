using System;
using UnityEngine;
using UnityEngine.UI;
using Modules.WDCore;
using TMPro;

public class ReportErrorView : MonoBehaviour
{
    public GameObject root;

    public Button reportError;
    public Button sendReport;
    public Button screenshot;
    public Button closeButton;

    public TMP_InputField userName;
    public TMP_InputField userEmail;
    public TMP_InputField description;

    public GameObject screenshotRoot;
    public RawImage screenshotPreview;

    private void Awake()
    {
        root.SetActive(false);
        reportError.gameObject.SetActive(false);
    }

    public void SetActivePanel(bool val)
    {
        root.SetActive(val);
    }
}
