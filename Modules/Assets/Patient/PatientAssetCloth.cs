using System.Collections;
using System.Collections.Generic;
using Modules.WDCore;
using UnityEngine;

namespace Modules.Assets.Patient
{
    public partial class PatientAsset
    {
        private readonly string[] _patientClothes = {"Top", "Bottom", "Shoes", "Pants", "Bra"};
        private List<(Mesh, int[], int)> _savedMeshes;
        
        private IEnumerator ShowClothes(bool val, bool isHideBlackout = true)
        {
            if(isNaked != val) yield break;
            yield return StartCoroutine(GameManager.Instance.blackout.Show());
            ShowClothesHelper(val);
            yield return StartCoroutine(GameManager.Instance.blackout.Hide());
        }

        private void ShowClothesHelper(bool val)
        {
            if(isNaked != val) return;
            if(_allChildren == null) return;
            isNaked = !val;
            
            RestoreHiddenMat(!val);
            
            for (var i = 0; i < 3; i++)
            {
                _allChildren.TryGetValue(_patientClothes[i], out var cloth);
                if(cloth != null) cloth.gameObject.SetActive(val);
            }
            
            for (var i = 3; i < 5; i++)
            {
                _allChildren.TryGetValue(_patientClothes[i], out var cloth);
                if (cloth != null) cloth.gameObject.SetActive(false);
            }
            
            RebakeBodyCollider();
        }
        
        private void RestoreHiddenMat(bool isToRestore)
        {
            if (_savedMeshes == null)
            {
                _savedMeshes = new List<(Mesh, int[], int)>();
                var smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinMesh in smrs)
                {
                    var skinMats = skinMesh.sharedMaterials;
                    for (var i = 0; i < skinMats.Length; i++)
                    {
                        var matName = skinMats[i].name.ToLower();
                        if (matName.Contains("body") || matName.Contains("leg"))
                            _savedMeshes.Add((skinMesh.sharedMesh, skinMesh.sharedMesh.GetTriangles(i), i));
                    }
                    skinMesh.updateWhenOffscreen = true;
                }
            }

            foreach (var savedMesh in _savedMeshes)
                savedMesh.Item1.SetTriangles(isToRestore ? savedMesh.Item2 : new int[0], savedMesh.Item3);
        }
    }
}
