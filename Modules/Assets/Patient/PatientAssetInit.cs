using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset : Asset
    {
        public Transform model;
        private Dictionary<string, Transform> _allChildren;
        private SkinnedMeshRenderer[] _meshes;

        public override void Init()
        {
            base.Init();
            if (model == null)
            {
                if (GameManager.Instance.scenarioLoader.CurrentScenario == null)
                {
                    StartCoroutine(GameManager.Instance.scenarioSelectorController.Init());
                }
                else
                {
                    GameManager.Instance.scenarioSelectorController.AddTabToMenu();
                }
                    
                return;
            }
            _allChildren = transform.GetComponentsInChildren<Transform>().ToDictionary(x => x.name);

            AddCollider();
            AddExternalMenuButton();
            ShowClothesHelper(true);
            SetupSalsa();
            StartCoroutine(StartPulseSwitch());
            AddSpecialAssetMenus();
        }

        private void AddSpecialAssetMenus()
        {
            assetMenu ??= new List<MedicalBase.MenuItem>();
            
            /*
            var generalTalk = new MedicalBase.MenuItem { 
                name = TextData.Get(89),
                call = GeneralTalk};
                */
            
            var stand = new MedicalBase.MenuItem { 
                name = TextData.Get(104),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(LaunchPatientStateById(1));
                }};
            
            var sit = new MedicalBase.MenuItem { 
                name = TextData.Get(105),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(LaunchPatientStateById(3));
                }};
            
            var lieDown = new MedicalBase.MenuItem { 
                name = TextData.Get(106),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(LaunchPatientStateById(2));
                }};
                
            // var lieDownLeft = new MedicalBase.MenuItem { 
            //     name = TextData.Get(191),
            //     call = () =>
            //     {
            //         GameManager.Instance.assetMenuController.SetActivePanel(false);
            //         StartCoroutine(LaunchPatientStateById(4));
            //     }};
            
            /*var sitForward = new MedicalBase.MenuItem { 
                name = TextData.Get(192),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(LaunchPatientStateById(5));
                }};*/
            
            var handsDown = new MedicalBase.MenuItem { 
                name = TextData.Get(96),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHands("HandsDown"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HandsDown");
                }};
            
            /*var handsBehindHead = new MedicalBase.MenuItem { 
                name = TextData.Get(97),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHands("HandsBehindHead"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HandsBehindHead");
                }};*/
            
            var handsForwardPalmsUp = new MedicalBase.MenuItem { 
                name = TextData.Get(98),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHands("HandsForwardPalmsUp"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HandsForwardPalmsUp");
                }};
            
            var handsForwardPalmsDown = new MedicalBase.MenuItem { 
                name = TextData.Get(99),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHands("HandsForwardPalmsDown"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HandsForwardPalmsDown");
                }};
            
            /*var handsOnChest = new MedicalBase.MenuItem { 
                name = TextData.Get(100),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHands("HandsOnChest"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HandsOnChest");
                }};*/
                
            /*var headStraight = new MedicalBase.MenuItem { 
                name = TextData.Get(101),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHead("HeadStraight"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HeadStraight");
                }};
                
            var headRight = new MedicalBase.MenuItem { 
                name = TextData.Get(102),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHead("HeadRight"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HeadRight");
                }};
            
            var headLeft = new MedicalBase.MenuItem { 
                name = TextData.Get(103),
                call = () =>
                {
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ControlHead("HeadLeft"));
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("HeadLeft");
                }};*/
            
            
            var undress = new MedicalBase.MenuItem { 
                name = TextData.Get(118),
                call = () =>
                {
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("PatientNaked");
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ShowClothes(false));
                }};
            
            var dress = new MedicalBase.MenuItem { 
                name = TextData.Get(119),
                call = () =>
                {
                    GameManager.Instance.checkTableController.RegisterTriggerInvoke("PatientInClothes");
                    GameManager.Instance.assetMenuController.SetActivePanel(false);
                    StartCoroutine(ShowClothes(true));
                }};
            
            //assetMenu.Add(generalTalk);
            assetMenu.Add(stand);
            assetMenu.Add(sit);
            //assetMenu.Add(sitForward);
            assetMenu.Add(lieDown);
            //assetMenu.Add(lieDownLeft);
            assetMenu.Add(handsDown);
            //assetMenu.Add(handsBehindHead);
            assetMenu.Add(handsForwardPalmsUp);
            assetMenu.Add(handsForwardPalmsDown);
            //assetMenu.Add(handsOnChest);
            //assetMenu.Add(headStraight);
            //assetMenu.Add(headRight);
            //assetMenu.Add(headLeft);
            assetMenu.Add(undress);
            assetMenu.Add(dress);
        }

        private IEnumerator StartPulseSwitch()
        {
            var _smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
            var skinMats = (from smr in _smrs from smrSharedMaterial in smr.sharedMaterials 
                where smrSharedMaterial.shader.name == "Universal Render Pipeline/Lit" 
                      && smrSharedMaterial.GetTexture("_DetailNormalMap") != null select smrSharedMaterial).ToList();
            
            var speed = GameManager.Instance.scenarioLoader.CurrentScenario.pulse;
            if(skinMats.Count < 1) yield break;
            while (true)
            {
                foreach (var skinMat in skinMats)
                {
                    var pulse = (Mathf.Sin(Time.time * speed * 0.06f) + 1.0f);
                    skinMat.SetFloat("_DetailNormalMapScale", pulse);
                }
                yield return null;
            }
        }
        
        private void AddCollider()
        {
            var spineName = _animator.avatar.humanDescription.human.
                FirstOrDefault(x => x.humanName == "Chest").boneName;
            _allChildren.TryGetValue(spineName, out var spine);
            
            _frontLookAim = new GameObject("FrontLookAim").transform;
            _frontLookAim.SetParent(spine, false);
            _frontLookAim.localPosition = new Vector3(0.0f, 0.5f, 1.0f);
            // var capsuleCol = spine.gameObject.AddComponent<CapsuleCollider>();
            // capsuleCol.height = 1.2f;
            // capsuleCol.radius = 0.16f;
            //_colliders = new Collider[] {capsuleCol};

            _meshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            _colliders = _meshes.Select(x => x.gameObject.AddComponent<MeshCollider>()).ToArray();

            AddColliderBridges();
        }

        private void AddExternalMenuButton()
        {
            if(!Input.touchSupported) return;
            
            var menuTarget = GameManager.Instance.scenarioLoader.contactPointManager.menuTarget;
            menuTarget.TurnOn(GameManager.Instance.starterController.GetCamera());
            menuTarget.gameObject.AddComponent<ColliderBridge>().asset = this;
            
            var headName = _animator.avatar.humanDescription.human.
                FirstOrDefault(x => x.humanName == "Head").boneName;
            _allChildren.TryGetValue(headName, out var head);
            menuTarget.transform.SetParent(head);

            ShowExternalMenuButton(true);
        }

        public void ShowExternalMenuButton(bool val)
        {
            if(!Input.touchSupported) return;
            
            var contactPointManager = GameManager.Instance.scenarioLoader.contactPointManager;
            if(contactPointManager == null) return;
            var menuTarget = contactPointManager.menuTarget;
            menuTarget.transform.localPosition = new Vector3(0.0f, 0.3f, 0.0f);
            menuTarget.gameObject.SetActive(val);
        }

        // public void Unload()
        // {
        //     StopAllCoroutines();
        //     if(_salsa != null && _salsa.queueProcessor != null)
        //         DestroyImmediate(_salsa.queueProcessor);
        //     if(_salsa != null)
        //         DestroyImmediate(_salsa);
        //     if(_eyes != null)
        //         DestroyImmediate(_eyes);
        //     if(_audioSource != null)
        //         DestroyImmediate(_audioSource);
        //     DestroyImmediate(this);
        // }
    }
}
