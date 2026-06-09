using UnityEngine;

public class ChangeGameObjectsOnGamemode : MonoBehaviour
{
    public Gamemode requiredGamemode = Gamemode.PVP;
    public ObjectState stateFormat = ObjectState.enable;
    public GameObject[] targets;

    public enum Gamemode
    {
        Hub,
        GrappleBall,
        HotPotato,
        Casual,
        FPS,
        PVP,
        Race,
        Custom
    }

    public enum ObjectState
    {
        enable,
        disable
    }
}
