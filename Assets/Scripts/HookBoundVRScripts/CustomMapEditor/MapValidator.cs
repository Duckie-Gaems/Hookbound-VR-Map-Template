#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class MapValidator
{
    public static List<string> Validate(MapDescriptor descriptor)
    {
        List<string> errors = new List<string>();

        if (descriptor == null)
        {
            errors.Add("❌ MapDescriptor missing.");
            return errors;
        }

        if (string.IsNullOrEmpty(descriptor.mapName))
            errors.Add("❌ Map name cannot be empty.");

        if (string.IsNullOrEmpty(descriptor.exportFolder))
            errors.Add("❌ Export folder not set.");

        // FakeSkybox check
        GameObject sky = GameObject.Find("FakeSkybox");
        if (sky == null)
        {
            errors.Add("❌ FakeSkybox is missing from the scene. Add it manually under your map root.");
            return errors;
        }

        MeshRenderer mr = sky.GetComponentInChildren<MeshRenderer>();
        if (mr == null)
            errors.Add("❌ FakeSkybox has no MeshRenderer child.");

        if (mr != null)
        {
            Material mat = mr.sharedMaterial;
            if (mat == null)
                errors.Add("❌ FakeSkybox material missing.");

            if (mat != null && mat.GetTexture("_Tex") == null)
                errors.Add("❌ FakeSkybox material has no cubemap assigned.");
        }

        // Player spawn
        if (GameObject.Find("PlayerSpawnPod") == null)
            errors.Add("❌ PlayerSpawnPod missing from scene.");

        return errors;
    }
}
#endif
