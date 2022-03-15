using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Modules.Playfab
{
    public class PlayFabCurrencyController : MonoBehaviour
    {
        private PlayFabCurrencyView _view;

        private void Awake()
        {
            _view = GetComponent<PlayFabCurrencyView>();
        }

        public void Init()
        {
            GetInventory();
            _view.canvas.SetActive(true);
        }
        
        private void GetInventory() {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), OnGetInventorySuccess, Debug.Log);
        }

        private void OnGetInventorySuccess(GetUserInventoryResult result)
        {
            result.VirtualCurrency.TryGetValue("VM", out var vmAmount);
            _view.SetVmAmount(vmAmount);
            
            
        }
    }
}
