using UnityEngine;
using EasyXRSystems;
public class EasyXRButtonGameObjectManager : XRButton
{
    public enum GameObjectManagerEnum{Enable, Disable}

    public GameObjectManagerEnum GameModeActiveState = GameObjectManagerEnum.Disable;
public GameObject[] EffectedObjects;
private GameObject ThisGameobject;
public bool IsToggle;
}
