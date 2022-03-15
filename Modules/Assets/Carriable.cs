using UnityEngine;

namespace Modules.Assets
{
    public abstract class Carriable : MonoBehaviour
    {
        public Asset asset;
        public Camera cam;
        public Transform assetTrans;
        protected (Vector3, Vector3, Transform) InitialLocation;
        public Transform _rightHand;
        public GameObject _vrPointer;
        public abstract void ReturnItem();
    }
}
