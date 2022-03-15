using System.Collections.Generic;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets
{
    public class Flashlight : MonoBehaviour
    {
        private Asset _asset;

        private void Awake()
        {
            _asset = GetComponent<Asset>();

            var oneAction = new MedicalBase.MenuItem
            {
                name = TextData.Get(181),
                call = CustomAction
            };
            _asset.assetMenu = new List<MedicalBase.MenuItem>{oneAction};
        }

        private void CustomAction()
        {
            Debug.Log("CustomAction");
            if(GameManager.Instance.assetController.assetInHands != null)
                GameManager.Instance.assetController.assetInHands.ReturnItem();
            
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            if(patientAsset == null || !GameManager.Instance.scenarioController.IsLaunched) return;
            StartCoroutine(patientAsset.VisualExam());
        }
    }
}
