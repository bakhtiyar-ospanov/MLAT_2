using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Assets;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Modules.MainMenu
{
    public class InventoryController : MonoBehaviour
    {
        private InventoryView _inventoryView;
        public List<Asset> availableItems = new List<Asset>();
        private Dictionary<Asset, AsyncOperationHandle<SceneInstance>> _handleByAsset 
            = new Dictionary<Asset, AsyncOperationHandle<SceneInstance>>();
        private Vector3 _putShift = new Vector3(0.0f, 0.4f, 0.0f);

        private void Awake()
        {
            _inventoryView = GetComponent<InventoryView>();
        }

        public void Init()
        {
            return;
            GameManager.Instance.mainMenuController.AddModule("Inventory", "", SetActivePanel, new []{_inventoryView.root.transform});
        }

        private void SetActivePanel(bool val)
        {
            _inventoryView.canvas.SetActive(val);
        }

        public void TakeItem(Asset asset)
        {
            if(availableItems.Contains(asset)) return;
            
            availableItems.Add(asset);
            
            var assetGo = asset.gameObject;
            assetGo.transform.SetParent(null);
            assetGo.transform.SetPositionAndRotation(Vector3.zero,  Quaternion.identity);
            DontDestroyOnLoad(assetGo);
            var handle = GameManager.Instance.starterController.SceneHandle;
            Addressables.ResourceManager.Acquire(handle);
            if(!_handleByAsset.ContainsKey(asset))
                _handleByAsset.Add(asset, handle);
            assetGo.SetActive(false);
            FillUpInventory();
        }

        private void FillUpInventory()
        {
            var assetNameById = BookDatabase.Instance.MedicalBook.assetById;
            
            var items = (from x in availableItems
                group x by x.assetId into g
                let count = g.Count()
                select new {Value = g.Key, Count = count}).ToList();
            
            var actionNames = items.Select(x => assetNameById[x.Value].name + (x.Count > 1 ? " (x" + x.Count + ")" : "")).ToList();
            var actions = items.Select(availableItem => (UnityAction) (() =>
            {
                GameManager.Instance.mainMenuController.ShowMenu(false);
                GameManager.Instance.assetMenuController.Init(
                    availableItems.FirstOrDefault(x => x.assetId == availableItem.Value), true);
            })).ToList();
            
            _inventoryView.SetValues(actionNames, actions);
        }

        public bool CheckItem(Asset asset)
        {
            return availableItems.Contains(asset);
        }

        public void PutItem(Asset asset)
        {
            availableItems.Remove(asset);
            FillUpInventory();
            StopAllCoroutines();
            var assetGo = asset.gameObject;
            SceneManager.MoveGameObjectToScene(assetGo, SceneManager.GetActiveScene());
            assetGo.SetActive(true);
            StartCoroutine(SnapObject(asset));
        }

        public IEnumerator SnapObject(Asset asset)
        {
            var assetGo = asset.gameObject;
            
            asset.BlockRigidBody(true);
            
            assetGo.transform.position = Vector3.zero;
            assetGo.transform.eulerAngles = Vector3.zero;
            
            var isClicked = false;
            var _cam = GameManager.Instance.starterController.GetCamera();
            var (upShift, forwardShift) = UIExtensions.FitToBounds(_cam, assetGo.transform);
            var eatFirstClick = true;

            while (!isClicked)
            {
                var ray = _cam.ScreenPointToRay(Input.mousePosition);
                var putPosition = _cam.transform.position + _cam.transform.forward * Mathf.Clamp(forwardShift, 0.4f, 4.0f);

                if (Physics.Raycast(ray, out var hit, 3.0f) && !hit.transform.CompareTag("Player") && !hit.transform.CompareTag("NoSnapArea"))
                {
                    putPosition = hit.point;
                }
                
                //putPosition = new Vector3(putPosition.x , putPosition.y + upShift.extents.y + 0.1f - upShift.center.y, putPosition.z);
                assetGo.transform.eulerAngles = Vector3.zero;
                assetGo.transform.position = putPosition;

                if (eatFirstClick)
                {
                    yield return new WaitForSeconds(0.01f);
                    eatFirstClick = false;
                }
                
                isClicked = Input.GetMouseButtonDown(0);
                yield return null;
            }
            
            asset.BlockRigidBody(false);
        }

        public void ReleaseItemFromMemory(Asset asset)
        {
            _handleByAsset.TryGetValue(asset, out var handle);
            if(!handle.IsValid()) return;

            _handleByAsset.Remove(asset);

            if (_handleByAsset.Values.Where(x => x.Equals(handle)).ToList().Count == 0)
                Addressables.Release(handle);
        }
    }
}
