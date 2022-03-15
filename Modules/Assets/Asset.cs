using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR;

namespace Modules.Assets
{
    public class Asset : MonoBehaviour
    {
        public string assetId;
        public string assetName;
        public Dictionary<string, Transform> actionPointById = new Dictionary<string, Transform>();
        public List<MedicalBase.MenuItem> assetMenu;
        public MedicalBase.Asset assetType;
        
        protected Collider[] _colliders;
        protected Animator _animator;
        private Animation _animation;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            assetId = name.Split(' ')[0];
            assetId = assetId.Replace("VA", "");
            Init();
        }
        
        public virtual void Init()
        {
            _colliders = GetComponentsInChildren<Collider>();
            AddColliderBridges();
            _rigidbody = GetComponentInChildren<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _animation = GetComponentInChildren<Animation>();
            
            BookDatabase.Instance.MedicalBook.assetById.TryGetValue(assetId, out assetType);
            if(assetType == null) return;
            
            assetName = assetType.name;
            
            foreach (Transform child in transform)
            {
                if(child.CompareTag("ContactPoint") && !actionPointById.ContainsKey(child.name))
                    actionPointById.Add(child.name, child);
            }

            var isSpecial = AddSpecialAsset();
            if(isSpecial) return;
            
            BookDatabase.Instance.MedicalBook.assetMenuById.TryGetValue(assetId, out assetMenu);
            ParseActions();
            ParseType();
        }

        private bool AddSpecialAsset()
        {
            switch (assetId)
            {
                case "316":
                    gameObject.AddComponent<Tonometer>();
                    return true;
                case "433":
                    gameObject.AddComponent<Glucometer>();
                    return true;
                case "443":
                    gameObject.AddComponent<InfraredThermometer>();
                    return true;
                case "437":
                    gameObject.AddComponent<Thermometer>();
                    return true;
                case "317":
                    gameObject.AddComponent<PulseOximeter>();
                    return true;
                case "308":
                    gameObject.AddComponent<Stethoscope>();
                    return true;
                case "318":
                    gameObject.AddComponent<Flashlight>();
                    return true;
                case "306":
                    gameObject.AddComponent<WeightScale>();
                    return true;
                case "408":
                    gameObject.AddComponent<Gloves>();
                    return true;
                case "314":
                    gameObject.AddComponent<Ophthalmoscope>();
                    return true;
                case "315":
                    gameObject.AddComponent<Otoscope>();
                    return true;
            }
            
            return false;
        }
        
        
        public void PlayAnimator(string animName)
        {
            if(_animator == null) return;
            _animator.Play(animName);
        }

        public void PlayAnimation(float startTime, float endTime)
        {
            StopAllCoroutines();
            StartCoroutine(PlayAnimationCor(startTime, endTime));
        }
        
        private void PlayAnimation(string _params)
        {
            if(_animation == null) return;
            
            var splitParams = _params.Split(',');
            float.TryParse(splitParams[0], out var start);
            float.TryParse(splitParams[1], out var end);
            if (Math.Abs(end) < 0.001f)
                end = _animation[_animation.clip.name].length;

            StartCoroutine(PlayAnimationCor(start, end));
        }
        
        private IEnumerator PlayAnimationCor(float startTime, float endTime)
        {
            if(Math.Abs(startTime - endTime) < 0.001f || _animation == null || _animation.clip == null)
                yield break;
            
            var clip = _animation.clip;
            _animation[clip.name].time = startTime;
            _animation[clip.name].speed = 1;
            _animation.Play(clip.name);

            while (_animation != null && _animation[_animation.clip.name].time < endTime)
            {
                yield return null;
            }

            if(_animation != null)
                _animation[_animation.clip.name].speed = 0;
        }

        
        private void ParseActions()
        {
            if(assetMenu == null) return;

            // remove Dimedus Courses location load
            if (XRSettings.enabled)
                assetMenu.RemoveAll(x => x.actions != null && x.actions.Contains("location.load(vl32)"));
            
            foreach (var menuItem in assetMenu)
            {
                if(menuItem.actions == null) continue;

                var callStack = new List<UnityAction>{ ()=> GameManager.Instance.assetMenuController.SetActivePanel(false)};
                callStack.AddRange(menuItem.actions.Select(ParseAction).Where(call => call != null).ToList());
                menuItem.call = () => { foreach (var action in callStack) action?.Invoke(); };
            }
        }
        
        readonly Dictionary<int, Regex> _patterns = new Dictionary<int, Regex>
        {
            {0, new Regex(@"location\.load\((.*)\)")},
            {2, new Regex(@"asset\((.*)\)\.complexAction\((.*)\)")},
            {3, new Regex(@"asset\((.*)\)\.animator\((.*)\)\.point\((.*)\)")},
            {4, new Regex(@"asset\((.*)\)\.animation\((.*)\)")},
            {6, new Regex(@"DimedusScenarioSelection\((.*)\)")}
        };

        private UnityAction ParseAction(string strAction)
        {
            var (type, match) = RegexMatcher(strAction);

            return type switch
            {
                0 => () => GameManager.Instance.starterController.InitNoWait(match.Groups[1].Value),
                2 => () => Debug.Log("NOT IMPLEMENTED"),
                3 => () => StartCoroutine(PlayAnimator(match.Groups[1].Value, match.Groups[2].Value,
                    match.Groups[3].Value)),
                4 => () => PlayAnimation(match.Groups[2].Value),
                //6 => () => GameManager.Instance.scenarioSelectorController.InitDimedusByArgs(match.Groups[1].Value),
                _ => null
            };
        }
        
        private (int, Match) RegexMatcher(string val)
        {
            foreach (var pattern in _patterns)
            {
                var match = pattern.Value.Match(val);
                if (match.Success)
                    return (pattern.Key, match);
            }
            return (-1, null);
        }

        private void ParseType()
        {
            if(assetType == null || assetType.inventory == "0") return;
            assetMenu ??= new List<MedicalBase.MenuItem>();
            
            switch (assetType.inventory)
            {
                case "1":
                    var putInInventory = new MedicalBase.MenuItem
                    {
                        name = TextData.Get(57),
                        call = () =>
                        {
                            GameManager.Instance.inventoryController.TakeItem(this); 
                            GameManager.Instance.assetMenuController.SetActivePanel(false);
                        }
                    };
                    var takeOutOfInventory = new MedicalBase.MenuItem
                    {
                        name = TextData.Get(58),
                        call = () =>
                        {
                            GameManager.Instance.inventoryController.PutItem(this);
                            GameManager.Instance.assetMenuController.SetActivePanel(false);
                        }
                    };
                    assetMenu.Add(putInInventory);
                    assetMenu.Add(takeOutOfInventory);
                    break;
                case "2":
                {
                    var takeInHand = new MedicalBase.MenuItem
                    {
                        name = TextData.Get(59),
                        call = () =>
                        {
                            StartCoroutine(GameManager.Instance.inventoryController.SnapObject(this));
                            GameManager.Instance.assetMenuController.SetActivePanel(false);
                        }
                    };
                    assetMenu.Add(takeInHand);
                    break;
                }
            }
        }
        
        public void BlockRigidBody(bool val)
        {
            if(_rigidbody != null)
                _rigidbody.isKinematic = val;
            
            foreach (var collider1 in _colliders)
                if(collider1 != null)
                    collider1.enabled = !val;
        }

        public void BlockCollider(bool val)
        {
            foreach (var collider1 in _colliders)
                if(collider1 != null)
                    collider1.enabled = !val;
        }

        private IEnumerator PlayAnimator(string id, string stateName, string actionPoint)
        {
            GameManager.Instance.assetMenuController.SetActivePanel(false);
            if (string.IsNullOrEmpty(id))
            {
                if(_animator == null) yield break;
                var point = !string.IsNullOrEmpty(actionPoint) 
                    ? GameManager.Instance.assetController.GetActionPoint(actionPoint) : null;

                if(point != null)
                    transform.SetPositionAndRotation(point.position, point.rotation);
                    
                _animator.Play(stateName);
                yield return StartCoroutine(AnimWaiter(_animator));
                GameManager.Instance.checkTableController.RegisterTriggerInvoke(stateName);
            }
            else
            {
                var handle = Addressables.InstantiateAsync(id);
                yield return handle;
                if(handle.Status != AsyncOperationStatus.Succeeded) yield break;
                
                var asset = handle.Result;
                actionPointById.TryGetValue(actionPoint, out var point);
                
                if(point != null)
                    asset.transform.SetPositionAndRotation(point.position, point.rotation);

                var animator = asset.GetComponentInChildren<Animator>();
                if(_animator == null) yield break;
                animator.Play(stateName);
                yield return StartCoroutine(AnimWaiter(animator));
                yield return new WaitForSeconds(1.0f);
                Addressables.ReleaseInstance(handle);
            }
        }

        private IEnumerator AnimWaiter(Animator animator)
        {
            while (animator != null && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) 
            { yield return null; }
        }
        
        protected void AddColliderBridges()
        {
            foreach (var col in _colliders)
            {
                var obj = col.gameObject;
                if(obj.GetComponent<ColliderBridge>() == null)
                    obj.AddComponent<ColliderBridge>().asset = this;
            }
        }

        private void OnDestroy()
        {
            if(GameManager.Instance != null && GameManager.Instance.inventoryController != null)
                GameManager.Instance.inventoryController.ReleaseItemFromMemory(this);
        }
    }
}
