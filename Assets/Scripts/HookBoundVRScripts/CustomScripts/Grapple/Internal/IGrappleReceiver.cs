using UnityEngine;

public interface IGrappleReceiver
{
    void OnGrappleAttached(MonoBehaviour hook);
    void OnGrappleDetached(MonoBehaviour hook);
}