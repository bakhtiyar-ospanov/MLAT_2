using System;
using TMPro;
using UnityEngine;

namespace Modules.Playfab
{
    public class PlayFabCurrencyView : MonoBehaviour
    {
        public GameObject canvas;
        public TextMeshProUGUI currencyTxt;
        
        private void Awake()
        {
            canvas.SetActive(false);
        }

        public void SetVmAmount(int val)
        {
            currencyTxt.text = val.ToString();
        }
    }
}
