using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyXRSystems;


public class EasyXRButtonTeleportationScript : XRButton
{
        [Header("Teleportation Destination")]
        public Transform TeleportDestination;

    [Header("Miscellaneous")]
    public float TeleportationDelay = 0f;
    public float gravityDisableTime = 1f;

}
