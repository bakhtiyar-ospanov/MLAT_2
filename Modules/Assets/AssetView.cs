using System;
using TMPro;
using UnityEngine;

namespace Modules.Assets
{
    public class AssetView : MonoBehaviour
    {
        public GameObject canvas;
        [SerializeField] private GameObject assetNameRoot;
        [SerializeField] private TextMeshProUGUI assetName;
        [SerializeField] private GameObject hintRoot;
        [SerializeField] private TextMeshProUGUI actionHint;

        private void Awake()
        {
            SetActivePanel(false);
            hintRoot.SetActive(false);
        }

        public void ShowAssetName(string title, string hint = "")
        {
            SetActivePanel(!string.IsNullOrEmpty(title));
            assetName.text = title;
            ShowHint(hint);
        }

        private void ShowHint(string val)
        {
            hintRoot.SetActive(!string.IsNullOrEmpty(val));
            actionHint.text = val;
        }

        public void SetActivePanel(bool val)
        {
            canvas.SetActive(val);
            assetNameRoot.SetActive(val);
        }
    }
}
