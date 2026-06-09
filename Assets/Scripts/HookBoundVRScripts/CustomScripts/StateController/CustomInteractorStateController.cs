using UnityEngine;

public class CustomInteractorStateController : MonoBehaviour
{
public InteractionBase interBase;
public GrappleLayer grapple;
public PlayerLayer player;

public GameObject[] effectedObjects;

    public enum InteractionBase
    {
        Player,
        Grapple
    }
    public enum GrappleLayer
    {
        BaseGrapple,
        FlingGrapple,
        InteractGrapple
    }
public enum PlayerLayer
    {
        PassThrough,
        Slippery
    }
}
