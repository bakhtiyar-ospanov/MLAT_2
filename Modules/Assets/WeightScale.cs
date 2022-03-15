using System.Collections.Generic;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets
{
    public class WeightScale : MonoBehaviour
    {
        private Asset _asset;
        private (Vector3, Vector3, Transform) _initialLocation;

        private void Awake()
        {
            _asset = GetComponent<Asset>();

            var oneAction = new MedicalBase.MenuItem
            {
                name = TextData.Get(90),
                call = CustomAction
            };
            _asset.assetMenu = new List<MedicalBase.MenuItem>{oneAction};
        }

        private void CustomAction()
        {
            Debug.Log("CustomAction");
            var patientAsset = GameManager.Instance.assetController.patientAsset;
            if(patientAsset == null || !GameManager.Instance.scenarioController.IsLaunched) return;
            StartCoroutine(patientAsset.StandOnWeightScale());
        }
    }
}
