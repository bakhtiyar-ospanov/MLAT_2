using System;
using System.Collections;
using System.Linq;
using CrazyMinnow.SALSA;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset
    {
        private Transform _frontLookAim;
        private Eyes _eyes;
        private Salsa _salsa;
        private Coroutine _delayedFollowPlayer;
        
        public void FollowPlayer(bool val, bool isStraightLook, Transform newTarget = null, bool isHeadRandom = true)
        {
            if(_eyes == null)
                return;
            
            if (val)
            {
                _eyes.lookTarget = GameManager.Instance.starterController.GetLookTarget();
                _eyes.useAffinity = !isStraightLook;
            }
            else
            {
                _eyes.useAffinity = !isStraightLook;
                _eyes.lookTarget = newTarget;
            }
            _eyes.headRandom = isHeadRandom;
        }

        private void SetupSalsa()
        {
            var smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (GetSMRandBlendIndex("Lip_Open", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Tongue_Raise", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Tongue_Out", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Mouth_Bottom_Lip_Bite", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Open", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Tight", smrs).blendIndex == -1 ||
                GetSMRandBlendIndex("Mouth_Bottom_Lip_Under", smrs).blendIndex == -1 ||
                GetBone("JawRoot") == null) 
                { SetupEyes(smrs); return; }
            
            // Configure Salsa\
            var modelObj = model.gameObject;
            _salsa = modelObj.AddComponent<Salsa>();
            _audioSource = modelObj.AddComponent<AudioSource>();
            _salsa.audioSrc = _audioSource;
            _salsa.queueProcessor = modelObj.AddComponent<QueueProcessor>();
            _salsa.useTimingsOverride = false;
            _salsa.originalUpdateDelay = 0.08509f;
            _salsa.visemes.Clear();

            // setup viseme 0 -- etc
            _salsa.visemes.Add(new LipsyncExpression("etc", new InspectorControllerHelperData(), 0f));
            var etcLE = _salsa.visemes[0].expData;

            var smrParams = GetSMRandBlendIndex("Lip_Open", smrs);
            etcLE.components[0].durationOn = .084f;
            etcLE.components[0].durationOff = .105f;
            etcLE.controllerVars[0].smr = smrParams.smr;
            etcLE.controllerVars[0].blendIndex = smrParams.blendIndex;

            // setup viseme 1 -- L
            _salsa.visemes.Add(new LipsyncExpression("L", new InspectorControllerHelperData(), 0f));
            var LLE = _salsa.visemes[1].expData;
            LLE.components[0].durationOn = .084f;
            LLE.components[0].durationOff = .105f;
            LLE.components[0].controlType = ExpressionComponent.ControlType.Bone;
            //LLE.components[0].isAnimatorControlled = true;
            var jawBone = GetBone("JawRoot");
            LLE.controllerVars[0].bone = jawBone;
            LLE.controllerVars[0].StoreStartTform();
            LLE.controllerVars[0].StoreEndTform();
            LLE.controllerVars[0].endTform.rot = Quaternion.Euler(LLE.controllerVars[0].endTform.rot.eulerAngles + new Vector3(0.0f, 0.0f, -5.416f));
            
            LLE.controllerVars.Add(new InspectorControllerHelperData());
            LLE.components.Add(new ExpressionComponent());
            LLE.components[1].name = "component 1";
            LLE.components[1].durationOn = .084f;
            LLE.components[1].durationOff = .105f;
            LLE.components[1].easing = LerpEasings.EasingType.CubicOut;
            LLE.components[1].controlType = ExpressionComponent.ControlType.Shape;
            smrParams = GetSMRandBlendIndex("Tongue_Raise", smrs);
            LLE.controllerVars[1].smr = smrParams.smr;
            LLE.controllerVars[1].blendIndex = smrParams.blendIndex;
            
            LLE.controllerVars.Add(new InspectorControllerHelperData());
            LLE.components.Add(new ExpressionComponent());
            LLE.components[2].name = "component 2";
            LLE.components[2].durationOn = .084f;
            LLE.components[2].durationOff = .105f;
            LLE.components[2].easing = LerpEasings.EasingType.CubicOut;
            LLE.components[2].controlType = ExpressionComponent.ControlType.Shape;
            smrParams = GetSMRandBlendIndex("Tongue_Out", smrs);
            LLE.controllerVars[2].smr = smrParams.smr;
            LLE.controllerVars[2].blendIndex = smrParams.blendIndex;
            
            // setup viseme 2 -- FV
            _salsa.visemes.Add(new LipsyncExpression("FV", new InspectorControllerHelperData(), 0f));
            var FVLE = _salsa.visemes[2].expData;
            smrParams = GetSMRandBlendIndex("Mouth_Bottom_Lip_Under", smrs);
            FVLE.controllerVars[0].smr = smrParams.smr;
            FVLE.controllerVars[0].blendIndex = smrParams.blendIndex;
            
            FVLE.controllerVars.Add(new InspectorControllerHelperData());
            FVLE.components.Add(new ExpressionComponent());
            FVLE.components[1].name = "component 1";
            FVLE.components[1].durationOn = .084f;
            FVLE.components[1].durationOff = .105f;
            FVLE.components[1].easing = LerpEasings.EasingType.CubicOut;
            FVLE.components[1].controlType = ExpressionComponent.ControlType.Shape;
            smrParams = GetSMRandBlendIndex("Mouth_Bottom_Lip_Bite", smrs);
            FVLE.controllerVars[1].smr = smrParams.smr;
            FVLE.controllerVars[1].blendIndex = smrParams.blendIndex;

            // setup viseme 3 -- O
            _salsa.visemes.Add(new LipsyncExpression("O", new InspectorControllerHelperData(), 0f));
            var OLE = _salsa.visemes[3].expData;
            OLE.components[0].durationOn = .084f;
            OLE.components[0].durationOff = .105f;
            OLE.components[0].controlType = ExpressionComponent.ControlType.Bone;
            OLE.controllerVars[0].bone = jawBone;
            OLE.controllerVars[0].StoreStartTform();
            OLE.controllerVars[0].StoreEndTform();
            OLE.controllerVars[0].endTform.rot = Quaternion.Euler(OLE.controllerVars[0].endTform.rot.eulerAngles + new Vector3(0.0f, 0.0f, -5.416f));

            OLE.controllerVars.Add(new InspectorControllerHelperData());
            OLE.components.Add(new ExpressionComponent());
            OLE.components[1].name = "component 1";
            OLE.components[1].durationOn = .084f;
            OLE.components[1].durationOff = .105f;
            OLE.components[1].easing = LerpEasings.EasingType.CubicOut;
            OLE.components[1].controlType = ExpressionComponent.ControlType.Shape;
            smrParams = GetSMRandBlendIndex("Tight", smrs);
            OLE.controllerVars[1].smr = smrParams.smr;
            OLE.controllerVars[1].blendIndex = smrParams.blendIndex;
            OLE.controllerVars[1].maxShape = 0.5486f;
            
            // // setup viseme 4 -- AI
            _salsa.visemes.Add(new LipsyncExpression("AI", new InspectorControllerHelperData(), 0f));
            var AILE = _salsa.visemes[4].expData;
            AILE.components[0].durationOn = .084f;
            AILE.components[0].durationOff = .105f;
            AILE.components[0].controlType = ExpressionComponent.ControlType.Bone;
            AILE.controllerVars[0].bone = jawBone;
            AILE.controllerVars[0].StoreStartTform();
            AILE.controllerVars[0].StoreEndTform();
            AILE.controllerVars[0].endTform.rot = Quaternion.Euler(AILE.controllerVars[0].endTform.rot.eulerAngles + new Vector3(0.0f, 0.0f, -7.348f));
            
            AILE.controllerVars.Add(new InspectorControllerHelperData());
            AILE.components.Add(new ExpressionComponent());
            AILE.components[1].name = "component 1";
            AILE.components[1].durationOn = .084f;
            AILE.components[1].durationOff = .105f;
            AILE.components[1].easing = LerpEasings.EasingType.CubicOut;
            AILE.components[1].controlType = ExpressionComponent.ControlType.Shape;
            smrParams = GetSMRandBlendIndex("Open", smrs);
            AILE.controllerVars[1].smr = smrParams.smr;
            AILE.controllerVars[1].blendIndex = smrParams.blendIndex;

            _salsa.visemes[0].trigger = 0.013f;
            _salsa.visemes[1].trigger = 0.116f;
            _salsa.visemes[2].trigger = 0.291f;
            _salsa.visemes[3].trigger = 0.397f;
            _salsa.visemes[4].trigger = 0.513f;
            _salsa.UpdateExpressionControllers();

            SetupEyes(smrs);
            GameManager.Instance.starterController.onMovingChanged += OnMovingChanged;
        }

        private void SetupEyes(SkinnedMeshRenderer[] smrs)
        {
            if(GetSMRandBlendIndex("Eye_Blink", smrs).blendIndex == -1 ||
               GetBone("Head") == null ||
               GetBone("L_Eye") == null ||
               GetBone("R_Eye") == null)
                return;
            
            // System Properties
            var modelObj = model.gameObject;
            _eyes = modelObj.AddComponent<Eyes>();
            _eyes.characterRoot = model;
            _eyes.queueProcessor = modelObj.GetComponent<QueueProcessor>();
            if(_eyes.queueProcessor == null)
                _eyes.queueProcessor = modelObj.AddComponent<QueueProcessor>();

            _eyes.lookTarget = GameManager.Instance.starterController.GetLookTarget();
            _eyes.affinityPercentage = 0.4f;
            _eyes.useAffinity = true;

            // Heads - Bone_Rotation
            _eyes.BuildHeadTemplate(Eyes.HeadTemplates.Bone_Rotation_XY);
            _eyes.heads[0].expData.controllerVars[0].bone = GetBone("Head");
            _eyes.heads[0].expData.components[0].easing = LerpEasings.EasingType.CubicOut;
            _eyes.heads[0].expData.components[0].durationOn = 0.3f;
            _eyes.heads[0].expData.components[0].durationOff = 0.3f;
            _eyes.heads[0].expData.components[0].isAnimatorControlled = true;
            _eyes.headTargetOffset.y = -0.3f;
            _eyes.headClamp = new Vector3(45.0f, 60.0f, 0.0f); 

            // Eyes - Bone_Rotation
            _eyes.BuildEyeTemplate(Eyes.EyeTemplates.Bone_Rotation);
            _eyes.RemoveExpression(ref _eyes.eyes, 2);
            
            _eyes.eyes[0].expData.controllerVars[0].bone = GetBone("L_Eye");
            //_eyes.eyes[0].expData.components[0].isAnimatorControlled = true;
            _eyes.eyes[1].expData.controllerVars[0].bone = GetBone("R_Eye");
            //_eyes.eyes[1].expData.components[0].isAnimatorControlled = true;
            _eyes.eyeClamp = new Vector3(5f, 45f);

            // Eyelids - Blendshapes
            _eyes.BuildEyelidTemplate(Eyes.EyelidTemplates.BlendShapes);
            _eyes.eyelidSelection = Eyes.EyelidSelection.Upper;
            _eyes.RemoveExpression(ref _eyes.blinklids, 1);
            
            var smrParams = GetSMRandBlendIndex("Eye_Blink", smrs);
            _eyes.blinklids[0].expData.controllerVars[0].smr = smrParams.smr;
            _eyes.blinklids[0].expData.controllerVars[0].blendIndex = smrParams.blendIndex;
            _eyes.blinklids[0].expData.components[0].durationOn = 0.02f;
            _eyes.blinklids[0].expData.components[0].durationHold = 0.05f;
            
            _eyes.blinklids[0].expData.controllerVars[1].smr = smrParams.smr;
            _eyes.blinklids[0].expData.controllerVars[1].blendIndex = smrParams.blendIndex;
            _eyes.blinklids[0].expData.components[1].durationOn = 0.02f;
            _eyes.blinklids[0].expData.components[1].durationHold = 0.05f;
            _eyes.blinklids[0].expData.components[1].enabled = false;

            _eyes.EnableEyelidTrack(true);
            _eyes.CopyBlinkToTrack();
            _eyes.eyelidPercentEyes = 0.03f;
            _eyes.tracklids[0].expData.controllerVars[0].smr = smrParams.smr;
            _eyes.tracklids[0].expData.controllerVars[0].blendIndex = smrParams.blendIndex;
            _eyes.tracklids[0].expData.components[0].durationOn = 0.1f;
            _eyes.tracklids[0].expData.components[0].durationHold = 0.1f;

            // Update runtime controllers
            _eyes.UpdateRuntimeExpressionControllers(ref _eyes.heads);
            _eyes.UpdateRuntimeExpressionControllers(ref _eyes.eyes);
            _eyes.UpdateRuntimeExpressionControllers(ref _eyes.blinklids);
            _eyes.UpdateRuntimeExpressionControllers(ref _eyes.tracklids);
            //_eyes.FixAllTransformAxes(ref _eyes.eyes, true);
            // Initialize the Eyes module
            _eyes.Initialize();
            isForcedStraightHead = false;
        }
        

        private (SkinnedMeshRenderer smr, int blendIndex) GetSMRandBlendIndex(string blendShapeName, SkinnedMeshRenderer[] skinMeshes)
        {
            foreach (var smr in skinMeshes)
            {
                if (smr.sharedMesh.GetBlendShapeIndex(blendShapeName) != -1)
                    return (smr, smr.sharedMesh.GetBlendShapeIndex(blendShapeName));
            }

            return (null, -1);
        }

        private Transform GetBone(string boneName)
        {
            return (from child in _allChildren 
                where child.Key.ToLower().Contains(boneName.ToLower()) 
                select child.Value).FirstOrDefault();
        }

        private void OnMovingChanged(bool val)
        {
            try
            {
                if(_delayedFollowPlayer != null) 
                    StopCoroutine(_delayedFollowPlayer);
                
                _delayedFollowPlayer = null;

                if(val)
                    FollowPlayer(true, true);
                else
                    _delayedFollowPlayer = StartCoroutine(DelayedStopFollowPlayer());
            }
            catch (Exception) { }
            
        }

        private IEnumerator DelayedStopFollowPlayer()
        {
            yield return new WaitForSeconds(3.0f);
            FollowPlayer(true, false);
        }
    }
}
