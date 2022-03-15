using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Books;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset
    {
        public bool isNaked = true;
        public bool isForcedStraightHead;
        public string currentPosition;
        public Transform currentPoint;

        private IEnumerator LaunchState(string animName, string actionPointId)
        {
            yield return StartCoroutine(GameManager.Instance.blackout.Show());
            FollowPlayer(false, true, _frontLookAim, false);

            actionPointById.TryGetValue(actionPointId, out var actionPoint);
            if (actionPoint != null)
            {
                currentPoint = actionPoint;
                model.SetPositionAndRotation(actionPoint.position, actionPoint.rotation);
                GameManager.Instance.starterController.LookAt(actionPoint);
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(0.05f);
            }
            
            if (actionPoint != null)
                ControlCouch(animName);

            _animator.Play(animName);
            currentPosition = animName;
            yield return StartCoroutine(ControlHead("HeadStraight"));

            if (!animName.Contains("lie") && !animName.Contains("sit_lean_forward"))
                FollowPlayer(true, false);

            yield return StartCoroutine(GameManager.Instance.blackout.Hide());
        }

        private void ControlCouch(string animName)
        {
            List<GameObject> couchList = new List<GameObject>(Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "VA49"));
            Animator couchAnimator = null;

            if (couchList.Count == 0) return;

            foreach (var couchObj in couchList)
            {
                couchAnimator = couchObj.GetComponentInChildren<Animator>();
                if (couchAnimator == null)
                    continue;
            }

            if (couchAnimator == null) return;

            if (!couchAnimator.isActiveAndEnabled)
                couchAnimator.enabled = true;

            if (animName.Contains("sit"))
            {
                couchAnimator.Play("sit");
            }
            else if (animName.Contains("lie"))
            {
                couchAnimator.Play("lie");
            }
        }


        private IEnumerator ControlHands(string state)
        {
            var layerIndex = _animator.GetLayerIndex("Hands");
            var weights = (0, 0);
            var speed = 1.0f;

            if (state == "HandsDown")
            {
                if (Math.Abs(_animator.GetLayerWeight(layerIndex)) < 0.1f)
                    yield break;
                weights = (1, 0);
            }
            else
            {
                if(_animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(state)) yield break;
                    
                if (Math.Abs(_animator.GetLayerWeight(layerIndex) - 1.0f) < 0.1f)
                    yield return StartCoroutine(ControlHands("HandsDown"));
                    
                _animator.Play(state, layerIndex);
                weights = (0, 1);
            }

            var elapsed = 0.0f;
            const float duration = 1.0f;
            while (elapsed <= duration )
            {
                _animator.SetLayerWeight(layerIndex, Mathf.SmoothStep( weights.Item1, weights.Item2, elapsed / duration ));
                elapsed += Time.deltaTime * speed;
                yield return null;
            }
            
            if(state == "HandsDown")
                _animator.Play("Idle", layerIndex);
            
            RebakeBodyCollider();
        }
        
        public IEnumerator ControlHead(string state)
        {
            int layerIndex = _animator.GetLayerIndex("Head");
            var weights = (0, 0);
            var speed = 0.85f;

            if (state == "HeadStraight")
            {
                var isLying = currentPosition.Contains("lie") || currentPosition.Contains("sit_lean_forward");
                if(!_eyes.headEnabled && !isForcedStraightHead && !isLying)
                    _eyes.EnableHead(true);
                else if(_eyes.headEnabled && (isForcedStraightHead || isLying))
                    _eyes.EnableHead(false);
                    
                if (Math.Abs(_animator.GetLayerWeight(layerIndex)) < 0.1f)
                    yield break;
                weights = (1, 0);
            }
            else
            {
                if(_animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(state)) yield break;
                    
                if (Math.Abs(_animator.GetLayerWeight(layerIndex) - 1.0f) < 0.1f)
                    yield return StartCoroutine(ControlHead("HeadStraight"));
                    
                _eyes.EnableHead(false);
                _animator.Play(state, layerIndex);
                weights = (0, 1);
            }
            
            var elapsed = 0.0f;
            var duration = 1.0f;
            while (elapsed <= duration )
            {
                _animator.SetLayerWeight(layerIndex, Mathf.SmoothStep( weights.Item1, weights.Item2, elapsed / duration ));
                elapsed += Time.deltaTime * speed;
                yield return null;
            }
            
            if(state == "HeadStraight")
                _animator.Play("Idle", layerIndex);
            
            RebakeBodyCollider();
        }

        public IEnumerator StandOnWeightScale()
        {
            yield return StartCoroutine(LaunchState("stand", "WeightPoint"));

            var diseaseController = GameManager.Instance.diseaseHistoryController;
            var patientInfo = GameManager.Instance.scenarioLoader.PatientInstance;
            
            diseaseController.AddNewValue("patientInfo", "patientHeightSubtitle", TextData.Get(91));
            diseaseController.ExpandValue("patientInfo", "patientHeightSubtitle", "patientHeight", $"{patientInfo.height} {TextData.Get(92)}");
            
            diseaseController.AddNewValue("patientInfo", "patientWeightSubtitle", TextData.Get(93));
            diseaseController.ExpandValue("patientInfo", "patientWeightSubtitle","patientWeight", $"{patientInfo.weight} {TextData.Get(94)}");

            var bmi = Math.Round(patientInfo.weight / Mathf.Pow(patientInfo.height / 100.0f, 2), 1);
            diseaseController.AddNewValue("patientInfo", "patienBMISubtitle", TextData.Get(95));
            diseaseController.ExpandValue("patientInfo", "patienBMISubtitle","patientBMI", $"{bmi}");

            GameManager.Instance.checkTableController.RegisterTriggerInvoke("PatientStandOnWeight");
        }
        
        public IEnumerator LaunchPatientStateById(int type)
        {
            switch (type)
            {
                case 3:
                    yield return StartCoroutine(LaunchState("sit", "SitPoint"));
                    break;
                case 2:
                    yield return StartCoroutine(LaunchState("lie_down", "LayDownPoint"));
                    break;
                case 5:
                    yield return StartCoroutine(LaunchState("sit_lean_forward", "SitPoint"));
                    break;
                case 4:
                    yield return StartCoroutine(LaunchState("lie_left_side", "LayDownPoint"));
                    break;
                case 1:
                    yield return StartCoroutine(LaunchState("stand", "StandPoint"));
                    break;
            }

            RebakeBodyCollider();
        }

        private void RebakeBodyCollider()
        {
            for (var i = 0; i < _meshes.Length; i++)
            {
                var bakedMesh = new Mesh();
                _meshes[i].BakeMesh(bakedMesh);
                var meshCol = _colliders[i] as MeshCollider;
                meshCol.sharedMesh = bakedMesh;
            }
        }
    }
}
