using UnityEngine;

namespace Modules.Starter
{
    public class FPCVR : FPC
    {
        public Transform player;
        public Transform lookTarget;
        public Transform rightHand;
        public Transform leftHand;
        public Camera cam;
        public Rigidbody playerRb;
        public GameObject pointer;
        //public Outliner outliner;
#if UNITY_XR
        public OVRScreenFade ovrScreenFade;
#endif

        public override void Init(GameObject playerStart)
        {
            player.position = playerStart == null ? new Vector3(0.0f, 1.3f, 0.0f) : 
                new Vector3(playerStart.transform.position.x, playerStart.transform.position.y + 1.3f, playerStart.transform.position.z);
            player.eulerAngles = playerStart == null ? new Vector3() : playerStart.transform.eulerAngles;

            SetKinematic(false);
        }

        public override Camera GetCamera()
        {
            return cam;
        }

        public override Transform GetLookTarget()
        {
            return lookTarget;
        }
        
        public override void SetKinematic(bool val)
        {
            playerRb.isKinematic = val;
        }

        public override void SetActivePanel(bool val)
        {
            
        }
        
        public override void LookAt(Transform target)
        {
            
        }

        public GameObject GetPointer()
        {
            return pointer;
        }

        public Transform GetRightHand()
        {
            return rightHand;
        }
        
        public Transform GetLeftHand()
        {
            return leftHand;
        }
    }
}
