using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Assets.Assistant;
using Modules.Assets.Patient;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.XR;
using VardixOpenSDK;
using VargatesOpenSDK;

namespace Modules.Assets
{
    public class AssetController : MonoBehaviour
    {
        [Header("Special Assets")]
        public AssistantAsset assistantAsset;
        public PatientAsset patientAsset;
        public Carriable assetInHands;
        
        public Dictionary<GameObject, Asset> assetById = new Dictionary<GameObject, Asset>();
        private AssetView _assetView;
        private Transform _vrCamera;

        private void Awake()
        {
            _assetView = GetComponent<AssetView>();
            if (XRSettings.enabled) 
                _vrCamera = GameManager.Instance.starterController.GetFPСVR().GetCamera().transform;
        }

        public IEnumerator SearchAssets()
        {
            yield return new WaitUntil(() => BookDatabase.Instance.isDone);
            Debug.Log("Asset search in scene");
            assetById.Clear();
            
            var assets = GameObject.FindGameObjectsWithTag("Asset");
            foreach (var asset in assets)
                assetById.Add(asset, asset.AddComponent<Asset>());
            
            AddSpecialAssets();
        }
        
        private void AddSpecialAssets()
        {
            var allObjects = FindObjectsOfType<GameObject>();
            
            // Assistant Asset
            var assistant = allObjects.FirstOrDefault(x => x.name == "VA309");
            if (assistant != null)
            {
                assistantAsset = assistant.AddComponent<AssistantAsset>();
                assetById.Add(assistant, assistantAsset);
                Debug.Log("Asset search in scene: Assistant VA309 added");
            }
            
            // Dimedus/Academix Patient Asset
            var patient = allObjects.FirstOrDefault(x => x.name == "VA304");
            patient = patient == null ? allObjects.FirstOrDefault(x => x.name == "VA8") : patient;
            if (patient != null)
            {
                patientAsset = patient.AddComponent<PatientAsset>();
                assetById.Add(patient, patientAsset);
                Debug.Log("Asset search in scene: Patient VA8 added");
            }
            
            // Doors
            var doors = allObjects.Where(x => x.name.StartsWith("vardoor")).ToList();
            foreach (var door in doors)
                door.AddComponent<Door>();
            
            // OneField
            var oneFields = FindObjectsOfType<OneField>();
            foreach (var oneField in oneFields)
                oneField.gameObject.AddComponent<OneFieldAsset>();
            
            // World Course
            var worldCourses = FindObjectsOfType<WorldCoursesInfo>();
            foreach (var worldCourse in worldCourses)
                worldCourse.gameObject.AddComponent<WorldCourseAsset>();            
            
        }

        public Transform GetActionPoint(string actionPointId)
        {
            foreach (var asset in assetById)
            {
                asset.Value.actionPointById.TryGetValue(actionPointId, out var actionPoint);
                if (actionPoint != null) return actionPoint;
            }
            return null;
        }

        public void ShowAssetName(Asset asset, Vector3 hitPoint = default)
        {
            if (asset == null)
            {
                _assetView.ShowAssetName(null);
                GameManager.Instance.starterController.SelectReticule(0);
                return;
            }
            
            var hint = asset.assetMenu is {Count: 1} || 
                       (asset.assetMenu is {Count: 2} && asset.assetType is {inventory: "1"})
                ? asset.assetMenu[0].name : "";
            
            if(asset.assetMenu != null && asset.assetMenu.Count > 0)
                GameManager.Instance.starterController.SelectReticule(1);

            if (!Input.touchSupported || XRSettings.enabled)
                _assetView.ShowAssetName(asset.assetName, hint);

            if (XRSettings.enabled)
                MoveCanvas(hitPoint);
        }
        
        public Asset GetAssetById(string id)
        {
            return assetById.FirstOrDefault(x => x.Key != null && x.Key.name == id).Value;
        }

        private void MoveCanvas(Vector3 hitPoint)
        {
            if(!XRSettings.enabled) return;
            
            var canvasTransform = _assetView.canvas.transform;
            canvasTransform.position = hitPoint;
            canvasTransform.rotation = Quaternion.LookRotation(canvasTransform.position - _vrCamera.position);
        }
    }
}
