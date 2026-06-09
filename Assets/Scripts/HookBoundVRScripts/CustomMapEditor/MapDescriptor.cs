using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapDescriptor : MonoBehaviour
{
    public enum CustomSkyboxSize { Small, Medium, Large }
 [System.Flags]
public enum Gamemode
{
    Casual      = 1 << 0,
    HotPotato   = 1 << 1,
    GrappleBall = 1 << 2,
    FPS         = 1 << 3,
    PVP         = 1 << 4,
    Race        = 1 << 5,
    Custom     = 1 << 6
}

    [Header("Map Info")]
    public string mapName = "MyMap";
    [Range(3,10)]
    public int MaxPlayers = 10;
    public Gamemode gamemodes;
    [Header("Customization")]
    public Cubemap customSkybox;
    public CustomSkyboxSize skyboxSize = CustomSkyboxSize.Medium;
    [HideInInspector]
    public string exportFolder = "";

#if UNITY_EDITOR
private bool ValidateOfficialPodPrefab(PlaceholderSpawner pod)
{
    // Load the official prefab from Resources
    GameObject official = Resources.Load<GameObject>("Prefabs/Internal/Placeholders/PodSpawnerPlaceholder");

    if (official == null)
    {
        EditorUtility.DisplayDialog(
            "Missing Official Prefab",
            "The official PodSpawnerPlaceholder prefab could not be found in Resources.\n" +
            "Export aborted.",
            "OK"
        );
        return false;
    }

    // Get GUID of official prefab
    string officialPath = AssetDatabase.GetAssetPath(official);
    string officialGUID = AssetDatabase.AssetPathToGUID(officialPath);

    // Get prefab source of the pod in the scene
    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(pod.gameObject);

    if (prefab == null)
    {
        EditorUtility.DisplayDialog(
            "Invalid Pod Spawner",
            $"The Pod spawner '{pod.name}' is not a prefab instance.\n" +
            "Do NOT unpack or duplicate the PodSpawnerPlaceholder.",
            "OK"
        );
        return false;
    }

    string prefabPath = AssetDatabase.GetAssetPath(prefab);
    string prefabGUID = AssetDatabase.AssetPathToGUID(prefabPath);

    if (prefabGUID != officialGUID)
    {
        EditorUtility.DisplayDialog(
            "Incorrect Pod Spawner Prefab",
            $"The Pod spawner '{pod.name}' is NOT the official PodSpawnerPlaceholder.\n" +
            "Please replace it with the correct prefab.",
            "OK"
        );
        return false;
    }

    return true;
}

private void ApplyCustomSkybox()
{
    Transform root = this.transform;
    Transform skyboxTransform = root.Find("FakeSkybox");

    // CASE 1: No custom skybox → remove skybox object if it exists
    if (customSkybox == null)
    {
        if (skyboxTransform != null)
            DestroyImmediate(skyboxTransform.gameObject);

        return;
    }

    GameObject skyboxObj;

    // CASE 2: Skybox object does NOT exist → create a NEW scene object
    if (skyboxTransform == null)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Internal/Skybox/FakeSkybox");
        if (prefab == null)
            return;

        // Instantiate normally (NOT as prefab instance)
        skyboxObj = Instantiate(prefab, root);
        skyboxObj.name = "FakeSkybox";

#if UNITY_EDITOR
        // This removes ALL prefab metadata safely
        PrefabUtility.RecordPrefabInstancePropertyModifications(skyboxObj);
        PrefabUtility.UnpackPrefabInstance(skyboxObj, PrefabUnpackMode.Completely, InteractionMode.UserAction);
#endif
    }
    else
    {
        skyboxObj = skyboxTransform.gameObject;
    }

    // Apply texture
    Renderer modelRenderer = skyboxObj.GetComponentInChildren<Renderer>(true);
    if (modelRenderer == null)
        return;

    Material m = modelRenderer.sharedMaterial;
    if (m != null && m.HasProperty("_Tex"))
        m.SetTexture("_Tex", customSkybox);

    // Apply scale
    float size = skyboxSize switch
    {
        CustomSkyboxSize.Small => 500f,
        CustomSkyboxSize.Medium => 1000f,
        CustomSkyboxSize.Large => 1500f,
        _ => 1000f
    };

    modelRenderer.transform.localScale = new Vector3(size, size, size);
}


private bool ValidateMap()
{
    PlaceholderSpawner[] spawners = FindObjectsOfType<PlaceholderSpawner>(true);

    if (spawners.Length == 0)
    {
        EditorUtility.DisplayDialog(
            "Map Invalid",
            "No PlaceholderSpawner objects found in the scene.\n" +
            "You must place at least one spawn point.",
            "OK"
        );
        return false;
    }

    int podCount = 0;
    PlaceholderSpawner podSpawner = null;

    foreach (var spawner in spawners)
    {
        var status = PrefabUtility.GetPrefabInstanceStatus(spawner.gameObject);

        if (status != PrefabInstanceStatus.Connected)
        {
            EditorUtility.DisplayDialog(
                "Invalid Spawn Prefab",
                $"The spawn point '{spawner.name}' is not a valid packed prefab.\n" +
                "Spawn points must use the official PlaceholderSpawner prefab.\n" +
                "Do NOT unpack or duplicate them.",
                "OK"
            );
            return false;
        }

        if (spawner.spawnType == PlaceholderSpawner.SpawnType.Pod)
        {
            podCount++;
            podSpawner = spawner;
        }
    }

    if (podCount == 0)
    {
        EditorUtility.DisplayDialog(
            "Missing Pod Spawn",
            "Your map must contain exactly ONE PlaceholderSpawner with spawnType = Pod.",
            "OK"
        );
        return false;
    }

    if (podCount > 1)
    {
        EditorUtility.DisplayDialog(
            "Multiple Pod Spawns Found",
            $"Your map contains {podCount} Pod spawners.\n" +
            "Only ONE Pod spawn is allowed per map.",
            "OK"
        );
        return false;
    }

    // ---------------------------------------------------------
    // NEW: Verify PodSpawnerPlaceholder is the official prefab
    // ---------------------------------------------------------
    if (!ValidateOfficialPodPrefab(podSpawner))
        return false;

    return true;
}
#if UNITY_EDITOR
[ContextMenu("Update skybox")]
private void ContextUpdateSkybox()
{
    ApplyCustomSkybox();
}
#endif

    public void ExportMap()
    {
        if (string.IsNullOrEmpty(exportFolder))
        {
            EditorUtility.DisplayDialog(
                "Export Folder Missing",
                "Please select an export folder before exporting.",
                "OK"
            );
            return;
        }

        if (!Directory.Exists(exportFolder))
            Directory.CreateDirectory(exportFolder);
        if (!ValidateMap())
            return;
        ApplyCustomSkybox();


        MapExporter.ExportFromDescriptor(this);
    }
    public Gamemode GetFirstGamemode()
{
    int mask = (int)gamemodes;

    if (mask == 0)
        return Gamemode.Casual; // fallback if nothing selected

    int firstBit = mask & -mask; // lowest selected bit
    return (Gamemode)firstBit;
}
public string GetGamemodeListJson()
{
    // Define your custom order here
Gamemode[] order = (Gamemode[])Enum.GetValues(typeof(Gamemode));


    List<string> list = new List<string>();

    foreach (var gm in order)
    {
        if (gamemodes.HasFlag(gm))
            list.Add($"\"{gm}\"");
    }

    // Return a raw JSON array
    return "[" + string.Join(",", list) + "]";
}

[System.Serializable]
public class Wrapper
{
    public List<string> values;
}

public int GetSelectedGamemodeCount()
{
    int mask = (int)gamemodes;
    int count = 0;

    while (mask != 0)
    {
        mask &= mask - 1;
        count++;
    }

    return count;
}
public static string NormalizeVersion(string version)
{
    string[] parts = version.Split('.');

    // Remove trailing ".0" parts
    List<string> trimmed = new List<string>();

    foreach (string p in parts)
        trimmed.Add(p);

    // Remove trailing zeros
    for (int i = trimmed.Count - 1; i > 0; i--)
    {
        if (trimmed[i] == "0")
            trimmed.RemoveAt(i);
        else
            break;
    }

    return string.Join(".", trimmed);
}

#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapDescriptor))]
public class MapDescriptorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapDescriptor desc = (MapDescriptor)target;

        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Export Controls", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Export Folder:", GUILayout.Width(100));
        desc.exportFolder = EditorGUILayout.TextField(desc.exportFolder);

        if (GUILayout.Button("Browse", GUILayout.Width(70)))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Export Folder", desc.exportFolder, "");
            if (!string.IsNullOrEmpty(selected))
            {
                desc.exportFolder = selected;
                EditorUtility.SetDirty(desc);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("EXPORT MAP", GUILayout.Height(35)))
        {
            desc.ExportMap();
        }

        EditorGUILayout.HelpBox(
            "Exports the current scene using the updated smart exporter.\n" +
            "Make sure the scene is saved before exporting.",
            MessageType.Info
        );
    }
}
[CustomPropertyDrawer(typeof(MapDescriptor.Gamemode))]
public class GamemodesDrawer : PropertyDrawer
{
    private bool foldout = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure at least Casual is selected
        if (property.intValue == 0)
            property.intValue = 1 << 0; // Casual = bit 0

        // Draw foldout
        foldout = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            foldout,
            label,
            true
        );

        if (!foldout)
            return;

        EditorGUI.indentLevel++;

        // Move rect down for the toggles
        position.y += EditorGUIUtility.singleLineHeight + 2;

        int currentMask = property.intValue;
        int selectedCount = CountBits(currentMask);

        var enumValues = System.Enum.GetValues(typeof(MapDescriptor.Gamemode));
        var enumNames = System.Enum.GetNames(typeof(MapDescriptor.Gamemode));

        for (int i = 0; i < enumValues.Length; i++)
        {
            int bit = 1 << i;
            bool isSelected = (currentMask & bit) != 0;

            bool disable = selectedCount >= 3 && !isSelected;

            EditorGUI.BeginDisabledGroup(disable);

            bool toggled = EditorGUI.ToggleLeft(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                enumNames[i],
                isSelected
            );

            EditorGUI.EndDisabledGroup();

            if (toggled != isSelected)
            {
                if (toggled)
                    currentMask |= bit;
                else
                    currentMask &= ~bit;

                selectedCount = CountBits(currentMask);

                // Prevent empty selection — always force Casual
                if (currentMask == 0)
                    currentMask = 1 << 0; // Casual
            }

            position.y += EditorGUIUtility.singleLineHeight + 2;
        }

        property.intValue = currentMask;

        EditorGUI.indentLevel--;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!foldout)
            return EditorGUIUtility.singleLineHeight;

        int count = System.Enum.GetValues(typeof(MapDescriptor.Gamemode)).Length;
        return EditorGUIUtility.singleLineHeight * (count + 1) + 4;
    }

    private int CountBits(int v)
    {
        int c = 0;
        while (v != 0)
        {
            v &= v - 1;
            c++;
        }
        return c;
    }
}
#endif
