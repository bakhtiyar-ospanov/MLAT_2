using UnityEngine;

namespace Modules.Starter
{
    public abstract class FPC : MonoBehaviour
    {
        public abstract void Init(GameObject playerStart);
        public abstract Camera GetCamera();
        public abstract Transform GetLookTarget();
        public abstract void LookAt(Transform target);
        public abstract void SetKinematic(bool val);
        public abstract void SetActivePanel(bool val);
    }
}
