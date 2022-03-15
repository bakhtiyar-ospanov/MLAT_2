using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Scenario
{
    public class PointDetector : MonoBehaviour
    {
        [SerializeField] private Transform refSkeleton;

        public IEnumerator Init(GameObject model)
        {
            if(model == null) yield break;
            var animator = model.GetComponentInChildren<Animator>();
            if(animator == null || animator.avatar == null) yield break;
            animator.Play("A-pose");
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.1f);
            
            var avatar = animator.avatar;
            var newAll = model.GetComponentsInChildren<Transform>().ToDictionary(x => x.name);
            var refAll = refSkeleton.GetComponentsInChildren<Transform>().ToDictionary(x => x.name);

            var contactPointManager = GameManager.Instance.scenarioLoader.contactPointManager;
            foreach (var point in contactPointManager.contactPoints)
            {
                var newHuman = avatar.humanDescription.human.
                    FirstOrDefault(x => x.humanName == point.Value.humanBone);
                if(newHuman.boneName == null) continue;
                newAll.TryGetValue(newHuman.boneName, out var newBone);
                if(newBone == null) continue;
                refAll.TryGetValue(point.Value.bone, out var refBone);
                if(refBone == null) continue;
                var savedPos = newBone.position;
                var savedRot = newBone.rotation;
                newBone.SetPositionAndRotation(refBone.position, refBone.rotation);
                point.Value.SetParent(newBone);
                newBone.SetPositionAndRotation(savedPos, savedRot);
            }
            
            var skinMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.1f);
            
            var colliders = new List<MeshCollider>();
            for (var i = 0; i < skinMeshes.Length; i++)
            {
                var objName = skinMeshes[i].gameObject.name;
                if (objName == "Top" || objName == "Bottom" || objName == "Shoes") continue;
                var meshCollider = skinMeshes[i].gameObject.AddComponent<MeshCollider>();
                var bakedMesh = new Mesh();
                skinMeshes[i].BakeMesh(bakedMesh);
                meshCollider.sharedMesh = bakedMesh;
                colliders.Add(meshCollider);
            }
        
            model.transform.position = new Vector3(0.0f, 50.0f, 0.0f);

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.5f);
            DetectPoints();
            yield return new WaitForFixedUpdate();
     
            foreach (var meshCollider in colliders)
                Destroy(meshCollider);
            
            model.transform.position = Vector3.zero;
        }


        private void DetectPoints()
        {
            var contactPointManager = GameManager.Instance.scenarioLoader.contactPointManager;
            foreach (var contactPoint in contactPointManager.contactPoints)
            {
                contactPoint.Value.RaycastOn();
            }
        }

        private void AttachToBones(Avatar avatar, Transform[] originalSkeletonTransform)
        {
            if(avatar == null) return;
        
            var boneByHuman = avatar.humanDescription.human.ToDictionary(x => x.humanName, y => y.boneName);
            var boneByName = originalSkeletonTransform.ToDictionary(key => key.name, value => value);
            var contactPointManager = GameManager.Instance.scenarioLoader.contactPointManager;
            foreach (var contactPoint in contactPointManager.contactPoints)
            {
                if(contactPointManager.pointBoneMapper.ContainsKey(contactPoint.Key))
                {
                    boneByHuman.TryGetValue(contactPointManager.pointBoneMapper[contactPoint.Key], out var boneName);
                    if(boneName == null) continue;
                    boneByName.TryGetValue(boneName, out var bone);
                    contactPoint.Value.SetParent(bone);
                }
            }

            
        }
    

    }
}
