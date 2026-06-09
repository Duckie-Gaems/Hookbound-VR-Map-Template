using UnityEngine;
using UnityEngine.Events;
public class GrappleEventOrganizer : MonoBehaviour, IGrappleReceiver
{
    [Header("Events")]
    public UnityEvent OnGrappleAttachedEvent;
    public UnityEvent OnGrappleDetachedEvent;
            public void OnGrappleAttached(MonoBehaviour hook)
    {
        // Code obsfucated
    }
                public void OnGrappleDetached(MonoBehaviour hook)
    {
                // Code obsfucated
    }
}
