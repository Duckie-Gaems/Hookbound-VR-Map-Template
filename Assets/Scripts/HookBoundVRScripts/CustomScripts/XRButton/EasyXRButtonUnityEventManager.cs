using UnityEngine;
using EasyXRSystems;
using UnityEngine.Events;

namespace EasyXRSystems
{
    public class EasyXRButtonUnityEventManager : XRButton
    {
        public enum TypesofTouch { Hand, Body, Both }
        [Header("Unity Event")]
        public UnityEvent OnPressSequence;
        public UnityEvent OffPressSequence;

        [Header("General Settings")]
        public TypesofTouch ButtonTouchMode;
        private bool HadPressed = false;

    }
}
