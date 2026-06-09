#if UNITY_EDITOR
using System.Collections.Generic;

public static class ScriptWhitelist
{
    public static readonly HashSet<string> Allowed = new HashSet<string>
    {
        "MapDescriptor",
        "MapExporter",
        "GrappleEventOrganizer",
        "IGrappleReceiver",
        "XRButton",
        "LuaCreationWindow",
        "LuaScriptHolder",
        "XRButtonLuaTriggerer",
        "PlaceholderSpawner",
        "PlayerAdjuster",
        "PlayerAdjusterButtonManager",
        "CustomInteractorStateController",
        "SticktivesWindTunnel",
        "EasyXRButtonGameObjectManager",
        "EasyXRButtonTeleportationScript",
        "EasyXRButtonTeleportObjectManager",
        "EasyXRButtonUnityEventManager",
        "ChangeGameObjectsOnGamemode",
        "GamemodeManagerSpawner",
        "LuaRuntime",
        "TextMeshPro",
        "UniversalAdditionalLightData"
    };
}
#endif
