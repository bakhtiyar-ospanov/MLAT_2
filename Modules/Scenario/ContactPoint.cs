using UnityEngine;

namespace Modules.Scenario
{
    public class ContactPoint : MonoBehaviour
    {
        public string pointName;
        public string bone;
        public string humanBone;

        public void RaycastOn()
        {
            transform.Translate(transform.up * 0.5f, Space.World);
            
            var ray = new Ray(transform.position, -transform.up);

            if (Physics.Raycast(ray, out var hit, 200.0f))
                transform.position = hit.point;
            
            if(pointName == "EXIT")
            {
                transform.Translate(transform.up * 0.08f, Space.World);
                transform.Translate(transform.right * 0.05f, Space.World);
            }

            if(pointName == "MENU")
            {
                transform.Translate(transform.up * 0.076f, Space.World);
                transform.Translate(-(transform.right * 0.055f), Space.World);
            }
                
        }

        public void SetParent(Transform newParent)
        {
            transform.SetParent(newParent, true);
        }

        public Transform GetParent()
        {
            return transform.parent;
        }
        
    }
}
