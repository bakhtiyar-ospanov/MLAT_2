using System;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Starter
{
    public class FPVAcademix : MonoBehaviour
    {
        public GameObject canvas;
        public Camera cam;
        public OrbitCamera orbitCamera;
        public Transform lookTarget;
        //public Outliner outliner;
        public Button tabletBtn;
        public Button helpBtn;
        public Button openWorldBtn;
        public Texture2D[] _cursorTextures;
    }
}
