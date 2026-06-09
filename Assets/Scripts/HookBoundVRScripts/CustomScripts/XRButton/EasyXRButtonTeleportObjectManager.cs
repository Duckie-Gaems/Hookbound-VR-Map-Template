using UnityEngine;
using EasyXRSystems;
using System.Threading.Tasks;

namespace EasyXRSystems
{
    public class EasyXRButtonTeleportObjectManager : XRButton
    {
        [Header("Teleportation Destination")]
        public Transform TeleportDestination;

        public GameObject[] EnabledObjects;
        public GameObject[] DisabledObjects;
        [Header("Miscellaneous")]
        public float TeleportationDelay = 0f;
        public float gravityDisableTime = 1f;
    }
}
