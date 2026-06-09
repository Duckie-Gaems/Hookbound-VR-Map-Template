#if UNITY_EDITOR
using UnityEngine;

public static class ScriptSanitizer
{
    public static int CleanScene()
    {
        int removed = 0;

        MonoBehaviour[] scripts = GameObject.FindObjectsOfType<MonoBehaviour>(true);

        foreach (var script in scripts)
        {
            if (script == null)
                continue;

            var type = script.GetType();
            string assembly = type.Assembly.GetName().Name;

            // 1. Unity built-in assemblies → ALWAYS SAFE
            if (assembly.StartsWith("UnityEngine") ||
                assembly.StartsWith("Unity.") ||
                assembly.StartsWith("UnityEditor"))
            {
                continue;
            }

if (assembly.StartsWith("UnityEngine") ||
    assembly.StartsWith("Unity.") ||
    assembly.Contains("TextMeshPro") ||
    assembly.Contains("TMPro") ||
    assembly.Contains("XR") ||
    assembly.Contains("InputSystem") ||
    assembly.Contains("RenderPipelines") ||   // ← THIS FIXES UniversalAdditionalLightData
    assembly.Contains("Universal")            // ← Extra safety
)
{
    continue;
}


            // 3. Only Assembly-CSharp scripts need whitelisting
            if (assembly == "Assembly-CSharp")
            {
                string name = type.Name;

                if (!ScriptWhitelist.Allowed.Contains(name))
                {
                    Debug.LogWarning($"[Whitelist] Removed unauthorized script '{name}' from '{script.gameObject.name}'");
                    Object.DestroyImmediate(script);
                    removed++;
                }
            }
        }

        return removed;
    }
}
#endif
